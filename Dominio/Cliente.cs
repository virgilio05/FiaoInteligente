namespace FiaoInteligente.Dominio;
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; private set; }
    public string Telefono { get; private set; }
    public decimal LimiteCredito { get; private set; }
    public bool Activo { get; private set; } = true;

    public Cliente(int id, string nombre, string telefono, decimal limiteCredito, bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (limiteCredito < 0) throw new ArgumentException("El límite de crédito no puede ser negativo.");
        Id = id; Nombre = nombre.Trim(); Telefono = telefono ?? ""; LimiteCredito = limiteCredito; Activo = activo;
    }
    public void CambiarLimite(decimal nuevo)
    {
        if (nuevo < 0) throw new ArgumentException("El límite no puede ser negativo.");
        LimiteCredito = nuevo;
    }
    public void Desactivar() => Activo = false;
}
