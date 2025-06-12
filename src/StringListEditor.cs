using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design; 

namespace CombineFilesVSExtension
{
    public class StringListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null)
            {
                return value;
            }

            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null)
            {
                return value;
            }

            List<string> currentList = value as List<string>;
            if (value != null && !(value is List<string>))
            {
         
                if (value is IEnumerable<string> enumerableValue)
                {
                    currentList = new List<string>(enumerableValue);
                }
                else
                {
                    return value;
                }
            }
            currentList = currentList ?? new List<string>();

            string title = "Edit Items";
            if (context?.PropertyDescriptor != null)
            {
                title = $"Edit {context.PropertyDescriptor.DisplayName}";
            }

            using (StringListEditorForm editorForm = new StringListEditorForm(currentList, title))
            {
                if (editorService.ShowDialog(editorForm) == DialogResult.OK)
                {
                    return editorForm.EditedList;
                }
            }

            return value;
        }
    }
}