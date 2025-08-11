using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;
using Dapper;

namespace CapaDatos
{
    public class CierreCaja_Dat
    {
        CapaDatos.Conexion conexion = new CapaDatos.Conexion();
        public List<CierreCaja> ObtenerDatosCierre(int idalmacen, string usuario)
        {
            int codusua = Convert.ToInt32(conexion.Consulta("select cod_usua from usuarios where login='" + usuario + "'"));

            double VentasDias = 0, VentaCredito = 0, AbonoCredito = 0, SaldoCredito = 0, TotalAbonosPedidos = 0, Anticipos = 0, MontoInicial = 0, VentaDia = 0, VentaPasada = 0, OtrosIngresos = 0, Gastos = 0, TotalIngresoEfectivo = 0, TotalEntregar = 0, TotalVentas = 0, TotalEfectivo = 0;
            int CodIndi = 0;
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
            var conectar = new SqlConnection(BDRestaurant);         

            DateTime fechaIni = new DateTime();
            DateTime fechaFin = DateTime.Now;
            fechaFin = fechaFin.AddMinutes(1);


            CodIndi = conectar.QueryFirst<int>("select top 1 cod_indi from inicio_dia where estado=1 and cod_caja=2 and cod_alma=" + idalmacen + " and cod_usua=" + codusua + " order by cod_indi desc");
            fechaIni = conectar.QueryFirst<DateTime>("select fecha from inicio_dia where cod_indi=" + CodIndi + "");

            MontoInicial = Convert.ToDouble(conexion.Consulta("select monto from inicio_dia where estado =1 and cod_indi=" + CodIndi + " and cod_alma=" + idalmacen + " and cod_usua=" + codusua + ""));


            VentasDias = Convert.ToDouble(conexion.Consulta("select isnull(SUM(monto), 0) from vi_ventasdias where Cod_Usua = " + codusua + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and fecha_venta >'" + fechaIni + "' and fecha_venta <= getdate() and cod_alma =" + idalmacen + ""));
            VentaCredito = Convert.ToDouble(conexion.Consulta("select isnull( sum(monto),0) as monto from venta where estado = 1  and  tip_esta = 1 and estado<>0 and fecha >= '" + fechaIni + "' and fecha < getdate() and MesaAbierta=0 and cod_usua=" + codusua + " and cod_alma = " + idalmacen + ""));
            AbonoCredito = Convert.ToDouble(conexion.Consulta("select isnull( sum(a.monto), 0) from venta v, Abonos a, Tipo_Estado_Venta tev where v.cod_vent  = a.cod_vent and v.Tip_Esta = tev.tip_esta and a.fecha >= '" + fechaIni + "' and a.fecha <= getdate() and tev.tip_esta = 1 and a.estado= 1 and a.Cod_Usua =" + codusua + " and a.cod_alma = " + idalmacen + " and v.fecha >= '" + fechaIni + "' and v.fecha <= getdate()"));
            TotalAbonosPedidos = Convert.ToDouble(conexion.Consulta("SELECT isnull(SUM (A.Monto ), 0) FROM Pedidos P, Abonos A WHERE P.cod_pedi = A.cod_Ped AND Cod_Vent =0 and  a.estado = 1 AND P.cod_alma = " + idalmacen + " AND A.Fecha  >= '" + fechaIni + "' AND A.Fecha  <= getdate() AND A.Cod_Usua =" + codusua + " "));
            Anticipos = Convert.ToDouble(conexion.Consulta("select isnull(sum(saldo),0) from vi_anticipo_cuenta where fecha>='" + fechaIni + "' and fecha< getdate() and cod_usua=" + codusua + " and cod_alma = " + idalmacen + ""));
            VentaDia = Convert.ToDouble(conexion.Consulta("select isnull(SUM(monto), 0) from vi_abonos where Cod_Usua = " + codusua + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and FechaVenta >'" + fechaIni + "' and FechaVenta <= getdate() and cod_alma_abo = " + idalmacen + " and tipo_ftran <>4"));
            VentaPasada = Convert.ToDouble(conexion.Consulta("select isnull( sum(monto), 0) from vi_abonos where cod_alma_abo =" + idalmacen + " and Cod_Usua = " + codusua + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and FechaVenta <'" + fechaIni + "'"));
            OtrosIngresos = Convert.ToDouble(conexion.Consulta("select isnull(sum(monto),0) from transacciones where estado=1 and tipo_tran=1 and cod_cuen>0 and fecha>='" + fechaIni + "' and fecha<getdate() and cod_alma=" + idalmacen + " and cod_usua=" + codusua + ""));
            Gastos = Convert.ToDouble(conexion.Consulta("select isnull(sum(monto),0) from vi_gastos where  fecha>='" + fechaIni + "' and fecha < getdate() and cod_alma=" + idalmacen + " and cod_usua=" + codusua + ""));

            TotalAbonosPedidos = TotalAbonosPedidos + Anticipos;

            SaldoCredito = VentaCredito - AbonoCredito;
            TotalVentas = VentasDias + VentaCredito;

            double AbonosEfectivo = 0, AbonosTarjeta = 0, AbonoCuentaCorriente = 0, AbonoTransferencia = 0, AbonoPagoQr = 0, Cortesia = 0, monto = 0, TotalAbonos = 0;

            SqlConnection con = new SqlConnection(BDRestaurant);
            con.Open();

            using (SqlCommand sqlCommand = new SqlCommand(@"select isnull(sum(monto),0)monto,tipo_ftran from vi_abonos where cod_alma_abo=" + idalmacen + " and cod_usua=" + codusua + " and fecha>='" + fechaIni + "' and fecha< getdate() group by tipo_ftran order by 2", con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                    monto = Convert.ToDouble(reader[0].ToString());
                    switch (Convert.ToInt32(reader[1].ToString()))
                    {
                        case 1:
                            AbonosEfectivo = monto;
                            break;
                        case 2:
                            AbonosEfectivo += monto;
                            break;
                        case 3:
                            AbonosTarjeta = monto;
                            break;
                        case 4:
                            Cortesia = monto;
                            break;
                        case 9:
                            AbonoTransferencia = monto;
                            break;
                        case 10:
                            AbonoCuentaCorriente = monto;
                            break;
                        case 13:
                            AbonoPagoQr = monto;
                            break;
                    }
                }
            }
            con.Close();

            TotalAbonos = AbonosEfectivo + AbonosTarjeta + AbonoCuentaCorriente + AbonoTransferencia;
            TotalIngresoEfectivo = MontoInicial + AbonosEfectivo + OtrosIngresos;
            TotalEntregar = TotalIngresoEfectivo + AbonosTarjeta + AbonoCuentaCorriente + AbonoTransferencia + AbonoPagoQr - Gastos;
            TotalEfectivo = MontoInicial + AbonosEfectivo + VentaPasada - Gastos;

            con.Open();
            List<CierreCaja> Lista = new List<CierreCaja>();
            CierreCaja Aux = new CierreCaja
            {
                VentasDia = VentasDias,
                Ventacredito = VentaCredito,
                AbonosEfectivo = AbonosEfectivo,
                AbonosTarjeta = AbonosTarjeta,
                Cortesia = Cortesia,
                AbonoCuentaCorriente = AbonoCuentaCorriente,
                AbonoTransferencia = AbonoTransferencia,
                AbonoPagoQr = AbonoPagoQr,
                Acuenta = AbonoCredito,
                SaldoCredito = SaldoCredito,
                AnticipoPedido = TotalAbonosPedidos,
                MontoInicial = MontoInicial,
                TotalAbono = TotalAbonos,
                Ventadia = VentaDia,
                VentaPasada = VentaPasada,
                OtrosIngresos = OtrosIngresos,
                Gastos = Gastos,
                TotalEntregar = TotalEntregar,
                TotalVenta = TotalVentas,
                TotalEfectivo = TotalEfectivo
            };
            Lista.Add(Aux);
            con.Close();
            Lista = _ = Lista ?? new List<CierreCaja>();
            return Lista;

        }


        public int GuardarCierre(List<CierreDia> Cierre)
        {
            int cod_cidi=0, codUsuario, CodIndi;
            double VentasDias = 0, cupones=0, Tc=0, TotalAbonosPedidos=0, VentaPasada=0, AbonoCredito=0, VentaCredito=0, VentaCupon=0, Ventapedido=0, pcc=0, cc=0, pccp=0, ccpasada = 0, cant_ventas=0, transferenciadia=0, transferenciaspasada=0, QRdia=0, QRpasada=0, tarjetadia=0, anticipostarj=0,
                tarjetaPasada=0, tarjetaPedido=0, abonoD=0, acf=0, asf=0;

            foreach (var Item in Cierre)
            {
                var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];
                var conectar = new SqlConnection(BDRestaurant);
                DateTime fechaIni = new DateTime();

                codUsuario = Convert.ToInt32(conexion.Consulta("select cod_usua from usuarios where login='" + Item.Usuario + "'"));

                CodIndi = conectar.QueryFirst<int>("select top 1 cod_indi from inicio_dia where estado=1 and cod_caja=2 and cod_alma=" + Item.CodAlma + " and cod_usua=" + codUsuario + " order by cod_indi desc");
                fechaIni = conectar.QueryFirst<DateTime>("select fecha from inicio_dia where cod_indi=" + CodIndi + "");

                VentasDias = conexion.Dato("select isnull(SUM(monto), 0) from vi_ventasdias where Cod_Usua = " + codUsuario + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and fecha_venta >'" + fechaIni + "' and fecha_venta <= getdate() and cod_alma =" + Item.CodAlma + "");
                cupones = conexion.Dato("select isnull(sum(monto),0) from abonos where estado=1 and (tipo_ftran=4) and Fecha > '" + fechaIni + "' and Fecha <= getdate() ");
                Tc = conexion.Dato("select isnull(oficial,0) oficial from bolsin where estado=1");
                TotalAbonosPedidos = conexion.Dato("SELECT isnull(SUM (A.Monto ), 0) FROM Pedidos P, Abonos A WHERE P.cod_pedi = A.cod_Ped AND Cod_Vent =0 and a.estado = 1 AND P.cod_alma = " + Item.CodAlma + " AND A.Fecha  >= '" + fechaIni + "' AND A.Fecha  <= getdate() AND A.Cod_Usua =" + codUsuario + " ");
                VentaPasada = conexion.Dato("select isnull( sum(monto), 0) from vi_abonos where cod_alma_abo =" + Item.CodAlma + " and Cod_Usua = " + codUsuario + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and FechaVenta <'" + fechaIni + "'");
                AbonoCredito = conexion.Dato("select isnull( sum(a.monto), 0) from venta v, Abonos a, Tipo_Estado_Venta tev where v.cod_vent  = a.cod_vent and v.Tip_Esta = tev.tip_esta and a.fecha >= '" + fechaIni + "' and a.fecha <= getdate() and tev.tip_esta = 1 and a.estado= 1 and a.Cod_Usua =" + codUsuario + " and a.cod_alma = " + Item.CodAlma + " and v.fecha >= '" + fechaIni + "' and v.fecha <= getdate()");
                VentaCredito = conexion.Dato("select isnull( sum(monto),0) as monto from venta where estado = 1  and  tip_esta = 1 and estado<>0 and fecha >= '" + fechaIni + "' and fecha < getdate() and MesaAbierta=0 and cod_usua=" + codUsuario + " and cod_alma = " + Item.CodAlma + "");

               
                VentaCupon = conexion.Dato("select isnull(sum(monto),0)monto,tipo_ftran from vi_abonos where cod_alma_abo=" + Item.CodAlma + " and cod_usua=" + codUsuario + " and fecha>='" + fechaIni + "' and fecha< getdate() and tipo_ftran=4 group by tipo_ftran order by 2");
               
                Ventapedido = conexion.Dato("select isnull( SUM(MONTO), 0) from vi_pedidos where ESTADO<>0 and fecha_ins >='" + fechaIni + "' and fecha_ins < getdate() and cod_usua=" + codUsuario + " and cod_alma=" + Item.CodAlma + "");
                pcc = conexion.Dato("select isnull( sum(a.monto), 0) from Abonos a , pedidos p where a.cod_alma = " + Item.CodAlma + " and a.estado = 1 and a.Tipo_ftran = 10 and a.cod_Ped =p.cod_pedi and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <= getdate() and a.Cod_Usua = " + codUsuario + " and (p.fecha_ins >='" + fechaIni + "' and p.fecha_ins <= getdate())");
                cc = conexion.Dato("select isnull( sum(a.monto), 0) from Abonos a , venta v where a.cod_alma = " + Item.CodAlma + " and a.estado = 1 and Tipo_ftran = 10 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha_ins >='" + fechaIni + "' and v.fecha_ins <= getdate())");
                cc += pcc;

                pccp = conexion.Dato("select isnull(  sum(a.monto), 0) from Abonos a , pedidos p where a.estado = 1 and a.Tipo_ftran = 10 and a.cod_Ped =p.cod_pedi and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and a.cod_alma = " + Item.CodAlma + " and (p.fecha_ins <'" + fechaIni + "' or p.fecha_ins > getdate())");
                ccpasada = conexion.Dato("select isnull(  sum(a.monto), 0) from Abonos a , venta v where a.estado = 1 and Tipo_ftran = 10 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and a.cod_alma = " + Item.CodAlma + " and (v.fecha_ins <'" + fechaIni + "' or v.fecha_ins > getdate())");
                ccpasada += pccp;

                cant_ventas = conexion.Dato("select COUNT(*) from venta where estado=1 and  fecha_ins > '" + fechaIni + "' and fecha_ins <= getdate() and cod_alma =" + Item.CodAlma + "and cod_usua=" + codUsuario );
                transferenciadia = conexion.Dato("select isnull( sum(a.monto), 0) from Abonos a , venta v where a.cod_alma = " + Item.CodAlma + " and a.estado = 1 and Tipo_ftran = 9 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha >='" + fechaIni + "' and v.fecha <= getdate())");
                transferenciaspasada = conexion.Dato("select isnull(  sum(a.monto), 0) from Abonos a , venta v where a.cod_alma=" + Item.CodAlma + " and a.estado = 1 and a.Tipo_ftran = 9 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha <'" + fechaIni + "' or v.fecha > getdate())");
                QRdia = conexion.Dato("select isnull( sum(a.monto), 0) from Abonos a , venta v where a.cod_alma = " + Item.CodAlma + " and a.estado = 1 and Tipo_ftran = 13 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha >='" + fechaIni + "' and v.fecha <= getdate())");
                QRpasada = conexion.Dato("select isnull(sum(a.monto), 0) from Abonos a , venta v where a.cod_alma=" + Item.CodAlma + " and a.estado = 1 and a.Tipo_ftran = 13 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha <'" + fechaIni + "' or v.fecha > getdate())");
                
                tarjetadia = conexion.Dato("select isnull(sum(a.monto), 0) from Abonos a , venta v where a.cod_alma = " + Item.CodAlma + " and a.estado = 1 and Tipo_ftran = 3 and a.tipo_tarj>0 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha >='" + fechaIni + "' and v.fecha <= getdate())");
                anticipostarj = conexion.Dato("select isnull(sum(saldo),0) from vi_anticipo_cuenta where fecha>='" + fechaIni + "' and fecha<getdate() and cod_usua=" + codUsuario + "and cod_alma = " + Item.CodAlma + "and tipo_ftran=3 ");
                tarjetadia += anticipostarj;

                tarjetaPasada = conexion.Dato("select isnull(  sum(a.monto), 0) from Abonos a , venta v where a.cod_alma=" + Item.CodAlma + " and a.estado = 1 and a.Tipo_ftran = 3 and a.tipo_tarj>0 and a.Cod_Vent = v.Cod_Vent and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and a.Cod_Usua = " + codUsuario + " and (v.fecha <'" + fechaIni + "' or v.fecha > getdate())");
                tarjetaPedido = conexion.Dato("select isnull( SUM (a.Monto ), 0) from abonos a where a.estado = 1 and a.fecha_ins >= '" + fechaIni + "' and a.fecha_ins <=getdate() and Tipo_ftran = 3 and Cod_Vent =0 and a.Cod_Usua = " + codUsuario + " and a.cod_alma = " + Item.CodAlma);
                tarjetadia += tarjetaPedido;

                abonoD = conexion.Dato("select isnull(SUM(monto), 0) from vi_ventasdias where (Tipo_ftran=1 or Tipo_ftran=2) and  Cod_Usua = " + codUsuario + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and fecha_venta >'" + fechaIni + "' and fecha_venta <= getdate() and cod_alma =" + Item.CodAlma);
                acf = conexion.Dato("select isnull(SUM(monto), 0) from vi_ventasdias where cod_vent  in(select cod_vent from factura) and (Tipo_ftran=1  or Tipo_ftran=2)  and   Cod_Usua = " + codUsuario + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and fecha_venta >'" + fechaIni + "' and fecha_venta <= getdate() and cod_alma =" + Item.CodAlma);
                asf = conexion.Dato("select isnull(SUM(monto), 0) from vi_ventasdias where cod_vent not  in(select cod_vent from factura) and (Tipo_ftran=1  or Tipo_ftran=2) and  Cod_Usua = " + codUsuario + " and Fecha > '" + fechaIni + "' and Fecha <= getdate() and fecha_venta >'" + fechaIni + "' and fecha_venta <= getdate() and cod_alma =" + Item.CodAlma);


                SqlConnection conn;
                conn = new SqlConnection(BDRestaurant);
                conn.Open();
                SqlTransaction trann;
                trann = conn.BeginTransaction();

                SqlCommand CierreDia;

                try
                {
                    CierreDia = new SqlCommand("PA_UIDS_cierre_dia", conn)
                    {
                        Connection = trann.Connection,
                        Transaction = trann,
                        CommandType = CommandType.StoredProcedure
                    };
                    CierreDia.Parameters.AddWithValue("@PE_TIPO_OP", "I");
                    CierreDia.Parameters.AddWithValue("@pe_cod_alma", Item.CodAlma);
                    CierreDia.Parameters.AddWithValue("@pe_cod_usua", codUsuario);
                    CierreDia.Parameters.AddWithValue("@pe_cod_indi", CodIndi);
                    CierreDia.Parameters.AddWithValue("@pe_ventas", VentasDias);
                    CierreDia.Parameters.AddWithValue("@pe_ingresos", Item.Ingresos);
                    CierreDia.Parameters.AddWithValue("@pe_gastos", Item.Gastos);
                    CierreDia.Parameters.AddWithValue("@pe_saldo", Item.Saldo);
                    CierreDia.Parameters.AddWithValue("@pe_entregado", Item.Entregado);
                    CierreDia.Parameters.AddWithValue("@pe_observacion", Item.Observacion);
                    CierreDia.Parameters.AddWithValue("@pe_descuentos", 0);
                    CierreDia.Parameters.AddWithValue("@pe_pagos_tarjeta", Item.PagosTarjetas);
                    CierreDia.Parameters.AddWithValue("@pe_pagos_cupones", cupones);
                    CierreDia.Parameters.AddWithValue("@pe_bolivianos", Item.Entregado);
                    CierreDia.Parameters.AddWithValue("@pe_dolares", Item.Dolares);
                    CierreDia.Parameters.AddWithValue("@pe_Cuentas", Item.Cuentas);
                    CierreDia.Parameters.AddWithValue("@pe_cupones", 0);
                    CierreDia.Parameters.AddWithValue("@pe_tc", Tc);
                    CierreDia.Parameters.AddWithValue("@abonopedido", TotalAbonosPedidos);
                    CierreDia.Parameters.AddWithValue("@ventaspsadas", VentaPasada);
                    CierreDia.Parameters.AddWithValue("@AbonoCredito", AbonoCredito);
                    CierreDia.Parameters.AddWithValue("@ventCredito", VentaCredito);
                    CierreDia.Parameters.AddWithValue("@ventaCupon", VentaCupon);
                    CierreDia.Parameters.AddWithValue("@ventapedido", Ventapedido);
                    CierreDia.Parameters.AddWithValue("@cc", cc);
                    CierreDia.Parameters.AddWithValue("@ccpasada", ccpasada);
                    CierreDia.Parameters.AddWithValue("@pe_cantidadV", cant_ventas);
                    CierreDia.Parameters.AddWithValue("@pe_transferencias", Item.Transferencias);
                    CierreDia.Parameters.AddWithValue("@pe_transferencia_dia", transferenciadia);
                    CierreDia.Parameters.AddWithValue("@pe_transferenciapasada", transferenciaspasada);
                    CierreDia.Parameters.AddWithValue("@pe_pagoqr", Item.PagoQr);
                    CierreDia.Parameters.AddWithValue("@pe_pagoqr_dia", QRdia);
                    CierreDia.Parameters.AddWithValue("@pe_pagoqrpasada", QRpasada);
                    CierreDia.Parameters.AddWithValue("@pe_tarjetas", 0);
                    CierreDia.Parameters.AddWithValue("@tarjetadia", tarjetadia);
                    CierreDia.Parameters.AddWithValue("@tarjetaspasadas", tarjetaPasada);
                    CierreDia.Parameters.AddWithValue("@tarjetapedido", tarjetaPedido);
                    CierreDia.Parameters.AddWithValue("@pe_AbonoD", abonoD);
                    CierreDia.Parameters.AddWithValue("@pe_ACF", acf);
                    CierreDia.Parameters.AddWithValue("@pe_ASF", asf);
                    CierreDia.Parameters.AddWithValue("@pe_efectivo", Item.Efectivo);

                    cod_cidi = Convert.ToInt32(CierreDia.ExecuteScalar());


                    CierreDia = new SqlCommand("pa_nuevo_inicio", conn)
                    {
                        Connection = trann.Connection,
                        Transaction = trann,                      
                        CommandType = CommandType.StoredProcedure
                    };
                    CierreDia.Parameters.AddWithValue("@pe_cod_alma", Item.CodAlma);
                    CierreDia.Parameters.AddWithValue("@pe_monto", Item.Saldo);
                    CierreDia.Parameters.AddWithValue("@pe_cod_usua", codUsuario);
                    CierreDia.Parameters.AddWithValue("@pe_cod_caja", 2);
                    CierreDia.ExecuteNonQuery();


                    CierreDia = new SqlCommand()
                    {
                        Connection = trann.Connection,
                        Transaction = trann,
                        CommandText = "update inicio_dia set cerrado=1 where cod_indi=" + CodIndi,
                        CommandType = CommandType.Text
                    };
                    CierreDia.ExecuteNonQuery();

                    trann.Commit();
                    conn.Close();

                }
                catch (Exception)
                {
                    trann.Rollback();
                    cod_cidi = 0;
                }


            }
            return cod_cidi;

        }



        public bool InicioDia(string usuario, int CodAlmacen)
        {
            bool estado=false;
            try
            {
                int codUsuario = Convert.ToInt32(conexion.Consulta("select cod_usua from usuarios where login='" + usuario + "'"));
                estado = Convert.ToBoolean(conexion.Consulta("select top 1 abierto from inicio_dia where estado=1 and cerrado=0 and cod_alma=" + CodAlmacen + " and cod_usua=" + codUsuario + " order by cod_indi desc"));

            }
            catch (Exception){
                estado = false;
            }
            return estado;
        }

        public List<DatoInicioDia> DatoInicio(string usuario, int CodAlmacen)
        {
            string consulta = "";
         
            int codUsuario = Convert.ToInt32(conexion.Consulta("select cod_usua from usuarios where login='" + usuario + "'"));
            var BDRestaurant = ConfigurationManager.AppSettings["restaurant"];               

            SqlConnection con;
            con = new SqlConnection(BDRestaurant);
            con.Open();
            List<DatoInicioDia> Lista = new List<DatoInicioDia>();

            consulta = @"select top 1 (CONVERT(varchar(20),fecha,103)+' '+ CONVERT(varchar(5),fecha,108)) fecha, (CONVERT(varchar(20),fecha_apertura,103)+' '+ CONVERT(varchar(5),fecha_apertura,108)) FechaApertura, abierto, cod_indi, monto from inicio_dia where estado=1 and cod_alma=" + CodAlmacen + " and cod_usua=" + codUsuario + " order by cod_indi desc";

            using (SqlCommand sqlCommand = new SqlCommand(consulta, con))
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                while (reader.Read())
                {
                    DatoInicioDia Dato = new DatoInicioDia
                    {
                        FechaInicio = reader[0].ToString(),
                        FechaApertura = reader[1].ToString(),
                        Abierto= Convert.ToBoolean(reader[2].ToString()),
                        cod_indi = Convert.ToInt32(reader[3].ToString()),
                        Monto = Convert.ToDouble(reader[4].ToString()),
                    };
                    Lista.Add(Dato);
                }
            }
            con.Close();
            Lista = _ = Lista ?? new List<DatoInicioDia>();
            return Lista;

        }

        public int GuardarInicioDia(double monto, int codIndi)
        {
            int estado = 1;
            try
            {
                conexion.Ejecutar("update inicio_dia set monto=" + monto + ", abierto=1, fecha_apertura=getdate() where cod_indi=" + codIndi + "");
            }
            catch (Exception)
            {
                estado = 0;                
            }
            return estado;
        }

    }
}
