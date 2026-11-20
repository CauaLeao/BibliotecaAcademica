using BibliotecaMVC.Exceptions;

namespace BibliotecaMVC.Models;

public class Categoria
{
    public int ID { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }

    // Lista privada com encapsulamento para evitar que a lista seja manipulada fora da classe

    private readonly List<Livro> _livros = new();
    public IReadOnlyCollection<Livro> Livros => _livros.AsReadOnly();

    // Agora o contrutor para as novas categorias
    public Categoria(string nome, string descricao)
    {
        Nome = ValidarTexto(nome, "O nome da categoria é obrigatório");
        Descricao = ValidarTexto(descricao, "A descrição da categoria é obrigatória");

    }

    // Um construtor para a reconstituição do objeto no Dapper, protegido para a hidratação do banco de dados.

    protected Categoria(int id, string nome, string descricao)
    {
        ID = id;
        Nome = nome;
        Descricao = descricao;
    }

    // O método Reconstituir chama esse construtor protegido:
    internal static Categoria Reconstituir(int id, string nome, string descricao)
    {
        return new Categoria(id, nome, descricao);
    }

    //Criando alterar nome e descrição separados, para que seja possível alterar apenas um dos campos, sem precisar passar os dois.
    public void AlterarNome(string nome)
    {
        var nomeValidado = ValidarTexto(nome, "O nome da categoria é obrigatório.");
        
        Nome = nomeValidado;
    }
    public void AlterarDescricao(string descricao)
    {
        var descricaoValidada = ValidarTexto(descricao, "A descrição da categoria é obrigatória.");
       
        Descricao = descricaoValidada;
    }

    // Métodos de adicionar e remover livros da categoria.

    internal void AdicionarLivro(Livro livro)
    {
        if (livro == null)
            throw new RegraNegocioException("Livro inválido.");

        if (!_livros.Contains(livro))
            _livros.Add(livro);
    }

    internal void RemoverLivro(Livro livro)
    {

        if (_livros.Contains(livro))
            _livros.Remove(livro);
    }

    private static string ValidarTexto(string texto, string mensagemErro)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new RegraNegocioException(mensagemErro); // se não tiver nada, exibe mensagem de erro

        return texto.Trim();
    }
}