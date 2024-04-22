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
    public partial class SituationForm : Form
    {
        public SituationForm()
        {
            InitializeComponent();
            this.DataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            //for (int i = 0; i < 10; i++) DataGridView1.Rows.Add("Строка " + (i + 1), "и это тоже, но в другой колонке");	          
        }
        private void OK_Button_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
            //DataGridView1.Rows.Add();
        }

        private void Copy()
        {
            this.DataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            Clipboard.SetDataObject(this.DataGridView1.GetClipboardContent());
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();       
        }
       
        private void Paste()
        {            
            int row = DataGridView1.CurrentCell.RowIndex;
            int col = DataGridView1.CurrentCell.ColumnIndex;            
            string s = Clipboard.GetText();
            string[] lines = s.Split('\n');
                
                foreach (string line in lines)
                {
                    if (line.Length >0)
                    {
                        if ((row+1) == DataGridView1.RowCount)
                        {                            
                            DataGridView1.Rows.Add();                                                   
                        }
                        string[] cells = line.Split('\t');
                        for (int i = 0; i < cells.GetLength(0); ++i)
                        {
                            if (col + i <this.DataGridView1.ColumnCount)
                            {
                                DataGridView1[col + i, row].Value =Convert.ChangeType(cells[i], DataGridView1[col + i, row].ValueType);                                
                            }
                            else
                            {                                
                                break;
                            }
                        }
                        row++;
                    }
                    else
                    {                        
                        break;
                    }
                }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C) this.Copy();
            if (e.Control && e.KeyCode == Keys.V) this.Paste();
            if (e.KeyCode == Keys.Delete) 
            {
                foreach (DataGridViewCell cell in this.DataGridView1.SelectedCells)
                {
                    cell.Value = null;
                }
                
            }
            
        } 
    }
}
