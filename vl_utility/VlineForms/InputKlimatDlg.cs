using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VlineLib;

namespace vl_utility
{
    public partial class InputKlimatDlg : Form
    {
        private string _dataBasePath;   //путь к базе данных(каталог)

        private CVLine m_line = null;    //указатель на ВЛ

        public CVLine Line
        {
            get { return m_line; }
            set { m_line = value; }
        }

        
        public InputKlimatDlg(string databasepath)
        {
            _dataBasePath = databasepath;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                CSVReader rdr2 = new CSVReader(_dataBasePath + "\\РКУ.csv");
                rdr2.FillCombo("Наименование", this.comboCityName, ';');

                if (m_line == null) throw new NotImplementedException();
                //Get Current line object   
                if (m_line.CityName != null)
                {
                    comboCityName.SelectedItem = m_line.CityName;
                    radioButton1.Checked = true;
                }
                else
                {
                    radioButton2.Checked = true;
                }
                if (m_line.VoltageClass > 0) comboVoltage.Text = m_line.VoltageClass.ToString();
                comboNumLines.Text = m_line.NumLines.ToString();
                comboMestnType.SelectedItem = m_line.MestnType - 1;
                comboGol.Text = m_line.GolNorm.ToString();
                comboWind.Text = m_line.VeterNorm.ToString();
                textRegGol.Text = m_line.GammaRegionGol.ToString();
                textRegVeter.Text = m_line.GammaRegionVeter.ToString();
                textTe.Text = m_line.Te.ToString();
                textTmin.Text = m_line.Tmin.ToString();
                textTmax.Text = m_line.Tmax.ToString();    
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message);
            }

            base.OnLoad(e);           
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            try
            {                
                //check input
                CheckTextValue(textTe, -50, 50, "Среднегодовая температура не соответствует ПУЭ");
                CheckTextValue(textTmin, -100, 100, "Минимальная температура не соответствует ПУЭ");
                CheckTextValue(textTmax, -100, 100, "Максимальная температура не соответствует ПУЭ");

                if (comboVoltage.Text == "") throw new Exception("Выберите напряжение линии");
                if (radioButton1.Checked && comboCityName.SelectedIndex == -1) throw new Exception("Выберите населенный пункт");
                if (comboGol.Text=="") throw new Exception("Выберите " + label1.Text);
                if (comboWind.Text == "") throw new Exception("Выберите " + label2.Text);
                

                //set params
                if (radioButton1.Checked) m_line.CityName = comboCityName.Text;
                if (radioButton2.Checked) m_line.CityName = null;

                m_line.VoltageClass = MyConvert.ToDouble(comboVoltage.Text);
                m_line.NumLines = Convert.ToInt32(comboNumLines.Text);
                m_line.MestnType = (byte)(comboMestnType.SelectedIndex + 1);
                m_line.GolNorm = MyConvert.ToDouble(comboGol.Text);
                m_line.VeterNorm = MyConvert.ToDouble(comboWind.Text);
                m_line.GammaRegionGol = MyConvert.ToDouble(textRegGol.Text);
                m_line.GammaRegionVeter = MyConvert.ToDouble(textRegVeter.Text);
                m_line.Te =MyConvert.ToDouble(textTe.Text);
                m_line.Tmin = MyConvert.ToDouble(textTmin.Text);
                m_line.Tmax = MyConvert.ToDouble(textTmax.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {

        }
      
        void CheckTextValue(TextBox txtBox, double minVal, double maxVal, string errorstring)
        {
            if (txtBox.Text == "") throw new Exception(errorstring);
            double val = MyConvert.ToDouble(txtBox.Text);
            if (val < minVal || val > maxVal) throw new Exception(errorstring);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupCity.Enabled= radioButton1.Checked;
            groupManual.Enabled = !radioButton1.Checked; 
        }

        private void comboCityName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //fill database values
            CSVReader rdr = new CSVReader(_dataBasePath + "\\РКУ.csv");

            //this.comboGol.SelectedIndex =Convert.ToInt32(rdr.QueryByValue("Наименование", this.comboCityName.Text, "Район по гололеду", ';'))-1;
            //this.comboWind.SelectedIndex = Convert.ToInt32(rdr.QueryByValue("Наименование", this.comboCityName.Text, "Район по ветру", ';')) - 1;
            //this.comboWindGol.SelectedIndex = Convert.ToInt32(rdr.QueryByValue("Наименование", this.comboCityName.Text, "Район по ветру во время гололеда", ';')) - 1;
            //this.comboGolWind10.SelectedIndex = Convert.ToInt32(rdr.QueryByValue("Наименование", this.comboCityName.Text, 
            //    "Район по ветру во время гололеда на провода и тросы 10 мм", ';')) - 1;

            //this.textTe.Text = rdr.QueryByValue("Наименование", this.comboCityName.Text, "Среднегодовая температура", ';');
            //this.textTmax.Text = rdr.QueryByValue("Наименование", this.comboCityName.Text, "Максимальная температура", ';');
            //this.textTmin.Text = rdr.QueryByValue("Наименование", this.comboCityName.Text, "Минимальная температура", ';');
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboVoltage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
