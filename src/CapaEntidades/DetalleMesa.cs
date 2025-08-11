using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class DetalleMesa
    {
        public int Cod_mesa { get; set; }
        public int Cod_venta { get; set; }
        public string Login { get; set; }
        public string Color { get; set; }
        public int C1 { get; set; }
        public int C2 { get; set; }
        public int C3 { get; set; }
        public double Monto { get; set; }
        public string hora { get; set; }
        public string descripcion { get; set; }
        public int Cod_Usua { get; set; }

        public int CodCuent { get; set; }
    }
}
