using Microsoft.Data.Sqlite;
namespace FiaoInteligente.Datos;

public static class Db
{
    const string Cs = "Data Source=fiao.db";

    public static SqliteConnection Abrir()
    {
        var c = new SqliteConnection(Cs); c.Open();
        Ejecutar(c, "PRAGMA foreign_keys=ON;");
        return c;
    }

    public static long Ejecutar(SqliteConnection c, string sql, params (string, object?)[] ps)
    {
        using var cmd = c.CreateCommand(); cmd.CommandText = sql;
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        cmd.ExecuteNonQuery();
        return Escalar(c, "SELECT last_insert_rowid();");
    }

    public static long Escalar(SqliteConnection c, string sql, params (string, object?)[] ps)
    {
        using var cmd = c.CreateCommand(); cmd.CommandText = sql;
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        var r = cmd.ExecuteScalar();
        return r is null or DBNull ? 0 : Convert.ToInt64(r);
    }

    // Script de creación (también disponible como Datos/schema.sql)
    public static void Crear()
    {
        using var c = Abrir();
        Ejecutar(c, File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "schema.sql")));
    }
}
