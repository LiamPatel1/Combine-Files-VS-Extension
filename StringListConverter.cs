using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq; 

namespace CombineFilesVSExtension
{
    public class StringListConverter : TypeConverter
    {
        // This method determines if the converter can convert the object to the destination type.
        // We want to convert List<string> to a string representation.
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }


        // This method performs the conversion.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<string> list)
            {
                if (list == null || !list.Any())
                {
                    return "(Empty)";
                }

                const int maxPreviewItems = 2; // How many items to show in preview
                const int maxPreviewLength = 50; // Max total length of preview string

                List<string> itemsToPreview = list.Take(maxPreviewItems).ToList();
                string preview = string.Join(", ", itemsToPreview);

                if (preview.Length > maxPreviewLength)
                {
                    preview = preview.Substring(0, maxPreviewLength - 3) + "...";
                }

                if (list.Count > maxPreviewItems || (list.Count == maxPreviewItems && preview.EndsWith("..."))) // Check if truncation happened or more items exist
                {
                    return $"{preview} ({list.Count} items)";
                }
                else
                {
                    return preview; // Just the preview if it's short and shows all items
                }
            }
            // If it's not a List<string> or not converting to string, use the base class's behavior.
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}