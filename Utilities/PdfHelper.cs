using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.IO;

namespace Utilities
{
    public class PdfHelper
    {
        public static byte[] SignPdf(byte[] pdfBytes, string signatureBase64)
        {
            try
            {
                // Convert base64 to bytes
                string base64Data = signatureBase64;
                if (signatureBase64.Contains(","))
                {
                    base64Data = signatureBase64.Split(',')[1];
                }
                byte[] signatureBytes = Convert.FromBase64String(base64Data);

                // Perform the merge using iText
                pdfBytes = RebuildPdf(pdfBytes);

                return AddImageToPdf(pdfBytes, signatureBytes, 350, 100, 150, 75); 
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SignPdf - PdfHelper: " + ex.Message);
                return null;
            }
        }

        public static byte[] AddImageToPdf(byte[] pdfBytes, byte[] imageBytes,float x, float y, float width, float height)
        {
            try
            {
                using var input = new MemoryStream(pdfBytes);
                using var output = new MemoryStream();

                var reader = new PdfReader(input);
                reader.SetUnethicalReading(true);

                var writer = new PdfWriter(output);

                using var pdf = new PdfDocument(reader, writer);

                var page = pdf.GetPage(1);

                var pdfCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);

                var imageData = iText.IO.Image.ImageDataFactory.Create(imageBytes);

                var rect = new iText.Kernel.Geom.Rectangle(x, y, width, height);

                pdfCanvas.AddImageFittedIntoRectangle(imageData, rect, false);

                pdf.Close();

                return output.ToArray();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddImageToPdf error: " + ex.ToString());
                return pdfBytes;
            }
        }
        public static byte[] RebuildPdf(byte[] pdfBytes)
        {
            using var input = new MemoryStream(pdfBytes);
            using var output = new MemoryStream();

            var reader = new PdfReader(input);
            reader.SetUnethicalReading(true);

            var writer = new PdfWriter(output);

            var pdf = new PdfDocument(reader, writer);

            pdf.Close();

            return output.ToArray();
        }
    }
}

