using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Barberia.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Asegúrate de tener esta directiva en la parte superior.
using System.Security.Claims;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Globalization;
using Rotativa.AspNetCore;

namespace Barberia.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly BdBarberiaContext _context;

        public CategoriasController(BdBarberiaContext context)
        {
            _context = context;
        }

        public IActionResult InsertarCategoria()
        {
            if (HttpContext.Session.GetString("Username")== null || HttpContext.Session.GetString("Username")== "")
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

        [HttpGet]
        public IActionResult InsertarProductos()
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
            var categorias = _context.Categorias.Where(ct => ct.Estatus == true).ToList();
            return View(categorias);
        }
        public IActionResult DownloadPDFProductos()
        {
            var productos = _context.Productos
                                    .Include(p => p.Categoria) // Incluye la categoría del producto.
                                    .Include(p => p.Usuario)   // Incluye el usuario del producto.
                                    .ToList();                // Convierte la consulta a una lista.

            return new ViewAsPdf("ProductosPDF", productos) 
            {
                FileName = "ProductosBarberiaLevy.pdf"
            };
        }

        public IActionResult Productos()
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
            // Incluimos tanto la categoría como el usuario en la consulta.
            var productos = _context.Productos
                                    .Include(p => p.Categoria)
                                    .Include(p => p.Usuario)
                                    .ToList();
            return View(productos);
        }


        [HttpPost]
        public IActionResult InsertarCategoria(Categoria categoria)
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
            try
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));

                if (usuario != null)
                {
                    categoria.UsuarioId = usuario.Id;
                    categoria.Estatus = true; // Estado activo
                    _context.Categorias.Add(categoria);
                    _context.SaveChanges();

                    TempData["SuccessMessageC"] = "Se ha agregado una nueva categoría.";
                    return View("InsertarCategoria");
                }
                else
                {
                    TempData["ErrorMessageC"] = "No se pudo insertar: hubo un error.";
                }
            }
            catch (Exception ex) // Puedes atrapar una excepción más específica si lo deseas.
            {
                TempData["ErrorMessageC"] = "No se pudo insertar: hubo un error.";
            }

            return View("InsertarCategoria");
        }

        public IActionResult Categorias()
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
            var categorias = _context.Categorias.Include(c => c.Usuario).ToList(); // Incluyendo el usuario.
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);

                if (categoria != null)
                {
                    categoria.Estatus = false;
                    _context.SaveChanges();

                    TempData["SuccessMessageCE"] = "Categoría marcada como inactiva.";
                }
                else
                {
                    TempData["ErrorMessageCE"] = "La categoría no fue encontrada.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageCE"] = "Hubo un error al tratar de marcar la categoría como inactiva.";
            }

            return RedirectToAction("Categorias");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View("EditarCategoria", categoria); // envía la categoría a EditarCategoria.cshtml
        }

        [HttpPost]
        public IActionResult Editar(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                var categoriaExistente = _context.Categorias.FirstOrDefault(c => c.CategoriaId == categoria.CategoriaId);
                if (categoriaExistente != null)
                {
                    categoriaExistente.Nombre = categoria.Nombre;
                    categoriaExistente.Descripcion = categoria.Descripcion;
                    _context.SaveChanges();

                    TempData["SuccessMessageCE"] = "Cambios guardados correctamente.";
                    return RedirectToAction("Categorias");
                }
            }
            return View(categoria);
        }
        [HttpPost]
        public IActionResult InsertarProductos(Producto producto, IFormFile imagen)
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
            if (producto.CategoriaId == null)
            {
                TempData["ErrorMessageP1"] = "!Eliga una Categoria";
                return RedirectToAction("InsertarProductos");
            }
            decimal valor = producto.Precio;
            string[] parts = valor.ToString().Split('.');
            if (parts[0].Length > 10 || (parts.Length > 1 && parts[1].Length > 2))
            {
                TempData["ErrorMessageP1"] = "Se Excedio el Limite del Precio";
                return RedirectToAction("InsertarProductos");
            }

            int stock = producto.Stock;

            string stockString = stock.ToString();

            if (stock < 0 || stockString.Length > 10)
            {
                TempData["ErrorMessageP1"] = "Se Excedio el Limite del Stock";
                return RedirectToAction("InsertarProductos");
            }

            if (ModelState.IsValid)
            {
                // Guardar la imagen en wwwroot/Productos
                if (imagen != null && imagen.Length > 0)
                {
                    // Crear un nombre único para la imagen usando una marca de tiempo.
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Productos", uniqueFileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    imagen.CopyTo(stream);

                    producto.Imagen = uniqueFileName; // Establecer el nombre de archivo único en el modelo de producto.
                }

                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                producto.Estatus = true;
                producto.UsuarioId = usuario.Id;
                _context.Productos.Add(producto);
                _context.SaveChanges();

                TempData["SuccessMessageP1"] = "Producto añadido con éxito!";
                return RedirectToAction("InsertarProductos");
            }

            return View();
        }

        [HttpGet]
        public IActionResult EliminarP(int id)
        {
            try
            {
                var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == id);
                if (producto == null)
                {
                    TempData["ErrorMessagePD"] = "Producto no encontrado.";
                    return RedirectToAction("Productos");
                }

                producto.Estatus = false;  // Cambiar el estado a inactivo
                _context.SaveChanges();

                TempData["SuccessMessagePD"] = "Producto eliminado (desactivado) con éxito.";
                return RedirectToAction("Productos");
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error (ex) en algún sistema de logging si tienes uno.
                TempData["ErrorMessagePD"] = "Hubo un error al intentar eliminar el producto.";
                return RedirectToAction("Productos");
            }
        }

        [HttpGet]
        public IActionResult EditarProducto(int id)
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
            var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == id);
            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = _context.Categorias.ToList();
            return View(producto);
        }

        [HttpPost]
        public IActionResult EditarProductoPost(Producto productoEditado, IFormFile imagen)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == productoEditado.ProductoId);
            if (producto == null)
            {
                return NotFound();
            }

            if (imagen == null)
            {
                ModelState.Remove("Imagen");
            }

            if (ModelState.IsValid)
            {
                // Guardar la imagen en wwwroot/Productos si se proporcionó una nueva imagen.
                if (imagen != null && imagen.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imagen.FileName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Productos", uniqueFileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    imagen.CopyTo(stream);
                    producto.Imagen = uniqueFileName;
                }
                else
                {
                    producto.Imagen = producto.Imagen;
                }

                producto.Nombre = productoEditado.Nombre;
                producto.Descripcion = productoEditado.Descripcion;

                if(productoEditado.Precio != producto.Precio)
                {
                    decimal precio;

                    if (!decimal.TryParse(Request.Form["Precio"].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio))
                    {
                        TempData["ErrorMessage_Cortes"] = "El formato del precio no es válido.";
                        return View("CortesEditar", producto);
                    }

                    producto.Precio = precio;
                }

                // ... otros campos que se deseen modificar.

                producto.CategoriaId = productoEditado.CategoriaId;

                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                producto.UsuarioId = usuario.Id;

                try
                {
                    _context.SaveChanges();
                    TempData["SuccessMessagePD"] = "Producto editado con éxito!";
                    return RedirectToAction("Productos");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessagPD"] = "Hubo un error al editar el producto. Por favor, inténtalo de nuevo.";
                    // Para depuración:
                    // TempData["ErrorMessage"] = ex.Message;
                    return View("EditarProducto", producto);
                }

            }

            ViewBag.Categorias = _context.Categorias.ToList();
            return View("EditarProducto", producto);
        }


    }
}
