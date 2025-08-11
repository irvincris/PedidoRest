using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaEntidades;

namespace AdminLTE.Web.Controllers
{
    public class CierreCajaController : Controller
    {
        CapaNegocio.CierreCaja_CN Caja = new CapaNegocio.CierreCaja_CN();
        CapaDatos.Conexion funcion = new CapaDatos.Conexion();
        // GET: CierreCaja
        public ActionResult Index()
        {
            if (funcion.Verificar())
            {
                int idalmacen = Convert.ToInt32(Session["idalmacen"].ToString());
                //ViewBag.ListaCajero = Caja.ObtenerCajero(idalmacen);
                return View("~/Views/CierreCaja.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public JsonResult ObtenerDetalleCaja()
        {
            string usuario = Session["usuario"].ToString();
            int idalmacen = Convert.ToInt32(Session["idalmacen"].ToString());
            return Json(Caja.ObtenerDetalleCierre(idalmacen, usuario));
        }

        public JsonResult GuardarCierre(List<CierreDia> DatoCierre)
        {
            int codCidi = Caja.GuardarCierre(DatoCierre);
            return new JsonResult { Data = new { dato = codCidi } };
        }

        public JsonResult ValidarInicioCaja(string usuario, int CodAlmacen)
        {
            bool Estado = Caja.InicioDia(usuario, CodAlmacen);
            return new JsonResult { Data = new { dato = Estado } };
        }

        public ActionResult InicioDia()
        {
            if (funcion.Verificar())
            {                     
                return View("~/Views/InicioDia.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public JsonResult ObtenerDatoInicioDia()
        {
            string usuario = Session["usuario"].ToString();
            int idalmacen = Convert.ToInt32(Session["idalmacen"].ToString());

            return Json(Caja.DatoInicioDia(usuario, idalmacen));

        }

        public JsonResult GuardarInicioDia(double monto, int codIndi)
        {            
            return new JsonResult { Data = new { dato = Caja.guardarInicioDia(monto,codIndi) } };
        }

    }
}