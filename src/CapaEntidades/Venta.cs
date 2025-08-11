using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class Venta
    {
        public int CodVenta { get; set; }
        public int CodClie { get; set; }
        public int CodSubClie { get; set; }
        public string Descripcion { get; set; }
        public double Monto { get; set; }
        public double Descuento { get; set; }
        public int TipVent { get; set; }
        public int TipMone { get; set; }      
        public int TipEsta { get; set; }
        public double Recargo { get; set; }
        public int CodAlma { get; set; }
        public int CodBolsin { get; set; }
        public int CodUsua { get; set; }
        public int Factura { get; set; }
        public double Efectivo { get; set; }
        public int Tipo { get; set; }
        public int TipoVenta { get; set; }
        public int CodCaja { get; set; }
        public string NombreComanda { get; set; }
        public int NroPedido { get; set; }
        public string MotivoDescuento { get; set; }
        public int CodMesero { get; set; }
        public string Mesa { get; set; }
        public int CodCuen { get; set; }
        public string Observacion { get; set; }
        public int UsuarioDesc { get; set; }
        public int CodPediVenta { get; set; }
        public int CodDelivery { get; set; }
        public int PrecioDelivery { get; set; }
        public int NumPedido { get; set; }
        public bool FacIncluida { get; set; }
        public int CodCanal { get; set; }
        public int CodSucuImp { get; set; }
        public int CodPunto { get; set; }
        public string Usuario { get; set; }
        public string NombreDelivery { get; set; }
        public int CodMesa { get; set; }
        public string descripcionMesa { get; set; }
        public string codigo { get; set; }
    }
}
