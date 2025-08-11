using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;

namespace CapaNegocio
{
    public class CierreCaja_CN
    {
        CapaDatos.CierreCaja_Dat Caja = new CapaDatos.CierreCaja_Dat();
        public List<CierreCaja> ObtenerDetalleCierre(int idalmacen, string usuario)
        {
            return Caja.ObtenerDatosCierre(idalmacen, usuario);
        }

        public int GuardarCierre(List<CierreDia> Cierre)
        {
            return Caja.GuardarCierre(Cierre);
        }

        public bool InicioDia(string usuario, int CodAlmacen)
        {
            return Caja.InicioDia(usuario,CodAlmacen);
        }

        public List<DatoInicioDia> DatoInicioDia(string usuario, int CodAlmacen)
        {
            return Caja.DatoInicio(usuario,CodAlmacen);
        }

        public int guardarInicioDia(double monto, int codIndi)
        {
            return Caja.GuardarInicioDia(monto,codIndi);
        }

    }
}
