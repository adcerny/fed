using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace Fed.Label.V2
{
    public class PrintLabelService : IPrintLabelService
    {
        public void PrintLabels(string deliveryId, string companyName, Date deliveryDate, int bagCount)
        {
            Bitmap label = null;
            var printDoc = new PrintDocument();

            printDoc.PrintPage +=
                new PrintPageEventHandler(
                    (sender, eventArgs) =>
                    {
                        eventArgs.Graphics.DrawImage(label, new Point(0, 0));
                    });

            for (var i = 1; i <= bagCount; i++)
            {
                label =
                    FedLabel
                        .Create(
                            deliveryId,
                            companyName,
                            deliveryDate,
                            i,
                            bagCount)
                        .ToImage();

                printDoc.Print();
            }
        }

        public List<byte[]> GetLabels(string deliveryId, string companyName, Date deliveryDate, int bagCount)
        {
            var images = new List<byte[]>();

            for (var i = 1; i <= bagCount; i++)
            {
                var label =
                    FedLabel
                        .Create(
                            deliveryId,
                            companyName,
                            deliveryDate,
                            i,
                            bagCount)
                        .ToImage();

                using (var stream = new MemoryStream())
                {
                    ImageConverter converter = new ImageConverter();
                    images.Add((byte[])converter.ConvertTo(label, typeof(byte[])));
                }
            }
            return images;
        }
    }
}
