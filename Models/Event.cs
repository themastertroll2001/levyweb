using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Barberia.Models;

public partial class Event
{
    public int Id { get; set; }
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Telefono { get; set; }

    [Display(Name = "Fecha de Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fechainicio { get; set; }

    [Display(Name = "Fecha Final")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Finfecha { get; set; }
}
