using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using CapaEntidades;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class Producto_dat
    {
        CapaDatos.Conexion conexion = new CapaDatos.Conexion();
        public List<Producto> ListaClasificacion(int idalmacen)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(@"select distinct  v.Cod_Clas,v.clasificacion, v.color from vi_lista_productos v,producto_almacen p
                    where v.tip_clas in (3,2,7,8,21,4,9) and p.estado = 1 and v.Cod_Prod = p.cod_prod and v.hab_vender = 1 and p.cod_alma=" + idalmacen + " order by 2");
                var result = context.Query<Producto>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public List<Producto> ListaProductos(int CodClass, int idalmacen, string nombreProducto)
        {
            int tipoPrecio = Convert.ToInt32(conexion.Dato("Select tipo_precio from almacen where cod_alma=" + idalmacen));
            bool estado = true;
            if (tipoPrecio==0)
            {                
                estado = Convert.ToBoolean(conexion.Consulta("Select valor from configuracion where variable='PRECIOXALMACEN'"));
                if (estado)
                {
                    tipoPrecio = Convert.ToInt32(conexion.Dato("Select tip_precio from almacen where cod_alma=" + idalmacen));
                }
                else
                {
                    tipoPrecio = 1;
                }
            }
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                //string consulta = "Select v.cod_prod,v.nombre,color, v.Precio, v.tip_clas, v.tip_unid from vi_productos_precios v,producto_almacen p where hab_vender=1 and (p.cod_alma='0' or p.cod_alma = " + idalmacen + ") and p.estado=1 and v.Cod_Prod=p.cod_prod " +
                //    "and (tip_clas=3 or tip_clas=2 or tip_clas=7 or tip_clas=9 or tip_clas=8 or tip_clas=4) and convert(datetime,convert(varchar(10),GETDATE(),103) + ' ' + convert(varchar(5),p.hora_inicio ,108)) <= getdate()  and convert(datetime,convert(varchar(10),GETDATE(),103) + ' ' + convert(varchar(5),p.hora_fin ,108)) >=getdate()";

                string consulta = @"Select v.cod_prod,v.nombre,v.color, vp.Precio, v.tip_clas, v.tip_unid, V.Clasificacion, v.Cod_Clas, v.Descripcion, v.imagen AS ImagenData from vi_productos_precios v,producto_almacen p,vi_precios vp where hab_vender=1 and (p.cod_alma='0' or p.cod_alma = " + idalmacen + ") and p.estado=1 and v.Cod_Prod=p.cod_prod and v.Cod_Prod=vp.Cod_Prod and (v.tip_clas=3 or v.tip_clas=2 or v.Tip_Clas=7 or v.Tip_Clas=9 or v.Tip_Clas=8 or v.Tip_Clas=4) " +
                "and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_inicio, 108)) <= getdate()  and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_fin, 108)) >= getdate() and vp.Tipo_Clie = " + tipoPrecio;


                if (CodClass > 0)
                {
                    consulta += " and v.cod_clas=" + CodClass;
                }
                else
                {
                    consulta += " and v.nombre like '%" + nombreProducto + "%'";
                }

                StringBuilder sql = new StringBuilder();
                sql.Append(consulta);
                var result = context.Query<Producto>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }




        public List<Producto> ListaComposicionPrueba(int CodProd, int idalmacen)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(@"Select cod_comp,tipo,producto nombre,cantidad from vi_detalle_composicion v,producto_almacen p 
                where (p.cod_alma='0' or p.cod_alma =" + idalmacen + ") and p.estado=1 and v.Cod_Prod=p.cod_prod and convert(datetime,convert(varchar(10),GETDATE(),103) + ' ' + convert(varchar(5),p.hora_inicio ,108))<=GETDATE()and convert(datetime,convert(varchar(10),GETDATE(),103) + ' ' + convert(varchar(5),p.hora_fin ,108))>=GETDATE() " +
                "and v.cod_pprod=" + CodProd + " and v.opcional=1");
                var result = context.Query<Producto>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }


        public List<Producto> ListaComposicion(int CodProd, int idalmacen, int TipoClasif)
        {
            DateTime finicio = new DateTime();
            
            string consulta = "";
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            int num = 0;

            int contCod = 0;        

            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<Producto> Lista = new List<Producto>();

            consulta = @"Select distinct v.cod_comp,v.tipo,v.producto nombre,v.cantidad,v.cod_deco, v.cod_prod Cod_ProdComp, porcion_llevar PorcionLlevar, porcion, precio, tip_clas, llevar from vi_detalle_composicion v,producto_almacen p 
                where(p.cod_alma = '0' or p.cod_alma = " + idalmacen + ") and p.estado = 1 and v.Cod_Prod = p.cod_prod and " +
                "convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_inicio, 108))<= GETDATE()and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_fin, 108))>= GETDATE() " +
                "and v.cod_pprod=" + CodProd + " and v.opcional=1 order by 1";

            //        consulta = @"Select distinct v.cod_comp,v.tipo,v.producto nombre,v.cantidad,v.cod_deco, v.cod_prod Cod_ProdComp, porcion_llevar PorcionLlevar, porcion, precio, tip_clas, llevar,convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_inicio, 108)) from vi_detalle_composicion v,producto_almacen p 
            //            where(p.cod_alma = '0' or p.cod_alma = " + idalmacen + ") and p.estado = 1 and v.Cod_Prod = p.cod_prod and " +
            //"convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_inicio, 108))<= GETDATE()and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_fin, 108))>= GETDATE() " +
            //"and v.cod_pprod=" + CodProd + " and v.opcional=1 order by 1";


            if (TipoClasif == 9)
            {
                consulta = @"select distinct v.cod_comp,v.tipo,v.producto nombre,v.cantidad,v.cod_deco,v.cod_prod Cod_ProdComp, porcion_llevar PorcionLlevar, porcion, precio, tip_clas, llevar from vi_detalle_composicion v , Menu m , detalle_menu d  ,menu_almacen a
                  where d.estado = 1 and m.estado = 1 and m.cod_menu = d.cod_menu and v.Cod_Prod = d.cod_prod and m.cod_menu = a.cod_menu
                  and v.opcional = 1   and a.cod_alma = " + idalmacen + " and v.cod_pprod=" + CodProd + " order by 1";
            }

            using (SqlCommand sqlCommand = new SqlCommand(consulta, con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                   
                    //contCod = Convert.ToInt32(conexion.Consulta(@"Select COUNT(v.cod_comp), v.tipo, v.cod_comp from vi_detalle_composicion v,producto_almacen p 
                    // where(p.cod_alma = '0' or p.cod_alma = " + idalmacen + ") and p.estado = 1 and v.Cod_Prod = p.cod_prod and convert(datetime, convert(varchar(10), GETDATE(), 103) + ' ' + convert(varchar(5), p.hora_inicio, 108)) <= GETDATE()and convert(datetime, convert(varchar(10), GETDATE(), 103) + ' ' + convert(varchar(5), p.hora_fin, 108)) >= GETDATE() " +
                    // "and v.cod_pprod = " + CodProd + " and v.opcional = 1 and v.cod_comp=" + Convert.ToInt32(reader[0].ToString()) + "" +
                    // "group by tipo, cod_comp "));

                    num += 1;
                    Producto Dato = new Producto();
                    Dato.Cod_Comp = Convert.ToInt32(reader[0].ToString());
                    Dato.Tipo = reader[1].ToString();
                    Dato.Nombre = reader[2].ToString();
                    Dato.Cantidad = Convert.ToInt32(reader[3].ToString());
                    Dato.Cod_deco = Convert.ToInt32(reader[4].ToString());
                    Dato.Cod_ProdComp = Convert.ToInt32(reader[5].ToString());
                    Dato.PorcionLlevar= Convert.ToDouble(reader[6].ToString());
                    Dato.Porcion = Convert.ToDouble(reader[7].ToString());
                    Dato.Precio = Convert.ToDouble(reader[8].ToString());
                    Dato.Tip_Clas = Convert.ToInt32(reader[9].ToString());
                    Dato.llevar = Convert.ToBoolean(reader[10].ToString());
                    Dato.SaltoLinea = "";                  

                    //if (num == contCod)
                    //{                       
                    //    num = 0;
                    //    Dato.SaltoLinea = "<br/>";
                    //}                   
                    Lista.Add(Dato);
                }
            }
            con.Close();
            Lista = _ = Lista == null ? new List<Producto>() : Lista;
            return Lista;
        }



        public List<ListaSelect> ListaAlmacen()
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_alma id,nombre from almacen where estado=1");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public string ObtenerNombre(string usuario)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            var conectar = new SqlConnection(BDRestaurant);
            string NombreUsuario = "";
            try
            {
                NombreUsuario = conectar.QueryFirst<string>("select p.Nombres+' '+p.Ape_Pat+' '+p.Ape_Mat Nombre from Usuarios u inner join Entes e on u.Cod_Ente = e.Cod_Ente inner join Persona p on e.cod_pers = p.Cod_Pers where u.estado=1 and u.login = '" + usuario + "'");
            }
            catch (Exception a)
            {
                NombreUsuario = "";
            }
            return NombreUsuario;

        }

        public List<ListaSelect> ListaCliente()
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_clie Id, nombre from vi_lista_clientes order by 2");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public List<ListaSelect> ListaAlmacenUsuario(string usuario)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(@"select a.cod_alma id,a.nombre from almacen a 
                inner join usuario_almacen ua on a.cod_alma = ua.cod_alma
                inner join Usuarios u on ua.cod_usu = u.Cod_Usua where u.login='" + usuario + "' and ua.estado=1 and a.estado=1 order by a.nombre");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }


        public List<Producto> ObtenerDetalleComposicion(int codVenta, int codProd, int CodDecv)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                //                sql.Append(@"select producto,tipo, nombre, cant_uni from vi_sub_detalle where opcional=1 and cod_vent=" + codVenta + " and cod_pprod=" + codProd);
                sql.Append(@"select producto,tipo, nombre, cant_uni from vi_sub_detalle where opcional=1 and cod_decv=" + CodDecv );
                var result = context.Query<Producto>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public bool ObtenerRolUsuario(string usuario)
        {
            bool estado;
          
            try
            {
                int codUsua = Convert.ToInt32(conexion.Dato("select cod_usua from usuarios where estado=1 and login='" + usuario + "'"));
                estado = Convert.ToBoolean(conexion.Consulta("SELECT estado from roles_usuario where estado=1 and cod_usu=" + codUsua + " and cod_role=6"));
            }
            catch (Exception e)
            {
                estado = false;
            }
            return estado;
        }

        public List<ListaSelect> ListaObservaciones()
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(@"select codigo Id, descripcion Nombre from OBSERVACIONPREDETERMINADA where estado = 1 order by descripcion");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public string ObtenerCodUsuario(string usuario)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            var conectar = new SqlConnection(BDRestaurant);
            string CodUsuario = "";
            try
            {
                CodUsuario = conectar.QueryFirst<string>("select cod_usua from usuarios where estado=1 and login = '" + usuario + "'");
            }
            catch (Exception a)
            {
                CodUsuario = "";
            }
            return CodUsuario;

        }


        public List<Producto> ListaCompuestoCodProd(int codProd)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            string consulta = "select cod_prod, porcion Porcion from vi_detalle_composicion where opcional=0 and cod_pprod=" + codProd + "";
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(consulta);
                var result = context.Query<Producto>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }


        public List<Producto> DatosComposicion(List<DetalleComposicion> composicion, int idAlmacen)
        {

            string producto = "";
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            double stock = 0, cantidad = 0;

            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<Producto> Lista = new List<Producto>();

            if (composicion!=null)
            {
                foreach (var item in composicion)
                {

                    var codprod = item.CodProd;
                    stock = conexion.Dato("select cantidad from existencia where cod_prod=" + item.CodProd + " and cod_alma=" + idAlmacen);

                    foreach (var itemAux in composicion)
                    {
                        if (item.CodProd == itemAux.CodProd)
                        {
                            cantidad += itemAux.Cantidad;
                        }
                    }

                    if (stock < cantidad)
                    {
                        if (producto.Length > 1)
                        {
                            producto = producto + "," + Convert.ToString(item.CodProd);
                        }
                        else
                        {
                            producto = Convert.ToString(item.CodProd);
                        }
                    }

                    cantidad = 0;
                }

                Producto Dato = new Producto();
                Dato.producto = producto;
                Lista.Add(Dato);

                con.Close();
                Lista = _ = Lista ?? new List<Producto>();
                return Lista;
            }
            else
            {
                return Lista;
            }


        }

        public string ObtenerPinUsuario(string usuario)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            var conectar = new SqlConnection(BDRestaurant);
            string CodUsuario = "";
            try
            {
                CodUsuario = conectar.QueryFirst<string>("select codigo from usuarios where estado=1 and login = '" + usuario + "'");
            }
            catch (Exception a)
            {
                CodUsuario = "";
            }
            return CodUsuario;

        }

    }
}
