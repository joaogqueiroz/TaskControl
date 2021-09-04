using AspNetMVCproject03.Reports.Data;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Reports.Pdfs
{
    public class TaskReportPdf
    {
        public byte[] ReportGenerator(TaskReportData data)
        {
            var memoryStream = new MemoryStream();
            var pdf = new PdfDocument(new PdfWriter(memoryStream));


            using (var document = new Document(pdf))
            {
                //Logo

                var img = ImageDataFactory.Create("https://seeklogo.com/images/C/corporate-company-logo-BAE6B43FF7-seeklogo.com.png");
                document.Add(new Image(img));
                document.Add(new Paragraph("\n"));

                //Repor title
                document.Add(new Paragraph("Tasks Report").SetFontSize(21));

                //User data
                document.Add(new Paragraph("User data:"));
                document.Add(new Paragraph($"Name {data.User.Name}"));
                document.Add(new Paragraph($"Email {data.User.Email}"));
                document.Add(new Paragraph("\n"));

                //Report range of time
                document.Add(new Paragraph("Date:"));
                document.Add(new Paragraph($"Start date {data.StartDate.ToString("MM/dd/yyyy")}"));
                document.Add(new Paragraph($"Finish date {data.FinishDate.ToString("MM/dd/yyyy")}"));
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("\n"));

                //Task table
                var table = new Table(5); //Number of columns
                table.AddHeaderCell("Task name");
                table.AddHeaderCell("Date");
                table.AddHeaderCell("Hour");
                table.AddHeaderCell("Description");
                table.AddHeaderCell("Priority");

                foreach (var item in data.Tasks)
                {
                    table.AddCell(item.Name);
                    table.AddCell(item.Date.ToString("MM/dd/yyyy"));
                    table.AddCell(item.Hour.ToString(@"hh\:mm"));
                    table.AddCell(item.Description); 
                    table.AddCell(item.Priority);
                }
                document.Add(table);
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph($"Ammount of tasks {data.Tasks.Count}"));

            }
            return memoryStream.ToArray();
        }
    }
}
