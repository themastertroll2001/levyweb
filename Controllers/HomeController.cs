using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Barberia.Models;
using Barberia.Data;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BdBarberiaContext _context;
    private readonly IDAL _idal;
    public HomeController(ILogger<HomeController> logger, BdBarberiaContext context, IDAL idal)
    {
        _logger = logger;
        _context = context;
        _idal = idal;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Citasdiarias()
    {
        ViewData["Events"] = JSONListEvent.GetEventListJSONString(_idal.GetEvents());
        return View();

    }

    public IActionResult CortesVista()
    {
        var cortes = _context.Cortes.ToList();
        cortes.ForEach(c => c.RutaArchivo = c.RutaArchivo == null ? null : "/Cortes/" + Path.GetFileName(c.RutaArchivo));
        return View(cortes);
    }

    public IActionResult VistaProductos(int? categoriaId, string ordenPrecio, int pagina = 1)
    {
        const int itemsPorPagina = 10;

        var productos = _context.Productos.Where(p => p.Estatus == true);

        if (categoriaId.HasValue)
        {
            productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
        }

        switch (ordenPrecio)
        {
            case "asc":
                productos = productos.OrderBy(p => p.Precio);
                break;
            case "desc":
                productos = productos.OrderByDescending(p => p.Precio);
                break;
        }

        var paginado = productos.Skip((pagina - 1) * itemsPorPagina).Take(itemsPorPagina).ToList();

        ViewBag.Categorias = _context.Categorias.ToList();
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = Math.Ceiling((double)productos.Count() / itemsPorPagina);

        return View(paginado);
    }





    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
