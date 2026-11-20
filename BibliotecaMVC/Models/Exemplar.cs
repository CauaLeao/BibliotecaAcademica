using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
namespace BibliotecaMVC.Models;

public class Exemplar
{
    public int ID { get; private set; }
    public Livro Livro { get; private set; }
    public string Codigo { get; private set; }
    public EstadoConservacao EstadoConservacao { get; private set; }
    public DateTime DataAquisicao { get; private set; }
    public DisponibilidadeExemplar Disponibilidade { get; private set; }


    // Construtor internal: acessível apenas pela classe Livro no mesmo assembly
    internal Exemplar(Livro livro, string codigo, EstadoConservacao estadoConservacao, DateTime dataAquisicao)
    {
        Livro = livro ?? throw new RegraNegocioException("O livro de referência é obrigatório.");
        Codigo = ValidarCodigo(codigo);
        EstadoConservacao = ValidarEstadoConservacao(estadoConservacao);
        DataAquisicao = dataAquisicao.Date;

        // Danificado inicia Indisponível, os demais iniciam Disponíveis

        Disponibilidade = EstadoConservacao == EstadoConservacao.Danificado
            ? DisponibilidadeExemplar.Indisponivel
            : DisponibilidadeExemplar.Disponivel;
    }


    // Reconstituição para o Dapper
    internal static Exemplar Reconstituir(int id, Livro livro, string codigo, EstadoConservacao estadoConservacao, DateTime dataAquisicao, DisponibilidadeExemplar disponibilidade)
    {
        return new Exemplar(livro, codigo, estadoConservacao, dataAquisicao)
        {
            ID = id,
            Disponibilidade = disponibilidade
        };
    }


    public void AtualizarDados(string codigo, DateTime dataAquisicao)
    {
        var codigoValidado = ValidarCodigo(codigo);
        var dataValidada = dataAquisicao.Date;

        Codigo = codigoValidado;
        DataAquisicao = dataValidada;
    }


    public void AlterarConservacao(EstadoConservacao novoEstado)
    {
        if (Disponibilidade == DisponibilidadeExemplar.Emprestado)
            throw new RegraNegocioException("Não é possível alterar o estado de conservação de um exemplar emprestado.");

        var estadoValidado = ValidarEstadoConservacao(novoEstado);

        EstadoConservacao = estadoValidado;

        // Danificado força indisponibilidade
        if (EstadoConservacao == EstadoConservacao.Danificado)
            Disponibilidade = DisponibilidadeExemplar.Indisponivel;
    }


    public void RegistrarEmprestimo()
    {
        // Apenas exemplares disponíveis e em bom estado físico
        if (Disponibilidade != DisponibilidadeExemplar.Disponivel)
            throw new RegraNegocioException("O exemplar não está disponível para empréstimo.");

        if (EstadoConservacao == EstadoConservacao.Danificado)
            throw new RegraNegocioException("Exemplares danificados não podem ser emprestados (RN11).");

        Disponibilidade = DisponibilidadeExemplar.Emprestado;
    }


    public void RegistrarDevolucao(EstadoConservacao novoEstado, bool impedimentoCirculacao)
    {
        if (Disponibilidade != DisponibilidadeExemplar.Emprestado)
            throw new RegraNegocioException("O exemplar não consta como emprestado.");

        // Valida o estado antes de tocar no objeto
        var estadoValidado = ValidarEstadoConservacao(novoEstado);

        // Verifica a disponibilidade 
        var novaDisponibilidade = (estadoValidado == EstadoConservacao.Danificado || impedimentoCirculacao)

            ? DisponibilidadeExemplar.Indisponivel
            : DisponibilidadeExemplar.Disponivel;

        
        EstadoConservacao = estadoValidado;
        Disponibilidade = novaDisponibilidade;
    }

    public void TornarIndisponivel()
    {
        if (Disponibilidade == DisponibilidadeExemplar.Emprestado)
            throw new RegraNegocioException("Não é possível tornar indisponível um exemplar emprestado.");

        Disponibilidade = DisponibilidadeExemplar.Indisponivel;
    }


    public void Disponibilizar()
    {
        if (Disponibilidade == DisponibilidadeExemplar.Emprestado)
            throw new RegraNegocioException("Não é possível liberar um exemplar atualmente emprestado.");

        if (EstadoConservacao == EstadoConservacao.Danificado)
            throw new RegraNegocioException("Exemplares danificados não podem ser disponibilizados.");

        Disponibilidade = DisponibilidadeExemplar.Disponivel;
    }


    // Classificação temporal por data de aquisição
    public ClassificacaoTemporal ClassificarPorTempo(DateTime dataReferencia)
    {
        if (dataReferencia.Date < DataAquisicao)
            throw new RegraNegocioException("A data de referência não pode ser anterior à data de aquisição.");

        var doisAnosAposAquisicao = DataAquisicao.AddYears(2);

        return dataReferencia.Date < doisAnosAposAquisicao
            ? ClassificacaoTemporal.Novo
            : ClassificacaoTemporal.Velho;
    }


    private static string ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraNegocioException("O código do exemplar é obrigatório.");

        return codigo.Trim().ToUpper();
    }


    private static EstadoConservacao ValidarEstadoConservacao(EstadoConservacao estado)
    {
        if (!Enum.IsDefined(estado))
            throw new RegraNegocioException("Estado de conservação inválido.");

        return estado;
    }
}