using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class RegistrosCorte
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public string NombreCliente { get; set; } = null!;

    public TimeSpan Hora { get; set; }

    public int CorteId { get; set; }

    public int UsuarioId { get; set; }

    public decimal Precio { get; set; }

    public int Estado { get; set; }

    public virtual Corte Corte { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
