using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Repositories.Dapper;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly string _connectionString;

    public CategoriaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
    }

    private SqliteConnection CriarConexao() => new SqliteConnection(_connectionString);

    private const string SelectBase = "SELECT ID, Nome, Descricao FROM Categorias";

    private class CategoriaRow
    {
        public int ID { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }

    private static Categoria MapearParaEntidade(CategoriaRow row)
        => Categoria.Reconstituir(row.ID, row.Nome, row.Descricao);

    public async Task<Categoria?> ObterPorIdAsync(int id)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE ID = @Id";
        var row = await conexao.QuerySingleOrDefaultAsync<CategoriaRow>(sql, new { Id = id });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<IEnumerable<Categoria>> ObterTodosAsync()
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} ORDER BY Nome";
        var rows = await conexao.QueryAsync<CategoriaRow>(sql);
        return rows.Select(MapearParaEntidade).ToList();
    }

    public async Task<Categoria?> ObterPorNomeAsync(string nome)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE LOWER(Nome) = LOWER(@Nome)";
        var row = await conexao.QuerySingleOrDefaultAsync<CategoriaRow>(sql, new { Nome = nome });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<int> AdicionarAsync(Categoria entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            INSERT INTO Categorias (Nome, Descricao)
            VALUES (@Nome, @Descricao);
            SELECT last_insert_rowid();
            """;

        return await conexao.ExecuteScalarAsync<int>(sql, new { entidade.Nome, entidade.Descricao });
    }

    public async Task AtualizarAsync(Categoria entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            UPDATE Categorias
            SET Nome = @Nome,
                Descricao = @Descricao
            WHERE ID = @ID;
            """;

        await conexao.ExecuteAsync(sql, new { entidade.ID, entidade.Nome, entidade.Descricao });
    }

    public async Task RemoverAsync(int id)
    {
        using var conexao = CriarConexao();
        const string sql = "DELETE FROM Categorias WHERE ID = @Id;";
        await conexao.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> PossuiLivrosAsync(int categoriaId)
    {
        using var conexao = CriarConexao();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Livros WHERE CategoriaID = @CategoriaId);";
        return await conexao.ExecuteScalarAsync<bool>(sql, new { CategoriaId = categoriaId });
    }
}
