using Microsoft.AspNetCore.Mvc;
using Barberia.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;

namespace Barberia.Controllers
{
    public class ModulosController : Controller
    {
        private readonly BdBarberiaContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ModulosController(BdBarberiaContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // Vista para insertar un nuevo corte
        [HttpGet]
        public IActionResult CortesInsertar()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        public async Task<IActionResult> Cortes()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var cortes = await _context.Cortes.ToListAsync();
            cortes.ForEach(c => c.RutaArchivo = c.RutaArchivo == null ? null : "/Cortes/" + Path.GetFileName(c.RutaArchivo));
            return View(cortes);
        }

        // Controlador

        // Método para cargar el formulario de edición
        public IActionResult EditarRegistroCorte(int id)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario1 = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario1.RolId != 3)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var registro = _context.RegistrosCortes.Find(id);
            if (registro == null)
            {
                return NotFound();
            }

            // Carga todos los cortes en el ViewBag
            ViewBag.Cortes = _context.Cortes.ToList();

            // Obtener el usuario actual (Asumiendo que tienes alguna lógica de autenticación)
            // En este ejemplo uso User.Identity.Name para obtener el nombre del usuario actualmente logueado.
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));

            if (usuario != null)
            {
                // Pasamos el UsuarioId al ViewBag para usarlo en la vista.
                ViewBag.UsuarioId = usuario.Id;
            }

            return View(registro);
        }


        // Método para manejar la petición POST después de editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarRegistroCorte(RegistrosCorte registro)
        {
            ModelState.Remove("Corte");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                registro.Corte = _context.Cortes.Find(registro.CorteId);
                registro.Usuario = _context.Usuarios.Find(registro.UsuarioId);
                registro.Estado = 1;
                registro.Fecha = DateTime.Today;
                _context.Entry(registro).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("RegistroCortes");
            }

            if (!ModelState.IsValid)
            {
                // Obtener los errores de validación
                var errors = ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .Select(x => new { x.Key, x.Value.Errors })
                            .ToArray();

                // Por ejemplo, para depuración, puedes imprimir los errores en la consola
                foreach (var error in errors)
                {
                    Console.WriteLine($"Campo: {error.Key}");
                    foreach (var detail in error.Errors)
                    {
                        Console.WriteLine($"- Error: {detail.ErrorMessage}");
                    }
                }

                // También puedes agregar estos errores a TempData o ViewBag para mostrarlos en la vista
                TempData["ValidationErrors"] = errors.SelectMany(e => e.Errors.Select(err => err.ErrorMessage)).ToList();
            }

            return View(registro);
        }


        public IActionResult RegistroCortes()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 3)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var loggedUsername = HttpContext.Session.GetString("Username");
            var user = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loggedUsername);

            if (user == null)
            {
                return NotFound();
            }

            // Obtiene todos los registros (activos e inactivos)
            var registros = _context.RegistrosCortes
                .Where(r => r.UsuarioId == user.Id)
                .Include(r => r.Usuario)
                .Include(r => r.Corte)
                .ToList();

            // Filtra solo los registros activos para calcular el total
            var registrosActivos = registros.Where(r => r.Estado == 1).ToList();

            // Calculando el total por mes para el usuario que inició sesión usando solo registros activos
            var totalPorMes = registrosActivos
                .GroupBy(r => r.Fecha.Month)
                .Select(group => new {
                    Mes = group.Key,
                    Total = group.Sum(r => r.Precio)
                }).ToDictionary(g => g.Mes, g => g.Total);

            ViewBag.TotalPorMes = totalPorMes;

            return View(registros);
        }

        public async Task<IActionResult> EliminarRegistroCorte(int id)
        {
            try
            {
                var registro = _context.RegistrosCortes.Find(id);
                if (registro == null)
                {
                    return NotFound();
                }

                // Actualizar el estado a inactivo
                registro.Estado = 0;

                _context.Update(registro);
                await _context.SaveChangesAsync();

                return RedirectToAction("RegistroCortes");
            }
            catch (Exception ex)
            {
                // Manejar la excepción según lo necesites
                return RedirectToAction("RegistroCortes");
            }
        }


        [HttpGet]
        public IActionResult RegistroCorteInsertar()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 3)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Cortes = _context.Cortes.ToList();
            return View(new RegistrosCorte());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroCorteInsertar(RegistrosCorte registro)
        {
            try
            {
                var loggedUser = HttpContext.Session.GetString("Username");
                var user = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loggedUser);

                if (user == null)
                {
                    TempData["ErrorMessage_RegCorte"] = "Usuario no encontrado.";
                    return View(registro);
                }

                registro.UsuarioId = user.Id;
                registro.Fecha = DateTime.Now;

                // Convertir el precio
                decimal precio;
                if (!decimal.TryParse(Request.Form["Precio"].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio))
                {
                    ModelState.AddModelError("Precio", "El formato del precio no es válido.");
                    return View(registro);
                }
                registro.Precio = precio;

                // Asignar el estado a 1
                registro.Estado = 1;

                _context.RegistrosCortes.Add(registro);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage_RegCorte"] = "Registro de corte añadido con éxito.";
                return RedirectToAction("RegistroCorteInsertar");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage_RegCorte"] = "Hubo un error al agregar el registro. Intenta de nuevo.";
                return View(registro);
            }
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var corte = _context.Cortes.FirstOrDefault(c => c.Id == id);
                if (corte == null)
                {
                    TempData["ErrorMessage_Cortes"] = "Corte no encontrado.";
                    return RedirectToAction("Cortes");
                }

                _context.Cortes.Remove(corte);
                _context.SaveChanges();

                TempData["SuccessMessage_Cortes"] = "Corte eliminado correctamente.";
            }
            catch (Exception ex)
            {
                // Podrías, si lo prefieres, registrar el error exacto en algún lado para tener más detalles.
                TempData["ErrorMessage_Cortes"] = "Hubo un error al eliminar el corte. Intenta de nuevo.";
            }

            return RedirectToAction("Cortes");
        }

        public IActionResult Editar(int id)
        {
            var corte = _context.Cortes.FirstOrDefault(c => c.Id == id);
            corte.RutaArchivo = corte.RutaArchivo == null ? null : "/Cortes/" + Path.GetFileName(corte.RutaArchivo);
            if (corte == null)
            {
                return NotFound();
            }
            return View("CortesEditar", corte);
        }

        // Acción para procesar el formulario e insertar un nuevo corte en la base de datos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CortesInsertar([Bind("NombreCorte,Precio")] Corte corte, IFormFile Archivo)
        {
            try
            {
                // Validar el archivo (puedes agregar más validaciones, como el tamaño, tipo, etc.)
                if (Archivo == null || Archivo.Length == 0)
                {
                    ModelState.AddModelError("Archivo", "El archivo es requerido.");
                    return View(corte);
                }

                // Guardar el archivo en la carpeta "Archivos" en el directorio raíz del proyecto
                var pathRoot = Path.Combine(_hostingEnvironment.WebRootPath, "Cortes");
                var nombreArchivoUnico = Guid.NewGuid().ToString() + "_" + Archivo.FileName;
                var pathArchivo = Path.Combine(pathRoot, nombreArchivoUnico);

                // Asegurarte de que el directorio exista
                if (!Directory.Exists(pathRoot))
                {
                    Directory.CreateDirectory(pathRoot);
                }

                using (var stream = new FileStream(pathArchivo, FileMode.Create))
                {
                    await Archivo.CopyToAsync(stream);
                }

                corte.RutaArchivo = pathArchivo;
                corte.NombreArchivoUnico = nombreArchivoUnico;

                // Intentar convertir el precio a decimal usando CultureInfo
                decimal precio;
                if (!decimal.TryParse(Request.Form["Precio"].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio))
                {
                    ModelState.AddModelError("Precio", "El formato del precio no es válido.");
                    return View(corte);
                }

                corte.Precio = precio;

                if (ModelState.IsValid)
                {
                    _context.Add(corte);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage_Cortes"] = "Se agregó el nuevo corte correctamente.";
                    return RedirectToAction("CortesInsertar");
                }
            }
            catch (Exception ex)
            {
                // Considerar el registro de errores aquí.
            }
            return View(corte);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCorte(Corte corte, IFormFile Archivo)
        {
            try
            {
               

                var corteExistente = _context.Cortes.FirstOrDefault(c => c.Id == corte.Id);
                if(corteExistente.Precio != corte.Precio)
                {
                    // Intentar convertir el precio a decimal usando CultureInfo
                    decimal precio;

                    if (!decimal.TryParse(Request.Form["Precio"].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio))
                    {
                        TempData["ErrorMessage_Cortes"] = "El formato del precio no es válido.";
                        return View("CortesEditar", corte);
                    }

                    corte.Precio = precio;
                }

                if (corteExistente == null)
                {
                    return NotFound();
                }

                corteExistente.NombreCorte = corte.NombreCorte;
                corteExistente.Precio = corte.Precio;
                string ruta = corteExistente.RutaArchivo;

               

                if (Archivo != null && Archivo.Length > 0)
                {
                    if (ruta != null)
                    {
                        // Borrar la imagen anterior si existe
                        var pathAnterior = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", corteExistente.RutaArchivo);
                        if (System.IO.File.Exists(pathAnterior))
                        {
                            System.IO.File.Delete(pathAnterior);
                        }
                    }
                    

                    // Guardar la nueva imagen
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cortes", Archivo.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await Archivo.CopyToAsync(stream);
                    }

                    corteExistente.RutaArchivo = "Cortes/" + Archivo.FileName;
                    corteExistente.NombreArchivoUnico = Archivo.FileName; // Modifica esto si necesitas un nombre único para el archivo
                }

                _context.Update(corteExistente);
                _context.SaveChanges();

                TempData["SuccessMessage_Cortes"] = "Corte actualizado correctamente.";
                return RedirectToAction("Cortes");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage_Cortes"] = "Hubo un error al actualizar el corte. Intenta de nuevo.";
                return View("CortesEditar", corte);
            }
        }

        public IActionResult VistaRegistrosCortes(string nombreUsuario, int? mes)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var registros = _context.RegistrosCortes
                        .Include(r => r.Usuario)
                        .Include(r => r.Corte)
                        .ToList();

            if (!string.IsNullOrEmpty(nombreUsuario))
            {
                registros = registros.Where(r => r.Usuario.NombreUsuario.Contains(nombreUsuario)).ToList();
            }

            if (mes.HasValue)
            {
                registros = registros.Where(r => r.Fecha.Month == mes.Value).ToList();
            }

            ViewBag.Usuarios = _context.Usuarios.Select(u => u.NombreUsuario).ToList();

            return View(registros);
        }
        public IActionResult DownloadPDFVistaRegistroCortes()
        {
            var registros = _context.RegistrosCortes
                         .Include(r => r.Usuario)
                         .Include(r => r.Corte)
                         .ToList();

            return new ViewAsPdf("RegistroCortesPDF", registros)
            {
                FileName = "RegistroCortesBarberiaLevy.pdf"
            };
        }
        public IActionResult IngresarVenta()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var productos = _context.Productos.Where(p => p.Estatus).ToList();
            ViewBag.Productos = new SelectList(productos, "ProductoId", "Nombre");

            // Ajusta la asignación de ViewBag.ProductosList para seleccionar solo las propiedades que necesitas.
            ViewBag.ProductosList = productos.Select(p => new
            {
                p.ProductoId,
                p.Nombre,
                p.Precio,
                p.Stock
            }).ToList();

            return View();
        }


        [HttpPost]
        public IActionResult GuardarVenta(Venta venta)
        {
            var producto = _context.Productos.Find(venta.ProductoId);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
            
            if (venta.ProductoId == null)
            {
                TempData["Message"] = "No se ingreso ningun producto";
                return RedirectToAction("IngresarVenta");
            }

            if (venta.Cantidad <= producto.Stock)
            {
                producto.Stock -= venta.Cantidad;
                if (producto.Stock == 0)
                {
                    producto.Estatus = false;
                }

                venta.Precio = producto.Precio;
                venta.Estatus = true; // Asegurando que siempre esté activo.
                venta.IdUsuario = usuario.Id;

                _context.Productos.Update(producto);
                _context.Ventas.Add(venta);
                _context.SaveChanges();

                TempData["Message"] = "Se logró con éxito la venta.";
                return RedirectToAction("IngresarVenta");
            }
            else
            {
                TempData["Message"] = "No hay suficiente stock para completar la venta.";
                return RedirectToAction("IngresarVenta");
            }
        }

        public IActionResult Ventas()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var ventasList = _context.Ventas
                                     .Include(v => v.IdUsuarioNavigation)
                                     .Include(v => v.Producto)
                                     .ToList();
            return View(ventasList);
        }
        public IActionResult DownloadPDFVentas()
        {
            var ventasList = _context.Ventas
                                     .Include(v => v.IdUsuarioNavigation)
                                     .Include(v => v.Producto)
                                     .ToList();

            return new ViewAsPdf("VentasPDF", ventasList)
            {
                FileName = "VentasBarberiaLevy.pdf"
            };
        }


    }
}
