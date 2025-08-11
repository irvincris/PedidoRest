using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminLTE.Web.Controllers
{
    public class LoginController : Controller
    {
        CapaNegocio.Producto_cn producto = new CapaNegocio.Producto_cn();
        
        // GET: Login
        public ActionResult Index()
        {
            Session["usuario"] = null;
            ViewBag.ListaAlmacen = producto.ObtenerAlmacen();
            return View("~/Views/Login.cshtml");
        }

        public JsonResult IniciarLogin(string usuario, string password, string idalmacen, string NombreAlmacen)
        {
            Session["idalmacen"] = idalmacen;
            Session["usuario"] = usuario;
            Session["nombreUsuario"] = producto.ObtenerNombre(usuario);
            Session["NombreAlmacen"] = NombreAlmacen;
            Session["CodUsuario"] = producto.ObtenerCodUsuario(usuario);
            Session["PinUsuario"] = producto.ObtenerPinUsuario(usuario);

            return (new JsonResult { Data = new { data = producto.VerificarUsuario(usuario, password) } });
        }

        public JsonResult verificar()
        {
            bool obj;
            if (System.Web.HttpContext.Current.Session["usuario"] == null){

                obj = false;
            }
            else{
                obj = true;
            }
            return (new JsonResult { Data = new { data = obj } });

        }

        public JsonResult ObtenerAlmacenUsuario(string usuario)
        {
            return Json(producto.ObtenerAlmacenUsuario(usuario));
        }

        public JsonResult VerificarRolUsuario()
        {
            string Usuario = Session["usuario"].ToString();
            bool estado = producto.ObtenerRolUsuario(Usuario);
            return new JsonResult { Data = new { dato = estado } };
        }

    }
}