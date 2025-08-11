using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Web.Security;
using System.Data;

namespace CapaDatos
{
    public class Conexion
    {
        public string CNX1;

        public Conexion()
        {
            this.CNX1 = ConfigurationManager.AppSettings["restaurant"];
        }
        //public string RetornarCnx()
        //{
        //    return CNX1;
        //}

        public bool VerificarUsuario(string usuario, string password)
        {
            bool resulPasww = false;
            int DatoRol = 0;
            string codUsua = Consulta("select cod_usua from usuarios where estado=1 and login='" + usuario + "'");
            DatoRol = Convert.ToInt32(Dato("select count(*) from roles_usuario where estado=1 and cod_usu='" + codUsua + "'"));
            if (DatoRol > 0)
            {
                var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
                string passw = "", salto = "";

                SqlConnection con = new SqlConnection(BDRestaurant); ;
                con.Open();

                using (SqlCommand sqlCommand = new SqlCommand("select Password, Salto from usuarios where estado=1 and login='" + usuario + "'", con))
                {
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    sqlCommand.Dispose();
                    while (reader.Read())
                    {
                        passw = reader[0].ToString();
                        salto = reader[1].ToString();
                    }
                }
                con.Close();

                string passwordConcat = string.Concat(password.ToUpper(), salto);
                string psw = FormsAuthentication.HashPasswordForStoringInConfigFile(passwordConcat, "SHA1");
                resulPasww = psw.Equals(passw);
            }

            return resulPasww;

        }

        public string Consulta(string sqlStr)
        {
            string valor = "";
            try
            {
                SqlDataReader reader;
                SqlConnection con;
                con = new SqlConnection(CNX1);
                con.Open();
                using (SqlCommand sqlCommand = new SqlCommand(sqlStr, con))
                {
                    reader = sqlCommand.ExecuteReader();
                    sqlCommand.Dispose();
                    while (reader.Read())
                    {
                        valor = reader[0].ToString();
                    }
                }
                reader.Close();
                con.Close();
               
            }
            catch (Exception)
            {
                valor = "";
            }
            return valor;
        }

        public double Dato(string sqlStr)
        {
            double valor = 0;
            try
            {
                SqlDataReader reader;
                SqlConnection con;
                con = new SqlConnection(CNX1);
                con.Open();
                using (SqlCommand sqlCommand = new SqlCommand(sqlStr, con))
                {
                    reader = sqlCommand.ExecuteReader();
                    sqlCommand.Dispose();
                    while (reader.Read())
                    {
                        valor = Convert.ToDouble(reader[0].ToString());
                    }
                }
                reader.Close();
                con.Close();

            }
            catch (Exception)
            {
                valor = 0;
            }
            return valor;
        }


        public bool Verificar()
        {
            var obj = false;
            if (System.Web.HttpContext.Current.Session["usuario"] == null)
            {
                obj = false;
            }
            else
            {
                obj = true;
            }
            return obj;            
        }

        public void Ejecutar(string sql)
        {
            SqlConnection con;
            con = new SqlConnection(CNX1);
            SqlCommand comando3 = new SqlCommand(sql, con);
            con.Open();
            comando3.ExecuteNonQuery();
            con.Close();
        }

    }
}
