using Fed.Core.ValueTypes;
using Fed.Label.V2;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace Fed.Label.Printing.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var deliveryId = "D6A-12-0";
            var bagCount = 5;

            Console.WriteLine("Printing labels...");

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
                            "House of Creative",
                            Date.Today,
                            i,
                            bagCount)
                        .ToImage();

                Console.WriteLine($"Printing label {i} out of {bagCount}...");

                printDoc.Print();
            }
        }
    }
}
