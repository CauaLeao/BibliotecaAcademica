using System.Globalization;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Repositories.Dapper;

public class EmprestimoRepository : IEmprestimoRepository
{
    private readonly string _connectionString;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IExemplarRepository _exemplarRepository;

    // Reutiliza IUsuarioRepository e IExemplarRepository para reconstituir as associações sem duplicar mapeamento
    public EmprestimoRepository(
        IConfiguration configuration,
        IUsuarioRepository usuarioRepository,
        IExemplarRepository exemplarRepository)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
        _usuarioRepository = usuarioRepository;
        _exemplarRepository = exemplarRepository;
    }

    private SqliteConnection CriarConexao() => new SqliteConnection(_connectionString);

    private const string SelectBase = """
        SELECT ID, DataRetirada, DataDevolucaoPrevista, DataDevolucaoEfetiva,
               PrazoDias, Renovado, UsuarioID, ExemplarID
        FROM Emprestimos
        """;

    private class EmprestimoRow
    {
        public int ID { get; set; }
        public string DataRetirada { get; set; } = string.Empty;
        public string DataDevolucaoPrevista { get; set; } = string.Empty;
        public string? DataDevolucaoEfetiva { get; set; }
        public int PrazoDias { get; set; }
        public int Renovado { get; set; }
        public int UsuarioID { get; set; }
        public int ExemplarID { get; set; }
    }

    private async Task<Emprestimo> MapearParaEntidadeAsync(EmprestimoRow row)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(row.UsuarioID)
            ?? throw new InvalidOperationException($"Usuário ID {row.UsuarioID} não encontrado para o empréstimo ID {row.ID}.");

        var exemplar = await _exemplarRepository.ObterPorIdAsync(row.ExemplarID)
            ?? throw new InvalidOperationException($"Exemplar ID {row.ExemplarID} não encontrado para o empréstimo ID {row.ID}.");

        var dataRetirada = DateTime.Parse(row.DataRetirada, CultureInfo.InvariantCulture);
        var dataPrevista = DateTime.Parse(row.DataDevolucaoPrevista, CultureInfo.InvariantCulture);
        DateTime? dataEfetiva = string.IsNullOrWhiteSpace(row.DataDevolucaoEfetiva)
            ? null
            : DateTime.Parse(row.DataDevolucaoEfetiva, CultureInfo.InvariantCulture);

        return Emprestimo.Reconstituir(
            row.ID,
            usuario,
            exemplar,
            dataRetirada,
            dataPrevista,
            dataEfetiva,
            row.PrazoDias,
            row.Renovado == 1);
    }

    private async Task<List<Emprestimo>> MapearListaAsync(IEnumerable<EmprestimoRow> rows)
    {
        var resultado = new List<Emprestimo>();
        foreach (var row in rows)
            resultado.Add(await MapearParaEntidadeAsync(row));
        return resultado;
    }

    public async Task<Emprestimo?> ObterPorIdAsync(int id)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE ID = @Id";
        var row = await conexao.QuerySingleOrDefaultAsync<EmprestimoRow>(sql, new { Id = id });
        return row is null ? null : await MapearParaEntidadeAsync(row);
    }

    public async Task<IEnumerable<Emprestimo>> ObterTodosAsync()
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} ORDER BY DataRetirada DESC, ID DESC";
        var rows = await conexao.QueryAsync<EmprestimoRow>(sql);
        return await MapearListaAsync(rows);
    }

    public async Task<IEnumerable<Emprestimo>> ObterPorUsuarioIdAsync(int usuarioId)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE UsuarioID = @UsuarioId ORDER BY DataRetirada DESC, ID DESC";
        var rows = await conexao.QueryAsync<EmprestimoRow>(sql, new { UsuarioId = usuarioId });
        return await MapearListaAsync(rows);
    }

    public async Task<IEnumerable<Emprestimo>> ObterAtivosPorUsuarioIdAsync(int usuarioId)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE UsuarioID = @UsuarioId AND DataDevolucaoEfetiva IS NULL ORDER BY DataDevolucaoPrevista";
        var rows = await conexao.QueryAsync<EmprestimoRow>(sql, new { UsuarioId = usuarioId });
        return await MapearListaAsync(rows);
    }

    public async Task<IEnumerable<Emprestimo>> ObterAtivosAsync()
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE DataDevolucaoEfetiva IS NULL ORDER BY DataDevolucaoPrevista";
        var rows = await conexao.QueryAsync<EmprestimoRow>(sql);
        return await MapearListaAsync(rows);
    }

    public async Task<Emprestimo?> ObterAtivoPorExemplarIdAsync(int exemplarId)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE ExemplarID = @ExemplarId AND DataDevolucaoEfetiva IS NULL";
        var row = await conexao.QuerySingleOrDefaultAsync<EmprestimoRow>(sql, new { ExemplarId = exemplarId });
        return row is null ? null : await MapearParaEntidadeAsync(row);
    }

    public async Task<int> AdicionarAsync(Emprestimo entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            INSERT INTO Emprestimos
                (DataRetirada, DataDevolucaoPrevista, DataDevolucaoEfetiva, PrazoDias, Renovado, UsuarioID, ExemplarID)
            VALUES
                (@DataRetirada, @DataDevolucaoPrevista, @DataDevolucaoEfetiva, @PrazoDias, @Renovado, @UsuarioID, @ExemplarID);
            SELECT last_insert_rowid();
            """;

        var parametros = new
        {
            DataRetirada = entidade.DataRetirada.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DataDevolucaoPrevista = entidade.DataDevolucaoPrevista.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DataDevolucaoEfetiva = entidade.DataDevolucaoEfetiva?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            entidade.PrazoDias,
            Renovado = entidade.Renovado ? 1 : 0,
            UsuarioID = entidade.Usuario.ID,
            ExemplarID = entidade.Exemplar.ID
        };

        return await conexao.ExecuteScalarAsync<int>(sql, parametros);
    }

    public async Task AtualizarAsync(Emprestimo entidade)
    {
        using var conexao = CriarConexao();
        const string sql = """
            UPDATE Emprestimos
            SET DataDevolucaoPrevista = @DataDevolucaoPrevista,
                DataDevolucaoEfetiva = @DataDevolucaoEfetiva,
                Renovado = @Renovado
            WHERE ID = @ID;
            """;

        var parametros = new
        {
            entidade.ID,
            DataDevolucaoPrevista = entidade.DataDevolucaoPrevista.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DataDevolucaoEfetiva = entidade.DataDevolucaoEfetiva?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Renovado = entidade.Renovado ? 1 : 0
        };

        await conexao.ExecuteAsync(sql, parametros);
    }

    public async Task RemoverAsync(int id)
    {
        using var conexao = CriarConexao();
        const string sql = "DELETE FROM Emprestimos WHERE ID = @Id;";
        await conexao.ExecuteAsync(sql, new { Id = id });
    }
}
