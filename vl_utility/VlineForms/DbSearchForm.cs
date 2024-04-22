using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace vl_utility.VlineForms
{
    public partial class DbSearchForm : Form
    {
        public string retDtlFullName = "";
        public string retDtlPath = "";

        public DbSearchForm()
        {
            InitializeComponent();
        }

        public DbSearchForm(string textBoxInpStr)
        {
            InitializeComponent();
            this.textBox1.Text = textBoxInpStr;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
            string inptStr = textBox1.Text;

            DataTable report = new DataTable();
            report.Columns.Add("Полное имя");
            report.Columns.Add("Наименование");
            report.Columns.Add("Путь");

            string search = "ПОЛНОЕ_ИМЯ LIKE '" + "*" + inptStr + "*" + "' OR НАИМЕНОВАНИЕ LIKE '" + "*" + inptStr + "*"+ "'";
            //DataTable tbl = VL_commands.GetQueryDetail(search);
            foreach (DictionaryEntry pair in VL_commands.m_usrform.FileMD5DetsTable)
            {
                string dbPath = VL_commands.m_usrform.db_path;
                System.Data.DataTable tbl1 = (pair.Value as HashFilesTable).table;
                try
                {
                    DataRow[] rows = tbl1.Select(search);
                    if (rows.Length > 0)
                    {
                        foreach (DataRow row in rows)
                        {
                            string fullPath = pair.Key as string;
                            DataRow newRow = report.NewRow();
                            newRow[0] = row["ПОЛНОЕ_ИМЯ"].ToString();
                            newRow[1] = row["НАИМЕНОВАНИЕ"].ToString();
                            newRow[2] = fullPath.Replace(dbPath, "");
                            report.Rows.Add(newRow);
                        }
                    }
                }
                catch (Exception)
                {   
                }
            }
            //Fill form
            dataGridView1.DataSource = report;            
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {            
            retDtlFullName = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value.ToString();
            retDtlPath = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
