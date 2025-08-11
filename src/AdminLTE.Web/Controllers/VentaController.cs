using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using CapaEntidades;
using QRCoder;
using System.Drawing;


using Microsoft.Reporting.WebForms;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing.Printing;
using System.Drawing.Imaging;

namespace AdminLTE.Web.Controllers
{
    public class VentaController : Controller
    {
        CapaDatos.Conexion funcion = new CapaDatos.Conexion();
        CapaNegocio.Producto_cn Producto = new CapaNegocio.Producto_cn();

        CapaNegocio.Venta_BL Venta = new CapaNegocio.Venta_BL();
        CapaEntidades.Venta VentaEntidad = new CapaEntidades.Venta();
        CapaNegocio.DetalleMesa_CN DetalleMesa = new CapaNegocio.DetalleMesa_CN();

        // GET: Venta
        public ActionResult Index(int m, int c)
        {
            if (funcion.Verificar())
            {
                //ViewBag.ListaCliente = Producto.ObtenerCliente();
                //ViewBag.ListaBanco = Venta.ObtenerBanco();
                //ViewBag.ListaEmpresaDelivery = Venta.ObtenerEmpresaDelivery();
                //ViewBag.ListaTipoTarjeta = Venta.ObtenerTipoTarjeta();
                //ViewBag.TipoDocumentoFact = Venta.ObtenerTipoDocumentoFac();
              
                if (c>0)
                {
                    m = 0;
                }
                ViewBag.tc = Venta.Tc();
                ViewBag.EstadoFact = Venta.EstadoFacturacion(Convert.ToInt32(Session["idalmacen"].ToString()));

                ViewBag.CodVenta = Venta.ObtenerCodVenta(m,c);  //id
                ViewBag.CodMesa = m;
                ViewBag.DescripcionMesa = Venta.DescripcionMesa(m);
                ViewBag.DescripcionCuenta = Venta.DescripcionCuenta(c);
                ViewBag.codCuenta = c;
                ViewBag.UsoCodigoUsuario = DetalleMesa.UsoCodigoUsuario();
                ViewBag.ListaClasificacion = Producto.ObtenerClasificacion(1);
                return View("~/Views/Pedido.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
          
        }

        public ActionResult Verificar()
        {
            ViewBag.Estado = 1;
            return View("~/Views/Inicio.cshtml");
        }

        public JsonResult VerificarStock(int codProd, int codAlmacen)
        {
            double Stock = Venta.VerificarStock(codProd, codAlmacen);
            return (new JsonResult { Data = new { data = Stock } });
        }
        public JsonResult ObtenerProducto(string producto)
        {
            string productos = Venta.ObtenerProducto(producto);
            return new JsonResult { Data = new { dato = productos } };
        }

        public JsonResult GuardarVenta(List<Venta> DatoVenta, List<DetalleVenta> DetalleVenta, List<DetalleComposicion> DetalleComposicion, List<ProductoEliminado> ProductoEliminado)
        {
            //string dato = Request.Form.Get("Venta");
            //VentaEntidad = JsonConvert.DeserializeObject<Venta>(dato);

           int codVent = Venta.GuardarVenta(DatoVenta, DetalleVenta, DetalleComposicion, ProductoEliminado);
            return new JsonResult { Data = new { dato = codVent } };
        }

        public JsonResult DatosCliente(int nroDocu)
        {
            return Json(Venta.ObtenerDatosDocumento(nroDocu));
        }

        public JsonResult VerificarProductoHomologado(int CodProducto)
        {
           int estado = Venta.VerificarProdHomologado(CodProducto);
            return new JsonResult { Data = new { dato = estado} };
        }


        [HttpPost]
        public ActionResult Qr1()
        {
            QRCodeGenerator qr = new QRCodeGenerator();
            QRCodeData qrCodeData = qr.CreateQrCode("system", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            ImageConverter convert = new ImageConverter();
            byte[] qrbyte = (byte[])convert.ConvertTo(qrCodeImage, typeof(byte[]));
            ViewBag.url = "data:image/png;base64," + Convert.ToBase64String(qrbyte);
            return View("~/Views/VentaRapida.cshtml");
            //return View("~/Views/VentaRapida.cshtml", qrbyte);


            //PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeImage = qrCode.GetGraphic(20);
            //string model = Convert.ToBase64String(qrCodeImage);
            //return View("~/Views/VentaRapida.cshtml", model);

        }

        public ActionResult ReporteNotaVenta(int codventa)
        {
            string EstadoObser = "";
            int codAlma = Convert.ToInt32(Session["idalmacen"].ToString());
            string ubicacionImpr = Venta.UbicacionImpresora(codAlma);

            string id = "Pdf";
            LocalReport lr = new LocalReport();
            string path = "";
            DataSet ds = new DataSet();
            DataSet dsAbono = new DataSet();

            path = Path.Combine(Server.MapPath("~/Informe"), "RptNotaVenta.rdlc");
            lr.ReportPath = path;

            ds = VentaDetalle(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables["tbventa"]));

            dsAbono = AbonoDetalle(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet2", dsAbono.Tables["tbabono"]));

            ds = Totales(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet3", ds.Tables["tbtotal"]));            

            //ReportParameter[] reportParameters = new ReportParameter[1];  Parametros
            //reportParameters[0] = new ReportParameter("TotalGeneral", "25", false);
            //lr.SetParameters(reportParameters);

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <EmbedFonts>None</EmbedFonts>
                   </DeviceInfo>";
            //"<DeviceInfo>" +         
            //" <OutputFormat>" + id + "</OutputFormat>" +
            //" <PageWidth>8.5in</PageWidth>" +
            //" <PageHeight>11in</PageHeight>" +
            //" <MarginTop>0in</MarginTop>" +
            //" <MarginLeft>0in</MarginLeft>" +
            //" <MarginRight>0in</MarginRight>" +
            //" <MarginBottom>0.0in</MarginBottom>" +
            //"</DeviceInfo>";
            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes = lr.Render(
            reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

            ////Imprimir////

            Warning[] warnings2;
            string[] streamIds;
            string mimeType2 = string.Empty;
            string encoding2 = string.Empty;
            string extension2 = string.Empty;
          
            try
            {
                byte[] bytes = lr.Render("Image", deviceInfo, out mimeType2, out encoding2, out extension2, out streamIds, out warnings2);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (PrintDocument printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = ubicacionImpr;
                        printDoc.PrintPage += (s, e) =>
                        {
                            Metafile pageImage = new Metafile(ms);
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        };
                        printDoc.Print(); 
                    }
                }
                ReporteComanda(codventa);
            }
            catch (Exception e)
            {
                EstadoObser = "Impresora no válida";
            }


            return new JsonResult { Data = new { dato = EstadoObser } };
            //return File(renderedBytes, mimeType);

            //var deviceInfo = @"<DeviceInfo>
            //        <EmbedFonts>None</EmbedFonts>
            //       </DeviceInfo>";
            //byte[] bytes = rdlc.Render("PDF", deviceInfo);d
        }


        public void ReporteComanda(int codventa)
        {
            int codAlma = Convert.ToInt32(Session["idalmacen"].ToString());
            string ubicacionImpr = Venta.UbicacionImpresora(codAlma);

            string id = "Pdf";
            LocalReport lr = new LocalReport();
            string path = "";
            DataSet ds = new DataSet();
            DataSet dsDetalleComp = new DataSet();

            path = Path.Combine(Server.MapPath("~/Informe"), "RptComanda.rdlc");
            lr.ReportPath = path;

            ds = VentaDetalle(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables["tbventa"]));

            dsDetalleComp = DsDetalleComposicion(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet2", dsDetalleComp.Tables["tbComposicion"]));

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <EmbedFonts>None</EmbedFonts>
                   </DeviceInfo>";
            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes = lr.Render(
            reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

            ////Imprimir////

            Warning[] warnings2;
            string[] streamIds;
            string mimeType2 = string.Empty;
            string encoding2 = string.Empty;
            string extension2 = string.Empty;

            try
            {
                byte[] bytes = lr.Render("Image", deviceInfo, out mimeType2, out encoding2, out extension2, out streamIds, out warnings2);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (PrintDocument printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = ubicacionImpr;
                        printDoc.PrintPage += (s, e) =>
                        {
                            Metafile pageImage = new Metafile(ms);
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        };
                        printDoc.Print();
                    }
                }
            }
            catch (Exception e)
            {
               
            }           
        }


        public ActionResult ReporteComandaPrueba(int codventa)
        {
            int codAlma = Convert.ToInt32(Session["idalmacen"].ToString());
            string ubicacionImpr = Venta.UbicacionImpresora(codAlma);

            string id = "Pdf";
            LocalReport lr = new LocalReport();
            string path = "";
            DataSet ds = new DataSet();
            DataSet dsDetalleComp = new DataSet();

            path = Path.Combine(Server.MapPath("~/Informe"), "RptComanda.rdlc");
            lr.ReportPath = path;

            ds = VentaDetalle(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables["tbventa"]));

            dsDetalleComp = DsDetalleComposicion(codventa);
            lr.DataSources.Add(new ReportDataSource("DataSet2", dsDetalleComp.Tables["tbComposicion"]));

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <EmbedFonts>None</EmbedFonts>
                   </DeviceInfo>";
            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes = lr.Render(
            reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);


            Warning[] warnings2;
            string[] streamIds;
            string mimeType2 = string.Empty;
            string encoding2 = string.Empty;
            string extension2 = string.Empty;

            try
            {
                byte[] bytes = lr.Render("Image", deviceInfo, out mimeType2, out encoding2, out extension2, out streamIds, out warnings2);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (PrintDocument printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = ubicacionImpr;
                        printDoc.PrintPage += (s, e) =>
                        {
                            Metafile pageImage = new Metafile(ms);
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        };
                        printDoc.Print();
                    }
                }
            }
            catch (Exception)
            {

            }

            return File(renderedBytes, mimeType);
        }

        private DataSet VentaDetalle(int id)
        {
            var cadenaConexion = ConfigurationManager.AppSettings["restaurant"];
            SqlConnection con = new SqlConnection(cadenaConexion);
            con.Open();

            SqlCommand cmd = new SqlCommand("select *,dbo.NumeroLiteral(totalVenta,' BS')MontoLiteral from Vi_VentaDetalle where cod_vent=" + id + "");
            using (con)
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "tbventa");
                    return (ds);
                }
            }
            con.Close();
        }

        private DataSet AbonoDetalle(int id)
        {
            var cadenaConexion = ConfigurationManager.AppSettings["restaurant"];
            SqlConnection con = new SqlConnection(cadenaConexion);
            con.Open();

            //string consulta = "select a.Cod_Vent,t.Descripcion,a.Monto from Abonos a inner join Tipo_Pago t on a.Tipo_ftran = t.Tip_Pago where a.cod_vent = " + id + "";
           
            string consulta = @"SELECT dbo.Pagos_ventas.cod_vent, dbo.Tipo_Pago.Descripcion, SUM( dbo.Pagos_ventas.monto) AS Monto
                        FROM dbo.Pagos_ventas INNER JOIN
                      dbo.Tipo_Pago ON dbo.Pagos_ventas.tipo_pago = dbo.Tipo_Pago.Tip_Pago INNER JOIN
                      dbo.Bolsin ON dbo.Pagos_ventas.cod_bols = dbo.Bolsin.cod_bolsin LEFT OUTER JOIN
                      dbo.Tipo_Tarjeta ON dbo.Pagos_ventas.tipo_tarj = dbo.Tipo_Tarjeta.tipo_tarj
                        WHERE(dbo.Pagos_ventas.estado = 1) AND(dbo.Pagos_ventas.cod_vent = " + id + ")" +
                        "GROUP BY dbo.Pagos_ventas.cod_vent, dbo.Pagos_ventas.tipo_pago, dbo.Tipo_Pago.Descripcion, dbo.Pagos_ventas.tipo_tarj, dbo.Pagos_ventas.observaciones, dbo.Pagos_ventas.numero, dbo.Bolsin.Oficial , dbo.Tipo_Pago.Descripcion, dbo.Tipo_Tarjeta.descripcion";

            SqlCommand cmd = new SqlCommand(consulta);
            using (con)
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "tbabono");
                    return (ds);
                }
            }
            con.Close();
        }

        private DataSet Totales(int id)
        {
            var cadenaConexion = ConfigurationManager.AppSettings["restaurant"];
            SqlConnection con = new SqlConnection(cadenaConexion);
            con.Open();

            SqlCommand cmd = new SqlCommand(@"select 'TOTAL BS' descripcion, SUM(total_producto)monto,1 from Vi_VentaDetalle 
            where Cod_Vent=" + id + " union select 'DESCUENTO' descripcion, Descuento monto,2 from Venta where Cod_Vent=" + id + " and Descuento>0  union  select 'TOTAL GENERAL' descripcion,(Monto-Descuento)monto,3 from Venta where Cod_Vent=" + id + " and Descuento>0 order by 3 ");
            using (con)
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "tbtotal");
                    return (ds);
                }
            }
            con.Close();
        }

        private DataSet DsDetalleComposicion(int id)
        {
            var cadenaConexion = ConfigurationManager.AppSettings["restaurant"];
            SqlConnection con = new SqlConnection(cadenaConexion);
            con.Open();

            SqlCommand cmd = new SqlCommand(@" select s.cant_UNI AS CANTIDAD, case when( s.tipo_detalle=2) then (Select nombre from Producto where cod_prod=c.cod_prod) + ' - ' + p.nombre else p.Nombre end as nombre, c.imprime,d.Cod_Decv ,s.tipo_detalle from Sub_Detalle s,Detalle_Venta d, venta v, 
            composicion c,Producto  p  where(p.Cod_Prod = s.cod_prod And c.cod_comp = s.cod_comp And v.Cod_Vent = d.Cod_Vent) 
            and d.Cod_Decv=s.cod_decv and c.imprime=1 and v.Cod_Vent=" + id + " order by p.nombre asc");
            using (con)
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "tbComposicion");
                    return (ds);
                }
            }
            con.Close();
        }

        public ActionResult ReporteNotaVentaPrueba(int codventa)
        { 
            string id = "Pdf";
            LocalReport lr = new LocalReport();
            string path = "";
            DataSet ds = new DataSet();
            DataSet dsAbono = new DataSet();

            path = Path.Combine(Server.MapPath("~/Informe"), "RptNotaVenta.rdlc");
            lr.ReportPath = path;

            ds = VentaDetalle(codventa);
            ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["tbventa"]);
            lr.DataSources.Add(rds);

            dsAbono = AbonoDetalle(codventa);
            ReportDataSource rds2 = new ReportDataSource("DataSet2", dsAbono.Tables["tbabono"]);
            lr.DataSources.Add(rds2);         
           
            string deviceInfo = @"<DeviceInfo>  
                    <OutputFormat>EMF</OutputFormat>
                    <EmbedFonts>None</EmbedFonts>
                   </DeviceInfo>";

            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            byte[] bytes = lr.Render("Image", deviceInfo, out mimeType, out encoding, out extension, out streamIds, out warnings);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrinterSettings.PrinterName = @"\\DESARROLLO3\Factura_des";
                    printDoc.PrintPage += (s, e) =>
                    {
                        Metafile pageImage = new Metafile(ms);
                        e.Graphics.DrawImage(pageImage, e.PageBounds);
                    };
                    printDoc.Print();
                }
            }
            return File(bytes, mimeType);
        }

        public ActionResult ImprimirReporte(int codventa)
        {
            LocalReport report = new LocalReport();
            report.ReportPath = Path.Combine(Server.MapPath("~/Informe"), "RptNotaVenta.rdlc");
            report.DataSources.Add(new ReportDataSource("DataSet1", VentaDetalle(88107)));
            report.DataSources.Add(new ReportDataSource("DataSet2", AbonoDetalle(88107)));

            string deviceInfo =
                @"<DeviceInfo>
            <OutputFormat>EMF</OutputFormat>
            <PageWidth>8.5in</PageWidth>
            <PageHeight>11in</PageHeight>
            <MarginTop>0.25in</MarginTop>
            <MarginLeft>0.25in</MarginLeft>
            <MarginRight>0.25in</MarginRight>
            <MarginBottom>0.25in</MarginBottom>
        </DeviceInfo>";

            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            byte[] bytes = report.Render("Image", deviceInfo, out mimeType, out encoding, out extension, out streamIds, out warnings);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrinterSettings.PrinterName = @"\\DESARROLLO3\Factura_des";
                    printDoc.PrintPage += (s, e) =>
                    {
                        Metafile pageImage = new Metafile(ms);
                        e.Graphics.DrawImage(pageImage, e.PageBounds);
                    };
                    printDoc.Print();
                }
            }

            return View();
        }


        public string ConvertirNumeroALetras(decimal numero)
        {
            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            string[] decenas = { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] especiales = { "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string resultado = "";

            if (numero < 0 || numero > 999999999.99m)
            {
                throw new ArgumentException("El número debe estar entre 0 y 999,999,999.99.");
            }

            if (numero == 0)
            {
                resultado = "cero";
            }
            else
            {
                int entero = (int)Math.Truncate(numero);
                int decimales = (int)Math.Round((numero - entero) * 100);
                resultado = ConvertirNumeroALetras(entero) + " con " + decimales.ToString("00") + "/100";
            }
            return resultado;
        }

        private string toText(double value)
        {
            string Num2Text = "";
            value = Math.Truncate(value);
            if (value == 0) Num2Text = "CERO";
            else if (value == 1) Num2Text = "UNO";
            else if (value == 2) Num2Text = "DOS";
            else if (value == 3) Num2Text = "TRES";
            else if (value == 4) Num2Text = "CUATRO";
            else if (value == 5) Num2Text = "CINCO";
            else if (value == 6) Num2Text = "SEIS";
            else if (value == 7) Num2Text = "SIETE";
            else if (value == 8) Num2Text = "OCHO";
            else if (value == 9) Num2Text = "NUEVE";
            else if (value == 10) Num2Text = "DIEZ";
            else if (value == 11) Num2Text = "ONCE";
            else if (value == 12) Num2Text = "DOCE";
            else if (value == 13) Num2Text = "TRECE";
            else if (value == 14) Num2Text = "CATORCE";
            else if (value == 15) Num2Text = "QUINCE";
            else if (value < 20) Num2Text = "DIECI" + toText(value - 10);
            else if (value == 20) Num2Text = "VEINTE";
            else if (value < 30) Num2Text = "VEINTI" + toText(value - 20);
            else if (value == 30) Num2Text = "TREINTA";
            else if (value == 40) Num2Text = "CUARENTA";
            else if (value == 50) Num2Text = "CINCUENTA";
            else if (value == 60) Num2Text = "SESENTA";
            else if (value == 70) Num2Text = "SETENTA";
            else if (value == 80) Num2Text = "OCHENTA";
            else if (value == 90) Num2Text = "NOVENTA";
            else if (value < 100) Num2Text = toText(Math.Truncate(value / 10) * 10) + " Y " + toText(value % 10);
            else if (value == 100) Num2Text = "CIEN";
            else if (value < 200) Num2Text = "CIENTO " + toText(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) Num2Text = toText(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) Num2Text = "QUINIENTOS";
            else if (value == 700) Num2Text = "SETECIENTOS";
            else if (value == 900) Num2Text = "NOVECIENTOS";
            else if (value < 1000) Num2Text = toText(Math.Truncate(value / 100) * 100) + " " + toText(value % 100);
            else if (value == 1000) Num2Text = "MIL";
            else if (value < 2000) Num2Text = "MIL " + toText(value % 1000);
            else if (value < 1000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0) Num2Text = Num2Text + " " + toText(value % 1000);
            }

            else if (value == 1000000) Num2Text = "UN MILLON";
            else if (value < 2000000) Num2Text = "UN MILLON " + toText(value % 1000000);
            else if (value < 1000000000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000) * 1000000);
            }

            else if (value == 1000000000000) Num2Text = "UN BILLON";
            else if (value < 2000000000000) Num2Text = "UN BILLON " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);

            else
            {
                Num2Text = toText(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }
            return Num2Text;

        }


        public void AgregarTextImprimir()
        {
            string path = @"D:\Comanda.txt";
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("COMANDA");
                sw.WriteLine("Codigo Venta:   5");
            }
            //PrintDocument pd = new PrintDocument();
            //pd.PrinterSettings.PrinterName = @"\\DESARROLLO3\Factura_des";
            //pd.DocumentName = path;
            //pd.Print();
           
        }


        public JsonResult ImprimirFactura(int CodVent, bool EstadoEnvioFact, string email, bool EstadoImprimir)
        {
            int codAlma = Convert.ToInt32(Session["idalmacen"].ToString());
            string estado = Venta.GenerarFactura(CodVent, codAlma, EstadoEnvioFact, email, EstadoImprimir);
            return new JsonResult { Data = new { dato = estado } };
        }



        public JsonResult ObtenerDetalleVenta(int codVenta)
        {
            return Json(Venta.ObtenerDetalleVenta(codVenta));
        }


        public void ReportePrecuenta(int codVenta)
        {     

            int codAlma = Convert.ToInt32(Session["idalmacen"].ToString());
            string ubicacionImpr = Venta.UbicacionImpresora(codAlma);

            string id = "Pdf";
            LocalReport lr = new LocalReport();
            string path = "";
            DataSet ds = new DataSet();
            DataSet dsDetalleComp = new DataSet();

            path = Path.Combine(Server.MapPath("~/Informe"), "RptPrecuenta.rdlc");
            lr.ReportPath = path;

            ds = DatosPrecuenta(codVenta);
            lr.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables["tbventa"]));

            string reportType = id;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <EmbedFonts>None</EmbedFonts>
                   </DeviceInfo>";
            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes = lr.Render(
            reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

            ////Imprimir////

            Warning[] warnings2;
            string[] streamIds;
            string mimeType2 = string.Empty;
            string encoding2 = string.Empty;
            string extension2 = string.Empty;

            try
            {
                byte[] bytes = lr.Render("Image", deviceInfo, out mimeType2, out encoding2, out extension2, out streamIds, out warnings2);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (PrintDocument printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = ubicacionImpr;
                        printDoc.PrintPage += (s, e) =>
                        {
                            Metafile pageImage = new Metafile(ms);
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        };
                        printDoc.Print();
                    }
                }
            }
            catch (Exception e)
            {

            }

            // return File(renderedBytes, mimeType);
        }

        private DataSet DatosPrecuenta(int id)
        {
            var cadenaConexion = ConfigurationManager.AppSettings["restaurant"];
            SqlConnection con = new SqlConnection(cadenaConexion);
            con.Open();

            SqlCommand cmd = new SqlCommand("SELECT Cod_Vent , monto,fecha, Descuento , recargo , mesa , SUM(Cantidad) as Cantidad , precio , Nombre Producto, cod_mesero , LOGIN , cliente , Nit,(Precio*sum(Cantidad))Subtotal FROM VI_PRECUENTA where refill=0 and estado = 1 and  cod_vent=" + id + "" +
                "group by Cod_Vent ,estado, monto, fecha, Descuento , recargo , mesa , precio , Nombre , cod_mesero , LOGIN , cliente , Nit");
            using (con)
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "tbventa");
                    return (ds);
                }
            }
            con.Close();
        }


        public JsonResult OpcionPrecuenta(int codVenta)
        {
            return Json(Venta.OpcionPrecuenta(codVenta));
        }

        public JsonResult StockProductoCompuesto(int codProdPrincipal, int codAlmacen, int cantidad)
        {
            return Json(Venta.StockProductoCompuesto(codProdPrincipal, codAlmacen, cantidad));
        }

        public JsonResult ObtenerCodigoUser(int CoduserDetalle)
        {
            return Json(Venta.ObtenerCodigoUser(CoduserDetalle));
        }

    }
}