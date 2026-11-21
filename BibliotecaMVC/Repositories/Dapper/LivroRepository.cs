using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Repositories.Dapper;

public class LivroRepository : ILivroRepository
{
    private readonly string _connectionString;

    public LivroRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
    }

    private SqliteConnection CriarConexao() => new SqliteConnection(_connectionString);

    // JOIN traz Livro + Categoria em uma única consulta (evita N+1 queries)
    private const string SelectBase = """
        SELECT l.ID, l.Titulo, l.Autor, l.ISBN, l.Resumo,
               c.ID AS CategoriaID, c.Nome AS CategoriaNome, c.Descricao AS CategoriaDescricao
        FROM Livros l
        INNER JOIN Categorias c ON c.ID = l.CategoriaID
        """;

    private class LivroRow
    {
        public int ID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Resumo { get; set; } = string.Empty;
        public int CategoriaID { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public string CategoriaDescricao { get; set; } = string.Empty;
    }

    private static Livro MapearParaEntidade(LivroRow row)
    {
        var categoria = Categoria.Reconstituir(row.CategoriaID, row.CategoriaNome, row.CategoriaDescricao);
        return Livro.Reconstituir(row.ID, row.Titulo, row.ISBN, row.Autor, row.Resumo, categoria);
    }

    public async Task<Livro?> ObterPorIdAsync(int id)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE l.ID = @Id";
        var row = await conexao.QuerySingleOrDefaultAsync<LivroRow>(sql, new { Id = id });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<IEnumerable<Livro>> ObterTodosAsync()
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} ORDER BY l.Titulo";
        var rows = await conexao.QueryAsync<LivroRow>(sql);
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<Livro?> ObterPorIsbnAsync(string isbn)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE l.ISBN = @Isbn";
        var row = await conexao.QuerySingleOrDefaultAsync<LivroRow>(sql, new { Isbn = isbn });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<IEnumerable<Livro>> BuscarAsync(string? termo, int? categoriaId)
    {
        using var conexao = CriarConexao();
        var filtros = new List<string>();
        var parametros = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            filtros.Add("(LOWER(l.Titulo) LIKE @Termo OR LOWER(l.Autor) LIKE @Termo OR LOWER(l.ISBN) LIKE @Termo)");
            parametros.Add("Termo", $"%{termo.Trim().ToLower()}%");
        }

        if (categoriaId.HasValue)
        {
            filtros.Add("l.CategoriaID = @CategoriaId");
            parametros.Add("CategoriaId", categoriaId.Value);
        }

        var where = filtros.Count > 0 ? " WHERE " + string.Join(" AND ", filtros) : string.Empty;
        var sql = $"{SelectBase}{where} ORDER BY l.Titulo";

        var rows = await conexao.QueryAsync<LivroRow>(sql, parametros);
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<int> AdicionarAsync(Livro entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            INSERT INTO Livros (Titulo, Autor, ISBN, Resumo, CategoriaID)
            VALUES (@Titulo, @Autor, @ISBN, @Resumo, @CategoriaID);
            SELECT last_insert_rowid();
            """;

        var parametros = new
        {
            entidade.Titulo,
            entidade.Autor,
            entidade.ISBN,
            entidade.Resumo,
            CategoriaID = entidade.Categoria.ID
        };

        return await conexao.ExecuteScalarAsync<int>(sql, parametros);
    }

    public async Task AtualizarAsync(Livro entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            UPDATE Livros
            SET Titulo = @Titulo,
                Autor = @Autor,
                ISBN = @ISBN,
                Resumo = @Resumo,
                CategoriaID = @CategoriaID
            WHERE ID = @ID;
            """;

        var parametros = new
        {
            entidade.ID,
            entidade.Titulo,
            entidade.Autor,
            entidade.ISBN,
            entidade.Resumo,
            CategoriaID = entidade.Categoria.ID
        };

        await conexao.ExecuteAsync(sql, parametros);
    }

    public async Task RemoverAsync(int id)
    {
        using var conexao = CriarConexao();
        const string sql = "DELETE FROM Livros WHERE ID = @Id;";
        await conexao.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> PossuiExemplaresAsync(int livroId)
    {
        using var conexao = CriarConexao();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Exemplares WHERE LivroID = @LivroId);";
        return await conexao.ExecuteScalarAsync<bool>(sql, new { LivroId = livroId });
    }
}
