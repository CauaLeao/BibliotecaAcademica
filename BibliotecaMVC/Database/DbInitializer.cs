using Dapper;
using Microsoft.Data.Sqlite;

namespace BibliotecaMVC.Database;

public static class DbInitializer
{
    public static void Inicializar(string connectionString, string caminhoScript)
    {
        var sql = File.ReadAllText(caminhoScript);

        using var connection = new SqliteConnection(connectionString);
        connection.Execute(sql);
    }
}
