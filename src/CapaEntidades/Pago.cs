using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class Pago
    {
        public double MontoBs { get; set; }
        public double MontoDolar { get; set; }
        public double MontoDolarBs { get; set; }
        public double MontoTransferencia { get; set; }
        public double NroTransferencia { get; set; }
        public int BancoTransferencia { get; set; }
        public double PagoQr { get; set; }
        public double NroQr { get; set; }
        public int BancoQr { get; set; }
        public double MontoTarjeta { get; set; }
        public int TipoTarjeta { get; set; }
        public int BancoTarjeta { get; set; }
        public int NroReferenciaTarjeta { get; set; }
        public double MontoTotal { get; set; }
    }
}
