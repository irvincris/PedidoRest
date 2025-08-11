using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class DetalleVenta
    {
        public int CodProd { get; set; }
        public int Cantidad { get; set; }
        public double Precio { get; set; }
        public double Descuento { get; set; }
        public double Subtotal { get; set; }
        public int TipoUnid { get; set; }
        public int CodAlma { get; set; }
        public int TipClas { get; set; }
        public int NroProducto { get; set; }
        public string NombreProducto { get; set; }
        public int cod_mesa { get; set; }
        public int Nuevo { get; set; }
        public int Cod_Decv { get; set; }
        public string descripcionMesa { get; set; }
        public string observacion { get; set; }
        public int codUsuaDetalle { get; set; }
    }
}
