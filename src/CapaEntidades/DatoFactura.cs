using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class DatoFactura
    {
        public int Id { get; set; }
        public int CodVent { get; set; }
        public double Monto { get; set; }
        public int NroDoc { get; set; }
        public int TipoDoc { get; set; }
        public string RazonSocial { get; set; }
        public string Email { get; set; }
        public string MetodoPago { get; set; }
        public string NroTarjeta { get; set; }
        public string complemento { get; set; }
    }
}
