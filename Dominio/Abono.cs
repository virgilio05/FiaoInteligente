namespace FiaoInteligente.Dominio;
public class Abono
{
    public int Id { get; set; }
    public int CreditoId { get; }
    public decimal Monto { get; }
    public DateTime Fecha { get; }
    public Abono(int id, int creditoId, decimal monto, DateTime fecha)
    { Id = id; CreditoId = creditoId; Monto = monto; Fecha = fecha; }
}
