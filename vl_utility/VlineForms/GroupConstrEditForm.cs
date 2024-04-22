using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;


namespace vl_utility
{ 
    public partial class GroupConstrEditForm : Form
    {
        private bool m_modified = false;    //флажок изменений
        ObjectId curRefid = ObjectId.Null;
        public List<CConstruct> cnstrList;  //собственно список с конструкциями
        public List<string> _cnstrXmlList;
        System.Data.DataTable tbl1;
        System.Data.DataTable tbl2;
        System.Data.DataTable tbl3;
        private BindingSource bindingSource1 = new BindingSource();
        private BindingSource bindingSource2 = new BindingSource();
        private BindingSource bindingSource3 = new BindingSource();

        private void ReloadTables()
        {
            bindingSource1.DataSource = tbl1;
            dataGridView1.DataSource = bindingSource1;

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            bindingSource2.DataSource = tbl2;
            dataGridView2.DataSource = bindingSource2;

            dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            bindingSource3.DataSource = tbl3;
            dataGridView3.DataSource = bindingSource3;

            dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView3.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

        }

        public GroupConstrEditForm(System.Data.DataTable Table1, System.Data.DataTable Table2, System.Data.DataTable Table3, 
            List<CConstruct> CnstrList, List<string> cnstrXmlList)
        {            
            InitializeComponent();

            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView2.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView3.ContextMenuStrip = this.contextMenuStrip1;

            this.tbl1 = Table1;
            this.tbl2 = Table2;
            this.tbl3 = Table3;
            this.cnstrList = CnstrList;
            _cnstrXmlList = cnstrXmlList;
            ReloadTables();            
        }
        
        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_modified = true;            
        }

        private void ConstrEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (m_modified)
            //{
            //    Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
            //          DocumentManager.MdiActiveDocument;
            //    DialogResult res = MessageBox.Show(this,
            //        "Сохранить изменения?", "Construct", MessageBoxButtons.YesNoCancel);
            //    if (res == DialogResult.Yes)
            //    {
            //        SaveConstruct();
            //    }
            //    if (res == DialogResult.No)
            //    {
            //        //do nothing                   
            //    }
            //    if (res == DialogResult.Cancel)
            //    {
            //        e.Cancel = true;
            //    }
            //    //this.Owner = activeDoc.Window as Form;
            //}
        }

        private void ConstrEditForm_Load(object sender, EventArgs e)
        {

        }

        private void ConstrEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
                  DocumentManager.MdiActiveDocument;
            activeDoc.Editor.Regen();
            //IntPtr h = activeDoc.Window.Handle;
            //Win32.ShowWindow(h, 5);
            //this.Close();
            //SetForegroundWindow(h);
            //SetFocus(h);
        }
        
        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_modified = true;            
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cnstrList != null)
                {
                    for (int i = 0; i < cnstrList.Count; i++)
                    {
                        CConstruct cnstr = cnstrList[i];
                        //fields
                        cnstr._fields.Clear();
                        for (int j = 0; j < this.tbl1.Rows.Count; j++)
                        {
                            string field = tbl1.Rows[j][0].ToString();
                            string val = tbl1.Rows[j][i + 1].ToString();
                            if (val != "")
                            {
                                DataRow newRow = cnstr._fields.NewRow();
                                newRow[0] = field;
                                newRow[1] = val;
                                cnstr._fields.Rows.Add(newRow);
                            }
                        }
                        //details
                        cnstr._details.Clear();
                        for (int j = 0; j < this.tbl2.Rows.Count; j++)
                        {
                            string field = tbl2.Rows[j][0].ToString();
                            string val = tbl2.Rows[j][i + 1].ToString();
                            if (val != "")
                            {
                                DataRow newRow = cnstr._details.NewRow();
                                newRow[0] = field;
                                newRow[1] = val;
                                cnstr._details.Rows.Add(newRow);
                            }
                        }
                        //constructs
                        cnstr._constructs.Clear();
                        for (int j = 0; j < this.tbl3.Rows.Count; j++)
                        {
                            string name = tbl3.Rows[j][0].ToString();
                            string qty = tbl3.Rows[j][i + 1].ToString();
                            if (qty != "")
                            {
                                DataRow newRow = cnstr._constructs.NewRow();
                                newRow[0] = name;
                                newRow[1] = qty;
                                newRow[2] = _cnstrXmlList[j];
                                cnstr._constructs.Rows.Add(newRow);
                            }
                        }
                        //save blockrefernces
                        cnstr.SaveToBlockRef();

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            try
            {
                DataGridView dgv = this.GetCurDataGridView();
                dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                Clipboard.SetDataObject(dgv.GetClipboardContent());
            }
            catch (Exception)
            {
            }
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void Paste()
        {
             try
            {
                DataGridView dgv = this.GetCurDataGridView();
                int row = dgv.CurrentCell.RowIndex;
                int col = dgv.CurrentCell.ColumnIndex;

                BindingSource source = dgv.DataSource as BindingSource;
                System.Data.DataTable tbl = source.DataSource as System.Data.DataTable;
                if (tbl == null) return;

                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');

                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        if ((row) == tbl.Rows.Count)
                        {
                            DataRow Drow = tbl.NewRow();
                            tbl.Rows.Add(Drow);
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
                ReloadTables();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        
        }

        private DataGridView GetCurDataGridView()
        {
            int tab = this.tabControl1.SelectedIndex;
            DataGridView dgv = null;
            switch (tab)
            {
                case 0: { dgv = dataGridView1; break; }
                case 1: { dgv = dataGridView2; break; }
                case 2: { dgv = dataGridView3; break; }
                default: throw new Exception("index not found;");
            }
            return dgv;
        }

        private void ZoomToBlock()
        {
            try
            {
                DataGridView dgv = this.GetCurDataGridView();                
                int col = dgv.CurrentCell.ColumnIndex;
                if (this.cnstrList != null)
                {                    
                    if (col > 0)
                    {
                        ObjectId id = cnstrList[col - 1].bRef.ObjectId;
                        if (!(id == ObjectId.Null || id == this.curRefid))                        
                        {
                            this.curRefid = id;
                            vl_utility.VL_commands.ViewEntityPos(id);
                        }
                    }
                }

            }
            catch (Exception ex)
            {                
                MessageBox.Show(ex.ToString());
            }
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            ZoomToBlock();
        }

        private void dataGridView2_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            ZoomToBlock();
        }

        private void dataGridView3_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            ZoomToBlock();
        }

    }
}
