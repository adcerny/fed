using Fed.Label.V1;
using Fed.Label.V1.Labels.A4Labels.Avery;
using System.IO;
using Xunit;

namespace Fed.Tests.Core
{
    public class CustomLabelCreatorTests
    {
        [Fact]
        public void TestCreateAPdf()
        {

            var labelDefinition = new L3474();
            var labelCreator = new CustomLabelCreator(labelDefinition)
            {
                IncludeLabelBorders = true
            };
            string font = "Verdana";
            int fontSize = 11;

            for (var i = 1; i <= 24; i++)
            {
                var label = new Fed.Label.V1.Label(Fed.Label.V1.Enums.Alignment.CENTER);
                label.AddText("Mon   04.03.19    9AM", font, fontSize, embedFont: true);
                label.AddText(string.Empty, font, fontSize, embedFont: true);
                label.AddText("Ikb Travel", font, fontSize, embedFont: true);
                label.AddText("Abir Burhan", font, fontSize, embedFont: true);
                label.AddText(string.Empty, font, fontSize, embedFont: true);
                label.AddText("Q 001 732", font, fontSize, embedFont: true);
                label.AddText($"Bag {i} of __", font, fontSize, embedFont: true);
                labelCreator.AddLabel(label);
            }


            var pdfStream = labelCreator.CreatePDF();

            var fileStream = File.Create(@".\pdf3474.pdf");
            pdfStream.CopyTo(fileStream);
            fileStream.Close();
            pdfStream.Close();

            // yeah, lame test
            Assert.True(File.Exists(@".\pdf3474.pdf"));


            // I comment this out to look at the pdf..
            // how would you test this?
            File.Delete(@".\pdf5160.pdf");

        }
    }
}
