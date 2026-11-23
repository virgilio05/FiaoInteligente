using FiaoInteligente.Datos;
using FiaoInteligente.Dominio;
namespace FiaoInteligente;

// Lógica de negocio que coordina varios objetos (fuera de la UI).
public class ColmadoServicio
{
    private readonly Repositorio _repo = new();
    public Repositorio Repo => _repo;

    // PROCESO con múltiples objetos: Venta + Producto(stock) + Cliente + Credito, todo en una transacción.
    public Venta RegistrarVenta(Cliente? cliente, IEnumerable<(int ProductoId, int Cantidad)> items, IMetodoPago metodo)
    {
        var venta = new Venta(cliente, metodo);
        foreach (var (pid, cant) in items)
        {
            var p = _repo.ObtenerProducto(pid) ?? throw new ReglaNegocioException($"Producto {pid} no existe.");
            venta.Agregar(p, cant);
        }
        if (!venta.Detalles.Any()) throw new ReglaNegocioException("La venta no tiene productos.");

        decimal deuda = 0; bool vencido = false;
        if (cliente != null)
        {
            var creditos = CreditosActualizados(cliente.Id);
            deuda = creditos.Sum(x => x.Saldo);
            vencido = creditos.Any(x => x.Estado == EstadoCredito.Vencido && x.Saldo > 0);
        }
        metodo.Validar(venta.Total, cliente, deuda, vencido); // polimorfismo

        using var c = Db.Abrir(); using var tx = c.BeginTransaction();
        venta.Id = (int)Db.Ejecutar(c, "INSERT INTO Venta(Fecha,ClienteId,MetodoPago,Total) VALUES(@f,@c,@m,@t)",
            ("@f", venta.Fecha.ToString("s")), ("@c", cliente?.Id), ("@m", metodo.Nombre), ("@t", venta.Total));
        foreach (var d in venta.Detalles)
        {
            Db.Ejecutar(c, "INSERT INTO DetalleVenta(VentaId,ProductoId,Cantidad,PrecioUnitario) VALUES(@v,@p,@q,@u)",
                ("@v", venta.Id), ("@p", d.Producto.Id), ("@q", d.Cantidad), ("@u", d.PrecioUnitario));
            Db.Ejecutar(c, "UPDATE Producto SET Stock=@s WHERE Id=@i", ("@s", d.Producto.Stock), ("@i", d.Producto.Id));
        }
        var credito = metodo.GenerarCredito(venta);
        if (credito != null)
            Db.Ejecutar(c, "INSERT INTO Credito(ClienteId,VentaId,Monto,Saldo,FechaVencimiento,Estado) VALUES(@c,@v,@m,@s,@f,@e)",
                ("@c", credito.ClienteId), ("@v", venta.Id), ("@m", credito.Monto), ("@s", credito.Saldo),
                ("@f", credito.FechaVencimiento.ToString("s")), ("@e", credito.Estado.ToString()));
        tx.Commit();
        return venta;
    }

    // Marca como Vencido (y guarda en BD) los créditos cuya fecha ya pasó.
    public List<Credito> CreditosActualizados(int clienteId)
    {
        var lista = _repo.CreditosDe(clienteId);
        using var c = Db.Abrir();
        foreach (var cr in lista)
        {
            var antes = cr.Estado;
            cr.ActualizarEstado(DateTime.Now);
            if (cr.Estado != antes) _repo.GuardarEstadoCredito(c, cr);
        }
        return lista;
    }

    public void RegistrarAbono(int creditoId, int clienteId, decimal monto)
    {
        var cr = CreditosActualizados(clienteId).FirstOrDefault(x => x.Id == creditoId)
                 ?? throw new ReglaNegocioException("Crédito no encontrado para ese cliente.");
        cr.Abonar(monto);
        using var c = Db.Abrir(); using var tx = c.BeginTransaction();
        _repo.GuardarEstadoCredito(c, cr);
        Db.Ejecutar(c, "INSERT INTO Abono(CreditoId,Monto,Fecha) VALUES(@c,@m,@f)",
            ("@c", creditoId), ("@m", monto), ("@f", DateTime.Now.ToString("s")));
        tx.Commit();
    }
}
