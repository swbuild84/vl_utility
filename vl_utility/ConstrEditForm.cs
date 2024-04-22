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
    public partial class ConstrEditForm : Form
    {
        public string db_path;
        public string databaseFolderName;
        private bool m_modified = false;    //флажок изменений

        private ObjectId blockRefId = ObjectId.Null;   //id аттрибута блока
        
        public ObjectId BlockRefId
        {
            get { return blockRefId; }
            set { blockRefId = value; xmlFilePath = ""; }
        }

        private string xmlFilePath = "";

        public string XmlFilePath
        {
            get { return xmlFilePath; }
            set 
            {
                try
                {
                    xmlFilePath = value;
                    blockRefId = ObjectId.Null;
                    m_cnstr.OpenFile(xmlFilePath);
                    ReloadTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    this.Close();
                }
            }
        }

        private BindingSource bindingSource1 = new BindingSource();
        private BindingSource bindingSource2 = new BindingSource();
        private BindingSource bindingSource3 = new BindingSource();

        CConstruct m_cnstr = new CConstruct();

        public CConstruct Construct
        {
            get { return m_cnstr; }
            set 
            { 
                m_cnstr = value;
                ReloadTables();
            }
        }

        private void ReloadTables()
        {
            bindingSource1.DataSource = m_cnstr._fields;
            dataGridView1.DataSource = bindingSource1;

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            bindingSource2.DataSource = m_cnstr._details;
            dataGridView2.DataSource = bindingSource2;

            dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            bindingSource3.DataSource = m_cnstr._constructs;
            dataGridView3.DataSource = bindingSource3;

            dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView3.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

        public ConstrEditForm(string DbPath, string dbFolderName)
        {            
            InitializeComponent();

            this.db_path = DbPath;
            this.databaseFolderName = dbFolderName;

            ReloadTables();            
        }

        private void dataGridView2_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode nd = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            string tag = nd.Tag.ToString();
            if (tag == "Detail")
            {
                //string FilePath = db_path.Replace(databaseFolderName, "") + nd.Parent.FullPath;
                string DtlName = nd.Text;
                DataRow row = m_cnstr._details.Rows.Add(DtlName, "1");
                ReloadTables();
            }
            if (tag == "Construct")
            {
                //string FilePath = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                //CConstruct constr = new CConstruct();
                //constr.OpenFile(FilePath);
                //m_cnstr._constructs.Rows.Add(constr.ToString(), "1");
            }
            CheckDuplicates(dataGridView2);
        }

        private void dataGridView2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", true))
            {
                e.Effect=DragDropEffects.Copy;
            }
            else
            {
                e.Effect=DragDropEffects.None;
            }
        }

        private void SaveConstruct()
        {
            try
            {
                if ((blockRefId != ObjectId.Null) && (xmlFilePath == ""))
                {
                    //save to construct
                    Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
                       DocumentManager.MdiActiveDocument;
                    Database db = activeDoc.Database;
                    using (DocumentLock docLock = activeDoc.LockDocument())
                    {
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            BlockReference bRef = (BlockReference)tr.GetObject(blockRefId, OpenMode.ForWrite);
                            if (bRef != null)
                            {
                                Autodesk.AutoCAD.DatabaseServices.AttributeCollection attcol = bRef.AttributeCollection;
                                foreach (ObjectId att in attcol)
                                {
                                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForWrite);
                                    if (atRef.Tag == "XML_CONSTRUCT")
                                    {
                                        atRef.TextString = m_cnstr.SaveToString();
                                    }
                                }
                            }
                            tr.Commit();
                        }
                    }
                }
                if ((blockRefId == ObjectId.Null) && (xmlFilePath != ""))
                {
                    //Save into file
                    m_cnstr.SaveToFile(xmlFilePath);
                }
                m_modified = false;
            }
                
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveConstruct();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_cnstr.SaveToFile(dlg.FileName);
                    //ReloadTables();
                    XmlFilePath = dlg.FileName;
                    this.Text = dlg.FileName;
                    m_modified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void dataGridView2_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView2);
        }

        private void dataGridView2_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView2);
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView2);            
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
            //Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
            //      DocumentManager.MdiActiveDocument;
            //IntPtr h = activeDoc.Window.Handle;
            //Win32.ShowWindow(h, 5);
            //this.Close();
            //SetForegroundWindow(h);
            //SetFocus(h);
        }
        private void CheckDuplicates(DataGridView dgv)
        {
            List<object> vals = new List<object>();
            foreach (DataGridViewRow dgvr in dgv.Rows)
            {
                object val=dgvr.Cells[0].Value;
                int index = vals.IndexOf(val);
                if (index == -1)
                {
                    dgvr.DefaultCellStyle.ForeColor = Color.Black;
                }
                if (index > -1)                    
                {
                    dgvr.DefaultCellStyle.ForeColor = Color.Red;
                    dgv.Rows[index].DefaultCellStyle.ForeColor = Color.Red;
                }
                vals.Add(val);
            }
        }

        private void dataGridView3_DragEnter(object sender, DragEventArgs e)
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

        private void dataGridView3_DragDrop(object sender, DragEventArgs e)
        {            
            TreeNode nd = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            string tag = nd.Tag.ToString();            
            if (tag == "Construct")
            {
                string FilePath = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                CConstruct constr = new CConstruct();
                constr.OpenFile(FilePath);
                if (constr.HasConstructs())
                {
                    MessageBox.Show("Construct has childs!");
                    return;
                }                
                m_cnstr._constructs.Rows.Add(System.IO.Path.GetFileNameWithoutExtension(FilePath), "1", constr.SaveToString());
                //m_cnstr._listConstructs.Add(constr);
                //constr.ReadFromString(m_cnstr._constructs.Rows[0][2].ToString());
            }
            CheckDuplicates(dataGridView3);
        }

        private void dataGridView3_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView3);
        }

        private void dataGridView3_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView3);
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_modified = true;
            CheckDuplicates(dataGridView3);
        }

        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int col = e.ColumnIndex;
            int row = e.RowIndex;
            if (col == 2)
            {
                string xmlConstr = dataGridView3.Rows[row].Cells[col].Value.ToString();
                CConstruct constr = new CConstruct();
                constr.ReadFromString(xmlConstr);

                /*ConstrEditForm frm = new ConstrEditForm(db_path, databaseFolderName);
                string FilePath = db_path.Replace(databaseFolderName, "") + clickedNode.FullPath;
                frm.XmlFilePath = FilePath;
                frm.Text = FilePath;
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(frm);*/
                
                //show editor form
                //no modal dialog
                ConstrEditForm frm = new ConstrEditForm(db_path, databaseFolderName);
                frm.Construct = constr; 
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
            }

        }

        private void toolStripButtonSAVEAS_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = db_path;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_cnstr.SaveToFile(dlg.FileName);
                    //ReloadTables();
                    XmlFilePath = dlg.FileName;
                    this.Text = dlg.FileName;
                    m_modified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlFilePath = dlg.FileName;
                    //delete old construct
                    m_cnstr = new CConstruct();
                    m_cnstr.OpenFile(xmlFilePath);
                    ReloadTables();
                    this.Text = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveConstruct();
        }

        private void MoveRowUpToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.GetCurDataGridView();
                int irow = dgv.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;

                BindingSource  source = dgv.DataSource as BindingSource;
                System.Data.DataTable table = source.DataSource as System.Data.DataTable;
                if (table == null) return;
                {
                    if (irow == 0) return;
                    DataRow prevRow = table.Rows[irow];
                    DataRow newRow = table.NewRow();
                    newRow.ItemArray = prevRow.ItemArray;
                    table.Rows.Remove(prevRow);
                    table.Rows.InsertAt(newRow, irow - 1);
                    ReloadTables();
                    dgv.CurrentCell = dgv.Rows[irow - 1].Cells[icol];
                }
            }
            catch (Exception)
            {               
                
            }            
        }

        private void MoveRowDownToolStripButton_Click(object sender, EventArgs e)
        {

            try
            {                
                DataGridView dgv = this.GetCurDataGridView(); 
                int irow = dgv.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;

                BindingSource source = dgv.DataSource as BindingSource;
                System.Data.DataTable table = source.DataSource as System.Data.DataTable;
                if (table == null) return;
                {
                    if (irow == table.Rows.Count - 1) return;
                    DataRow prevRow = table.Rows[irow];
                    DataRow newRow = table.NewRow();
                    newRow.ItemArray = prevRow.ItemArray;
                    table.Rows.Remove(prevRow);
                    table.Rows.InsertAt(newRow, irow + 1);
                    ReloadTables();
                    dgv.CurrentCell = dgv.Rows[irow + 1].Cells[icol];
                }
            }
            catch (Exception)
            {

            }         
        }

        private void DeleteRowToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.GetCurDataGridView();
                int irow = dgv.CurrentCell.RowIndex;
                BindingSource source = dgv.DataSource as BindingSource;
                System.Data.DataTable table = source.DataSource as System.Data.DataTable;
                if (table == null) return;
                {
                    DataRow curRow = table.Rows[irow];
                    table.Rows.Remove(curRow);
                    ReloadTables();
                }
            }
            catch (Exception)
            {                
                
            }
        }

        private void AddRowStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.GetCurDataGridView();
                int irow = dgv.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;
                BindingSource source = dgv.DataSource as BindingSource;
                System.Data.DataTable table = source.DataSource as System.Data.DataTable;
                if (table == null) return;
                {
                    DataRow newRow = table.NewRow();
                    table.Rows.InsertAt(newRow, irow);
                    ReloadTables();
                    dgv.CurrentCell = dgv.Rows[irow].Cells[icol];
                }
            }
            catch (Exception)
            {

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

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            this.Cut();
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            this.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            this.Paste();
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

        private void Cut()
        {
            try
            {
                //copy
                DataGridView dgv = this.GetCurDataGridView();
                dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                Clipboard.SetDataObject(dgv.GetClipboardContent());
                //delete
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    cell.Value = "";
                }
                ReloadTables();
            }
            catch (Exception)
            {
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.C) this.Copy();
                if (e.Control && e.KeyCode == Keys.V) this.Paste();
                if (e.KeyCode == Keys.Delete)
                {
                    foreach (DataGridViewCell cell in this.dataGridView1.SelectedCells)
                    {
                        cell.Value = null;
                    }

                }
            }
            catch (Exception)
            {
                
            }
        }

        private void dataGridView2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.C) this.Copy();
                if (e.Control && e.KeyCode == Keys.V) this.Paste();
                if (e.KeyCode == Keys.Delete)
                {
                    foreach (DataGridViewCell cell in this.dataGridView2.SelectedCells)
                    {
                        cell.Value = null;
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        private void dataGridView3_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.C) this.Copy();
                if (e.Control && e.KeyCode == Keys.V) this.Paste();
                if (e.KeyCode == Keys.Delete)
                {
                    foreach (DataGridViewCell cell in this.dataGridView3.SelectedCells)
                    {
                        cell.Value = null;
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        private void dataGridView2_MouseUp(object sender, MouseEventArgs e)
        {

            // Load context menu on right mouse click
            DataGridView.HitTestInfo hitTestInfo;
            if (e.Button == MouseButtons.Right)
            {
                hitTestInfo = dataGridView2.HitTest(e.X, e.Y);
                // If column is first column
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell && hitTestInfo.ColumnIndex == 0)
                {                    
                    string detail = dataGridView2.Rows[hitTestInfo.RowIndex].Cells[0].Value.ToString();
                    DataRow row = VL_commands.GetDetail(detail);
                    List<MenuItem> mnitems = new List<MenuItem>();
                    if (row != null)
                    {
                        
                        if (UserControl1.CheckFields(row, new string[] { "HREF1" }))
                        {
                            mnitems.Add(new MenuItem(row["HREF1"].ToString(), new System.EventHandler(DetailContext_Click)));
                            if (UserControl1.CheckFields(row, new string[] { "HREF2" }))
                            {
                                mnitems.Add(new MenuItem(row["HREF2"].ToString(), new System.EventHandler(DetailContext_Click)));
                            }
                        }
                    }
                    if (mnitems.Count > 0)
                    {
                        ContextMenu mnu = new ContextMenu(mnitems.ToArray());
                        mnu.Show(dataGridView2, new Point(e.X, e.Y));
                    } 
                    
                }
                
            }
        }

        private void DetailContext_Click(object sender, EventArgs e)
        {
            try
            {
                MenuItem itm = (MenuItem)sender;
                string href = itm.Text;                
                if (href.StartsWith("\\"))
                {
                    DataGridView dgv = this.dataGridView2;
                    int irow = dgv.CurrentCell.RowIndex;
                    int icol = dataGridView1.CurrentCell.ColumnIndex;
                    string dtlname = dgv.Rows[irow].Cells[0].Value.ToString();

                    TreeNode[] nodes = VL_commands.m_usrform.treeView1.Nodes.Find(dtlname, true);
                    if (nodes.Length != 1) return;
                    string fullPath = nodes[0].Parent.FullPath;
                    string FilePath = db_path.Replace(databaseFolderName, "") + fullPath;
                    href = FilePath + href;
                }
                System.Diagnostics.Process.Start(href);
            }
            catch (Exception)
            {  
            }           
        }

        private void SearchStripButton_Click(object sender, EventArgs e)
        {
            int tab = this.tabControl1.SelectedIndex;
            if (tab != 1) return;
            vl_utility.VlineForms.DbSearchForm frm = new vl_utility.VlineForms.DbSearchForm();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
            if (frm.DialogResult == DialogResult.OK)
            {
                string DtlName = frm.retDtlFullName;
                DataRow row = m_cnstr._details.Rows.Add(DtlName, "1");
                ReloadTables();
                //dataGridView2_CellBeginEdit(dataGridView2, new DataGridViewCellCancelEventArgs(1,2));
            }
        }

        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

        }
        
    }
}
