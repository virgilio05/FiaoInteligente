using Microsoft.Data.Sqlite;
using FiaoInteligente.Dominio;
namespace FiaoInteligente.Datos;

public class Repositorio
{
    public Producto GuardarProducto(Producto p)
    {
        using var c = Db.Abrir();
        if (p.Id == 0)
            p.Id = (int)Db.Ejecutar(c, "INSERT INTO Producto(Nombre,Precio,Stock,Activo) VALUES(@n,@p,@s,@a)",
                ("@n", p.Nombre), ("@p", p.Precio), ("@s", p.Stock), ("@a", p.Activo ? 1 : 0));
        else
            Db.Ejecutar(c, "UPDATE Producto SET Nombre=@n,Precio=@p,Stock=@s,Activo=@a WHERE Id=@i",
                ("@n", p.Nombre), ("@p", p.Precio), ("@s", p.Stock), ("@a", p.Activo ? 1 : 0), ("@i", p.Id));
        return p;
    }

    public List<Producto> BuscarProductos(string filtro = "", bool soloActivos = true)
    {
        using var c = Db.Abrir(); var lista = new List<Producto>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id,Nombre,Precio,Stock,Activo FROM Producto WHERE Nombre LIKE @f AND (@a=0 OR Activo=1) ORDER BY Nombre";
        cmd.Parameters.AddWithValue("@f", $"%{filtro}%"); cmd.Parameters.AddWithValue("@a", soloActivos ? 1 : 0);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(new Producto(r.GetInt32(0), r.GetString(1), r.GetDecimal(2), r.GetInt32(3), r.GetInt32(4) == 1));
        return lista;
    }

    public Producto? ObtenerProducto(int id) => BuscarProductos("", false).FirstOrDefault(p => p.Id == id);

    public Cliente GuardarCliente(Cliente cl)
    {
        using var c = Db.Abrir();
        if (cl.Id == 0)
            cl.Id = (int)Db.Ejecutar(c, "INSERT INTO Cliente(Nombre,Telefono,LimiteCredito,Activo) VALUES(@n,@t,@l,@a)",
                ("@n", cl.Nombre), ("@t", cl.Telefono), ("@l", cl.LimiteCredito), ("@a", cl.Activo ? 1 : 0));
        else
            Db.Ejecutar(c, "UPDATE Cliente SET Nombre=@n,Telefono=@t,LimiteCredito=@l,Activo=@a WHERE Id=@i",
                ("@n", cl.Nombre), ("@t", cl.Telefono), ("@l", cl.LimiteCredito), ("@a", cl.Activo ? 1 : 0), ("@i", cl.Id));
        return cl;
    }

    public List<Cliente> BuscarClientes(string filtro = "")
    {
        using var c = Db.Abrir(); var lista = new List<Cliente>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id,Nombre,Telefono,LimiteCredito,Activo FROM Cliente WHERE Nombre LIKE @f ORDER BY Nombre";
        cmd.Parameters.AddWithValue("@f", $"%{filtro}%");
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(new Cliente(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetDecimal(3), r.GetInt32(4) == 1));
        return lista;
    }

    public Cliente? ObtenerCliente(int id) => BuscarClientes().FirstOrDefault(c => c.Id == id);

    public List<Credito> CreditosDe(int clienteId)
    {
        using var c = Db.Abrir(); var lista = new List<Credito>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id,ClienteId,VentaId,Monto,Saldo,FechaVencimiento,Estado FROM Credito WHERE ClienteId=@c ORDER BY FechaVencimiento";
        cmd.Parameters.AddWithValue("@c", clienteId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Credito(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetDecimal(3), r.GetDecimal(4),
                DateTime.Parse(r.GetString(5)), Enum.Parse<EstadoCredito>(r.GetString(6))));
        return lista;
    }

    public void GuardarEstadoCredito(SqliteConnection c, Credito cr) =>
        Db.Ejecutar(c, "UPDATE Credito SET Saldo=@s,Estado=@e WHERE Id=@i",
            ("@s", cr.Saldo), ("@e", cr.Estado.ToString()), ("@i", cr.Id));

    // REPORTE (agregación): deuda total por cliente con saldo pendiente.
    public List<(string Cliente, decimal Deuda, int Creditos)> ReporteDeudas()
    {
        using var c = Db.Abrir(); var l = new List<(string, decimal, int)>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT cl.Nombre, SUM(cr.Saldo), COUNT(*) FROM Credito cr
            JOIN Cliente cl ON cl.Id=cr.ClienteId WHERE cr.Saldo>0 GROUP BY cl.Id ORDER BY 2 DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) l.Add((r.GetString(0), r.GetDecimal(1), r.GetInt32(2)));
        return l;
    }
}
