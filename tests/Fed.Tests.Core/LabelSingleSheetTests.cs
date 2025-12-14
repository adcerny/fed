using Fed.Label.V1.Labels.A4Labels.Avery;
using System.IO;
using Xunit;

namespace Fed.Tests.Core
{
    public class SingleSheetLabelCreatorTests
    {
        [Fact]
        public void TestCreateSingleSheetPDF()
        {

            var labelDefinition = new L7654();
            var labelCreator = new Fed.Label.V1.SingleSheetLabelCreator(labelDefinition);
            labelCreator.IncludeLabelBorders = true;
            labelCreator.AddText("Fed by Abel & Cole", "Verdana", 12, embedFont: true);
            labelCreator.AddText("A Fed team is a happpy team!", "Verdana", 12, embedFont: true);


            var pdfStream = labelCreator.CreatePDF();
            var pdfName = "pdf7654.pdf";

            var fileStream = File.Create(@".\" + pdfName);
            pdfStream.CopyTo(fileStream);
            fileStream.Close();
            pdfStream.Close();

            // yeah, lame test
            Assert.True(File.Exists(@".\" + pdfName));


            // I comment this out to look at the pdf..
            // how would you test this?
            File.Delete(@".\" + pdfName);

        }
    }
}
