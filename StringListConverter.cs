using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq; 

namespace CombineFilesVSExtension
{
    public class StringListConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<string> list)
            {
                if (list == null || !list.Any())
                {
                    return "(Empty)";
                }

                const int maxPreviewItems = 2; 
                const int maxPreviewLength = 50; 

                List<string> itemsToPreview = list.Take(maxPreviewItems).ToList();
                string preview = string.Join(", ", itemsToPreview);

                if (preview.Length > maxPreviewLength)
                {
                    preview = preview.Substring(0, maxPreviewLength - 3) + "...";
                }

                if (list.Count > maxPreviewItems || (list.Count == maxPreviewItems && preview.EndsWith("..."))) 
                {
                    return $"{preview} ({list.Count} items)";
                }
                else
                {
                    return preview; 
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}