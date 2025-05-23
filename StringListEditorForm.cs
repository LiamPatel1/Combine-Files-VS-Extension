using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CombineFilesVSExtension
{
    public partial class StringListEditorForm : Form
    {
        public List<string> EditedList { get; private set; }

        public StringListEditorForm(IEnumerable<string> initialList, string title = "Edit List")
        {
            InitializeComponent();
            this.Text = title; // Set the window title
            EditedList = initialList != null ? new List<string>(initialList) : new List<string>();
            // Populate TextBox, ensuring Environment.NewLine is used for display
            textBoxEntries.Lines = EditedList.ToArray();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Get lines, filter out empty/whitespace-only lines, and trim
            EditedList = textBoxEntries.Lines
                                     .Where(l => !string.IsNullOrWhiteSpace(l))
                                     .Select(l => l.Trim())
                                     .ToList();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ----- Manually added designer code (or use the WinForms designer) -----
        private System.ComponentModel.IContainer components = null;
        private TextBox textBoxEntries;
        private Button btnOK;
        private Button btnCancel;
        private Label lblInstructions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBoxEntries = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = true;
            this.lblInstructions.Location = new System.Drawing.Point(12, 9);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(130, 13); // Adjust size/text as needed
            this.lblInstructions.TabIndex = 3;
            this.lblInstructions.Text = "Enter one item per line:";
            // 
            // textBoxEntries
            // 
            this.textBoxEntries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxEntries.Location = new System.Drawing.Point(12, 28); // Adjusted Y for label
            this.textBoxEntries.Multiline = true;
            this.textBoxEntries.Name = "textBoxEntries";
            this.textBoxEntries.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxEntries.Size = new System.Drawing.Size(360, 192); // Adjusted height
            this.textBoxEntries.TabIndex = 0;
            this.textBoxEntries.AcceptsReturn = true; // Allow new lines
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(216, 226);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(297, 226);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // StringListEditorForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.textBoxEntries);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "StringListEditorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit List"; // Default title, will be overridden
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}