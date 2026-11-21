using System.Globalization;
using BibliotecaMVC.Enums;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Repositories.Dapper;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString; // recebe o endereõ do banco

    public UsuarioRepository(IConfiguration configuration) // injeta as configurações feitas em appsettings.json
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada."); // se o atributo receber nulo, gera a exceção
    }

    private SqliteConnection CriarConexao() => new SqliteConnection(_connectionString); // helper que encurta a instância de uma nova conexão

    private static (TipoUsuario Tipo, string? Matricula, int? Perfil, string? RegistroInstitucional) ExtrairCamposEspecificos(Usuario entidade)
    {
        return entidade switch // verifica o tipo real da entidade
        {   // se for tal tipo de usuário, retorna os devidos atributos
            Administrador adm => (TipoUsuario.Administrador, adm.Matricula, null, null),
            Bibliotecario bib => (TipoUsuario.Bibliotecario, bib.Matricula, null, null),
            Cliente cli       => (TipoUsuario.Cliente, null, (int)cli.Perfil, cli.RegistroInstitucional),
            _                 => throw new InvalidOperationException($"Tipo de usuário não suportado: {entidade.GetType().Name}")
        };
    }

    private const string SelectBase = """
        SELECT ID, Nome, CPF, Email, SenhaHash, DataCadastro, CadastroAtivo, TipoUsuario, Matricula, Perfil, RegistroInstitucional
        FROM Usuarios
        """; // base repetível para comandos de SELECT no banco

    private class UsuarioRow // DTO da classe Usuario
    {
        public int ID { get; set; }
        public string Nome { get; set; } = string.Empty; // torna os atributos de string vazios, mas não nulos
        public string CPF { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string DataCadastro { get; set; } = string.Empty;
        public int CadastroAtivo { get; set; }
        public int TipoUsuario { get; set; }
        public string? Matricula { get; set; } // "?" indica que o atributo pode ser passado como nulo
        public int? Perfil { get; set; }
        public string? RegistroInstitucional { get; set; }
    }

    private static Usuario MapearParaEntidade(UsuarioRow row) // traduz um UsuarioRow do banco para um objeto real Usuario
    {
        var dataCadastro = DateTime.Parse(row.DataCadastro, CultureInfo.InvariantCulture); // converte a data em string que veio do banco pra DateTime
        var cadastroAtivo = row.CadastroAtivo == 1; // converte para booleano o CadastroAtivo que veio do banco, que estava em inteiro

        switch ((TipoUsuario)row.TipoUsuario) // preenche os dados com base em TipoUsuario
        {
            case TipoUsuario.Cliente:
                if (row.Perfil is null || row.RegistroInstitucional is null)
                    throw new InvalidOperationException($"Dados incompletos para o Cliente ID{row.ID}.");

                return Cliente.Reconstituir(
                    row.ID, row.Nome, row.CPF, row.Email, row.SenhaHash, dataCadastro, cadastroAtivo, (TipoCliente)row.Perfil.Value, row.RegistroInstitucional
                );

            case TipoUsuario.Bibliotecario:
                if (row.Matricula is null)
                    throw new InvalidOperationException($"Dados incompletos para o Bibliotecario ID{row.ID}.");

                return Bibliotecario.Reconstituir(
                    row.ID, row.Nome, row.CPF, row.Email, row.SenhaHash, dataCadastro, cadastroAtivo, row.Matricula
                );

            case TipoUsuario.Administrador:
                if (row.Matricula is null)
                    throw new InvalidOperationException($"Dados incompletos para o Administrador ID{row.ID}.");

                return Administrador.Reconstituir(
                    row.ID, row.Nome, row.CPF, row.Email, row.SenhaHash, dataCadastro, cadastroAtivo, row.Matricula
                );
            default:
                throw new InvalidOperationException($"TipoUsuario inválido no banco: {row.TipoUsuario}.");
        }
    }

    public async Task<Usuario?> ObterPorIdAsync(int id) // busca por ID assíncrona
    {
        using var conexao = CriarConexao(); // instancia a conexão
        var sql = $"{SelectBase} WHERE ID = @Id"; // completa o comando de SelectBase com a ação desejada
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new {Id = id}); // realiza a consulta no banco, passando o parâmetro pro "@Id" 
                                                                                           // da consulta e retornando um UsuarioRow com os dados consultados
        return row is null ? null : MapearParaEntidade(row); // se o método não encontrar um usuário, retorna null, senão, reconstitui ele do banco
    }

    public async Task<IEnumerable<Usuario>> ObterTodosAsync() // retorna uma sequência de usuários, mas que não é uma lista
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} ORDER BY Nome";
        var rows = await conexao.QueryAsync<UsuarioRow>(sql);
        return rows.Select(MapearParaEntidade); // retorna a lista e vai traduzindo as entidades para classes concretas
    }

    public async Task<int> AdicionarAsync(Usuario entidade) // adiciona Usuario ao banco
    {
        var (tipo, matricula, perfil, registroInstitucional) = ExtrairCamposEspecificos(entidade); // atribui os campos do helper as respectivas variáveis
        using var conexao = CriarConexao();
        const string sql = """
            INSERT INTO Usuarios
                (Nome, CPF, Email, SenhaHash, DataCadastro, CadastroAtivo, TipoUsuario, Matricula, Perfil, RegistroInstitucional)
            VALUES
                (@Nome, @CPF, @Email, @SenhaHash, @DataCadastro, @CadastroAtivo, @TipoUsuario, @Matricula, @Perfil, @RegistroInstitucional);
            SELECT last_insert_rowid(); 
            """; // a última linha faz a atribuição automática do ID
        var parametros = new // cria um objeto anônimo com os dados recebidos do parâmetro
        {
            entidade.Nome,
            entidade.CPF,
            entidade.Email,
            entidade.SenhaHash,
            DataCadastro = entidade.DataCadastro.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            CadastroAtivo = entidade.CadastroAtivo ? 1 : 0,
            TipoUsuario = (int)tipo,
            Matricula = matricula,
            Perfil = perfil,
            RegistroInstitucional = registroInstitucional
        };
        return await conexao.ExecuteScalarAsync<int>(sql, parametros); // atribui os parâmetros aos @s do comando e roda ele no banco, retornando o ID recém gerado
    }

    public async Task AtualizarAsync(Usuario entidade) // atualiza os dados mutáveis de um usuário existente
    {
        var (_, matricula, perfil, registroInstitucional) = ExtrairCamposEspecificos(entidade);
        using var conexao = CriarConexao();
        const string sql = """
            UPDATE Usuarios
            SET Nome = @Nome,
                Email = @Email,
                SenhaHash = @SenhaHash,
                CadastroAtivo = @CadastroAtivo,
                Matricula = @Matricula,
                Perfil = @Perfil,
                RegistroInstitucional = @RegistroInstitucional
            WHERE ID = @ID;
            """;

        var parametros = new
        {
            entidade.ID,
            entidade.Nome,
            entidade.Email,
            entidade.SenhaHash,
            CadastroAtivo = entidade.CadastroAtivo ? 1 : 0,
            Matricula = matricula,
            Perfil = perfil,
            RegistroInstitucional = registroInstitucional
        };

        await conexao.ExecuteAsync(sql, parametros);
    }

    public async Task RemoverAsync(int id) // remove um usuário por ID (protegido por ON DELETE RESTRICT no banco)
    {
        using var conexao = CriarConexao();
        const string sql = "DELETE FROM Usuarios WHERE ID = @Id;";
        await conexao.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email) // busca usuário por e-mail (ignorando maiúsculas/minúsculas)
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE LOWER(Email) = LOWER(@Email)";
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new { Email = email });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<Usuario?> ObterPorCpfAsync(string cpf) // busca usuário por CPF
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE CPF = @Cpf";
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new { Cpf = cpf });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<Usuario?> ObterPorMatriculaAsync(string matricula) // busca bibliotecário/administrador por matrícula
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE Matricula = @Matricula";
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new { Matricula = matricula });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<Usuario?> ObterPorRegistroInstitucionalAsync(string registroInstitucional) // busca cliente por RA/RI
    {
        using var conexao = CriarConexao();
        var sql = $"{SelectBase} WHERE RegistroInstitucional = @RegistroInstitucional";
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new { RegistroInstitucional = registroInstitucional });
        return row is null ? null : MapearParaEntidade(row);
    }

    public async Task<bool> PossuiEmprestimosAsync(int usuarioId) // verifica se o usuário possui empréstimos vinculados
    {
        using var conexao = CriarConexao();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Emprestimos WHERE UsuarioID = @UsuarioId);";
        return await conexao.ExecuteScalarAsync<bool>(sql, new { UsuarioId = usuarioId });
    }
}