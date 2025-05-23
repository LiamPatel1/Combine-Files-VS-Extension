using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CombineFilesVSExtension
{
    public class StringDictionaryEditor : UITypeEditor
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

            // Display a form as a dialog.
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null)
            {
                return value;
            }

      
            Dictionary<string, string> currentDictionary = value as Dictionary<string, string>;
            if (currentDictionary == null)
            {
                currentDictionary = new Dictionary<string, string>();
            }

            using (StringDictionaryEditorForm editorForm = new StringDictionaryEditorForm(currentDictionary))
            {
                if (editorService.ShowDialog(editorForm) == DialogResult.OK)
                {
                    return editorForm.EditedDictionary;
                }
            }

            return value;
        }
    }
}