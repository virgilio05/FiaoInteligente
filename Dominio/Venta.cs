namespace FiaoInteligente.Dominio;
public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; } = DateTime.Now;
    public Cliente? Cliente { get; }
    public IMetodoPago MetodoPago { get; }
    private readonly List<DetalleVenta> _detalles = new();
    public IReadOnlyList<DetalleVenta> Detalles => _detalles;
    public decimal Total => _detalles.Sum(d => d.Subtotal); // CÁLCULO

    public Venta(Cliente? cliente, IMetodoPago metodoPago) { Cliente = cliente; MetodoPago = metodoPago; }

    public void Agregar(Producto p, int cantidad)
    {
        p.Descontar(cantidad); // valida stock (regla de negocio)
        _detalles.Add(new DetalleVenta(p, cantidad));
    }
}
