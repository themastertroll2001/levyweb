﻿using System;
using System.Collections.Generic;

namespace Barberia.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estatus { get; set; }

    public int? UsuarioId { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual Usuario? Usuario { get; set; }
}
