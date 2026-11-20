using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BibliotecaMVC.Models;

public class  Livro
{
    public int ID { get; private set; }
    public string Titulo { get; private set; }
    public string ISBN { get; private set; }
    public string Autor { get; private set; }
    public string Resumo { get; private set; }
    public Categoria Categoria { get; private set; }


    // criando lista privada readonly para encapsular e evitar que a lista seja manipulada fora da classe
    private readonly List<Exemplar> _exemplares = new();
    public IReadOnlyCollection<Exemplar> Exemplares => _exemplares.AsReadOnly();

    public Livro(string titulo, string isbn, string autor, string resumo, Categoria categoria)
    {
        Titulo = ValidarTexto(titulo, "O título do livro é obrigatório");
        ISBN = ValidarISBN(isbn);
        Autor = ValidarTexto(autor, "O autor do livro é obrigatório");
        Resumo = ValidarTexto(resumo, "O resumo do livro é obrigatório");
        Categoria = categoria ?? throw new RegraNegocioException("A categoria do livro é obrigatória");
        
        Categoria.AdicionarLivro(this);// Registrando o livro na categoria associada
    }

    // Construtor para hidratação via Dapper
    internal static Livro Reconstituir(int id, string titulo, string isbn, string autor, string resumo, Categoria categoria)

    {
        return new Livro(titulo, isbn, autor, resumo, categoria)
        {
            ID = id
        };
    }

    public void AtualizarDados(string titulo, string isbn, string autor, string resumo)
    {
        // Valida todos os campos em variáveis temporárias primeiro
        var tituloValidado = ValidarTexto(titulo, "O título do livro é obrigatório.");
        var isbnValidado = ValidarISBN(isbn);
        var autorValidado = ValidarTexto(autor, "O autor do livro é obrigatório.");
        var resumoValidado = ValidarTexto(resumo, "O resumo do livro é obrigatório.");


        // Só altera o estado do objeto se todas as validações acima passarem com sucesso
        Titulo = tituloValidado;
        ISBN = isbnValidado;
        Autor = autorValidado;
        Resumo = resumoValidado;
    }

    // Método para alterar a categoria do livro, garante a coerência dos dois lados da agregação
    public void AlterarCategoria(Categoria novaCategoria)
    {
        if (novaCategoria == null)
            throw new RegraNegocioException("A nova categoria não pode ser nula.");

        if (Categoria.ID == novaCategoria.ID)
            return;

        Categoria.RemoverLivro(this);
        Categoria = novaCategoria;
        Categoria.AdicionarLivro(this);
    }

    // O livro cria e gerencia o ciclo de vida dos exemplares.
    public Exemplar CriarExemplar(string codigo, EstadoConservacao estadoConservacao, DateTime dataAquisicao)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraNegocioException("O código do exemplar é obrigatório.");

        var codigoFormatado = codigo.Trim().ToUpper();

        if (_exemplares.Any(e => e.Codigo.Equals(codigoFormatado, StringComparison.OrdinalIgnoreCase)))
            throw new RegraNegocioException("Já existe um exemplar com esse código cadastrado para este livro.");

        var exemplar = new Exemplar(this, codigoFormatado, estadoConservacao, dataAquisicao);
        _exemplares.Add(exemplar);

        return exemplar;
    }

    // Método para remover um exemplar, garantindo que ele não esteja emprestado
    public void RemoverExemplar(Exemplar exemplar)
    {
        if (exemplar == null)
            throw new RegraNegocioException("Exemplar inválido.");

        if (exemplar.Disponibilidade == DisponibilidadeExemplar.Emprestado)
            throw new RegraNegocioException("Não é permitido remover um exemplar que está atualmente emprestado.");


        _exemplares.Remove(exemplar);
    }

    private static string ValidarTexto(string texto, string mensagemErro)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new RegraNegocioException(mensagemErro);

        return texto.Trim();
    }
    private static string ValidarISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new RegraNegocioException("O ISBN é obrigatório.");

        return isbn.Replace("-", "").Trim();
    }

}