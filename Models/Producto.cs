using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string? Imagen { get; set; }

    public bool Estatus { get; set; }

    public int? CategoriaId { get; set; }

    public int? UsuarioId { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
