using Plataforma.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Plataforma.Controllers
{
    public class AccesoController : Controller
    {
        // Cadena de conexión original
        static string cadena = "Data Source=DESKTOP-336M82L\\SQLEXPRESS;Initial Catalog=Prueba;Integrated Security=False;User Id=sa;Password=root;MultipleActiveResultSets=True";

        // GET: Acceso
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(usuario Usuario)
        {
            if (string.IsNullOrEmpty(Usuario.usuario1) || string.IsNullOrEmpty(Usuario.password))
            {
                ViewData["Mensaje"] = "Debe ingresar un usuario y una contraseña válidos.";
                return View();
            }

            using (SqlConnection cn = new SqlConnection(cadena))
            {
            
                    SqlCommand cmd = new SqlCommand("pa_consultaUsuario", cn);
                    cmd.Parameters.AddWithValue("@usuario1", Usuario.usuario1);
                    cmd.Parameters.AddWithValue("@password", Usuario.password);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int existe = Convert.ToInt32(reader["Band"]);
                        string mensaje = reader["Mensaje"].ToString();

                        if (existe == 1)
                        {
                            usuario usuario = new usuario
                            {
                                id = Convert.ToByte(reader["id"]),
                                nombre = reader["nombre"].ToString(),
                                apellido = reader["apellido"].ToString(),
                                usuario1 = reader["usuario2"].ToString(), // Usa el nombre correcto de la columna aquí
                                matricula = reader["matricula"].ToString(),
                                estatus = Convert.ToByte(reader["estatus"])
                            };

                            Session["usuario"] = usuario;
                            ViewData["UsuarioNombre"] = usuario.nombre;
                            ViewData["UsuarioUsuario"] = usuario.usuario1;
                            return RedirectToAction("CalificacionesPorMaestro"); // Redirigir al usuario después del login exitoso
                        }
                        else
                        {
                            ViewData["Mensaje"] = mensaje;
                        }
                    }
                    else
                    {
                        ViewData["Mensaje"] = "Error inesperado al verificar el usuario.";
                    }
                    reader.Close();
                }                           
            return View();
        }

        public ActionResult CalificacionesPorMaestro()
        {
            var calificaciones = new List<Models.Calificacione>();

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("pa_CalificacionesPorMaestro", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    var usuarios = Session["usuario1"] as usuario;
                    if (usuarios == null)
                    {
                        ViewData["Mensaje"] = "No hay sesión activa.";
                        return RedirectToAction("Login");
                    }

                    ViewData["UsuarioID"] = usuarios.id;
                    cmd.Parameters.AddWithValue("@id", usuarios.id);

                    cn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        calificaciones.Add(new Models.Calificacione
                        {
                            Matricula = reader["matricula"].ToString(),
                            Calificacion = Convert.ToInt32(reader["calificacion"]),
                            Tarea = reader["tarea"].ToString()
                        });
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    ViewData["Mensaje"] = "Ocurrió un error: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                }
            }

            ViewData["CalificacionesCount"] = calificaciones.Count;
            return View("Index", calificaciones);
        }
    }
}
