using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class Corte
{
    public int Id { get; set; }

    public string NombreCorte { get; set; } = null!;

    public decimal Precio { get; set; }

    public string? RutaArchivo { get; set; }

    public string? NombreArchivoUnico { get; set; }

    public virtual ICollection<RegistrosCorte> RegistrosCortes { get; set; } = new List<RegistrosCorte>();
}
