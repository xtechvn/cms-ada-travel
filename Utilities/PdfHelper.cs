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

                return AddImageToPdf(pdfBytes, signatureBytes, 400, 700, 150, 75); 
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SignPdf - PdfHelper: " + ex.Message);
                return null;
            }
        }

        public static byte[] AddImageToPdf(byte[] pdfBytes, byte[] imageBytes,
                          float x, float y, float width, float height)
        {
            try
            {
                using var input = new MemoryStream(pdfBytes);
                using var output = new MemoryStream();

                var readerProperties = new iText.Kernel.Pdf.ReaderProperties();

                var reader = new iText.Kernel.Pdf.PdfReader(input, readerProperties);

                // QUAN TR?NG: fix Unknown PdfException
                reader.SetUnethicalReading(true);

                var writer = new iText.Kernel.Pdf.PdfWriter(output,
                    new iText.Kernel.Pdf.WriterProperties()
                        .SetFullCompressionMode(true));

                using var pdf = new iText.Kernel.Pdf.PdfDocument(reader, writer);

                var document = new iText.Layout.Document(pdf);

                var imageData = iText.IO.Image.ImageDataFactory.Create(imageBytes);

                var image = new iText.Layout.Element.Image(imageData)
                    .SetFixedPosition(1, x, y)
                    .ScaleToFit(width, height);

                document.Add(image);

                document.Close();

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

