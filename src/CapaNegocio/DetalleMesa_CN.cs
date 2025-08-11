using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;

namespace CapaNegocio
{
    public class DetalleMesa_CN
    {
        CapaDatos.DetalleMesa_Dat DetalleMesas = new CapaDatos.DetalleMesa_Dat();
        public List<DetalleMesa> DetalleMesa(int idalmacen, int idplano)
        {
            return (DetalleMesas.ObtenerDetalleMesa(idalmacen, idplano));
        }
        public List<ListaSelect> ListaPlano(int idalmacen)
        {
            return (DetalleMesas.ListaPlano(idalmacen));
        }

        public bool EstadoMesa(int CodMesa, string usuario, int codVenta, int CodCuen)
        {
            return (DetalleMesas.EstadoMesa(CodMesa, usuario, codVenta, CodCuen));
        }

        public int EstadoVentaMesa(int CodVenta)
        {
            return (DetalleMesas.EstadoVentaMesa(CodVenta));
        }

        public int VerificarEstadoMesa(int CodMesa)
        {
            return (DetalleMesas.EstadoMesaVent(CodMesa));
        }

        public List<DetalleMesa> DetalleCuenta(int idalmacen, int idusuario)
        {
            return (DetalleMesas.ObtenerDetalleCuentas(idalmacen, idusuario));
        }

        public int GuardarCuentaTemporal(string Descripcion, int idalmacen, int idusuario)
        {
            return (DetalleMesas.GuardarCuentaTemporal(Descripcion,idalmacen, idusuario));
        }

        public bool UsoCodigoUsuario()
        {
            return DetalleMesas.UsoCodigoUsuario();
        }

    }
}
