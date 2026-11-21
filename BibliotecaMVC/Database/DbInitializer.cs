using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Database;

public static class DbInitializer
{
    public static void Inicializar(string connectionString, string caminhoScript) // recebe o banco e o caminho de seu script de criação
    {
        // as primeiras 4 linhas a seguir previnem que a conexão do banco não funcione caso o
        // projeto seja executado na raiz BibliotecaAcademica/, onde o diretório Database não existe
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource; // quebra a string recebida em partes e retorna apenas o caminho do arquivo
        var diretorio = Path.GetDirectoryName(dataSource); // recebe apenas o diretório, sem o arquivo
        if (!string.IsNullOrWhiteSpace(diretorio))
            Directory.CreateDirectory(diretorio); // cria o diretório Database se ele não existir

        var sql = File.ReadAllText(caminhoScript); // joga o texto de script.sql pra memória

        using var connection = new SqliteConnection(connectionString); // instancia a conexão na memória
        connection.Execute(sql); // abre a conexão com o banco, cria o .db se ele não existir e executa os comandos do script.sql
    }
}
