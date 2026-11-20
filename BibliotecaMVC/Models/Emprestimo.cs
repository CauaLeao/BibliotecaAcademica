using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
namespace BibliotecaMVC.Models;

public class Emprestimo
{
    public int ID { get; private set; }
    public Usuario Usuario { get; private set; }
    public Exemplar Exemplar { get; private set; }
    public DateTime DataRetirada { get; private set; }
    public DateTime DataDevolucaoPrevista { get; private set; }
    public DateTime? DataDevolucaoEfetiva { get; private set; }
    public int PrazoDias { get; private set; }
    public bool Renovado { get; private set; }


    // Situação calculada com base na data de devolução efetiva
    public SituacaoEmprestimo Situacao => DataDevolucaoEfetiva.HasValue
        ? SituacaoEmprestimo.Encerrado
        : SituacaoEmprestimo.Ativo;

    // Construtor para criar um novo empréstimo
    public Emprestimo(Usuario usuario, Exemplar exemplar, DateTime dataRetirada, int prazoDias)
    {
        Usuario = usuario ?? throw new RegraNegocioException("O usuário é obrigatório.");

        Exemplar = exemplar ?? throw new RegraNegocioException("O exemplar é obrigatório.");

        if (!Usuario.CadastroAtivo)
            throw new RegraNegocioException("Usuários inativos não podem realizar empréstimos (RN03).");

        if (prazoDias <= 0)
            throw new RegraNegocioException("O prazo de empréstimo em dias deve ser positivo.");

        PrazoDias = prazoDias;
        DataRetirada = dataRetirada.Date;
        DataDevolucaoPrevista = DataRetirada.AddDays(prazoDias);
        DataDevolucaoEfetiva = null;
        Renovado = false;

        // Atualiza a disponibilidade do exemplar para Emprestado (RN11)
        Exemplar.RegistrarEmprestimo();
    }


    // Reconstituição do Dapper sem disparar regras de novo empréstimo
    internal static Emprestimo Reconstituir(int id, Usuario usuario, Exemplar exemplar, DateTime dataRetirada, DateTime dataDevolucaoPrevista, DateTime? dataDevolucaoEfetiva, int prazoDias, bool renovado)
    {
        return new Emprestimo(usuario, exemplar, dataRetirada, prazoDias)
        {
            ID = id,
            DataDevolucaoPrevista = dataDevolucaoPrevista,
            DataDevolucaoEfetiva = dataDevolucaoEfetiva,
            Renovado = renovado
        };
    }


    // Renovação de empréstimo
    public void Renovar(DateTime dataReferencia)
    {
        if (Situacao != SituacaoEmprestimo.Ativo)
            throw new RegraNegocioException("Apenas empréstimos ativos podem ser renovados.");

        if (Renovado)
            throw new RegraNegocioException("Este empréstimo já foi renovado uma vez e não permite nova renovação (RN17).");

        if (EstaAtrasado(dataReferencia))
            throw new RegraNegocioException("Empréstimos em atraso não podem ser renovados (RN17).");

        if (!Usuario.CadastroAtivo)
            throw new RegraNegocioException("Usuário com cadastro inativo não pode renovar empréstimos (RN17).");

        DataDevolucaoPrevista = DataDevolucaoPrevista.AddDays(PrazoDias);

        Renovado = true;
    }

    //  Registro da devolução do emprestimo, atualizando a data de devolução efetiva e a disponibilidade do exemplar.
    public void RegistrarDevolucao(DateTime dataDevolucao)
    {
        if (Situacao == SituacaoEmprestimo.Encerrado)
            throw new RegraNegocioException("Este empréstimo já se encontra encerrado.");

        var dataDevolucaoSemHora = dataDevolucao.Date;

        if (dataDevolucaoSemHora < DataRetirada)
            throw new RegraNegocioException("A data de devolução não pode ser anterior à data de retirada.");

        // Só grava a data após passar por todas as checagens
        DataDevolucaoEfetiva = dataDevolucaoSemHora;
    }

    // Verificação de atraso
    public bool EstaAtrasado(DateTime dataReferencia)
    {
        var dataComparacao = (Situacao == SituacaoEmprestimo.Encerrado)

            ? DataDevolucaoEfetiva!.Value.Date
            : dataReferencia.Date;

        return dataComparacao > DataDevolucaoPrevista.Date;
    }


    // Cálculo de multa de R$ 1,00 por dia de atraso
    public decimal CalcularMulta(DateTime dataReferencia)
    {
        var dataFinal = (Situacao == SituacaoEmprestimo.Encerrado)

            ? DataDevolucaoEfetiva!.Value.Date
            : dataReferencia.Date;

        var diasAtraso = (dataFinal - DataDevolucaoPrevista.Date).Days;

        if (diasAtraso <= 0)

            return 0.00m;

        const decimal valorDiariaMulta = 1.00m;

        return diasAtraso * valorDiariaMulta;
    }

}