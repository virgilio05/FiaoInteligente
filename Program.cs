using FiaoInteligente;
using FiaoInteligente.Dominio;
using FiaoInteligente.Datos;

Db.Crear();
var svc = new ColmadoServicio();
var repo = svc.Repo;

while (true)
{
    Console.WriteLine("\n=== EL FIAO INTELIGENTE ===");
    Console.WriteLine(" 1. Nuevo producto");
    Console.WriteLine(" 2. Buscar producto");
    Console.WriteLine(" 3. Reponer stock");
    Console.WriteLine(" 4. Desactivar producto");
    Console.WriteLine(" 5. Nuevo cliente");
    Console.WriteLine(" 6. Buscar cliente");
    Console.WriteLine(" 7. Nueva venta");
    Console.WriteLine(" 8. Registrar abono");
    Console.WriteLine(" 9. Ver créditos de cliente");
    Console.WriteLine("10. Reporte de deudas");
    Console.WriteLine(" 0. Salir");
    try
    {
        switch (Leer("Opción"))
        {
            case "1": repo.GuardarProducto(new Producto(0, Leer("Nombre"), LeerDec("Precio"), LeerInt("Stock"))); Console.WriteLine("Guardado."); break;
            case "2": foreach (var p in repo.BuscarProductos(Leer("Filtro (vacío=todos)"))) Console.WriteLine($"{p.Id}. {p.Nombre} {p.Precio:C} stock:{p.Stock}"); break;
            case "3": { var p = repo.ObtenerProducto(LeerInt("Id producto")) ?? throw new ReglaNegocioException("No existe."); p.Reponer(LeerInt("Cantidad")); repo.GuardarProducto(p); Console.WriteLine("Stock actualizado."); break; }
            case "4": { var p = repo.ObtenerProducto(LeerInt("Id producto")) ?? throw new ReglaNegocioException("No existe."); p.Desactivar(); repo.GuardarProducto(p); Console.WriteLine("Desactivado."); break; }
            case "5": repo.GuardarCliente(new Cliente(0, Leer("Nombre"), Leer("Teléfono"), LeerDec("Límite de crédito"))); Console.WriteLine("Guardado."); break;
            case "6": foreach (var c in repo.BuscarClientes(Leer("Filtro"))) Console.WriteLine($"{c.Id}. {c.Nombre} límite:{c.LimiteCredito:C}"); break;
            case "7": Vender(); break;
            case "8": svc.RegistrarAbono(LeerInt("Id crédito"), LeerInt("Id cliente"), LeerDec("Monto")); Console.WriteLine("Abono registrado."); break;
            case "9": foreach (var cr in repo.CreditosDe(LeerInt("Id cliente"))) { cr.ActualizarEstado(DateTime.Now); Console.WriteLine($"#{cr.Id} saldo:{cr.Saldo:C} vence:{cr.FechaVencimiento:d} [{cr.Estado}]"); } break;
            case "10": foreach (var (n, d, k) in repo.ReporteDeudas()) Console.WriteLine($"{n}: {d:C} ({k} créditos)"); break;
            case "0": return;
            default: Console.WriteLine("Opción inválida."); break;
        }
    }
    catch (ReglaNegocioException ex) { Console.WriteLine($"[REGLA] {ex.Message}"); }
    catch (ArgumentException ex) { Console.WriteLine($"[DATO INVÁLIDO] {ex.Message}"); }
    catch (Exception ex) { Console.WriteLine($"[ERROR] {ex.Message}"); }
}

void Vender()
{
    Cliente? cliente = null;
    if (Leer("¿Cliente? (id o vacío)") is { Length: > 0 } s && int.TryParse(s, out var cid))
        cliente = repo.ObtenerCliente(cid) ?? throw new ReglaNegocioException("Cliente no existe.");
    var items = new List<(int, int)>();
    do items.Add((LeerInt("Id producto"), LeerInt("Cantidad"))); while (Leer("¿Otro producto? (s/n)").ToLower() == "s");
    IMetodoPago metodo = Leer("Pago: 1.Efectivo 2.Crédito 3.Transferencia") switch
    {
        "2" => new PagoCredito(-1),
        "3" => new PagoTransferencia(Leer("Referencia")),
        _ => new PagoEfectivo()
    };
    var v = svc.RegistrarVenta(cliente, items, metodo);
    Console.WriteLine($"Venta #{v.Id} registrada. Total {v.Total:C} ({metodo.Nombre}).");
}

string Leer(string t) { Console.Write(t + ": "); return Console.ReadLine()?.Trim() ?? ""; }
int LeerInt(string t) => int.TryParse(Leer(t), out var n) ? n : throw new ArgumentException("Debe ser un número entero.");
decimal LeerDec(string t) => decimal.TryParse(Leer(t), out var n) ? n : throw new ArgumentException("Debe ser un número.");