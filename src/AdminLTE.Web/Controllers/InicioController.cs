using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace AdminLTE.Web.Controllers
{
    public class InicioController : Controller
    {
      
        CapaDatos.Conexion funcion = new CapaDatos.Conexion();
        // GET: Inicio
        public ActionResult Index()
        {
            if (funcion.Verificar())
            {
                string idalmacen = Session["idalmacen"].ToString();             
                return View("~/Views/Inicio.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }
    }
}