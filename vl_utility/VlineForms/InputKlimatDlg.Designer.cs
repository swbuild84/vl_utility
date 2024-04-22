namespace vl_utility
{
    partial class InputKlimatDlg
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
            this.groupCity = new System.Windows.Forms.GroupBox();
            this.comboCityName = new System.Windows.Forms.ComboBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OKBtn = new System.Windows.Forms.Button();
            this.groupManual = new System.Windows.Forms.GroupBox();
            this.textTmax = new System.Windows.Forms.TextBox();
            this.textTmin = new System.Windows.Forms.TextBox();
            this.textTe = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboWind = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboGol = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.comboVoltage = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboNumLines = new System.Windows.Forms.ComboBox();
            this.textRegGol = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textRegVeter = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.comboMestnType = new System.Windows.Forms.ComboBox();
            this.groupCity.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupManual.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupCity
            // 
            this.groupCity.Controls.Add(this.comboCityName);
            this.groupCity.Location = new System.Drawing.Point(12, 55);
            this.groupCity.Name = "groupCity";
            this.groupCity.Size = new System.Drawing.Size(402, 74);
            this.groupCity.TabIndex = 0;
            this.groupCity.TabStop = false;
            // 
            // comboCityName
            // 
            this.comboCityName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCityName.FormattingEnabled = true;
            this.comboCityName.Location = new System.Drawing.Point(6, 29);
            this.comboCityName.Name = "comboCityName";
            this.comboCityName.Size = new System.Drawing.Size(347, 21);
            this.comboCityName.TabIndex = 0;
            this.comboCityName.SelectedIndexChanged += new System.EventHandler(this.comboCityName_SelectedIndexChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 38);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(120, 17);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Населенный пункт";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 145);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(117, 17);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Точная настройка";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CancelBtn);
            this.groupBox2.Controls.Add(this.OKBtn);
            this.groupBox2.Controls.Add(this.groupManual);
            this.groupBox2.Controls.Add(this.radioButton1);
            this.groupBox2.Controls.Add(this.radioButton2);
            this.groupBox2.Controls.Add(this.groupCity);
            this.groupBox2.Location = new System.Drawing.Point(12, 121);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(420, 449);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Климатические условия";
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(267, 416);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(103, 24);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "&Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(151, 416);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(103, 24);
            this.OKBtn.TabIndex = 4;
            this.OKBtn.Text = "&OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // groupManual
            // 
            this.groupManual.Controls.Add(this.textRegVeter);
            this.groupManual.Controls.Add(this.label9);
            this.groupManual.Controls.Add(this.textRegGol);
            this.groupManual.Controls.Add(this.label3);
            this.groupManual.Controls.Add(this.textTmax);
            this.groupManual.Controls.Add(this.textTmin);
            this.groupManual.Controls.Add(this.textTe);
            this.groupManual.Controls.Add(this.label8);
            this.groupManual.Controls.Add(this.label7);
            this.groupManual.Controls.Add(this.label6);
            this.groupManual.Controls.Add(this.comboWind);
            this.groupManual.Controls.Add(this.label2);
            this.groupManual.Controls.Add(this.comboGol);
            this.groupManual.Controls.Add(this.label1);
            this.groupManual.Location = new System.Drawing.Point(12, 161);
            this.groupManual.Name = "groupManual";
            this.groupManual.Size = new System.Drawing.Size(402, 248);
            this.groupManual.TabIndex = 3;
            this.groupManual.TabStop = false;
            // 
            // textTmax
            // 
            this.textTmax.Location = new System.Drawing.Point(256, 214);
            this.textTmax.Name = "textTmax";
            this.textTmax.Size = new System.Drawing.Size(135, 20);
            this.textTmax.TabIndex = 16;
            // 
            // textTmin
            // 
            this.textTmin.Location = new System.Drawing.Point(256, 181);
            this.textTmin.Name = "textTmin";
            this.textTmin.Size = new System.Drawing.Size(135, 20);
            this.textTmin.TabIndex = 15;
            // 
            // textTe
            // 
            this.textTe.Location = new System.Drawing.Point(256, 150);
            this.textTe.Name = "textTe";
            this.textTe.Size = new System.Drawing.Size(135, 20);
            this.textTe.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 214);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(177, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Максимальная температура tmax";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 181);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(168, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Минимальная температура tmin";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 150);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(165, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Среднегодовая температура te";
            // 
            // comboWind
            // 
            this.comboWind.FormattingEnabled = true;
            this.comboWind.Items.AddRange(new object[] {
            "400",
            "500",
            "650",
            "800",
            "1000",
            "1250",
            "1500"});
            this.comboWind.Location = new System.Drawing.Point(256, 46);
            this.comboWind.Name = "comboWind";
            this.comboWind.Size = new System.Drawing.Size(135, 21);
            this.comboWind.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(217, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Нормативное ветровое давление W0, Па";
            // 
            // comboGol
            // 
            this.comboGol.FormattingEnabled = true;
            this.comboGol.Items.AddRange(new object[] {
            "10",
            "15",
            "20",
            "25",
            "30",
            "35",
            "40"});
            this.comboGol.Location = new System.Drawing.Point(256, 18);
            this.comboGol.Name = "comboGol";
            this.comboGol.Size = new System.Drawing.Size(137, 21);
            this.comboGol.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Нормативная толщина стенки гололеда bэ, мм";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.comboMestnType);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.comboNumLines);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.comboVoltage);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(420, 103);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Данные по ВЛ";
            // 
            // comboVoltage
            // 
            this.comboVoltage.FormattingEnabled = true;
            this.comboVoltage.Items.AddRange(new object[] {
            "0,4",
            "1",
            "3",
            "6",
            "10",
            "15",
            "20",
            "35",
            "110",
            "150",
            "220",
            "330",
            "500",
            "750"});
            this.comboVoltage.Location = new System.Drawing.Point(268, 19);
            this.comboVoltage.Name = "comboVoltage";
            this.comboVoltage.Size = new System.Drawing.Size(135, 21);
            this.comboVoltage.TabIndex = 0;
            this.comboVoltage.SelectedIndexChanged += new System.EventHandler(this.comboVoltage_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Напряжение, кВ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Количество цепей";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // comboNumLines
            // 
            this.comboNumLines.FormatString = "N0";
            this.comboNumLines.FormattingEnabled = true;
            this.comboNumLines.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.comboNumLines.Location = new System.Drawing.Point(268, 49);
            this.comboNumLines.Name = "comboNumLines";
            this.comboNumLines.Size = new System.Drawing.Size(135, 21);
            this.comboNumLines.TabIndex = 4;
            this.comboNumLines.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // textRegGol
            // 
            this.textRegGol.Location = new System.Drawing.Point(256, 80);
            this.textRegGol.Name = "textRegGol";
            this.textRegGol.Size = new System.Drawing.Size(135, 20);
            this.textRegGol.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(233, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Региональный коэффициент Yр по гололеду";
            // 
            // textRegVeter
            // 
            this.textRegVeter.Location = new System.Drawing.Point(256, 113);
            this.textRegVeter.Name = "textRegVeter";
            this.textRegVeter.Size = new System.Drawing.Size(135, 20);
            this.textRegVeter.TabIndex = 20;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 116);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(215, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Региональный коэффициент Yр по ветру";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 79);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Тип местности";
            // 
            // comboMestnType
            // 
            this.comboMestnType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMestnType.FormattingEnabled = true;
            this.comboMestnType.Items.AddRange(new object[] {
            "A",
            "B",
            "C"});
            this.comboMestnType.Location = new System.Drawing.Point(268, 76);
            this.comboMestnType.Name = "comboMestnType";
            this.comboMestnType.Size = new System.Drawing.Size(135, 21);
            this.comboMestnType.TabIndex = 6;
            // 
            // InputKlimatDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(444, 582);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.MaximumSize = new System.Drawing.Size(460, 620);
            this.MinimumSize = new System.Drawing.Size(460, 620);
            this.Name = "InputKlimatDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Климатические условия";
            this.groupCity.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupManual.ResumeLayout(false);
            this.groupManual.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupCity;
        private System.Windows.Forms.ComboBox comboCityName;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupManual;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboGol;
        private System.Windows.Forms.ComboBox comboWind;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.TextBox textTmax;
        private System.Windows.Forms.TextBox textTmin;
        private System.Windows.Forms.TextBox textTe;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox comboVoltage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboNumLines;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textRegVeter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textRegGol;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboMestnType;       
    }
}