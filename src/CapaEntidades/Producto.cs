using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class Producto
    {
        public int Cod_Clas { get; set; }
        public int Cod_Prod { get; set; }
        public int Cod_ProdComp { get; set; }
        public int Cod_Comp { get; set; }
        public int Cod_deco { get; set; }
        public string Clasificacion { get; set; }
        public string Color { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public double Precio { get; set; }
        public string SaltoLinea { get; set; }
        public int Tip_Clas { get; set; }
        public int tip_unid { get; set; }
        public double PorcionLlevar { get; set; }
        public double Porcion { get; set; }
        public bool llevar { get; set; }
        public string producto { get; set; }
        public int cant_uni { get; set; }
        public string Descripcion { get; set; }

        public byte[] ImagenData { get; set; } 

        public string ImagenBase64
        {
            get
            {
                return ImagenData != null ? Convert.ToBase64String(ImagenData) : "";
            }
        }
    }
}
