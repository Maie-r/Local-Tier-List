using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Local_Tier_List.Data.TierLists
{
    public static class GlobalStyleFunctions
    {
        public static string GetMostReadableTextColor(string hexColor)
        {
            double Lumithreshold = 0.6;
            double Alphathreshold = 0.7;
            hexColor = hexColor.TrimStart('#');

            byte r = Convert.ToByte(hexColor[0..2], 16);
            byte g = Convert.ToByte(hexColor[2..4], 16);
            byte b = Convert.ToByte(hexColor[4..6], 16);
            byte a = hexColor.Length == 8 ? Convert.ToByte(hexColor[6..8], 16) : (byte)255;

            double alpha = a / 255.0;
            if (alpha < Alphathreshold) return "#FFFFFF";

            double Linearize(byte c)
            {
                double s = c / 255.0;
                return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }

            // W3C relative luminance formula
            double luminance = 0.2126 * Linearize(r)
                             + 0.7152 * Linearize(g)
                             + 0.0722 * Linearize(b);

            // >0.179 is the threshold where black becomes more readable
            return luminance > Lumithreshold ? "#000000" : "#FFFFFF";
        }

        public static string GetTierFontSize(string name, double multiplier, bool allowbreak)
        {
            double size;
            if (name.Length > 6)
            {
                size = (1 - (((double)name.Length + 1) / (name.Length + 1.5))) * 400;
            }
            else
            {
                size = 30;
            }
            var extra = allowbreak ? "; word-break: break-all;" : "";
            size *= multiplier;
            return $"font-size: {size.ToString("F2", CultureInfo.InvariantCulture)}px {extra}";
        }

        public static string GetItemFontSize(string name, double multiplier, bool allowbreak)
        {
            double size;
            size = (1 - (((double)name.Length + 60) / (name.Length + 95))) * 50;
            size *= multiplier;
            return $"font-size: {size.ToString("F2", CultureInfo.InvariantCulture)}px{(allowbreak ? "; text-overflow: ellipsis" : "; text-overflow: ellipsis")}";
        }
    }
}
