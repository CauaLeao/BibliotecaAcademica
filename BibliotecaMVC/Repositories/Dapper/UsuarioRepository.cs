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
        var row = await conexao.QuerySingleOrDefaultAsync<UsuarioRow>(sql, new {Id = id});
        return row is null ? null : MapearParaEntidade(row);
    }
}