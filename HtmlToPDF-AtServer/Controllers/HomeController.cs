using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pechkin;
using Pechkin.Synchronized; // Installing PECHKIN.Synchronized from Nuget (Simple PECHKIN will install automatically)
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

        // Method to Convert Template HTML with Dynamic Data to PDF and Save it on Server

        public ActionResult ConvertPdf()
        {

            // Setting Up Global Configuration for PDF writer 

            GlobalConfig gc = new GlobalConfig();

            gc.SetMargins(new Margins(100, 100, 100, 100))
                .SetDocumentTitle("Test document")
                .SetPaperSize(PaperKind.Letter);


            // Initializing PECHKIN object with Global Configuration

            IPechkin pechkin = new SynchronizedPechkin(gc);

            // ANY model Object (Custom Model) with Data in It

            test newTest = new test { Id = 2, Name = "Arslan" };

            // Getting Image path from Server Folder. If any user/admin upload there image to Server
            // Path can vary according to your needs where you put your uploads.

            string imgPath = Server.MapPath("~/Content/") + "polar.jpg";

            // Method writting at Bottom to convert Image to Base 64. So you can print images on PDF
            // Just pass Image path string created above to it and it will return base64 string

            var base64Image = ImageToBase64(imgPath);

            // Reading TEMPLATE.HTML from stored location to be ready and converted into PDF
            // Converting HTML into String (Stream) to pass on network or replace tags inside its TEXT.

            string body = string.Empty;
            StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/sample.html"));
            using (reader)
            {
                body = reader.ReadToEnd();
            }

            // REPLACING tags inside TEMPLATE.HTML with dynamic data Taken from any MODEL.

            body = body.Replace("{NAME}", newTest.Name);
            body = body.Replace("{ID}", Convert.ToString(newTest.Id));
            body = body.Replace("{SRC}", base64Image);                      // This is Image src <img src={SRC} > , replaced with BASE64 string

            // CONVERTING Body(html stream) to Bytes with PECHKIN to be Convert to PDF asnd able to store on server.

            byte[] pdfContent = pechkin.Convert(body);

            // Setting Up Directory Path. Where PDF files will be save on Server.

            string directory = Server.MapPath("~/SavedPDF/");

            // Setting up PDF File Name (Dynamic Name for every client Name with Date)

            string filename = "clientPDF_" + newTest.Name + "_" + DateTime.Now.ToString("dd-MMM-yyyy") + ".pdf";

            // Writing Byte Array (PDF CONTENT) to File completely.  If Success The SUCCESS messeg will be written on Console.

            if (ByteArrayToFile(directory + filename, pdfContent))
            {
                Console.WriteLine("PDF Succesfully created");
            }
            else
            {
                Console.WriteLine("Cannot create PDF");
            }

            // Returing to Main Page

            return RedirectToAction("Index");
        }

        // Method To Wrie BYTES on FILE

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

        // Method To Load image form Server Path and Convert it to Base 64

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