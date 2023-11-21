using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class TdRolesUsuario
{
    public int IdRolesTd { get; set; }

    public int IdUsuario { get; set; }

    public int IdRole { get; set; }

    public int IdEstatus { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Usuario { get; set; }

    public virtual TcEstatusRole IdEstatusNavigation { get; set; } = null!;

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
