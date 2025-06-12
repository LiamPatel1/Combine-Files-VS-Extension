using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;

namespace CombineFilesVSExtension
{
    public class StringDictionaryConverter : TypeConverter
    {
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                if (!stringValue.StartsWith("{")) return new Dictionary<string, string>();
                try
                {
                    return _serializer.Deserialize<Dictionary<string, string>>(stringValue) ?? new Dictionary<string, string>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CombineFilesVSExtension - StringDictionaryConverter] Failed to deserialize: {ex}");
                    return new Dictionary<string, string>();
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Dictionary<string, string> dict)
            {
                if (context == null)
                {
                    return _serializer.Serialize(dict);
                }
                if (dict == null || !dict.Any()) return "(Empty)";
                return $"({dict.Count} key/value pairs)";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}