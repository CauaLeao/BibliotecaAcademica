using System.Globalization;
using BibliotecaMVC.Enums;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Repositories.Dapper;

public class ExemplarRepository : IExemplarRepository
{
    private readonly string _connectionString;

    public ExemplarRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
    }

    private SqliteConnection CriarConexao() => new SqliteConnection(_connectionString);

    // JOIN traz Exemplar + Livro + Categoria em uma única consulta
    private const string SelectBase = """
        SELECT e.ID, e.Codigo, e.EstadoConservacao, e.DataAquisicao, e.Disponibilidade,
               l.ID AS LivroID, l.Titulo AS LivroTitulo, l.Autor AS LivroAutor, l.ISBN AS LivroISBN, l.Resumo AS LivroResumo,
               c.ID AS CategoriaID, c.Nome AS CategoriaNome, c.Descricao AS CategoriaDescricao
        FROM Exemplares e
        INNER JOIN Livros l ON l.ID = e.LivroID
        INNER JOIN Categorias c ON c.ID = l.CategoriaID
        """;

    private class ExemplarRow
    {
        public int ID { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int EstadoConservacao { get; set; }
        public string DataAquisicao { get; set; } = string.Empty;
        public int Disponibilidade { get; set; }
        public int LivroID { get; set; }
        public string LivroTitulo { get; set; } = string.Empty;
        public string LivroAutor { get; set; } = string.Empty;
        public string LivroISBN { get; set; } = string.Empty;
        public string LivroResumo { get; set; } = string.Empty;
        public int CategoriaID { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public string CategoriaDescricao { get; set; } = string.Empty;
    }

    private static Exemplar MapearParaEntidade(ExemplarRow row)
    {
        var categoria = Categoria.Reconstituir(row.CategoriaID, row.CategoriaNome, row.CategoriaDescricao);
        var livro = Livro.Reconstituir(row.LivroID, row.LivroTitulo, row.LivroISBN, row.LivroAutor, row.LivroResumo, categoria);
        var dataAquisicao = DateTime.Parse(row.DataAquisicao, CultureInfo.InvariantCulture);

        return Exemplar.Reconstituir(
            row.ID,
            livro,
            row.Codigo,
            (EstadoConservacao)row.EstadoConservacao,
            dataAquisicao,
            (DisponibilidadeExemplar)row.Disponibilidade);
    }

    public async Task<Exemplar?> ObterPorIdAsync(int id)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE e.ID = @Id";
        var row = await conexao.QuerySingleOrDefaultAsync<ExemplarRow>(sql, new { Id = id });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<IEnumerable<Exemplar>> ObterTodosAsync()
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} ORDER BY l.Titulo, e.Codigo";
        var rows = await conexao.QueryAsync<ExemplarRow>(sql);
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<Exemplar?> ObterPorCodigoAsync(string codigo)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE LOWER(e.Codigo) = LOWER(@Codigo)";
        var row = await conexao.QuerySingleOrDefaultAsync<ExemplarRow>(sql, new { Codigo = codigo });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<IEnumerable<Exemplar>> ObterPorLivroIdAsync(int livroId)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE e.LivroID = @LivroId ORDER BY e.Codigo";
        var rows = await conexao.QueryAsync<ExemplarRow>(sql, new { LivroId = livroId });
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<IEnumerable<Exemplar>> ObterDisponiveisPorLivroIdAsync(int livroId)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE e.LivroID = @LivroId AND e.Disponibilidade = @Disponivel ORDER BY e.Codigo";
        var rows = await conexao.QueryAsync<ExemplarRow>(sql, new
        {
            LivroId = livroId,
            Disponivel = (int)DisponibilidadeExemplar.Disponivel
        });
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<int> AdicionarAsync(Exemplar entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            INSERT INTO Exemplares (Codigo, EstadoConservacao, DataAquisicao, Disponibilidade, LivroID)
            VALUES (@Codigo, @EstadoConservacao, @DataAquisicao, @Disponibilidade, @LivroID);
            SELECT last_insert_rowid();
            """;

        var parametros = new
        {
            entidade.Codigo,
            EstadoConservacao = (int)entidade.EstadoConservacao,
            DataAquisicao = entidade.DataAquisicao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Disponibilidade = (int)entidade.Disponibilidade,
            LivroID = entidade.Livro.ID
        };

        return await conexao.ExecuteScalarAsync<int>(sql, parametros);
    }

    public async Task AtualizarAsync(Exemplar entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            UPDATE Exemplares
            SET Codigo = @Codigo,
                EstadoConservacao = @EstadoConservacao,
                DataAquisicao = @DataAquisicao,
                Disponibilidade = @Disponibilidade
            WHERE ID = @ID;
            """;

        var parametros = new
        {
            entidade.ID,
            entidade.Codigo,
            EstadoConservacao = (int)entidade.EstadoConservacao,
            DataAquisicao = entidade.DataAquisicao.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Disponibilidade = (int)entidade.Disponibilidade
        };

        await conexao.ExecuteAsync(sql, parametros);
    }

    public async Task RemoverAsync(int id)
    {
        using var conexao = CriarConexao();
        const string sql = "DELETE FROM Exemplares WHERE ID = @Id;";
        await conexao.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> PossuiEmprestimosAsync(int exemplarId)
    {
        using var conexao = CriarConexao();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Emprestimos WHERE ExemplarID = @ExemplarId);";
        return await conexao.ExecuteScalarAsync<bool>(sql, new { ExemplarId = exemplarId });
    }
}
