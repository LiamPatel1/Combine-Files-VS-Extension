using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;

namespace CombineFilesVSExtension
{
    public class StringListConverter : TypeConverter
    {
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        //  called by the persistence layer when loading a setting.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                // If the user somehow edited the preview text, this will safely fail.
                if (!stringValue.StartsWith("[")) return new List<string>();
                try
                {
                    return _serializer.Deserialize<List<string>>(stringValue) ?? new List<string>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CombineFilesVSExtension - StringListConverter] Failed to deserialize: {ex}");
                    return new List<string>();
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<string> list)
            {
                //  its the persistence layer. Return the full JSON to save.
                if (context == null)
                {
                    return _serializer.Serialize(list);
                }

                //  it's the UI. Return a user-friendly preview string.
                if (list == null || !list.Any()) return "(Empty)";

                const int maxPreviewItems = 2;
                const int maxPreviewLength = 50;
                string preview = string.Join(", ", list.Take(maxPreviewItems));
                if (preview.Length > maxPreviewLength) preview = preview.Substring(0, maxPreviewLength - 3) + "...";
                if (list.Count > maxPreviewItems || preview.EndsWith("...")) return $"{preview} ({list.Count} items)";
                return preview;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}