using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class ProductoEliminado
    {
        public int codProd { get; set; }
        public int codDecv { get; set; }
        public string observacion { get; set; }
        public string Mesa { get; set; }
        public string NombProdAux { get; set; }
        public double CantAux { get; set; }
        public int CoduserDetalle { get; set; }
        public string LoginDetalle { get; set; }
    }
}
