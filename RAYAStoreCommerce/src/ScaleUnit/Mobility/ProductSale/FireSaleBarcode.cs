using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    internal static class FireSaleBarcode
    {
        public static ProductSaleBarcode Evaluate(string prefix, string barcodeId)
        {
            if (String.IsNullOrEmpty(prefix) || String.IsNullOrEmpty(barcodeId))
                return null;
            if (!barcodeId.StartsWith(prefix))
                return null;

            string barcodeText = barcodeId.Substring(prefix.Length);
            if (String.IsNullOrEmpty(barcodeText))
                return null;

            int barcodeParam;
            if (!Int32.TryParse(barcodeText.Substring(0, 1), out barcodeParam))
                return null;

            barcodeText = barcodeText.Substring(1);
            if (String.IsNullOrEmpty(barcodeText))
                return null;

            int priceLength = BitEncoder.Get(barcodeParam, 2) ? 5 : 4;
            if (barcodeText.Length < priceLength)
                return null;

            string priceText = barcodeText.Substring(0, priceLength);
            int price;
            if (!Int32.TryParse(priceText, out price))
                return null;

            ProductSaleBarcode psb = new ProductSaleBarcode();
            psb.Price = (decimal)price / 100;

            barcodeText = barcodeText.Substring(priceLength);
            if (String.IsNullOrEmpty(barcodeText))
                return null;

            if (BitEncoder.Get(barcodeParam, 1))
            {
                if (barcodeText.Length < 5)
                    return null;

                string expDateText = barcodeText.Substring(0, 5);
                int expDate;
                if (!Int32.TryParse(expDateText, out expDate))
                    return null;

                int year = expDate / 1000;
                int day = expDate % 1000;
                psb.ExpDate = new DateTime(year + 2000, 1, 1).AddDays(day - 1);

                barcodeText = barcodeText.Substring(5);
                if (String.IsNullOrEmpty(barcodeText))
                    return null;
            }

            if (BitEncoder.Get(barcodeParam, 0))
            {
                if (barcodeText.Length < 2)
                    return null;

                psb.ReasonCode = barcodeText.Substring(0, 2);

                barcodeText = barcodeText.Substring(2);
                if (String.IsNullOrEmpty(barcodeText))
                    return null;
            }

            psb.BarcodeId = barcodeText;

            return psb;
        }

        public static string Format(string prefix, ProductSaleBarcode barcode)
        {
            ThrowIf.Null<ProductSaleBarcode>(barcode, "barcode");
            ThrowIf.NullOrWhiteSpace(barcode.BarcodeId, "barcode.BarcodeId");

            if (String.IsNullOrEmpty(prefix))
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, "FireSale Barcode Prefix is not defined.");

            int barcodeParam = 0;

            string priceText;
            int priceRounded = (int)Math.Round(Math.Abs(barcode.Price * 100), 0);
            if (priceRounded < 10000)
            {
                priceText = priceRounded.ToString("D4");
            }
            else if (priceRounded < 100000)
            {
                priceText = priceRounded.ToString("D5");
                barcodeParam = BitEncoder.Set(barcodeParam, 2, true);
            }
            else
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, "Price Amount exceeds maximum allowed.");

            string reasonCode = String.Empty;
            if (!String.IsNullOrEmpty(barcode.ReasonCode))
            {
                if (barcode.ReasonCode.Length != 2)
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, "Reason Code must be 2 characters in length.");
                reasonCode = barcode.ReasonCode;
                barcodeParam = BitEncoder.Set(barcodeParam, 0, true);
            }

            string expDate = String.Empty;
            if (barcode.ExpDate.HasValue)
            {
                int year = barcode.ExpDate.Value.Year - 2000;
                int day = barcode.ExpDate.Value.DayOfYear;
                int date = year * 1000 + day;
                expDate = date.ToString("D5");
                barcodeParam = BitEncoder.Set(barcodeParam, 1, true);
            }

            return prefix + barcodeParam.ToString("D") + priceText + expDate + reasonCode + barcode.BarcodeId;
        }
    }
}
