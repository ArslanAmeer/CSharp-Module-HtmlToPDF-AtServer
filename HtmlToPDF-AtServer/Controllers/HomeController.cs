using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pechkin;
using Pechkin.Synchronized;
using System.IO;
using HtmlToPDF_AtServer.Models;

namespace HtmlToPDF_AtServer.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConvertPdf()
        {
            GlobalConfig gc = new GlobalConfig();

            gc.SetMargins(new Margins(100, 100, 100, 100))
                .SetDocumentTitle("Test document")
                .SetPaperSize(PaperKind.Letter);

            IPechkin pechkin = new SynchronizedPechkin(gc);

            test newTest = new test { Id = 2, Name = "Arslan" };

            string imgPath = Server.MapPath("~/Content/") + "polar.jpg";

            var base64Image = ImageToBase64(imgPath);

            string body = string.Empty;
            StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/sample.html"));
            using (reader)
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{NAME}", newTest.Name);
            body = body.Replace("{ID}", Convert.ToString(newTest.Id));
            body = body.Replace("{SRC}", base64Image);

            byte[] pdfContent = pechkin.Convert(body);

            string directory = Server.MapPath("~/SavedPDF/");
            string filename = "clientPDF_" + newTest.Name + "_" + DateTime.Now.ToString("dd-MMM-yyyy") + ".pdf";

            if (ByteArrayToFile(directory + filename, pdfContent))
            {
                Console.WriteLine("PDF Succesfully created");
            }
            else
            {
                Console.WriteLine("Cannot create PDF");
            }

            return RedirectToAction("Index");
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                fileStream.Write(byteArray, 0, byteArray.Length);
                fileStream.Close();

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception caught in process while trying to save : {0}", exception.ToString());
            }

            return false;
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                    viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                    ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        private string ImageToBase64(string imagePath)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    image.Save(mStream, image.RawFormat);
                    byte[] imageBytes = mStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);

                    return "data:image/jpg;base64," + base64String;
                }
            }
        }
    }
}