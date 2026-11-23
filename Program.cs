using FiaoInteligente;
using FiaoInteligente.Dominio;
using FiaoInteligente.Datos;

Console.OutputEncoding = System.Text.Encoding.UTF8;
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
    Console.WriteLine("11. Cambiar límite de crédito de cliente");
    Console.WriteLine("12. Cambiar precio de producto");
    Console.WriteLine("13. Desactivar cliente");
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
            case "9": foreach (var cr in svc.CreditosActualizados(LeerInt("Id cliente"))) { Console.WriteLine($"#{cr.Id} saldo:{cr.Saldo:C} vence:{cr.FechaVencimiento:d} [{cr.Estado}]"); } break;
            case "10": foreach (var (n, d, k) in repo.ReporteDeudas()) Console.WriteLine($"{n}: {d:C} ({k} créditos)"); break;
            case "11": { var cl = repo.ObtenerCliente(LeerInt("Id cliente")) ?? throw new ReglaNegocioException("No existe."); cl.CambiarLimite(LeerDec("Nuevo límite")); repo.GuardarCliente(cl); Console.WriteLine("Límite actualizado."); break; }
            case "12": { var pr = repo.ObtenerProducto(LeerInt("Id producto")) ?? throw new ReglaNegocioException("No existe."); pr.CambiarPrecio(LeerDec("Nuevo precio")); repo.GuardarProducto(pr); Console.WriteLine("Precio actualizado."); break; }
            case "13": { var cl = repo.ObtenerCliente(LeerInt("Id cliente")) ?? throw new ReglaNegocioException("No existe."); cl.Desactivar(); repo.GuardarCliente(cl); Console.WriteLine("Cliente desactivado."); break; }
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
    var s = Leer("¿Cliente? (id o vacío)");
    if (s.Length > 0)
    {
        if (!int.TryParse(s, out var cid)) throw new ArgumentException("El id del cliente debe ser un número.");
        cliente = repo.ObtenerCliente(cid) ?? throw new ReglaNegocioException("Cliente no existe.");
    }
    var items = new List<(int, int)>();
    do items.Add((LeerInt("Id producto"), LeerInt("Cantidad"))); while (Leer("¿Otro producto? (s/n)").ToLower() == "s");
    IMetodoPago metodo = Leer("Pago: 1.Efectivo 2.Crédito 3.Transferencia") switch
    {
        "2" => new PagoCredito(),
        "3" => new PagoTransferencia(Leer("Referencia")),
        _ => new PagoEfectivo()
    };
    var v = svc.RegistrarVenta(cliente, items, metodo);
    Console.WriteLine($"Venta #{v.Id} registrada. Total {v.Total:C} ({metodo.Nombre}).");
}

string Leer(string t) { Console.Write(t + ": "); return Console.ReadLine()?.Trim() ?? ""; }
int LeerInt(string t) => int.TryParse(Leer(t), out var n) ? n : throw new ArgumentException("Debe ser un número entero.");
decimal LeerDec(string t) => decimal.TryParse(Leer(t), out var n) ? n : throw new ArgumentException("Debe ser un número.");
