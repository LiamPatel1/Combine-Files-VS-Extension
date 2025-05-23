using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design; // Required for IWindowsFormsEditorService

namespace CombineFilesVSExtension
{
    public class StringListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Indicates that this editor will display a modal dialog.
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null)
            {
                return value;
            }

            // Get the IWindowsFormsEditorService, which is used to display Windows Forms.
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null)
            {
                return value;
            }

            // The value passed in is the current List<string>.
            List<string> currentList = value as List<string>;
            if (value != null && !(value is List<string>))
            {
                // If it's some other IEnumerable<string> like string[], convert it.
                // This makes the editor more robust if the property type changes slightly.
                if (value is IEnumerable<string> enumerableValue)
                {
                    currentList = new List<string>(enumerableValue);
                }
                else
                {
                    // Not a recognized collection of strings, fall back or return original.
                    return value;
                }
            }

            // If currentList is still null (e.g., property was null), initialize to an empty list.
            currentList = currentList ?? new List<string>();

            string title = "Edit Items";
            if (context?.PropertyDescriptor != null)
            {
                // Use the property's display name for a more descriptive title
                title = $"Edit {context.PropertyDescriptor.DisplayName}";
            }

            using (StringListEditorForm editorForm = new StringListEditorForm(currentList, title))
            {
                if (editorService.ShowDialog(editorForm) == DialogResult.OK)
                {
                    // Return the new list from the form.
                    return editorForm.EditedList;
                }
            }

            // If the dialog was canceled, return the original value.
            return value;
        }
    }
}