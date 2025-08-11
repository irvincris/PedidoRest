using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XpertoRes.Controllers
{
    public class DetalleMesaController : Controller
    {
        CapaDatos.Conexion funcion = new CapaDatos.Conexion();
        CapaNegocio.DetalleMesa_CN DetalleMesa = new CapaNegocio.DetalleMesa_CN();
        // GET: DetalleMesa
        public ActionResult Index()
        {
            if (funcion.Verificar())
            {
                ViewBag.ListaPlano = DetalleMesa.ListaPlano(Convert.ToInt32(Session["idalmacen"].ToString()));
                Session["UsoCodigoUsuario"] = DetalleMesa.UsoCodigoUsuario();
                return View("~/Views/DetalleMesa.cshtml");
                //return View("~/Views/Home/Index.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public JsonResult ListaMesa(int idalmacen, int idplano)
        {
            return Json(DetalleMesa.DetalleMesa(idalmacen, idplano));
        }

        public JsonResult EstadoMesa(int codMesa, string usuario, int codVenta, int CodCuen)
        {
            bool codVent = DetalleMesa.EstadoMesa(codMesa, usuario, codVenta, CodCuen);
            return (new JsonResult { Data = new { data = codVent } });
        }

        public JsonResult EstadoVentaMesa(int codVenta)
        {
            int estado = DetalleMesa.EstadoVentaMesa(codVenta);
            return (new JsonResult { Data = new { data = estado } });
        }

        public JsonResult VerificarEstadoMesa(int codMesa)
        {
            int estado = DetalleMesa.VerificarEstadoMesa(codMesa);
            return (new JsonResult { Data = new { data = estado } });
        }

        public JsonResult ObtenerCuentas(int idalmacen, int idusuario)
        {
            return Json(DetalleMesa.DetalleCuenta(idalmacen, idusuario));
        }

        public JsonResult GuardarCuentaTemporal(string Descripcion, int idalmacen, int idusuario)
        {
            int estado = DetalleMesa.GuardarCuentaTemporal(Descripcion,idalmacen, idusuario);
            return (new JsonResult { Data = new { data = estado } });
        }

    }
}