namespace FiaoInteligente.Dominio;
public class DetalleVenta
{
    public Producto Producto { get; }
    public int Cantidad { get; }
    public decimal PrecioUnitario { get; }
    public decimal Subtotal => PrecioUnitario * Cantidad; // CÁLCULO
    public DetalleVenta(Producto producto, int cantidad)
    {
        Producto = producto; Cantidad = cantidad; PrecioUnitario = producto.Precio;
    }
}
