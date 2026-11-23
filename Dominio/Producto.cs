namespace FiaoInteligente.Dominio;
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public int Stock { get; private set; }
    public bool Activo { get; private set; } = true;

    public Producto(int id, string nombre, decimal precio, int stock, bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (precio <= 0) throw new ArgumentException("El precio debe ser mayor que 0.");
        if (stock < 0) throw new ArgumentException("El stock no puede ser negativo.");
        Id = id; Nombre = nombre.Trim(); Precio = precio; Stock = stock; Activo = activo;
    }

    // REGLA (restricción): no se puede vender más de lo que hay en inventario.
    public void Descontar(int cantidad)
    {
        if (!Activo) throw new ReglaNegocioException($"'{Nombre}' está desactivado.");
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que 0.");
        if (cantidad > Stock)
            throw new ReglaNegocioException($"Stock insuficiente de '{Nombre}': hay {Stock}, se pidieron {cantidad}.");
        Stock -= cantidad;
    }
    public void Reponer(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que 0.");
        Stock += cantidad;
    }
    public void CambiarPrecio(decimal nuevo)
    {
        if (nuevo <= 0) throw new ArgumentException("El precio debe ser mayor que 0.");
        Precio = nuevo;
    }
    public void Desactivar() => Activo = false;
}
