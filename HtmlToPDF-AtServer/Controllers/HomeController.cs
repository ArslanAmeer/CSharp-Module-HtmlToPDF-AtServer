using System;
using System.Collections.Generic;
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

        public ActionResult ConnvertPDF()
        {
            GlobalConfig gc = new GlobalConfig();

            gc.SetMargins(new Margins(100, 100, 100, 100))
                .SetDocumentTitle("Test document")
                .SetPaperSize(PaperKind.Letter);

            IPechkin pechkin = new SynchronizedPechkin(gc);

            ObjectConfig configuration = new ObjectConfig();

            test newTest = new test { Id = 2, Name = "Arslan" };

            string body = string.Empty;
            StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/sample.html"));
            using (reader)
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{NAME}", newTest.Name);
            body = body.Replace("{ID}", Convert.ToString(newTest.Id));

            //string HTML_FILEPATH = Server.MapPath("~/Views/Shared/sample.html");

            //configuration
            //    .SetAllowLocalContent(true)
            //    .SetPageUri(@"file:///" + HTML_FILEPATH);

            byte[] pdfContent = pechkin.Convert(body);

            string directory = Server.MapPath("~/SavedPDF/");
            string filename = "hello_world.pdf";

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

        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                FileStream _FileStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write);
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                Console.WriteLine("Exception caught in process while trying to save : {0}", _Exception.ToString());
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
    }
}