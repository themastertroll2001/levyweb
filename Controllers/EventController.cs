using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Barberia.Data;
using Barberia.Models;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;

namespace Barberia.Controllers
{

    public class EventController : Controller
    {
        private readonly IDAL _dal;
        private readonly BdBarberiaContext _context;

        public EventController(IDAL dal, BdBarberiaContext context)
        {
            _dal = dal;
            _context = context;
        }

        // GET: Event

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2 && usuario.RolId != 3)
                {
                    return RedirectToAction("Index", "Event");
                }
            }
            if (TempData["Alert"] != null)
            {
                ViewData["Alert"] = TempData["Alert"];
            }
            return View(_dal.GetEvents());
        }

        // GET: Event/Details/5
        public IActionResult Details(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2 && usuario.RolId != 3) 
                {
                    return RedirectToAction("Index", "Event");
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var @event = _dal.GetEvent((int)id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }
        public IActionResult DownloadPDF()
        {
            var eventos = _dal.GetEvents();  // Obtiene todos los eventos.

            return new ViewAsPdf("IndexPDF", eventos)
            {
                FileName = "CitasBarberiaLevy.pdf"
            };
        }
        // GET: Event/Create Cliente
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Event/Create  Cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection form)
        {
            try
            {
                var telefono = form["Telefono"].ToString();
                if (telefono.Length != 8)
                {
                    ViewData["Alert"] = "El número de teléfono debe tener exactamente 8 dígitos.";
                    return View();
                }

                var newEvent = new Event
                {
                    Nombre = form["Nombre"],
                    Descripcion = form["Descripcion"],
                    Telefono = int.Parse(form["Telefono"]),
                    Fechainicio = DateTime.Parse(form["Fechainicio"]),
                    Finfecha = DateTime.Parse(form["Finfecha"])
                };
                var existingEvent = _dal.GetEvents().FirstOrDefault(e =>
                  e.Fechainicio < newEvent.Finfecha && e.Finfecha > newEvent.Fechainicio);

                if (existingEvent != null)
                {
                    ViewData["Alert"] = "Ya existe una cita para el rango de tiempo seleccionado.";
                    return View();  // Devolver la vista con el mensaje de error
                }

                _dal.CreateEvent(newEvent);
                TempData["Alert"] = "¡Éxito! Has creado un nuevo evento: " + form["Nombre"];
                return RedirectToAction("Citasdiarias", "Home");
            }
            catch (Exception ex)
            {
                ViewData["Alert"] = "Ocurrió un error: " + ex.Message;
                return View();
            }
        }


        // GET: Event/Edit/5
        public IActionResult Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2 && usuario.RolId != 3)
                {
                    return RedirectToAction("Index", "Event");
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var @event = _dal.GetEvent((int)id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event); // Aquí puedes pasar un ViewModel si es necesario
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection form)
        {
            try
            {
                var updatedEvent = new Event
                {
                    Id = id,
                    Nombre = form["Nombre"],
                    Descripcion = form["Descripcion"],
                    Telefono = int.Parse(form["Telefono"]),
                    Fechainicio = DateTime.Parse(form["Fechainicio"]),
                    Finfecha = DateTime.Parse(form["Finfecha"])
                 
                };
                _dal.UpdateEvent(updatedEvent);
                TempData["Alert"] = "¡Éxito! Has modificado el evento: " + form["Nombre"];
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["Alert"] = "Ocurrió un error: " + ex.Message;
                return View();
            }
        }

        // GET: Event/Delete/5
        public IActionResult Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2 && usuario.RolId != 3)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            if (id == null)
            {
                return NotFound();
            }

            var @event = _dal.GetEvent((int)id);

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _dal.DeleteEvent(id);
            TempData["Alert"] = "Has eliminado un evento.";
            return RedirectToAction(nameof(Index));
        }
    }
}
