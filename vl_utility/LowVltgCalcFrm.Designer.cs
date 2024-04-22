namespace vl_utility
{
    partial class LowVltgCalcFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxTrans = new System.Windows.Forms.ComboBox();
            this.labelTrans = new System.Windows.Forms.Label();
            this.dataGridViewLines = new System.Windows.Forms.DataGridView();
            this.Наименование = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Длина = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLines)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxTrans
            // 
            this.comboBoxTrans.FormattingEnabled = true;
            this.comboBoxTrans.Location = new System.Drawing.Point(125, 18);
            this.comboBoxTrans.Name = "comboBoxTrans";
            this.comboBoxTrans.Size = new System.Drawing.Size(94, 21);
            this.comboBoxTrans.TabIndex = 0;
            // 
            // labelTrans
            // 
            this.labelTrans.AutoSize = true;
            this.labelTrans.Location = new System.Drawing.Point(21, 20);
            this.labelTrans.Name = "labelTrans";
            this.labelTrans.Size = new System.Drawing.Size(89, 13);
            this.labelTrans.TabIndex = 1;
            this.labelTrans.Text = "Трансформатор";
            // 
            // dataGridViewLines
            // 
            this.dataGridViewLines.AllowDrop = true;
            this.dataGridViewLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Наименование,
            this.Длина});
            this.dataGridViewLines.Location = new System.Drawing.Point(12, 64);
            this.dataGridViewLines.Name = "dataGridViewLines";
            this.dataGridViewLines.Size = new System.Drawing.Size(508, 218);
            this.dataGridViewLines.TabIndex = 2;
            this.dataGridViewLines.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewLines_DragEnter);
            this.dataGridViewLines.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewLines_DragDrop);
            // 
            // Наименование
            // 
            this.Наименование.HeaderText = "Наименование";
            this.Наименование.Name = "Наименование";
            this.Наименование.ReadOnly = true;
            // 
            // Длина
            // 
            this.Длина.HeaderText = "Длина";
            this.Длина.Name = "Длина";
            // 
            // LowVltgCalcFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 420);
            this.Controls.Add(this.dataGridViewLines);
            this.Controls.Add(this.labelTrans);
            this.Controls.Add(this.comboBoxTrans);
            this.Name = "LowVltgCalcFrm";
            this.Text = "LowVltgCalcFrm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxTrans;
        private System.Windows.Forms.Label labelTrans;
        private System.Windows.Forms.DataGridView dataGridViewLines;
        private System.Windows.Forms.DataGridViewTextBoxColumn Наименование;
        private System.Windows.Forms.DataGridViewTextBoxColumn Длина;
    }
}