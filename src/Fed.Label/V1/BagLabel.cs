using Fed.Core.Entities;
using Fed.Label.V1.Labels.A4Labels.Avery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fed.Label.V1
{
    public static class BagLabel
    {
        public static Stream GenerateBagLabels(IList<Delivery> deliveries, int labelsPerOrder, bool showBagNumbers = true)
        {
            var labelDefinition = new L7160();
            var labelCreator = new CustomLabelCreator(labelDefinition);
            string font = "Calibri";
            int fontSizeS = 9;
            int fontSizeL = 13;
            int fontSizeXL = 15;
            int fontSizeXXL = 17;
            var bold = new Enums.FontStyle[] { Enums.FontStyle.BOLD };

            foreach (var delivery in deliveries)
            {

                string displayTime = DateTime.Today.Add(delivery.LatestTime).ToString("htt");
                string displayDate = delivery.DeliveryDate.Value.ToString("dd.MM.yyyy");

                //is this a split delivery?
                var isSplit = delivery.Orders.Count() == 1 && delivery.Orders.Single().SplitDeliveriesByOrder;

                for (var i = 1; i <= labelsPerOrder; i++)
                {
                    string bagNumber = showBagNumbers ? i.ToString() : " ";

                    var label = new Label(Enums.Alignment.CENTER);
                    label.AddHeadingLeft(delivery.ShortId, font, fontSizeXXL, embedFont: true, fontStyles: bold);
                    label.AddHeadingRight(displayDate, font, fontSizeS, embedFont: true, fontStyles: bold);
                    label.AddText(string.Empty, font, fontSizeL, embedFont: true, fontStyles: bold);
                    label.AddText(delivery.DeliveryCompanyName, font, isSplit ? fontSizeL : fontSizeXL, embedFont: true, fontStyles: bold);
                    if (isSplit)
                        label.AddText($"{ delivery.Orders.Single().OrderName}", font, fontSizeL, embedFont: true, fontStyles: bold);
                    label.AddText(string.Empty, font, fontSizeL, embedFont: true, fontStyles: bold);
                    label.AddText($"Bag {bagNumber} / ", font, fontSizeL, embedFont: true, fontStyles: bold);
                    labelCreator.AddLabel(label);

                }
            }

            return labelCreator.CreatePDF();
        }
    }
}
