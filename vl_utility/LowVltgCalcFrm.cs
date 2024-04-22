using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace vl_utility
{
    public partial class LowVltgCalcFrm : Form
    {
        private BindingSource bindingSource1 = new BindingSource();
        private DataTable tbl = new DataTable();
        public string db_path;
        public string databaseFolderName;

        public LowVltgCalcFrm(string DbPath, string dbFolderName)
        {
            InitializeComponent();
            
            this.db_path = DbPath;
            this.databaseFolderName = dbFolderName;
            ReloadTables(); 
        }

        private void ReloadTables()
        {
            bindingSource1.DataSource = tbl;
            dataGridViewLines.DataSource = bindingSource1;
        }

        private void dataGridViewLines_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode nd = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            string tag = nd.Tag.ToString();
            if (tag == "Detail")
            {
                
                //string FilePath = db_path.Replace(databaseFolderName, "") + nd.Parent.FullPath;
                string DtlName = nd.Text;
                DataRow row = VL_commands.GetDetail(DtlName);
                if (!CheckProvod(row)) return;
                //DataRow row = m_cnstr._details.Rows.Add(DtlName, "1");
                ReloadTables();
            }            
        }

        private void dataGridViewLines_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", true))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private bool CheckProvod(DataRow row)
        {
            string[] fields = { "Imax","Rph","Xph","Rnul","Xnul"};
            return CheckFields(row, fields);
        }

        private bool CheckFields(DataRow row, string[] fields)
        {
            foreach (string field in fields)
            {
                if (!row.Table.Columns.Contains(field)) return false;
                string val = row[field].ToString();
                if (val == "") return false;
            }
            return true;
        }
    }
}
