// StringDictionaryConverter.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace CombineFilesVSExtension
{
    public class StringDictionaryConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Dictionary<string, string> dict)
            {
                if (dict == null || dict.Count == 0)
                    return "(Empty)";
                return $"{dict.Count} entr" + (dict.Count == 1 ? "y" : "ies");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}