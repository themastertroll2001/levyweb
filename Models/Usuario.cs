using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string NombreUsuario { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public string? TokenVerificacionCorreo { get; set; }

    public string? TokenRecuperacionContrasena { get; set; }

    public DateTime? FechaTokenRecuperacionContrasena { get; set; }

    public bool? CuentaVerificada { get; set; }

    public int? RolId { get; set; }

    public virtual ICollection<Categoria> Categoria { get; set; } = new List<Categoria>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<RegistrosCorte> RegistrosCortes { get; set; } = new List<RegistrosCorte>();

    public virtual Role? Rol { get; set; }

    public virtual ICollection<TdRolesUsuario> TdRolesUsuarios { get; set; } = new List<TdRolesUsuario>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
