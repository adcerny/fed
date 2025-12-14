using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Fed.Label.V2
{
    public class FedLabel
    {
        public string DeliveryId { get; }
        public string CompanyName { get; }
        public string Date { get; }
        public string BagNumber { get; }
        public string TotalBagCount { get; }

        private FedLabel(string deliveryId, string companyName, DateTime date, int bagNumber, int totalBagCount)
        {
            DeliveryId = deliveryId;
            CompanyName = companyName;
            Date = date.ToString("dd.MM.yyyy");
            BagNumber = bagNumber.ToString();
            TotalBagCount = totalBagCount.ToString();
        }

        public static FedLabel Create(
            string deliveryId,
            string companyName,
            DateTime date,
            int bagNumber,
            int totalBagCount)
            => new FedLabel(
                deliveryId,
                companyName,
                date,
                bagNumber,
                totalBagCount);

        public Bitmap ToImage()
        {
            const int labelWidth = 400;
            const int labelHeight = 290;
            const int padding = 30;
            var bitmap = new Bitmap(labelWidth, labelHeight);
            var graphics = Graphics.FromImage(bitmap);
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            graphics.DrawImage(bitmap, 0, 0, labelWidth, labelHeight);
            graphics.FillRectangle(Brushes.White, 0, 0, labelWidth - 1, labelHeight - 1);
            graphics.DrawRectangle(Pens.Black, 0, 0, labelWidth - 1, labelHeight - 1);

            // --------------------
            // Delivery ID
            // --------------------

            var deliveryIdFont = new Font(FontFamily.GenericSansSerif, 20.0f, FontStyle.Bold);
            var deliveryIdMeasures = graphics.MeasureString(DeliveryId, deliveryIdFont);

            graphics.DrawString(
                DeliveryId,
                deliveryIdFont,
                Brushes.Black,
                new PointF(padding, padding));

            // --------------------
            // Date
            // --------------------

            var dateFont = new Font(FontFamily.GenericSansSerif, 15.0f);
            var dateMeasures = graphics.MeasureString(Date, dateFont);

            graphics.DrawString(
                Date,
                dateFont,
                Brushes.Black,
                new PointF(
                    labelWidth - dateMeasures.Width - padding,
                    (padding + (deliveryIdMeasures.Height / 2)) - (dateMeasures.Height / 2)));

            // --------------------
            // Company Name
            // --------------------

            var companyNameFont =
                graphics.GetFittedFont(
                    CompanyName,
                    new Font(FontFamily.GenericSansSerif, 40.0f, FontStyle.Bold),
                    labelWidth - (padding * 2),
                    labelHeight - (padding * 2));

            var companyNameMeasures = graphics.MeasureString(CompanyName, companyNameFont);

            graphics.DrawString(
                CompanyName,
                companyNameFont,
                Brushes.Black,
                new PointF(
                    (labelWidth / 2) - (companyNameMeasures.Width / 2),
                    (labelHeight / 2) - (companyNameMeasures.Height / 2)));

            // --------------------
            // Bag
            // --------------------

            var bagText = $"Bag {BagNumber} / {TotalBagCount}";
            var bagFont = new Font(FontFamily.GenericSansSerif, 30.0f);
            var bagMeasures = graphics.MeasureString(bagText, bagFont);

            graphics.DrawString(
                bagText,
                bagFont,
                Brushes.Black,
                new PointF(
                    (labelWidth / 2) - (bagMeasures.Width / 2),
                    labelHeight - padding - companyNameMeasures.Height));

            return bitmap;
        }
    }
}
