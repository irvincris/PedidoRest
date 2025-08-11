using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace AdminLTE.Web.Controllers
{
    public class QRController : Controller
    {
        // GET: QR

        //[HttpPost]
        public ActionResult Index()
        {
            ViewBag.txtQRCode = "prueba";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("prueba", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            //System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            //imgBarCode.Height = 150;
            //imgBarCode.Width = 150;
            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ViewBag.imageBytes = ms.ToArray();
                    //imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                }
            }
            return View("~/Views/QR.cshtml");
        }

        //public ActionResult Index2()
        //{
        //    //QRCoder.QRCodeGenerator QR = new QRCoder.QRCodeGenerator();
        //    //ASCIIEncoding ASSCII = new ASCIIEncoding();
        //    //var z = QR.CreateQrCode(ASSCII.GetBytes(textBox1.Text), QRCoder.QRCodeGenerator.ECCLevel.H);
        //    //QRCoder.PngByteQRCode png = new QRCoder.PngByteQRCode();
        //    //png.SetQRCodeData(z);
        //    //var arr = png.GetGraphic(10);
        //    //MemoryStream ms = new MemoryStream();
        //    //ms.Write(arr, 0, arr.Length);
        //    //Bitmap b = new Bitmap(ms);
        //    //pictureBox1.Image = b;
        //}
    }
}