namespace FiaoInteligente.Dominio;

// POLIMORFISMO real: cada método de pago valida y reacciona distinto.
public interface IMetodoPago
{
    string Nombre { get; }
    void Validar(decimal total, Cliente? cliente, decimal deudaPendiente, bool tieneCreditoVencido);
    Credito? GenerarCredito(Venta venta);
}

public class PagoEfectivo : IMetodoPago
{
    public string Nombre => "Efectivo";
    public void Validar(decimal total, Cliente? c, decimal d, bool v) { }
    public Credito? GenerarCredito(Venta venta) => null;
}

public class PagoTransferencia : IMetodoPago
{
    public string Referencia { get; }
    public string Nombre => "Transferencia";
    public PagoTransferencia(string referencia) { Referencia = referencia; }
    public void Validar(decimal total, Cliente? c, decimal d, bool v)
    {
        if (string.IsNullOrWhiteSpace(Referencia))
            throw new ReglaNegocioException("La transferencia requiere número de referencia.");
    }
    public Credito? GenerarCredito(Venta venta) => null;
}

public class PagoCredito : IMetodoPago
{
    public int DiasPlazo { get; }
    public string Nombre => "Crédito";
    public PagoCredito(int diasPlazo = 15) { DiasPlazo = diasPlazo; }

    public void Validar(decimal total, Cliente? c, decimal deuda, bool vencido)
    {
        if (c is null || !c.Activo) throw new ReglaNegocioException("El fiao requiere un cliente activo.");
        if (vencido) throw new ReglaNegocioException("El cliente tiene un crédito vencido; debe pagar antes de fiar.");
        if (deuda + total > c.LimiteCredito)
            throw new ReglaNegocioException(
                $"Supera el límite de crédito: deuda {deuda:C} + compra {total:C} > límite {c.LimiteCredito:C}.");
    }
    public Credito? GenerarCredito(Venta v) =>
        new Credito(0, v.Cliente!.Id, v.Id, v.Total, v.Total, v.Fecha.AddDays(DiasPlazo), EstadoCredito.Activo);
}
