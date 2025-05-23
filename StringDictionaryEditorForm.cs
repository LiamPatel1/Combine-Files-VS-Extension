using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CombineFilesVSExtension
{
    public partial class StringDictionaryEditorForm : Form
    {
        public Dictionary<string, string> EditedDictionary { get; private set; }

        public StringDictionaryEditorForm(Dictionary<string, string> initialDictionary)
        {
            InitializeComponent();
            EditedDictionary = new Dictionary<string, string>(initialDictionary);
            PopulateGrid();
        }

        private void PopulateGrid()
        {
            dataGridViewPairs.Rows.Clear();
            foreach (var kvp in EditedDictionary)
            {
                dataGridViewPairs.Rows.Add(kvp.Key, kvp.Value);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            EditedDictionary.Clear();
            foreach (DataGridViewRow row in dataGridViewPairs.Rows)
            {
                if (row.IsNewRow) continue; 

                var keyCell = row.Cells["KeyColumn"];
                var valueCell = row.Cells["ValueColumn"];

                string key = keyCell.Value?.ToString();
                string value = valueCell.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (!EditedDictionary.ContainsKey(key))
                    {
                        EditedDictionary[key] = value ?? string.Empty;
                    }
                    else
                    {
                        // Maybe in future give an option to "ok" or "cancel"?
                        EditedDictionary[key] = value ?? string.Empty;
                        MessageBox.Show($"Duplicate key '{key}' found. Using the last value.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private System.ComponentModel.IContainer components = null;
        private DataGridView dataGridViewPairs;
        private Button btnOK;
        private Button btnCancel;
        private DataGridViewTextBoxColumn KeyColumn;
        private DataGridViewTextBoxColumn ValueColumn;


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
            this.dataGridViewPairs = new System.Windows.Forms.DataGridView();
            this.KeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPairs)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewPairs
            // 
            this.dataGridViewPairs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewPairs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPairs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.KeyColumn,
            this.ValueColumn});
            this.dataGridViewPairs.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewPairs.Name = "dataGridViewPairs";
            this.dataGridViewPairs.Size = new System.Drawing.Size(460, 288);
            this.dataGridViewPairs.TabIndex = 0;
            // 
            // KeyColumn
            // 
            this.KeyColumn.HeaderText = "Key (File Pattern)";
            this.KeyColumn.Name = "KeyColumn";
            this.KeyColumn.Width = 200;
            // 
            // ValueColumn
            // 
            this.ValueColumn.HeaderText = "Value (Type for Markdown)";
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.Width = 200;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(316, 306);
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
            this.btnCancel.Location = new System.Drawing.Point(397, 306);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // DictionaryEditorForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 341);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dataGridViewPairs);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "DictionaryEditorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Type Matching";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPairs)).EndInit();
            this.ResumeLayout(false);
        }
    }
}