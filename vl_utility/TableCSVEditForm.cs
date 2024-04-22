using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Autodesk.AutoCAD.Windows.Data;

namespace vl_utility
{
    public partial class TableCSVEditForm : Form
    {
        string m_filepath = "";
        bool m_modified = false;
        DataTable tbl;
        private BindingSource bindingSource1 = new BindingSource();
 
        public TableCSVEditForm(string FilePath, string detailname)
        {
            m_filepath = FilePath;
            
            if (!File.Exists(m_filepath)) throw new FileNotFoundException();

            InitializeComponent();
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.Text = Path.GetFileName(m_filepath);

            CSVReader rdr = new CSVReader(m_filepath);
            tbl = rdr.GetDataTable('\t');
            bindingSource1.DataSource = tbl;
            this.dataGridView1.DataSource = bindingSource1;
            for(int i=0; i<tbl.Rows.Count; i++)
            {
                if(tbl.Rows[i]["ПОЛНОЕ_ИМЯ"].ToString()==detailname)
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];
                }
            }
        }

        private void AddColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                InputBox frm = new InputBox();
                frm.SetLabel("Введите имя столбца: ");
                DialogResult res = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);                
                if (res == DialogResult.OK)
                {
                    tbl.Columns.Add(frm.GetText());                   
                    m_modified = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_modified = true;
        }

        private void TableCSVEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_modified)
            {                
                DialogResult res = MessageBox.Show(this,
                    "Сохранить изменения?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    Save();
                    this.DialogResult = DialogResult.Yes;
                }
                if (res == DialogResult.No)
                {
                    //do nothing 
                    this.DialogResult = DialogResult.No;
                }
                if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                
            }
        }

        private void Save()
        {
            using (StreamWriter sw = new StreamWriter(m_filepath, false, Encoding.Unicode))
            {
                string line = "";
                for (int j = 0; j < tbl.Columns.Count-1; j++)
                {
                    line += tbl.Columns[j].ColumnName + "\t";
                }
                line += tbl.Columns[tbl.Columns.Count - 1].ColumnName;
                sw.WriteLine(line);

                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    line = "";
                    for (int j = 0; j < tbl.Columns.Count-1; j++)
                    {
                        line += tbl.Rows[i][j] + "\t";
                    }
                    line += tbl.Rows[i][tbl.Columns.Count - 1];
                    sw.WriteLine(line);
                }                
            }
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            this.dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            Clipboard.SetDataObject(this.dataGridView1.GetClipboardContent());
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void Paste()
        {
            try
            {
                int row = dataGridView1.CurrentCell.RowIndex;
                int col = dataGridView1.CurrentCell.ColumnIndex;
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        if ((row) == tbl.Rows.Count)
                        {
                            DataRow Drow = tbl.NewRow();

                            //bindingSource1.ResetBindings(true);
                            //dataGridView1_CellValueChanged(this.dataGridView1,new DataGridViewCellEventArgs(col,row));

                            tbl.Rows.Add(Drow);
                            //dataGridView1.Rows.Add();
                        }
                        string[] cells = line.Split('\t');
                        for (int i = 0; i < cells.GetLength(0); ++i)
                        {
                            if (col + i < tbl.Columns.Count)
                            {
                                tbl.Rows[row][col + i] = cells[i];
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
                m_modified = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            m_modified = true;
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            m_modified = true;
        }

        private void TableCSVEditForm_Load(object sender, EventArgs e)
        {
            //add events
            this.dataGridView1.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dataGridView1_RowsRemoved);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnAdded);
        }
    }
}
