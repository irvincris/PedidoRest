using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using CapaEntidades;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Microsoft.Reporting.WebForms;
using System.IO;
using QRCoder;

//using System.Timers;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;


namespace CapaDatos
{
    public class Venta_DAL
    {
        CapaDatos.Conexion conexion = new CapaDatos.Conexion();
        private Timer timer;
        public DataSet DTSDatos = new DataSet();


        public List<ListaSelect> ListaBanco()
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_banco id, banco nombre from cuenta_banco where estado=1");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public List<ListaSelect> ListaEmpresaDelivery()
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_delivery id, nombre from empresa_delivery where estado=1");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public double VerificarStock(int codProd, int codAlmacen)
        {
            //EnviarCorreo("","","");
            double Stock = 0;
            Stock = Convert.ToDouble(conexion.Dato("select cantidad from existencia where cod_prod=" + codProd + " and cod_alma=" + codAlmacen + ""));
            return Stock;
        }

        public string ObtenerProductos(string producto)
        {
            string productos = conexion.Consulta("SELECT STUFF((SELECT ',' + nombre from producto where cod_prod in (" + producto + ")FOR XML PATH ('')),1,1, '')");
            return productos;
        }

        public double ObtenerDatoCambio()
        {
            double tc = Convert.ToDouble(conexion.Consulta("select venta from bolsin where estado=1"));
            return tc;
        }

        public List<ListaSelect> ObtenerTipoTarjeta()
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select tipo_tarj id, descripcion nombre from tipo_tarjeta where estado=1");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public List<ListaSelect> TipoDocumentoFac()
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select cod_regi id, descripcion nombre from F_Cat_TipoDocumentoIdentidad where estado=1");
                var result = context.Query<ListaSelect>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }


        public void guardarejemplo(List<Venta> array1)
        {
            string ds = "";
            foreach (var item in array1)
            {
                ds = item.Descripcion;
            }
        }

        int codUsuario = 0, TipoTarjeta = 0, nroRefTarjeta = 0;

        public int GuardarVenta(List<Venta> Vent, List<DetalleVenta> DetalleVenta, List<DetalleComposicion> DetalleComposicion, List<ProductoEliminado> ProductoEliminado)
        {

            int idAlmacen, DetCom = 0, CodVent = 0;
            foreach (var item in Vent)
            {
                idAlmacen = item.CodAlma;
                codUsuario = Convert.ToInt32(conexion.Consulta("select cod_usua from usuarios where login='" + item.Usuario + "'"));
                int codSucImport = Convert.ToInt32(conexion.Consulta("Select cod_suc_imp from almacen where cod_alma =" + item.CodAlma + ""));
                int Tipo_Costo = 1;
                try
                {
                    Tipo_Costo = Convert.ToInt32(conexion.Consulta("Select Valor from configuracion where variable ='Tipo_Costo'"));
                }
                catch (Exception)
                {
                    Tipo_Costo = 1;
                }
              

                var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
                SqlConnection conn;
                conn = new SqlConnection(BDRestaurant);
                conn.Open();
                SqlTransaction trann;
                trann = conn.BeginTransaction();

                SqlCommand Venta, Venta2;
                DataTable data;
                SqlDataAdapter sqlData;


                try
                {
                    Venta = new SqlCommand("PA_UIDS_Venta", conn)
                    {
                        Connection = trann.Connection,
                        Transaction = trann,
                        CommandType = CommandType.StoredProcedure
                    };

                    Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Vent", -1));
                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Clie", item.CodClie));
                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Sub_Clie", "0"));
                    Venta.Parameters.Add(new SqlParameter("@pe_descripcion", ""));
                    Venta.Parameters.Add(new SqlParameter("@pe_Monto", item.Monto));
                    Venta.Parameters.Add(new SqlParameter("@pe_Descuento", item.Descuento));
                    Venta.Parameters.Add(new SqlParameter("@pe_Tip_vent", 2));
                    Venta.Parameters.Add(new SqlParameter("@pe_Tip_mone", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_Tip_esta", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_recargo", "0"));
                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", item.CodAlma));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_bolsin", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                    Venta.Parameters.Add(new SqlParameter("@pe_factura", "0"));
                    Venta.Parameters.Add(new SqlParameter("@pe_efectivo", item.Efectivo));
                    Venta.Parameters.Add(new SqlParameter("@pe_tipo", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_tipo_venta", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_caja", 1));
                    Venta.Parameters.Add(new SqlParameter("@pe_nro_pedido", 0));
                    Venta.Parameters.Add(new SqlParameter("@pe_motivo_descuento", ""));
                    Venta.Parameters.Add(new SqlParameter("@cod_mesero", codUsuario));
                    Venta.Parameters.Add(new SqlParameter("@mesa", item.descripcionMesa));
                    Venta.Parameters.Add(new SqlParameter("@cod_cuen", item.CodCuen));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_pedi_venta", 1));

                    if (item.CodMesa == 0)
                    {
                        Venta.Parameters.Add(new SqlParameter("@pe_mesa_abierta", 0));
                    }
                    else
                    {
                        Venta.Parameters.Add(new SqlParameter("@pe_mesa_abierta", 1));
                    }

                    Venta.Parameters.Add(new SqlParameter("@pe_cod_mesa", item.CodMesa));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_sucu_imp", codSucImport));
                    Venta.Parameters.Add(new SqlParameter("@pe_cod_punto", 0));
                    Venta.Parameters.Add(new SqlParameter("@pe_Pedido_Web", 1));

                    if (item.CodVenta == 0)
                    {
                        CodVent = Convert.ToInt32(Venta.ExecuteScalar());

                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            CommandText = "update mesa set cod_vent=" + CodVent + ", mesero='" + item.Usuario + "' where cod_mesa=" + item.CodMesa,
                            CommandType = CommandType.Text
                        };
                        Venta.ExecuteNonQuery();
                    }
                    else
                    {
                        CodVent = item.CodVenta;
                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            //CommandText = "update Venta set Monto=" + Convert.ToDouble(item.Monto) + " where cod_vent=" + item.CodVenta,
                            CommandText = "update Venta set Monto = @Monto where cod_vent = @CodVenta",
                            CommandType = CommandType.Text
                        };
                        Venta.Parameters.AddWithValue("@Monto", item.Monto);
                        Venta.Parameters.AddWithValue("@CodVenta", item.CodVenta);
                        Venta.ExecuteNonQuery();
                    }


                    foreach (var itemDetalle in DetalleVenta)
                    {

                        if (itemDetalle.Nuevo == 1)
                        {

                            Venta = new SqlCommand("PA_UIDS_Detalle_Venta", conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandType = CommandType.StoredProcedure
                            };
                            Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_vent", CodVent));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", itemDetalle.CodProd));
                            Venta.Parameters.Add(new SqlParameter("@pe_cantidad", itemDetalle.Cantidad));
                            Venta.Parameters.Add(new SqlParameter("@pe_precio", itemDetalle.Precio));
                            Venta.Parameters.Add(new SqlParameter("@pe_descuento", itemDetalle.Descuento));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Unid", itemDetalle.TipoUnid));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                            Venta.Parameters.Add(new SqlParameter("@pe_observaciones", itemDetalle.observacion));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));
                            Venta.Parameters.Add(new SqlParameter("@venta_c", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_prec_esp", 0));

                            Venta.Parameters.Add(new SqlParameter("@pe_cod_interno", null));
                            Venta.Parameters.Add(new SqlParameter("@pe_CodigoProducto", 0));
                            Venta.Parameters.Add(new SqlParameter("@pe_CodigoUnidad", 0));
                            Venta.Parameters.Add(new SqlParameter("@pe_DescripcionProducto", ""));
                            Venta.Parameters.Add(new SqlParameter("@pe_CodigoActividad", null));
                            Venta.Parameters.Add(new SqlParameter("@pe_Pedido_Web", 1));

                            DetCom = Convert.ToInt32(Venta.ExecuteScalar());

                            if (itemDetalle.TipClas != 3 && itemDetalle.TipClas != 4 && itemDetalle.TipClas != 8 && itemDetalle.TipClas != 9)
                            {


                                double costo = 0;                         
                                //costo = conexion.Dato("seleCt dbo.costosproductos (" + itemDetalle.CodProd + "," + idAlmacen + ")");

                                Venta = new SqlCommand()
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann,
                                    CommandText = "seleCt dbo.costosproductos (" + itemDetalle.CodProd + "," + idAlmacen + ")",
                                    CommandType = CommandType.Text
                                };
                                costo = Convert.ToDouble(Venta.ExecuteScalar());


                                Venta = new SqlCommand("pa_existencia", conn)
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann,
                                    CommandType = CommandType.StoredProcedure
                                };
                                Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", itemDetalle.CodProd));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", itemDetalle.Cantidad));
                                Venta.ExecuteNonQuery();


                                Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann,
                                    CommandType = CommandType.StoredProcedure
                                };
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", itemDetalle.CodProd));
                                Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                Venta.Parameters.Add(new SqlParameter("@pe_salida", itemDetalle.Cantidad));
                                Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));                                
                                Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));                             
                                Venta.ExecuteNonQuery();

                            }



                            /////DetalleComp  tipo 1

                            double porciones = 0, totalesprod = 0, PorcionOp = 0, totalCostos = 0;


                            foreach (var itemDetalleCompTotales in DetalleComposicion)
                            {
                                if (itemDetalleCompTotales.NroProducto == itemDetalle.NroProducto && itemDetalleCompTotales.TipoComp == 1 && itemDetalleCompTotales.Nuevo == 1)
                                {
                                    porciones = itemDetalleCompTotales.Porcion;
                                    totalesprod += ((itemDetalleCompTotales.Cantidad * porciones * itemDetalleCompTotales.CantidadProdPrincipal) * itemDetalleCompTotales.Precio);
                                }

                            }

                            foreach (var itemDetalleComp in DetalleComposicion)
                            {
                                if (itemDetalleComp.NroProducto == itemDetalle.NroProducto && itemDetalleComp.TipoComp == 1 && itemDetalleComp.Nuevo == 1)
                                {
                                    if (itemDetalleComp.Porcion != 0)
                                    {

                                        Venta = new SqlCommand("PA_UIDS_Sub_Detalle", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };
                                        Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_decv", DetCom));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_comp", itemDetalleComp.CodComp));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", itemDetalleComp.CodProd));
                                        //Venta.Parameters.Add(new SqlParameter("@pe_cantidad", itemDetalleComp.Cantidad * porciones * itemDetalleComp.CantidadProdPrincipal));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cantidad", itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cant_uni", itemDetalleComp.Cantidad * itemDetalleComp.CantidadProdPrincipal));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_opcional", true));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));

                                        PorcionOp = itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal;

                                        double prodPrecio = 0;
                                        if (totalesprod != 0)
                                        {
                                            prodPrecio = (((itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal) * itemDetalleComp.Precio) / totalesprod) * itemDetalleComp.PrecioGeneral * (itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal);
                                        }

                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", prodPrecio));
                                        Venta.Parameters.Add(new SqlParameter("@pe_tipo_detalle", 1));
                                        Venta.ExecuteNonQuery();

                                        if (itemDetalleComp.TipClas != 3 && itemDetalleComp.TipClas != 4)
                                        {


                                            double costo = 0;                                      
                                            //costo = conexion.Dato("seleCt dbo.costosproductos (" + itemDetalleComp.CodProd + "," + idAlmacen + ")");

                                            Venta = new SqlCommand()
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandText = "seleCt dbo.costosproductos (" + itemDetalleComp.CodProd + "," + idAlmacen + ")",
                                                CommandType = CommandType.Text
                                            };
                                            costo = Convert.ToDouble(Venta.ExecuteScalar());


                                            Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", itemDetalleComp.CodProd));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_salida", itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal));
                                            Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                            Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                            Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                            Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                            Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                            Venta.ExecuteNonQuery();



                                            Venta = new SqlCommand("pa_existencia", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", itemDetalleComp.CodProd));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", itemDetalleComp.Cantidad * itemDetalleComp.Porcion * itemDetalleComp.CantidadProdPrincipal));
                                            Venta.ExecuteNonQuery();
                                        }

                                    }



                                    ////// DetalleSubComposicion tipo 2 composicion opcional //////

                                    double PorcionOc = 0;
                                    foreach (var DetalleSubCompTotales in DetalleComposicion)
                                    {
                                        if (DetalleSubCompTotales.NroProducto == itemDetalle.NroProducto && DetalleSubCompTotales.TipoComp == 2 && DetalleSubCompTotales.CodProdPrincipal == itemDetalleComp.CodProd && DetalleSubCompTotales.Nuevo == 1 && DetalleSubCompTotales.NC == itemDetalleComp.NC)
                                        {
                                            PorcionOc = DetalleSubCompTotales.Porcion;
                                            totalesprod += (DetalleSubCompTotales.Cantidad * PorcionOp) * DetalleSubCompTotales.Precio;
                                            totalCostos += (DetalleSubCompTotales.Cantidad * PorcionOp) * DetalleSubCompTotales.Precio;
                                        }

                                    }

                                    foreach (var DetalleSubComp in DetalleComposicion)
                                    {
                                        if (DetalleSubComp.NroProducto == itemDetalle.NroProducto && DetalleSubComp.TipoComp == 2 && DetalleSubComp.CodProdPrincipal == itemDetalleComp.CodProd && DetalleSubComp.Nuevo == 1 && DetalleSubComp.NC == itemDetalleComp.NC)
                                        {
                                            if (DetalleSubComp.Porcion != 0)
                                            {

                                                PorcionOc = DetalleSubComp.Porcion * DetalleSubComp.Cantidad;

                                                Venta = new SqlCommand("PA_UIDS_Sub_Detalle", conn)
                                                {
                                                    Connection = trann.Connection,
                                                    Transaction = trann,
                                                    CommandType = CommandType.StoredProcedure
                                                };
                                                Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                                Venta.Parameters.Add(new SqlParameter("@pe_cod_decv", DetCom));
                                                Venta.Parameters.Add(new SqlParameter("@pe_cod_comp", DetalleSubComp.CodComp));
                                                Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", DetalleSubComp.CodProd));
                                                Venta.Parameters.Add(new SqlParameter("@pe_cantidad", PorcionOp * PorcionOc));
                                                Venta.Parameters.Add(new SqlParameter("@pe_cant_uni", PorcionOp));
                                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                                                Venta.Parameters.Add(new SqlParameter("@pe_opcional", true));
                                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));

                                                double prodPrecio1 = 0;
                                                if (prodPrecio1 > 0)
                                                {
                                                    prodPrecio1 = (((PorcionOp * PorcionOc) * DetalleSubComp.Precio) / totalesprod) * itemDetalleComp.Precio * PorcionOp * PorcionOc;
                                                }
                                                Venta.Parameters.Add(new SqlParameter("@pe_precio", prodPrecio1));
                                                Venta.Parameters.Add(new SqlParameter("@pe_tipo_detalle", 2));
                                                Venta.ExecuteNonQuery();

                                                if (DetalleSubComp.TipClas != 3 && DetalleSubComp.TipClas != 4)
                                                {


                                                    double costo = 0;                                              
                                                    //costo = conexion.Dato("seleCt dbo.costosproductos (" + DetalleSubComp.CodProd + "," + idAlmacen + ")");

                                                    Venta = new SqlCommand()
                                                    {
                                                        Connection = trann.Connection,
                                                        Transaction = trann,
                                                        CommandText = "seleCt dbo.costosproductos (" + DetalleSubComp.CodProd + "," + idAlmacen + ")",
                                                        CommandType = CommandType.Text
                                                    };
                                                    costo = Convert.ToDouble(Venta.ExecuteScalar());


                                                    Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                                    {
                                                        Connection = trann.Connection,
                                                        Transaction = trann,
                                                        CommandType = CommandType.StoredProcedure
                                                    };
                                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", DetalleSubComp.CodProd));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_salida", PorcionOp * PorcionOc));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                                    Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                                    Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                                    Venta.ExecuteNonQuery();


                                                    Venta = new SqlCommand("pa_existencia", conn)
                                                    {
                                                        Connection = trann.Connection,
                                                        Transaction = trann,
                                                        CommandType = CommandType.StoredProcedure
                                                    };
                                                    Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", DetalleSubComp.CodProd));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                                    Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", PorcionOp * PorcionOc));
                                                    Venta.ExecuteNonQuery();
                                                }

                                            }


                                        }
                                    }


                                    ///// Composicion dentro de otra Composicion obligatorios ///////
                                    totalesprod = 0;
                                    double porcionC = 0;


                                    Venta = new SqlCommand("select cod_comp,cod_prod,producto,Cod_Clas,porcion,Tip_Clas,porcion, porcion_llevar,llevar,costo,precio from vi_detalle_composicion where opcional=0 and cod_pprod=" + itemDetalleComp.CodProd + "", conn)
                                    {
                                        Connection = trann.Connection,
                                        Transaction = trann,
                                    };
                                    sqlData = new SqlDataAdapter(Venta);
                                    var data2 = new DataTable();
                                    sqlData.Fill(data2);


                                    foreach (DataRow dat in data2.Rows)
                                    {
                                        porcionC = Convert.ToDouble(dat["porcion"].ToString());
                                        totalesprod += ((porcionC * PorcionOp)) * Convert.ToDouble(dat["precio"].ToString());
                                        totalCostos += ((porcionC * PorcionOp)) * Convert.ToDouble(dat["costo"].ToString());
                                    }

                                    foreach (DataRow reader in data2.Rows)
                                    {
                                        Venta = new SqlCommand("PA_UIDS_Sub_Detalle", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };

                                        Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_decv", DetCom));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_comp", Convert.ToInt32(reader["cod_comp"].ToString())));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", Convert.ToInt32(reader["cod_prod"].ToString())));
                                        porcionC = Convert.ToDouble(reader["porcion"].ToString());
                                        Venta.Parameters.Add(new SqlParameter("@pe_cantidad", PorcionOp * porcionC));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cant_uni", PorcionOp));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_opcional", false));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));

                                        double proPrecio = 0;
                                        if (totalesprod > 0)
                                        {
                                            proPrecio = (((PorcionOp * porcionC) * Convert.ToDouble(reader["precio"].ToString())) / totalesprod) * itemDetalleComp.Precio * PorcionOp * porcionC;
                                        }

                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", proPrecio));
                                        Venta.Parameters.Add(new SqlParameter("@pe_tipo_detalle", 2));
                                        Venta.ExecuteNonQuery();

                                        if (Convert.ToInt32(reader["Tip_Clas"].ToString()) != 3 && Convert.ToInt32(reader["Tip_Clas"].ToString()) != 4)
                                        {


                                            double costo = 0;                                     
                                            //costo = conexion.Dato("seleCt dbo.costosproductos (" + Convert.ToInt32(reader["cod_prod"].ToString()) + "," + idAlmacen + ")");

                                            Venta = new SqlCommand()
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandText = "seleCt dbo.costosproductos (" + Convert.ToInt32(reader["cod_prod"].ToString()) + "," + idAlmacen + ")",
                                                CommandType = CommandType.Text
                                            };
                                            costo = Convert.ToDouble(Venta.ExecuteScalar());


                                            Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(reader["cod_prod"].ToString())));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_salida", PorcionOp * porcionC));
                                            Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                            Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                            Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                            Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                            Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                            Venta.ExecuteNonQuery();




                                            Venta = new SqlCommand("pa_existencia", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(reader["cod_prod"].ToString())));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", PorcionOp * porcionC));
                                            Venta.ExecuteNonQuery();
                                        }
                                    }

                                }

                            }



                            if (itemDetalle.Nuevo == 1)
                            {

                                ////// Obligatorios 2 codPrincipal ////////                      

                                double porcion = 0, porcionObli = 0, PrecioObligatorio = 0;
                                int CodProdObligatorio;

                                string consulta = "select cod_comp,cod_prod,producto,Cod_Clas,porcion,Tip_Clas,porcion, porcion_llevar,llevar,costo,precio from vi_detalle_composicion where opcional=0 and cod_pprod=" + itemDetalle.CodProd + "";

                                Venta = new SqlCommand(consulta, conn)
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann
                                };

                                sqlData = new SqlDataAdapter(Venta);
                                var data1 = new DataTable();
                                sqlData.Fill(data1);

                                foreach (DataRow row1 in data1.Rows)
                                {
                                    porcion = Convert.ToDouble(row1["porcion"]);
                                    totalesprod += ((itemDetalle.Cantidad * porcion)) * Convert.ToDouble(row1["precio"]);
                                }


                                foreach (DataRow dato in data1.Rows)
                                {
                                    Venta = new SqlCommand("PA_UIDS_Sub_Detalle", conn)
                                    {
                                        Connection = trann.Connection,
                                        Transaction = trann,
                                        CommandType = CommandType.StoredProcedure
                                    };
                                    int cc = Convert.ToInt32(dato["cod_prod"].ToString());

                                    Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_decv", DetCom));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_comp", Convert.ToInt32(dato["cod_comp"].ToString())));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", Convert.ToInt32(dato["cod_prod"].ToString())));
                                    porcion = Convert.ToDouble(dato["porcion"].ToString());
                                    Venta.Parameters.Add(new SqlParameter("@pe_cantidad", itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString())));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cant_uni", itemDetalle.Cantidad));
                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                                    Venta.Parameters.Add(new SqlParameter("@pe_opcional", false));
                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));
                                    CodProdObligatorio = Convert.ToInt32(dato["cod_prod"].ToString());
                                    PrecioObligatorio = Convert.ToDouble(dato["precio"].ToString());

                                    porcionObli = itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString());
                                    double proPrecio = 0;
                                    if (totalesprod > 0)
                                    {
                                        proPrecio = (((itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString())) * Convert.ToDouble(dato["precio"].ToString())) / totalesprod) * itemDetalle.Precio * (itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString()));
                                    }

                                    Venta.Parameters.Add(new SqlParameter("@pe_precio", proPrecio));
                                    Venta.Parameters.Add(new SqlParameter("@pe_tipo_detalle", 1));

                                    Venta.ExecuteNonQuery();

                                    if (Convert.ToInt32(dato["Tip_Clas"].ToString()) != 3 && Convert.ToInt32(dato["Tip_Clas"].ToString()) != 4 && Convert.ToInt32(dato["Tip_Clas"].ToString()) != 8)
                                    {


                                        double costo = 0;                            
                                        //costo = conexion.Dato("seleCt dbo.costosproductos (" + Convert.ToInt32(dato["cod_prod"].ToString()) + "," + idAlmacen + ")");

                                        Venta = new SqlCommand()
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandText = "seleCt dbo.costosproductos (" + Convert.ToInt32(dato["cod_prod"].ToString()) + "," + idAlmacen + ")",
                                            CommandType = CommandType.Text
                                        };
                                        costo = Convert.ToDouble(Venta.ExecuteScalar());


                                        Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(dato["cod_prod"].ToString())));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                        Venta.Parameters.Add(new SqlParameter("@pe_salida", itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString())));
                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                        Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                        Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                        Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                        Venta.ExecuteNonQuery();



                                        Venta = new SqlCommand("pa_existencia", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };
                                        Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(dato["cod_prod"].ToString())));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", itemDetalle.Cantidad * Convert.ToDouble(dato["porcion"].ToString())));
                                        Venta.ExecuteNonQuery();
                                    }


                                    ////////Composicion dentro de otra composicion//////////

                                    double totalcostosub = 0, porcionC = 0;

                                    Venta2 = new SqlCommand("select cod_comp,cod_prod,producto,Cod_Clas,porcion,Tip_Clas,porcion, porcion_llevar,llevar,costo,precio from vi_detalle_composicion where opcional=0 and cod_pprod=" + CodProdObligatorio + "", conn)
                                    {
                                        Connection = trann.Connection,
                                        Transaction = trann,
                                    };

                                    sqlData = new SqlDataAdapter(Venta2);
                                    var data2 = new DataTable();
                                    sqlData.Fill(data2);

                                    foreach (DataRow dato2 in data2.Rows)
                                    {
                                        porcionC = Convert.ToDouble(dato2["porcion"].ToString());

                                        totalesprod += ((porcionObli * porcionC)) * Convert.ToDouble(dato2["precio"].ToString());
                                        totalcostosub += ((porcionObli * porcionC) * Convert.ToDouble(dato2["costo"].ToString()));
                                    }

                                    foreach (DataRow dato4 in data2.Rows)
                                    {
                                        Venta = new SqlCommand("PA_UIDS_Sub_Detalle", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };

                                        Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_decv", DetCom));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_comp", Convert.ToInt32(dato4["cod_comp"].ToString())));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", Convert.ToInt32(dato4["cod_prod"].ToString())));
                                        porcionC = Convert.ToDouble(dato4["porcion"].ToString());

                                        Venta.Parameters.Add(new SqlParameter("@pe_cantidad", porcionObli * porcionC));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cant_uni", porcionObli));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_opcional", false));
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", idAlmacen));


                                        double proPrecioC = 0;
                                        if (totalesprod > 0)
                                        {
                                            proPrecioC = (((porcionObli * porcionC) * Convert.ToDouble(dato4["precio"].ToString())) / totalesprod) * PrecioObligatorio * (porcionObli * porcionC);
                                        }

                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", proPrecioC));
                                        Venta.Parameters.Add(new SqlParameter("@pe_tipo_detalle", 2));
                                        Venta.ExecuteNonQuery();

                                        if (Convert.ToInt32(dato4["Tip_Clas"].ToString()) != 3 && Convert.ToInt32(dato4["Tip_Clas"].ToString()) != 4)
                                        {


                                            double costo = 0;                                         
                                            //costo = conexion.Dato("seleCt dbo.costosproductos (" + Convert.ToInt32(dato4["cod_prod"].ToString()) + "," + idAlmacen + ")");

                                            Venta = new SqlCommand()
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandText = "seleCt dbo.costosproductos (" + Convert.ToInt32(dato4["cod_prod"].ToString()) + "," + idAlmacen + ")",
                                                CommandType = CommandType.Text
                                            };
                                            costo = Convert.ToDouble(Venta.ExecuteScalar());


                                            Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(dato4["cod_prod"].ToString())));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_salida", porcionObli * porcionC));
                                            Venta.Parameters.Add(new SqlParameter("@pe_precio", costo));
                                            Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                            Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                            Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                            Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                            Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                            Venta.ExecuteNonQuery();



                                            Venta = new SqlCommand("pa_existencia", conn)
                                            {
                                                Connection = trann.Connection,
                                                Transaction = trann,
                                                CommandType = CommandType.StoredProcedure
                                            };
                                            Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(dato4["cod_prod"].ToString())));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                            Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", porcionObli * porcionC));
                                            Venta.ExecuteNonQuery();
                                        }
                                    }

                                }

                            }

                        }

                    }



                    //////PRocesoEliminacionProducto

                    if (ProductoEliminado != null)
                    {
                        int codUsuaDetalle = 0;
                        string LoginDetalle = "";
                        foreach (var itemProdEliminado in ProductoEliminado)
                        {
                            codUsuaDetalle = itemProdEliminado.CoduserDetalle;
                            LoginDetalle = itemProdEliminado.LoginDetalle;
                            if (LoginDetalle == null)
                            {
                                LoginDetalle = item.Usuario;
                            }

                            Venta = new SqlCommand("PA_UIDS_Detalle_Venta", conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandType = CommandType.StoredProcedure
                            };

                            Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "DM"));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Decv", itemProdEliminado.codDecv));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", itemProdEliminado.CoduserDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_observaciones", itemProdEliminado.observacion));
                            Venta.Parameters.Add(new SqlParameter("@venta_c", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Vent", CodVent));
                            Venta.Parameters.Add(new SqlParameter("@mesa", item.descripcionMesa));
                            Venta.ExecuteNonQuery();


                            Venta = new SqlCommand("PA_UIDS_ProductoAnulado", conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandType = CommandType.StoredProcedure
                            };

                            Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", itemProdEliminado.codProd));
                            Venta.Parameters.Add(new SqlParameter("@pe_CodVent", CodVent));
                            Venta.Parameters.Add(new SqlParameter("@pe_Mesero", LoginDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_Mesa", item.descripcionMesa));
                            Venta.Parameters.Add(new SqlParameter("@pe_nombre", itemProdEliminado.NombProdAux));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", itemProdEliminado.CantAux));
                            Venta.Parameters.Add(new SqlParameter("@pe_impreso", 0));
                            Venta.ExecuteNonQuery();

                        }

                        int contCuenta = 0, codCuentaMesero = 0, cod_venta_mesero = 0;
                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            CommandText = "select count(*) from cuentastemporales where estado = 1 and tipo=1 and cod_alma=" + item.CodAlma + " and cod_usua=" + codUsuaDetalle,
                            CommandType = CommandType.Text
                        };
                        contCuenta = Convert.ToInt32(Venta.ExecuteScalar());

                        if (contCuenta <= 0)
                        {

                            Venta = new SqlCommand("PA_UIDS_CuentasTemporales", conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandType = CommandType.StoredProcedure
                            };

                            Venta.Parameters.Add(new SqlParameter("@pe_tipo_op", "I"));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_cuen", 0));
                            Venta.Parameters.Add(new SqlParameter("@pe_descripcion", LoginDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_vent_temp", "-1"));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuaDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_mesero", LoginDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_alma", item.CodAlma));
                            Venta.Parameters.Add(new SqlParameter("@pe_tipo", 1));
                            codCuentaMesero = Convert.ToInt32(Venta.ExecuteScalar());

                        }
                        else
                        {
                            Venta = new SqlCommand()
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandText = "select cod_cuen from cuentastemporales where tipo = 1 and estado = 1 and cod_usua = " + codUsuaDetalle + " and cod_alma=" + item.CodAlma,
                                CommandType = CommandType.Text
                            };
                            codCuentaMesero = Convert.ToInt32(Venta.ExecuteScalar());
                        }

                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            CommandText = "select cod_vent_temp from cuentastemporales where tipo = 1 and estado = 1 and cod_usua = " + codUsuaDetalle + " and cod_alma=" + item.CodAlma,
                            CommandType = CommandType.Text
                        };
                        cod_venta_mesero = Convert.ToInt32(Venta.ExecuteScalar());

                        int cod_Vent = 0;
                        if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                        {
                            Venta = new SqlCommand("PA_UIDS_Venta", conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandType = CommandType.StoredProcedure
                            };
                            Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Vent", -1));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Clie", item.CodClie));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Sub_Clie", "0"));
                            Venta.Parameters.Add(new SqlParameter("@pe_descripcion", "REVISAR CUENTAS MESEROS"));
                            Venta.Parameters.Add(new SqlParameter("@pe_Monto", item.Monto));
                            Venta.Parameters.Add(new SqlParameter("@pe_Descuento", item.Descuento));
                            Venta.Parameters.Add(new SqlParameter("@pe_Tip_vent", 2));
                            Venta.Parameters.Add(new SqlParameter("@pe_Tip_mone", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_Tip_esta", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_recargo", "0"));
                            Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", item.CodAlma));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_bolsin", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuaDetalle));
                            Venta.Parameters.Add(new SqlParameter("@pe_factura", "0"));
                            Venta.Parameters.Add(new SqlParameter("@pe_efectivo", item.Efectivo));
                            Venta.Parameters.Add(new SqlParameter("@pe_tipo", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_tipo_venta", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_caja", 1));
                            Venta.Parameters.Add(new SqlParameter("@pe_nro_pedido", Convert.ToInt32(0)));
                            Venta.Parameters.Add(new SqlParameter("@pe_motivo_descuento", ""));
                            Venta.Parameters.Add(new SqlParameter("@cod_mesero", codUsuaDetalle));
                            Venta.Parameters.Add(new SqlParameter("@mesa", item.descripcionMesa));
                            Venta.Parameters.Add(new SqlParameter("@cod_cuen", Convert.ToInt32(0)));
                            Venta.Parameters.Add(new SqlParameter("@pe_cod_pedi_venta", Convert.ToInt32(1)));

                            if (item.CodMesa == 0)
                            {
                                Venta.Parameters.Add(new SqlParameter("@pe_mesa_abierta", Convert.ToInt32(0)));
                            }
                            else
                            {
                                Venta.Parameters.Add(new SqlParameter("@pe_mesa_abierta", Convert.ToInt32(1)));
                            }

                            Venta.Parameters.Add(new SqlParameter("@pe_Pedido_Web", Convert.ToInt32(1)));
                            cod_Vent = Convert.ToInt32(Venta.ExecuteScalar());
                        }

                        string consultaCuentaTemp = "";
                        if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                        {
                            consultaCuentaTemp = "update cuentastemporales set cod_vent_temp = " + cod_Vent + " where cod_cuen = " + codCuentaMesero;
                        }
                        else
                        {
                            consultaCuentaTemp = "update cuentastemporales set cod_vent_temp = " + cod_venta_mesero + " where cod_cuen = " + codCuentaMesero;
                        }

                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            CommandText = consultaCuentaTemp,
                            CommandType = CommandType.Text
                        };
                        Venta.ExecuteNonQuery();

                        int nuevoDetalle = 0;
                        string consultaSubDetalle = "", UpdateVenta = "";
                        foreach (var ProdEliminacion in ProductoEliminado)
                        {

                            Venta = new SqlCommand("Select * from detalle_venta where cod_decv=" + ProdEliminacion.codDecv, conn)
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                            };
                            sqlData = new SqlDataAdapter(Venta);
                            var data2 = new DataTable();
                            sqlData.Fill(data2);
                            foreach (DataRow row1 in data2.Rows)
                            {

                                int tipo_clasi = Convert.ToInt32(conexion.Consulta("Select c.tip_clas from producto p,clasificacion_productos c where p.cod_clas=c.cod_clas and p.cod_prod=" + Convert.ToInt32(row1["cod_prod"])));
                                if (tipo_clasi != 4 && tipo_clasi != 9 && tipo_clasi != 3)
                                {

                                    /////// 18/07/25

                                    //Venta = new SqlCommand("pa_existencia", conn)
                                    //{
                                    //    Connection = trann.Connection,
                                    //    Transaction = trann,
                                    //    CommandType = CommandType.StoredProcedure
                                    //};
                                    //Venta.Parameters.Add(new SqlParameter("@pe_tipo", "I"));
                                    //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(row1["cod_prod"])));
                                    //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                    //Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", Convert.ToDouble(row1["cantidad"])));
                                    //Venta.ExecuteNonQuery();

                                    ////////////

                                    Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                    {
                                        Connection = trann.Connection,
                                        Transaction = trann,
                                        CommandType = CommandType.StoredProcedure
                                    };
                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(row1["cod_prod"])));
                                    Venta.Parameters.AddWithValue("@pe_cod_alma_orig", 0);
                                    Venta.Parameters.Add(new SqlParameter("@pe_entrada", Convert.ToDouble(row1["cantidad"])));
                                    Venta.Parameters.Add(new SqlParameter("@pe_precio", Convert.ToDouble(row1["costo"])));
                                    Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                    Venta.Parameters.Add(new SqlParameter("@pe_accion", "DE"));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_dest", idAlmacen));
                                    Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                    Venta.ExecuteNonQuery();


                                    Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                    {
                                        Connection = trann.Connection,
                                        Transaction = trann,
                                        CommandType = CommandType.StoredProcedure
                                    };
                                    Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(row1["cod_prod"])));
                                    Venta.Parameters.AddWithValue("@pe_cod_alma_orig", idAlmacen);
                                    Venta.Parameters.Add(new SqlParameter("@pe_salida", Convert.ToDouble(row1["cantidad"])));
                                    Venta.Parameters.Add(new SqlParameter("@pe_precio", Convert.ToDouble(row1["costo"])));
                                    int numReg = 0;
                                    if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                                    {
                                        numReg = cod_Vent;
                                    }
                                    else
                                    {
                                        numReg = cod_venta_mesero;
                                    }
                                    Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", numReg));
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                    Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                    Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                    Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                    Venta.Parameters.Add(new SqlParameter("@pe_variable", true));
                                    Venta.ExecuteNonQuery();

                                }


                                Venta = new SqlCommand("PA_UIDS_Detalle_Venta", conn)
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann,
                                    CommandType = CommandType.StoredProcedure
                                };
                                Venta.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
                                if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                                {
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_vent", cod_Vent));
                                }
                                else
                                {
                                    Venta.Parameters.Add(new SqlParameter("@pe_cod_vent", cod_venta_mesero));
                                }
                                Venta.Parameters.Add(new SqlParameter("@pe_cod_prod", Convert.ToInt32(row1["cod_prod"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_cantidad", Convert.ToDouble(row1["cantidad"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_precio", Convert.ToDouble(row1["precio"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_descuento", "0"));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Unid", Convert.ToInt32(row1["cod_unid"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_Usua", codUsuaDetalle));
                                Venta.Parameters.Add(new SqlParameter("@pe_observaciones", (string)row1["observaciones"]));
                                Venta.Parameters.Add(new SqlParameter("@pe_Cod_alma", Convert.ToInt32(row1["cod_alma"])));
                                Venta.Parameters.Add(new SqlParameter("@venta_c", Convert.ToInt32(0)));
                                Venta.Parameters.Add(new SqlParameter("@pe_cod_prec_esp", Convert.ToInt32(row1["cod_prec_esp"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_decv_elim", Convert.ToInt32(row1["cod_decv"])));
                                Venta.Parameters.Add(new SqlParameter("@pe_Pedido_Web", Convert.ToInt32(1)));
                                nuevoDetalle = Convert.ToInt32(Venta.ExecuteScalar());

                                /// 18/07/25
                                //Venta = new SqlCommand("pa_existencia", conn)
                                //{
                                //    Connection = trann.Connection,
                                //    Transaction = trann,
                                //    CommandType = CommandType.StoredProcedure
                                //};
                                //Venta.Parameters.Add(new SqlParameter("@pe_tipo", "S"));
                                //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(row1["cod_prod"].ToString())));
                                //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", Convert.ToInt32(row1["cod_alma"])));
                                //Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", Convert.ToDouble(row1["cantidad"])));
                                //Venta.ExecuteNonQuery();
                                //////


                                Venta = new SqlCommand("Select * from sub_detalle where cod_decv=" + ProdEliminacion.codDecv, conn)
                                {
                                    Connection = trann.Connection,
                                    Transaction = trann,
                                };
                                sqlData = new SqlDataAdapter(Venta);
                                data2 = new DataTable();
                                sqlData.Fill(data2);

                                foreach (DataRow sd in data2.Rows)
                                {
                                    tipo_clasi = Convert.ToInt32(conexion.Consulta("Select c.tip_clas from producto p,clasificacion_productos c where p.cod_clas=c.cod_clas and p.cod_prod=" + Convert.ToInt32(sd["cod_prod"])));
                                    if (tipo_clasi != 4 && tipo_clasi != 9 && tipo_clasi != 3)
                                    {
                                        Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(sd["cod_prod"])));
                                        Venta.Parameters.AddWithValue("@pe_cod_alma_orig", 0);
                                        //Venta.Parameters.Add(new SqlParameter("@pe_entrada", (double)sd["cantidad"]));
                                        Venta.Parameters.AddWithValue("@pe_entrada", Convert.ToDouble(sd["cantidad"]));
                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", Convert.ToDouble(sd["costo"])));
                                        Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", CodVent));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_accion", "DE"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_dest", idAlmacen));
                                        Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                        Venta.ExecuteNonQuery();


                                        Venta = new SqlCommand("PA_UIDS_KardexHistorico", conn)
                                        {
                                            Connection = trann.Connection,
                                            Transaction = trann,
                                            CommandType = CommandType.StoredProcedure
                                        };
                                        Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(sd["cod_prod"])));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_alma_orig", idAlmacen));
                                        Venta.Parameters.Add(new SqlParameter("@pe_salida", Convert.ToDouble(sd["cantidad"])));
                                        Venta.Parameters.Add(new SqlParameter("@pe_precio", Convert.ToDouble(sd["costo"])));
                                        int numReg = 0;
                                        if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                                        {
                                            numReg = cod_Vent;
                                        }
                                        else
                                        {
                                            numReg = cod_venta_mesero;
                                        }
                                        Venta.Parameters.Add(new SqlParameter("@pe_numero_reg", numReg));
                                        Venta.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
                                        Venta.Parameters.Add(new SqlParameter("@pe_accion", "IN"));
                                        Venta.Parameters.AddWithValue("@pe_cod_alma_dest", 0);
                                        Venta.Parameters.Add(new SqlParameter("@pe_TIPO", "VENTA"));
                                        Venta.Parameters.Add(new SqlParameter("@pe_variable", true));
                                        Venta.ExecuteNonQuery();



                                        ////////// 18/07/25

                                        //Venta = new SqlCommand("pa_existencia", conn)
                                        //{
                                        //    Connection = trann.Connection,
                                        //    Transaction = trann,
                                        //    CommandType = CommandType.StoredProcedure
                                        //};
                                        //Venta.Parameters.Add(new SqlParameter("@pe_tipo", "I"));
                                        //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Prod", Convert.ToInt32(sd["cod_prod"])));
                                        //Venta.Parameters.Add(new SqlParameter("@pe_Cod_Alma", idAlmacen));
                                        //Venta.Parameters.Add(new SqlParameter("@pe_Cantidad", Convert.ToDouble(sd["cantidad"])));
                                        //Venta.ExecuteNonQuery();

                                    }
                                }
                            }

                            Venta = new SqlCommand()
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandText = "update detalle_venta set estado = 0 where cod_decv = " + ProdEliminacion.codDecv,
                                CommandType = CommandType.Text
                            };
                            Venta.ExecuteNonQuery();


                            if (cod_venta_mesero == -1 || cod_venta_mesero == 0)
                            {
                                consultaSubDetalle = "update sub_detalle set cod_decv = " + nuevoDetalle + ", cod_usua=" + codUsuaDetalle + " where cod_decv = " + ProdEliminacion.codDecv;
                                UpdateVenta = "update venta Set Monto = (Select SUM(precio * cantidad) from detalle_venta where Cod_Vent = " + cod_Vent + ") where Cod_Vent = " + cod_Vent;
                            }
                            else
                            {
                                consultaSubDetalle = "update sub_detalle set cod_decv =" + nuevoDetalle + ", cod_usua=" + codUsuaDetalle + " where cod_decv = " + ProdEliminacion.codDecv;
                                UpdateVenta = "update venta Set Monto = (Select SUM(precio * cantidad) from detalle_venta where Cod_Vent = " + cod_venta_mesero + ") where Cod_Vent = " + cod_venta_mesero;
                            }
                            Venta = new SqlCommand()
                            {
                                Connection = trann.Connection,
                                Transaction = trann,
                                CommandText = consultaSubDetalle,
                                CommandType = CommandType.Text
                            };
                            Venta.ExecuteNonQuery();

                        }


                        Venta = new SqlCommand()
                        {
                            Connection = trann.Connection,
                            Transaction = trann,
                            CommandText = UpdateVenta,
                            CommandType = CommandType.Text
                        };
                        Venta.ExecuteNonQuery();

                    }


                    trann.Commit();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    trann.Rollback();
                    CodVent = 0;
                }

            }

            return CodVent;

        }



        public void LlenarTablaT(string consulta, string tabla, System.Data.SqlClient.SqlTransaction tranS)
        {
            try
            {
                var CMDProducto = new System.Data.SqlClient.SqlCommand();
                var DTAProducto = new System.Data.SqlClient.SqlDataAdapter();
                CMDProducto.Connection = tranS.Connection;
                CMDProducto.Transaction = tranS;
                CMDProducto.CommandText = "sp_consulta";
                CMDProducto.Parameters.AddWithValue("@consulta", consulta);
                CMDProducto.CommandType = CommandType.StoredProcedure;
                Llenar_Datos(ref DTAProducto, ref DTSDatos, CMDProducto, tabla);
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox("NO SE PUDO COMPLETAR LA CONSULTA, INTENTE DE NUEVO POR FAVOR");
            }

        }


        public void Llenar_Datos(ref System.Data.SqlClient.SqlDataAdapter DTA, ref DataSet DTS, System.Data.SqlClient.SqlCommand CMD, string Nombre, bool mens = true)
        {
            if (DTS.Tables.Contains(Nombre) == true)
            {
                DTS.Tables.Remove(Nombre);
            }

            DTA.SelectCommand = CMD;
            try
            {
                DTA.Fill(DTS, Nombre);
            }
            catch (Exception ex)
            {
                //if (mens)
                //{
                //    Interaction.MsgBox("ERROR AL CARGAR DATOS" + Constants.vbCrLf + Constants.vbCrLf + ex.Message);
                //}
            }

        }


        public void Pago(SqlTransaction trann, int tipo, double monto, double abono, DateTime fecha, int numero, int codBanco, int codVent)
        {

            SqlCommand Abono = new SqlCommand("PA_UIDS_Abonos")
            {
                Connection = trann.Connection,
                Transaction = trann,
                CommandType = CommandType.StoredProcedure
            };

            Abono.Parameters.Add(new SqlParameter("@PE_TIPO_OP", "I"));
            Abono.Parameters.Add(new SqlParameter("@pe_cod_vent", codVent));
            Abono.Parameters.Add(new SqlParameter("@pe_monto", abono));
            Abono.Parameters.Add(new SqlParameter("@pe_Fecha", fecha));
            Abono.Parameters.Add(new SqlParameter("@pe_observacion", ""));
            Abono.Parameters.Add(new SqlParameter("@pe_tipo_ftran", tipo));
            Abono.Parameters.Add(new SqlParameter("@pe_cod_usua", codUsuario));
            Abono.Parameters.Add(new SqlParameter("@pe_cod_cuen", Convert.ToInt32(0)));
            Abono.Parameters.Add(new SqlParameter("@pe_cod_caja", 1));
            Abono.Parameters.Add(new SqlParameter("@cod_ped", Convert.ToInt32(0)));
            Abono.Parameters.Add(new SqlParameter("@pe_numero", numero));
            Abono.Parameters.Add(new SqlParameter("@pe_tipo_tarj", TipoTarjeta));
            Abono.Parameters.Add(new SqlParameter("@pe_Cod_Banco", codBanco));
            Abono.ExecuteNonQuery();

            Abono = new SqlCommand()
            {
                Connection = trann.Connection,
                Transaction = trann,
                CommandText = "insert into pagos_ventas values(" + codVent + "," + tipo + "," + monto + "," + TipoTarjeta + ",'', " + nroRefTarjeta + ",1,1,0)",
                CommandType = CommandType.Text
            };
            Abono.ExecuteNonQuery();

        }


        public List<DatosCliente> ObtenerDatosDocumento(int NroDocu)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("select complemento, Nombre, razon_social, email_envio from f_vi_nombres_clientes where nro_documento='" + NroDocu + "' or nit='" + NroDocu + "' or ci='" + NroDocu + "'");
                var result = context.Query<DatosCliente>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }


        public void IMPRIMIR(int codAlma)
        {
            string aa = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\images\Xperto.png");
            string ubicacion = conexion.Consulta("select ubicacion from vi_impresoras_almacen where cod_alma=" + codAlma + "");

            try
            {
                SFE.DatosConexion.Servidor = "DESARROLLO3";
                SFE.DatosConexion.Base = "XpertoVenta";
                SFE.DatosConexion.Usuario = "cya";
                SFE.DatosConexion.Password = "guga";
                SFE.DatosConexion.cod_usua = 1;
                SFE.DatosConexion.cod_empresa = 1;
                SFE.DatosConexion.cod_rol = 1;
                SFE.Procesos.nombre_impresora = @"\\DESARROLLO3\" + ubicacion;

                SFE.Procesos.tip_conf_impre = 1;
                int codEmpr = Convert.ToInt32(conexion.Consulta("select cod_empr from F_vi_almacenes_sucursal where cod_alma=" + codAlma + ""));
                SFE.Procesos.CargarServiciosSIN(codEmpr);
                SFE.Procesos.VerFactura(9190, false, true, false, 0, "", "", false, "", false, 1, false, false, 1, false);
            }
            catch (Exception e)
            {
                throw;
            }

        }


        public void GenerarQr()
        {
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData qrCodeData = qr.CreateQrCode("system", QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            string model = Convert.ToBase64String(qrCodeImage);

        }


        public int VerificarProductoHomologado(int codProd)
        {
            bool valor = false;
            int estado = 1;
            valor = Convert.ToBoolean(conexion.Consulta("select valor from F_ConfiguracionFacturacion where dato='HABILITADO'"));

            try
            {
                if (valor)
                {
                    estado = 2;
                    int codprod = Convert.ToInt32(conexion.Consulta("select isnull(cod_prod,0) from F_vi_RelacionProdSIN where cod_prod=" + codProd + ""));
                    if (codProd > 0)
                    {
                        estado = 3;
                    }
                }
            }
            catch (Exception)
            {
                estado = 2;
            }
            return estado;
        }


        public string UbicacionImpresora(int codAlma)
        {
            string ubicacion = conexion.Consulta("select ubicacion from vi_impresoras_almacen where cod_alma=" + codAlma + "");
            return ubicacion;
        }

        public int EstadoFacturacion(int codAlma)
        {
            int estado = 0;
            bool valor = Convert.ToBoolean(conexion.Consulta("select valor from F_ConfiguracionFacturacion where dato='HABILITADO'"));
            if (valor)
            {
                estado = 1;
            }
            return estado;
        }


        public string GenerarFacturaOpcion2(int CodVent, int CodAlma)
        {
            string ubicacion = conexion.Consulta("select ubicacion from vi_impresoras_almacen where cod_alma=" + CodAlma);
            int codFact = 0;
            string observacion = "";
            Thread.Sleep(5000);
            try
            {
                codFact = Convert.ToInt32(conexion.Consulta("select cod_fact from F_FacturaSFL where cod_vent=" + CodVent + ""));
                observacion = conexion.Consulta("select observaciones from Datos_FacturaWeb where and tip_esta=3 cod_vent=" + CodVent);
            }
            catch (Exception)
            {
            }

            try
            {
                SFE.DatosConexion.Servidor = "DESARROLLO3";
                SFE.DatosConexion.Base = "XpertoVenta";
                SFE.DatosConexion.Usuario = "cya";
                SFE.DatosConexion.Password = "guga";
                SFE.DatosConexion.cod_usua = 1;
                SFE.DatosConexion.cod_empresa = 1;
                SFE.DatosConexion.cod_rol = 1;
                SFE.Procesos.nombre_impresora = @"\\DESARROLLO3\" + ubicacion;
                SFE.Procesos.tip_conf_impre = 1;
                int codEmpr = Convert.ToInt32(conexion.Consulta("select cod_empr from F_vi_almacenes_sucursal where cod_alma=" + CodAlma + ""));
                SFE.Procesos.CargarServiciosSIN(codEmpr);

            }
            catch (Exception e)
            {
            }
            return observacion;
        }


        public string GenerarFactura(int CodVent, int CodAlma, bool EstadoEnvioFact, string email, bool EstadoImprimir)
        {
            string ubicacion = conexion.Consulta("select ubicacion from vi_impresoras_almacen where cod_alma=" + CodAlma);
            int Valor = Convert.ToInt32(conexion.Consulta("select valor from F_ConfiguracionFacturacion where Dato='TIEMPO ESPERA SERVICIOS' and estado=1"));
            string empresa = conexion.Consulta("select isnull(nombre,'') from informacion_empresa ");

            string observacion = "";
            int codFact = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool variable = true;

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(Valor) && variable)
            {
                try
                {
                    codFact = Convert.ToInt32(conexion.Consulta("select top 1 cod_fact from F_FacturaSFL where cod_vent=" + CodVent + " order by 1 desc"));
                    observacion = conexion.Consulta("select observaciones from Datos_FacturaWeb where and tip_esta=3 cod_vent=" + CodVent);
                }
                catch (Exception)
                {
                }

                if (codFact > 0)
                {
                    try
                    {
                        string FechaFact, numero, NombreArchivo, Mensaje;
                        DateTime fecha = Convert.ToDateTime(conexion.Consulta("select top 1 fecha,cod_fact from F_FacturaSFL where cod_vent=" + CodVent + " order by 2 desc"));
                        numero = conexion.Consulta("select top 1 numero, cod_fact from F_FacturaSFL where cod_vent=" + CodVent + " order by 2 desc");
                        FechaFact = fecha.ToString("ddMMyyyy");
                        NombreArchivo = "Factura" + FechaFact + "_" + numero + ".pdf";

                        Mensaje = "Estimado cliente, enviamos de forma adjunta su factura electrónica. Atte: " + empresa;

                        string rutaArchivo = @"C:\Documentos\";
                        string Ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\BootCris.INI");
                        NombreArchivo = rutaArchivo + NombreArchivo;

                        StreamReader sr = new StreamReader(Ruta);
                        string servidor = sr.ReadLine();
                        string base_c = sr.ReadLine();
                        string usuario = sr.ReadLine();
                        string pasww = sr.ReadLine();
                        //sr.Peek();

                        SFE.DatosConexion.Servidor = servidor;
                        SFE.DatosConexion.Base = base_c;
                        SFE.DatosConexion.Usuario = usuario;
                        SFE.DatosConexion.Password = pasww;
                        SFE.DatosConexion.cod_usua = 1;
                        SFE.DatosConexion.cod_empresa = 1;
                        SFE.DatosConexion.cod_rol = 1;
                        SFE.Procesos.nombre_impresora = ubicacion;  //@"\\DESARROLLO3\"
                        SFE.Procesos.tip_conf_impre = 1;
                        int codEmpr = Convert.ToInt32(conexion.Consulta("select cod_empr from F_vi_almacenes_sucursal where cod_alma=" + CodAlma));
                        SFE.Procesos.CargarServiciosSIN(codEmpr);
                        if (EstadoImprimir)
                        {
                            SFE.Procesos.VerFactura(codFact, false, true, false, 0, "", "", false, "", false, 1, false, false, 1, false); //Impresion
                        }

                        variable = false;

                        if (EstadoEnvioFact)
                        {
                            SFE.Procesos.VerFactura(codFact, false, false, true, 0, "", "", false, rutaArchivo, false, 1, false, false, 1, false); //Exportar
                            EnviarCorreo(email, "", Mensaje, NombreArchivo);
                            if (File.Exists(NombreArchivo))
                            {
                                File.Delete(NombreArchivo);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        observacion = e.ToString();
                    }
                }
            }
            if (codFact == 0)
            {
                observacion = "No se pudo generar la factura";
            }
            stopwatch.Stop();
            return observacion;
        }


        public void EnviarCorreo(string email, string subjet, string body, string NombreArchivo)
        {
            string Servidor = conexion.Consulta("select valor from F_ConfiguracionFacturacion where Dato='SERVIDOR CORREO' and estado=1");
            string Puerto = conexion.Consulta("select valor from F_ConfiguracionFacturacion where Dato='PUERTO' and estado=1");
            string Remitente = conexion.Consulta("select valor from F_ConfiguracionFacturacion where Dato='REMITENTE' and estado=1");
            string Password = conexion.Consulta("select valor from F_ConfiguracionFacturacion where Dato='CONTRASEÑA' and estado=1");

            try
            {
                var message = new MailMessage();
                message.To.Add(new MailAddress(email));

                message.From = new MailAddress(Remitente);
                message.Subject = subjet;
                message.Body = string.Format(body);
                message.IsBodyHtml = true;

                Attachment archivoAdjunto = new Attachment(NombreArchivo);
                message.Attachments.Add(archivoAdjunto);

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = Remitente,
                        Password = Password
                    };
                    smtp.Credentials = credential;
                    smtp.Host = Servidor;
                    smtp.Port = Convert.ToInt32(Puerto);
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                    smtp.Dispose();
                }
                message.Dispose();
            }
            catch (Exception e)
            {

            }
        }

        public List<DetalleVenta> ObtenerDetalleVenta(int codVenta)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                //string consulta = "select v.cod_prod CodProd,v.Nombre as NombreProducto,v.Cantidad,v.Precio,(v.Cantidad*v.Precio)Subtotal,v.cod_unid TipoUnid,v.tip_clas TipClas,m.cod_mesa,v.Cod_Decv,m.descripcion descripcionMesa,v.observaciones observacion from vi_ventaDetalle v inner join mesa m on v.cod_vent=m.cod_vent" +
                //    " where v.cod_prod>0 and v.cod_vent=" + codVenta + " order by v.cod_decv";

                string consulta = "select v.cod_prod CodProd,v.Nombre as NombreProducto,v.Cantidad,v.Precio,(v.Cantidad*v.Precio)Subtotal,v.cod_unid TipoUnid,v.tip_clas TipClas,v.Cod_Decv,v.observaciones observacion,v.codUsuaDetalle from vi_ventaDetalle v " +
                " where v.cod_prod>0 and v.cod_vent=" + codVenta + " order by v.cod_decv";

                StringBuilder sql = new StringBuilder();
                sql.Append(consulta);
                var result = context.Query<DetalleVenta>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public List<DetalleVenta> ObtenerDetalleVenta222(int codVenta)
        {

            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                //string consulta = "select v.cod_prod CodProd,v.Nombre as NombreProducto,v.Cantidad,v.Precio,(v.Cantidad*v.Precio)Subtotal,v.cod_unid TipoUnid,v.tip_clas TipClas,m.cod_mesa,v.Cod_Decv,m.descripcion descripcionMesa,v.observaciones observacion from vi_ventaDetalle v inner join mesa m on v.cod_vent=m.cod_vent" +
                //    " where v.cod_prod>0 and v.cod_vent=" + codVenta + " order by v.cod_decv";

                string consulta = "select v.cod_prod CodProd,v.Nombre as NombreProducto,v.Cantidad,v.Precio,(v.Cantidad*v.Precio)Subtotal,v.cod_unid TipoUnid,v.tip_clas TipClas,v.Cod_Decv,v.observaciones observacion,v.codUsuaDetalle from vi_ventaDetalle v " +
                " where v.cod_prod>0 and v.cod_vent=" + codVenta + " order by v.cod_decv";

                StringBuilder sql = new StringBuilder();
                sql.Append(consulta);
                var result = context.Query<DetalleVenta>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

        public int OpcionPrecuenta(int codVenta)
        {
            int estado = 0;
            try
            {
                conexion.Ejecutar("update Venta set imp_precuenta=1 where cod_vent=" + codVenta);
                estado = 1;
            }
            catch (Exception)
            {
                estado = 0;
            }

            return estado;
        }

        public string DescripcionMesa(int codMesa)
        {

            string valor = conexion.Consulta("select descripcion from mesa where estado=1 and cod_mesa=" + codMesa);
            return valor;
        }

        public string ObtenerCodVenta(int codMesa, int codCuenta)
        {
            string valor = "";
            valor = conexion.Consulta("select cod_vent from mesa where cod_mesa=" + codMesa);
            if (codCuenta > 0)
            {
                valor = conexion.Consulta("select cod_vent_temp from CuentasTemporales where cod_cuen=" + codCuenta);
            }

            return valor;
        }


        public List<Producto> VerificarStockComposicion(int CodProd, int codAlmacen, int cantidad)
        {

            string consulta = "";
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            double stock = 0, porcion = 0;

            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<Producto> Lista = new List<Producto>();

            consulta = @"Select v.cod_comp,v.tipo,v.producto nombre,v.cantidad,v.cod_deco, v.cod_prod Cod_ProdComp, porcion_llevar PorcionLlevar, porcion, precio, tip_clas, llevar,v.cod_pprod,V.opcional from vi_detalle_composicion v,producto_almacen p 
            where(p.cod_alma = '0' or p.cod_alma = " + codAlmacen + ") and p.estado = 1 and v.Cod_Prod = p.cod_prod and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_inicio, 108))<= GETDATE()and convert(datetime, convert(varchar(10),GETDATE(),103) +' ' + convert(varchar(5), p.hora_fin, 108))>= GETDATE() " +
            "and v.cod_pprod=" + CodProd + " and v.opcional=0 order by 1";



            using (SqlCommand sqlCommand = new SqlCommand(consulta, con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                    Producto Dato = new Producto();
                    stock = conexion.Dato("select cantidad from existencia where cod_alma=" + codAlmacen + " and cod_prod=" + Convert.ToInt32(reader[5].ToString()));
                    porcion = cantidad * Convert.ToDouble(reader[7].ToString());
                    if (stock < porcion)
                    {
                        Dato.Cod_ProdComp = Convert.ToInt32(reader[5].ToString());
                        Lista.Add(Dato);
                    }
                }
            }
            con.Close();
            Lista = _ = Lista == null ? new List<Producto>() : Lista;
            return Lista;
        }

        public string DescripcionCuenta(int codCuenta)
        {

            string valor = conexion.Consulta("select descripcion from CuentasTemporales where cod_cuen=" + codCuenta);
            return valor;
        }

        public List<Venta> ObtenerCodigoUser(int CoduserDetalle)
        {
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            using (var context = new SqlConnection(BDRestaurant))
            {
                string consulta = "select Login Usuario,codigo from usuarios where cod_usua=" + CoduserDetalle;
                StringBuilder sql = new StringBuilder();
                sql.Append(consulta);
                var result = context.Query<Venta>(sql.ToString(), null, commandType: CommandType.Text).ToList();
                return result.ToList();
            }
        }

    }
}
