namespace FiaoInteligente.Dominio;
public enum EstadoCredito { Activo, Pagado, Vencido }

public class Credito
{
    public int Id { get; set; }
    public int ClienteId { get; }
    public int VentaId { get; set; }
    public decimal Monto { get; }
    public decimal Saldo { get; private set; }
    public DateTime FechaVencimiento { get; }
    public EstadoCredito Estado { get; private set; }

    public Credito(int id, int clienteId, int ventaId, decimal monto, decimal saldo, DateTime vence, EstadoCredito estado)
    {
        Id = id; ClienteId = clienteId; VentaId = ventaId; Monto = monto; Saldo = saldo;
        FechaVencimiento = vence; Estado = estado;
    }

    // REGLA basada en estado: Activo -> Vencido si pasó la fecha; Activo/Vencido -> Pagado si saldo = 0.
    public void ActualizarEstado(DateTime hoy)
    {
        if (Estado == EstadoCredito.Activo && hoy.Date > FechaVencimiento.Date)
            Estado = EstadoCredito.Vencido;
    }

    // REGLA (cálculo + estado + restricción): el abono reduce el saldo y puede saldar el crédito.
    public void Abonar(decimal monto)
    {
        if (monto <= 0) throw new ArgumentException("El abono debe ser mayor que 0.");
        if (Estado == EstadoCredito.Pagado) throw new ReglaNegocioException("El crédito ya está pagado.");
        if (monto > Saldo) throw new ReglaNegocioException($"El abono ({monto:C}) supera el saldo ({Saldo:C}).");
        Saldo -= monto;
        if (Saldo == 0) Estado = EstadoCredito.Pagado;
    }
}
