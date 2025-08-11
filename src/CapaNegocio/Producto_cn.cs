using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;

namespace CapaNegocio
{
    public class Producto_cn
    {
        CapaDatos.Producto_dat Producto = new CapaDatos.Producto_dat();
        CapaDatos.Conexion DatoUsuario = new CapaDatos.Conexion();
        public List<ListaSelect> ObtenerAlmacen()
        {
            return (Producto.ListaAlmacen());
        }
        public bool VerificarUsuario(string usuario, string password)
        {
            return DatoUsuario.VerificarUsuario(usuario, password);
        }
        public string ObtenerNombre(string usuario)
        {
            return Producto.ObtenerNombre(usuario);
        }

        public List<Producto> ObtenerClasificacion(int idalmacen)
        {
            return (Producto.ListaClasificacion(idalmacen));
        }

        public List<Producto> ObtenerProductos(int CodClass, int idalmacen, string nombreProducto)
        {
            return (Producto.ListaProductos(CodClass, idalmacen, nombreProducto));
        }

        public List<Producto> Obtenercomposicion(int CodProd, int idalmacen, int TipoClasif)
        {
            return (Producto.ListaComposicion(CodProd, idalmacen, TipoClasif));
        }

        public List<ListaSelect> ObtenerCliente()
        {
            return (Producto.ListaCliente());
        }

        public List<ListaSelect> ObtenerAlmacenUsuario(string usuario)
        {
            return (Producto.ListaAlmacenUsuario(usuario));
        }

        public List<Producto> ObtenerDetalleComposicion(int codVenta, int codprod, int CodDecv)
        {
            return (Producto.ObtenerDetalleComposicion(codVenta,codprod, CodDecv));
        }

        public bool ObtenerRolUsuario(string usuario)
        {
            return Producto.ObtenerRolUsuario(usuario);
        }

        public List<ListaSelect> ObtenerObservaciones()
        {
            return (Producto.ListaObservaciones());
        }

        public string ObtenerCodUsuario(string usuario)
        {
            return Producto.ObtenerCodUsuario(usuario);
        }

        public List<Producto> ObtenerListaCompuesto(int codprod)
        {
            return (Producto.ListaCompuestoCodProd(codprod));
        }

        public List<Producto> DatosComposicion(List<CapaEntidades.DetalleComposicion> DatoComposicion, int idAlmacen)
        {
            return (Producto.DatosComposicion(DatoComposicion, idAlmacen));
        }

        public string ObtenerPinUsuario(string usuario)
        {
            return Producto.ObtenerPinUsuario(usuario);
        }

    }
}
