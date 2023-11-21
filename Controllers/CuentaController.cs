using Microsoft.AspNetCore.Mvc;
using Barberia.Data;
using Barberia.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


namespace Barberia.Controllers
{
    public class CuentaController : Controller
    {
        private readonly Data.DbContext _contexto;

        private readonly Barberia.Data.DbContext _dbContext;

        private readonly BdBarberiaContext _context;

        public CuentaController(Data.DbContext contexto, Barberia.Data.DbContext dbContext, BdBarberiaContext context)
        {
            _contexto = contexto;
            _dbContext = dbContext;
            _context = context;
        }

        public ActionResult Registrarse()
        {
            return View();
        }

        public ActionResult Recuperar()
        {
            return View();
        }

        public IActionResult HistorialRoles(int id)
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var roles = _context.Roles.ToList();

            var loggedUser = HttpContext.Session.GetString("Username");
            var loggedUserRole = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loggedUser)?.RolId;

            // Si el usuario loggeado tiene un rol de 2, no se mostrará el rol 1 ni el rol 2 en el combobox
            if (loggedUserRole == 2)
            {
                roles = roles.Where(r => r.Id != 1 && r.Id != 2).ToList();
            }

            var userRole = _context.Usuarios.Find(id)?.RolId;
            if (userRole.HasValue)
            {
                roles = roles.Where(r => r.Id != userRole.Value).ToList();
            }

            var historialRoles = _context.TdRolesUsuarios
                .Where(r => r.IdUsuario == id)
                .Include(r => r.IdEstatusNavigation)
                .Include(r => r.IdRoleNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .ToList();

            ViewBag.Roles = roles;
            ViewBag.IdUsuario = id;

            return View(historialRoles);
        }

        [HttpPost]
        public IActionResult CambiarRol(int rolSeleccionado, int idUsuario)
        {
            // Obtiene el loggedUser
            var loggedUser = HttpContext.Session.GetString("Username");

            // Actualizar la tabla Usuarios
            var usuario = _context.Usuarios.Find(idUsuario);
            if (usuario != null)
            {
                usuario.RolId = rolSeleccionado;
                _context.Usuarios.Update(usuario);
                _context.SaveChanges();
            }

            // Buscar registro en TdRolesUsuarios y actualizar id_estatus a 2
            var registroExistente = _context.TdRolesUsuarios.FirstOrDefault(r => r.IdUsuario == idUsuario && r.IdEstatus == 1);
            if (registroExistente != null)
            {
                registroExistente.IdEstatus = 2;
                _context.TdRolesUsuarios.Update(registroExistente);
            }

            // Insertar nuevo registro en TdRolesUsuarios
            var nuevoRegistro = new TdRolesUsuario
            {
                IdUsuario = idUsuario,
                IdRole = rolSeleccionado,
                IdEstatus = 1,
                Fecha = DateTime.Now,
                Usuario = loggedUser
            };
            _context.TdRolesUsuarios.Add(nuevoRegistro);

            _context.SaveChanges();

            return RedirectToAction("HistorialRoles", new { id = idUsuario });
        }

        public IActionResult DashBoardUsuarios()
        {
            if (HttpContext.Session.GetString("Username") == null || HttpContext.Session.GetString("Username") == "")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == HttpContext.Session.GetString("Username"));
                if (usuario.RolId != 1 && usuario.RolId != 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var usuarios = new List<UsuarioModel>();
            using (SqlConnection conn = new SqlConnection(_dbContext.Valor))
            {
                conn.Open();

                // Obtener el nombre de usuario logueado
                var loggedUser = HttpContext.Session.GetString("Username");

                // Buscar el RolId del usuario logueado
                var loggedUserRole = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loggedUser)?.RolId;

                // Comenzamos construyendo la consulta base.
                StringBuilder query = new StringBuilder();
                query.Append("SELECT u.*, r.Nombre as NombreRol FROM Usuarios u JOIN Roles r ON u.RolId = r.Id WHERE u.CuentaVerificada = 1");

                // Basado en el rol del usuario logueado, ajustamos la consulta.
                if (loggedUserRole.HasValue)
                {
                    switch (loggedUserRole.Value)
                    {
                        case 1:
                            // No necesita agregar nada a la consulta, ya que el RolId 1 puede ver todos los usuarios con CuentaVerificada = 1
                            break;

                        case 2:
                            // Si el RolId es 2, mostramos usuarios cuyo RolId es 3 o 4
                            query.Append(" AND (u.RolId = 3 OR u.RolId = 4)");
                            break;

                            // Puedes añadir más condiciones si es necesario.
                            // case X:
                            //     query.Append(" Y tu condición aquí");
                            //     break;
                    }
                }

                using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var usuario = new UsuarioModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                NombreCompleto = reader["NombreCompleto"].ToString(),
                                NombreUsuario = reader["NombreUsuario"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                RolId = Convert.ToInt32(reader["RolId"]),
                                // ... (añade el resto de las propiedades que desees)
                            };
                            usuario.RolNombre = reader["NombreRol"].ToString();
                            usuarios.Add(usuario);
                        }
                    }
                }
            }

            return View(usuarios);
        }



        public class Hasher
        {
            public static string ComputeSha256Hash(string rawData)
            {
                // Crea un objeto SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // Calcula el Hash 
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                    // Convierte el arreglo de bytes a cadena hexadecimal 
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }

                    // Devuelve la cadena en hexadecimal 
                    return builder.ToString();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Recuperar(string correo)
        {
            using (SqlConnection conn = new SqlConnection(_dbContext.Valor))
            {
                await conn.OpenAsync();

                var command = new SqlCommand($"SELECT * FROM Usuarios WHERE Correo = @Correo", conn);
                command.Parameters.AddWithValue("@Correo", correo);

                var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    TempData["Error"] = "Correo no registrado.";
                    return View();
                }

                UsuarioModel usuario = new UsuarioModel();
                while (reader.Read())
                {
                    usuario.Id = Convert.ToInt32(reader["Id"]);
                    usuario.Correo = reader["Correo"].ToString();
                    usuario.CuentaVerificada = Convert.ToByte(reader["CuentaVerificada"]);
                }
                reader.Close();

                if (usuario.CuentaVerificada != 1)
                {
                    TempData["Error"] = "Su cuenta no ha sido validada, debe validarla.";
                    return View();
                }

                // Genera el nuevo token y fecha
                var recuperarToken = Guid.NewGuid().ToString();
                var fechaActual = DateTime.Now;

                // Actualiza el usuario con el token y la fecha
                command = new SqlCommand($"UPDATE Usuarios SET TokenRecuperacionContrasena = @Token, FechaTokenRecuperacionContrasena = @Fecha WHERE Correo = @Correo", conn);
                command.Parameters.AddWithValue("@Token", recuperarToken);
                command.Parameters.AddWithValue("@Fecha", fechaActual);
                command.Parameters.AddWithValue("@Correo", correo);

                await command.ExecuteNonQueryAsync();
                Email2 email = new Email2();
                email.Enviar(correo, recuperarToken.ToString());
            }

            TempData["Success"] = "Por favor, revisa tu correo electrónico para seguir con el proceso de recuperación.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Token2(string valor)
        {
            // Si el valor del token no se proporciona o es nulo, redirige al usuario al index de home.
            if (string.IsNullOrEmpty(valor))
            {
                TempData["Error2"] = "Token inválido o ha expirado.";
                return RedirectToAction("Index", "Home");
            }

            using (SqlConnection conn = new SqlConnection(_dbContext.Valor))
            {
                await conn.OpenAsync();

                var command = new SqlCommand($"SELECT * FROM Usuarios WHERE TokenRecuperacionContrasena = @Token", conn);
                command.Parameters.AddWithValue("@Token", valor);

                var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    TempData["Error2"] = "Token inválido o ha expirado.";
                    return RedirectToAction("Index", "Home");
                }

                reader.Close();
                ViewBag.Token = valor;
                // Si el token es válido, simplemente devuelve la vista (asumiendo que tienes una vista llamada Token2 para este método)
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarContrasena(string NewPassword, string Token)
        {
            using (SqlConnection conn = new SqlConnection(_dbContext.Valor))
            {
                string hashedPassword = Hasher.ComputeSha256Hash(NewPassword);
                await conn.OpenAsync();

                // Actualiza la contraseña y anula el token
                var command = new SqlCommand($"UPDATE Usuarios SET Contrasena = @Contrasena, TokenRecuperacionContrasena = NULL WHERE TokenRecuperacionContrasena = @Token", conn);
                command.Parameters.AddWithValue("@Contrasena", hashedPassword); // Aquí podría considerar almacenar una versión hash de la contraseña, por razones de seguridad.
                command.Parameters.AddWithValue("@Token", Token);

                await command.ExecuteNonQueryAsync();
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult Registrarse(UsuarioModel u)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int newUserId = 0;
                    using (SqlConnection con = new(_contexto.Valor))
                    {
                        using (SqlCommand cmd = new("sp_registrar", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar).Value = u.NombreCompleto;
                            cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar).Value = u.NombreUsuario;
                            cmd.Parameters.Add("@Correo", SqlDbType.NVarChar).Value = u.Correo;

                            string hashedPassword = Hasher.ComputeSha256Hash(u.Contrasena);

                            cmd.Parameters.Add("@Contrasena", SqlDbType.NVarChar).Value = hashedPassword;
                            var token = Guid.NewGuid();
                            cmd.Parameters.Add("TokenVerificacionCorreo", SqlDbType.NVarChar).Value = token.ToString();
                            cmd.Parameters.Add("@CuentaVerificada", SqlDbType.Bit).Value = 0;
                            cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = 4;
                            con.Open();
                            newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                            con.Close();

                            // Luego de obtener el ID, realizamos la inserción en TD_rolesUsuarios
                            using (SqlCommand cmdRole = new("INSERT INTO TD_rolesUsuarios (id_usuario, id_role, id_estatus, Fecha, usuario) VALUES (@IdUsuario, 4, 1, @Fecha, @Usuario)", con))
                            {
                                cmdRole.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = newUserId;
                                cmdRole.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = DateTime.Now;
                                cmdRole.Parameters.Add("@Usuario", SqlDbType.NVarChar).Value = u.NombreUsuario;
                                con.Open();
                                cmdRole.ExecuteNonQuery();
                                con.Close();
                            }

                            Email email = new Email();

                            if (u.Correo!=null)
                            {
                                email.Enviar(u.Correo, token.ToString());
                            }
                            con.Close();
                        }
                    }
                    return RedirectToAction("Token", "Cuenta");
                }
            }
            catch (System.Exception e)
            {
                string pattern = @"\(([^)]+)\)";  // Busca texto entre paréntesis
                Match match = Regex.Match(e.Message, pattern);
                if (match.Success && e.Message.Contains("Infracción de la restricción UNIQUE KEY"))
                {
                    string duplicateValue = match.Groups[1].Value;  // Obtener el valor entre paréntesis

                    // Comprobar si el valor duplicado es un correo o un nombre de usuario
                    if (duplicateValue.Contains("@"))
                    {
                        ViewData["error"] = $"El correo {duplicateValue} ya existe y no puede ser duplicado.";
                    }
                    else if (!duplicateValue.Contains("gmail"))
                    {
                        ViewData["error"] = $"El usuario {duplicateValue} ya existe y no puede ser duplicado.";
                    }
                    else
                    {
                        // En caso de que no cumpla con ninguna de las condiciones anteriores
                        ViewData["error"] = $"El valor {duplicateValue} ya existe y no puede ser duplicado.";
                    }
                }
                else
                {
                    ViewData["error"] = e.Message; // Otros errores, muestra el mensaje original
                }
                return View();
            }
            return View();
        }

        public ActionResult Token()
        {
            string token = Request.Query["valor"];

            if (token != null)
            {
                try
                {
                    using (SqlConnection con = new(_contexto.Valor))
                    {
                        using (SqlCommand cmd = new("sp_Validar", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("TokenVerificacionCorreo", SqlDbType.NVarChar).Value = token;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            ViewData["mensaje"] = "Su cuenta ha sido validada exitosamente!";
                            con.Close();
                        }
                    }
                    return View();
                }
                catch(System.Exception e)
                {
                    ViewData["mensaje"] = e.Message;
                    return View();
                }
            }
            else
            {
                ViewData["mensaje"] = "Verifique su correo para activar su cuenta.";
                return View();
            }
        }



        [HttpPost]
        public async Task<IActionResult> Login(string UsernameOrEmail, string Password)
        {
            using (SqlConnection conn = new SqlConnection(_dbContext.Valor))
            {
                await conn.OpenAsync();

                // Determinar si UsernameOrEmail es un correo o un nombre de usuario
                bool isEmail = Regex.IsMatch(UsernameOrEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                string queryField = isEmail ? "Correo" : "NombreUsuario";

                // Usar parámetros SQL en lugar de concatenación de cadenas
                var command = new SqlCommand($"SELECT NombreUsuario, Contrasena, CuentaVerificada, RolId FROM Usuarios WHERE {queryField} = @UsernameOrEmail", conn);

                command.Parameters.AddWithValue("@UsernameOrEmail", UsernameOrEmail);

                var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    TempData["Errorlog"] = "El usuario no existe.";
                    return RedirectToAction("Login", "Home");
                }

                while (reader.Read())
                {
                    var hashedPasswordFromDb = reader["Contrasena"].ToString();
                    string hashedPassword = Hasher.ComputeSha256Hash(Password);

                    if (hashedPassword != hashedPasswordFromDb)
                    {
                        TempData["Errorlog"] = "La contraseña es incorrecta.";
                        return RedirectToAction("Login", "Home");
                    }

                    if (Convert.ToInt32(reader["CuentaVerificada"]) != 1)
                    {
                        TempData["Errorlog"] = "Su cuenta no ha sido activada.";
                        return RedirectToAction("Login", "Home");
                    }

                    HttpContext.Session.SetString("Username", reader["NombreUsuario"].ToString());
                    // Guardar RoleId en la sesión
                    HttpContext.Session.SetInt32("RolId", Convert.ToInt32(reader["RolId"]));
                }

                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

    }
}
