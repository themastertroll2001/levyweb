using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int? IdUsuario { get; set; }

    public int? ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public string NombreCliente { get; set; }

    public bool Estatus { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual Producto? Producto { get; set; }
}
