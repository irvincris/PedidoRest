using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminLTE.Web.Controllers
{
    public class ProductoController : Controller
    {
        CapaDatos.Conexion funcion = new CapaDatos.Conexion();
        CapaNegocio.Producto_cn Producto = new CapaNegocio.Producto_cn();
        // GET: Producto
        public ActionResult Index()
        {
            if (funcion.Verificar())
            {                   
                return View("~/Views/ProductoStock.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public JsonResult ListaAlmacen(int idalmacen)
        {
            return Json(Producto.ObtenerAlmacen());
        }

        public JsonResult ListarClasificacion(int idalmacen)
        {
            return Json(Producto.ObtenerClasificacion(idalmacen));
        }

        public JsonResult ListarProducto(int CodClas, int idalmacen, string nombreProducto)
        {
            return Json(Producto.ObtenerProductos(CodClas,idalmacen, nombreProducto));
        }

        public JsonResult ListarComposicion(int CodProd, int idalmacen, int TipClasif)
        {
            return Json(Producto.Obtenercomposicion(CodProd, idalmacen, TipClasif));
        }

        public JsonResult ObtenerDetalleComposicion(int codVenta, int codprod, int CodDecv)
        {
            return Json(Producto.ObtenerDetalleComposicion(codVenta,codprod, CodDecv));
        }

        public JsonResult ListarObservaciones()
        {
            return Json(Producto.ObtenerObservaciones());
        }

        public JsonResult ObtenerListaCompuesto(int codProdPrincipal)
        {
            return Json(Producto.ObtenerListaCompuesto(codProdPrincipal));
        }

        public JsonResult DatosComposicion(List<CapaEntidades.DetalleComposicion> DatoComposicion, int idAlmacen)
        {

            return Json(Producto.DatosComposicion(DatoComposicion,idAlmacen));
        }

    }
}