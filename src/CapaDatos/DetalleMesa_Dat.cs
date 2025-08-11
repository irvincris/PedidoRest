using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;
using Dapper;

namespace CapaDatos
{
    public class DetalleMesa_Dat
    {
        CapaDatos.Conexion conexion = new CapaDatos.Conexion();
        public List<DetalleMesa> ObtenerDetalleMesa(int idalmacen, int idplano)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            string DatoColor;
     
            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<DetalleMesa> Lista = new List<DetalleMesa>();

            using (SqlCommand sqlCommand = new SqlCommand(@"select distinct m.cod_mesa,me.Login,m.cod_vent Cod_venta,case when(m.cod_vent=0)then 'lightblue'else us.color end as Color,isnull(PARSENAME(replace(case when(m.cod_vent=0)then 'lightblue'else us.color end,';','.'),3),0)as c1,ISNULL(PARSENAME(replace(case when(m.cod_vent=0)then 'lightblue'else us.color end,';','.'),2),0) as c2,isnull(PARSENAME(replace(case when(m.cod_vent=0)then 'lightblue'else us.color end,';','.'),1),0) as c3,isnull(v.Monto,0)Monto,CONVERT(varchar(5),v.fecha_ins,108)hora,m.descripcion,isnull(me.cod_usua,0) from Mesa m left outer join
                                    Venta v on m.cod_vent = v.Cod_Vent left outer join
                                    vi_meseros me on v.Cod_mesero = me.Cod_Usua left outer join
                                    Usuarios us on me.Cod_Usua=us.Cod_Usua
                                    where m.cod_alma = " + idalmacen + " and m.cod_plano=" + idplano + " and unida=0 and m.estado=1 order by 1", con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                    DetalleMesa Aux = new DetalleMesa();
                    Aux.Cod_mesa = Convert.ToInt32(reader[0].ToString());
                    Aux.Login = reader[1].ToString();
                    Aux.Cod_venta = Convert.ToInt32(reader[2].ToString());
                    try
                    {
                        Color inf = Color.FromArgb(Convert.ToInt32(reader[4].ToString()), Convert.ToInt32(reader[5].ToString()), Convert.ToInt32(reader[6].ToString()));
                        DatoColor = "#" + inf.R.ToString("X2") + inf.G.ToString("X2") + inf.B.ToString("X2");

                    }
                    catch (Exception)
                    {
                        DatoColor = reader[3].ToString();
                    }
                    Aux.Color = DatoColor;
                    Aux.Monto= Convert.ToDouble(reader[7].ToString());
                    Aux.hora = reader[8].ToString();
                    Aux.descripcion = reader[9].ToString();
                    Aux.Cod_Usua = Convert.ToInt32(reader[10].ToString());
                    Lista.Add(Aux);
                }
            }
            con.Close();
            Lista = _ = Lista ?? new List<DetalleMesa>();
            return Lista;

        }

        public List<ListaSelect> ListaPlano(int codAlmacen)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_plano Id, nombre from plano where estado=1 and cod_alma=" + codAlmacen + " order by 2");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public bool EstadoMesa(int codMesa, string usuario, int codVenta, int CodCuen)
        {
            bool estado = false;
            int IdVenta2=0;

            if (codMesa>0)
            {
                //IdVenta = Convert.ToInt32(conexion.Dato("select cod_vent from mesa where cod_mesa=" + codMesa + " and mesero<>'" + usuario + "'"));
                IdVenta2 = Convert.ToInt32(conexion.Dato("select cod_vent from mesa where cod_mesa=" + codMesa + " and mesero='" + usuario + "'"));

                if (IdVenta2 != codVenta)
                {
                    estado = true;
                }
            }
            if (CodCuen>0)
            {
                IdVenta2 = Convert.ToInt32(conexion.Dato("select cod_vent_temp from cuentasTemporales where cod_cuen=" + CodCuen + " and mesero='" + usuario + "'"));

                if (IdVenta2 != codVenta)
                {
                    estado = true;
                }
            }


            return estado;
        }

        public int EstadoVentaMesa(int codVenta)
        {
            int tipEsta = 0;
            tipEsta = Convert.ToInt32(conexion.Dato("select tip_esta from Venta where cod_vent=" + codVenta));
            return tipEsta;
        }

        public int EstadoMesaVent(int codMesa)
        {
            int CodVenta = 0;
            CodVenta = Convert.ToInt32(conexion.Dato("select cod_vent from mesa where cod_mesa=" + codMesa));
            return CodVenta;
        }


        public List<DetalleMesa> ObtenerDetalleCuentas(int idalmacen, int idusuario)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            string DatoColor;

            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<DetalleMesa> Lista = new List<DetalleMesa>();

            using (SqlCommand sqlCommand = new SqlCommand(@"select distinct c.cod_cuen, c.descripcion,c.mesero Login,c.cod_vent_temp Cod_venta,case when(c.cod_vent_temp=0)then 'lightblue'else us.color end as Color,PARSENAME(replace(us.color,';','.'),3)as c1,PARSENAME(replace(us.color,';','.'),2)as c2,PARSENAME(replace(us.color,';','.'),1) as c3 
                                    from CuentasTemporales c left outer join
                                    Venta v on c.cod_vent_temp = v.Cod_Vent left outer join
                                    Usuarios us on c.Cod_Usua=us.Cod_Usua
                                    where c.cod_alma = " + idalmacen + " and c.cod_usua=" + idusuario + " and c.cod_vent=0 and c.tipo=0 order by 1", con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                    DetalleMesa Aux = new DetalleMesa();
                    Aux.CodCuent = Convert.ToInt32(reader[0].ToString());
                    Aux.descripcion = reader[1].ToString();
                    Aux.Login = reader[2].ToString();
                    Aux.Cod_venta = Convert.ToInt32(reader[3].ToString());
                    DatoColor = reader[4].ToString();
                    try
                    {
                        if (Aux.Cod_venta>0)
                        {
                            Color inf = Color.FromArgb(Convert.ToInt32(reader[5].ToString()), Convert.ToInt32(reader[6].ToString()), Convert.ToInt32(reader[7].ToString()));
                            DatoColor = "#" + inf.R.ToString("X2") + inf.G.ToString("X2") + inf.B.ToString("X2");
                        }
                    }
                    catch (Exception)
                    {
                        //DatoColor = reader[4].ToString();
                    }
                    Aux.Color = DatoColor;
                    Lista.Add(Aux);
                }
            }
            con.Close();
            Lista = _ = Lista ?? new List<DetalleMesa>();
            return Lista;
        }

        public int GuardarCuentaTemporal(string Descripcion, int idalmacen, int idusuario)
        {
            int Codcuenta = 1;
            try
            {
                string Nombusuario = conexion.Consulta("select login from usuarios where cod_usua=" + idusuario);
                conexion.Ejecutar("insert into cuentasTemporales(descripcion,cod_usua,mesero,cod_alma,tipo)values('" + Descripcion + "'," + idusuario + ",'" + Nombusuario + "'," + idalmacen + ",0)");
            }
            catch (Exception)
            {
                Codcuenta = 0;               
            }
            return Codcuenta;
        }
        public bool UsoCodigoUsuario()
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            var conectar = new SqlConnection(BDRestaurant);
            bool valor;
            try
            {
                valor = conectar.QueryFirst<bool>("select Valor from Configuracion where Variable='UsoCodigoUsuario'");
            }
            catch (Exception e)
            {
                valor = false;
            }
            return valor;

        }


    }
}
