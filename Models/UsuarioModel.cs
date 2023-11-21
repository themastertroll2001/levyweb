using System.ComponentModel.DataAnnotations;
namespace Barberia.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        [Required]
        public string NombreCompleto { get; set; }
        [Required]
        public string NombreUsuario { get; set; }
        [Required]
        public string? Correo { get; set; }
        [Required]
        public string? Contrasena { get; set; }
        public string? TokenVerificacionCorreo { get; set; }
        public string? TokenRecuperacionContrasena { get; set; }
        public string? FechaTokenRecuperacionContrasena { get; set; } 
        public byte CuentaVerificada { get; set; }  
        public int RolId { get; set; }
        public string? RolNombre { get; set; }


    }
}
