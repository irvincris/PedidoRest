using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaEntidades;

namespace CapaNegocio
{
    public class Venta_BL
    {
        CapaDatos.Venta_DAL Venta = new CapaDatos.Venta_DAL();
        public List<ListaSelect> ObtenerBanco()
        {
            return (Venta.ListaBanco());
        }
        public List<ListaSelect> ObtenerEmpresaDelivery()
        {
            return (Venta.ListaEmpresaDelivery());
        }

        public double VerificarStock(int codProd, int codAlmacen)
        {
            return Venta.VerificarStock(codProd,codAlmacen);
        }
        public string ObtenerProducto(string producto)
        {
            return Venta.ObtenerProductos(producto);
        }

        public List<ListaSelect> ObtenerTipoTarjeta()
        {
            return (Venta.ObtenerTipoTarjeta());
        }

        public int GuardarVenta(List<Venta> Vent, List<DetalleVenta> DetalleVenta, List<DetalleComposicion> DetalleComposicion, List<ProductoEliminado> ProductoEliminado)
        {
           return Venta.GuardarVenta(Vent, DetalleVenta, DetalleComposicion, ProductoEliminado);
        }

        public double Tc()
        {
            return Venta.ObtenerDatoCambio();
        }

        public List<ListaSelect> ObtenerTipoDocumentoFac()
        {
            return (Venta.TipoDocumentoFac());
        }

        public List<DatosCliente> ObtenerDatosDocumento(int nroDocu)
        {
            return (Venta.ObtenerDatosDocumento(nroDocu));
        }

        public int VerificarProdHomologado(int CodProd)
        {
            return Venta.VerificarProductoHomologado(CodProd);
        }

        public string UbicacionImpresora(int codAlma)
        {            
            return Venta.UbicacionImpresora(codAlma);
        }
        public int EstadoFacturacion(int codAlma)
        {
            return Venta.EstadoFacturacion(codAlma);
        }

        public string GenerarFactura(int CodVent, int CodAlma, bool EstadoEnvioFact, string email, bool EstadoImprimir)
        {
            return Venta.GenerarFactura(CodVent, CodAlma, EstadoEnvioFact,email, EstadoImprimir);
        }




        /////////////////
        public List<DetalleVenta> ObtenerDetalleVenta(int codVenta)
        {
            return (Venta.ObtenerDetalleVenta(codVenta));
        }

        public int OpcionPrecuenta(int codVenta)
        {
            return Venta.OpcionPrecuenta(codVenta);
        }

        public string DescripcionMesa(int codMesa)
        {
            return Venta.DescripcionMesa(codMesa);
        }

        public List<Producto> StockProductoCompuesto(int codprod, int codAlmacen, int cantidad)
        {
            return (Venta.VerificarStockComposicion(codprod, codAlmacen, cantidad));
        }

        public string DescripcionCuenta(int codCuenta)
        {
            return Venta.DescripcionCuenta(codCuenta);
        }

        public string ObtenerCodVenta(int codMesa, int codCuen)
        {
            return Venta.ObtenerCodVenta(codMesa, codCuen);
        }

        public List<Venta> ObtenerCodigoUser(int CoduserDetalle)
        {
            return Venta.ObtenerCodigoUser(CoduserDetalle);
        }

    }
}
