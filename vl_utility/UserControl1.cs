using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using System.Drawing;
using System.Resources;
using Autodesk.AutoCAD.ApplicationServices;
using System.Diagnostics;
using System.Collections;
using System.Security.Cryptography;

namespace vl_utility
{
    public class CustomTextBox : System.Windows.Forms.ToolStripTextBox
    {
        public EventHandler EnterKeyDownEventHandler;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
           if (keyData == Keys.Enter)
           {
              //Escape key press, handle it your way but be sure to return true
               EnterKeyDownEventHandler(this, EventArgs.Empty);
              return true;
           }
           else
           {
              return false;
           }
        }
    }

    public partial class UserControl1 : UserControl
    {
        public string db_path;
        ImageList list = new ImageList();
        private const string databaseFolderName = "Database";
        TreeNode m_rightSelectedNode = null;
        public Hashtable FileMD5DetsTable = new Hashtable();
        private Hashtable FilesTable = new Hashtable();
        private static Form imgFrm = new Form();

        public UserControl1()
        {            
            InitializeComponent();

            try
            {
                string[] commands = { "vl_specification", "vl_poopor", "vl_cedit", "vl_num", "vl_index", "vl_matchpr" ,"calcmech", "gredit"};
                ToolStripMenuItem[] items = new ToolStripMenuItem[commands.Length];
                for (int i = 0; i < commands.Length; i++)
                {
                    items[i] = new ToolStripMenuItem();
                    items[i].Name = commands[i];
                    items[i].Tag = commands[i];
                    items[i].Text = commands[i];
                    items[i].Click += new System.EventHandler(CommandMenuItemClickHandler);                    
                }                
                this.toolStripMenuItem2.DropDownItems.AddRange((ToolStripItem[]) items);


                //Assembly assem = Assembly.GetExecutingAssembly();
                //string name = assem.ManifestModule.Name;
                //db_path = assem.Location.Replace(name, "") + databaseFolderName;

                
                list.Images.Add(Resource1.folder);
                list.Images.Add(Resource1.folder);  
                list.Images.Add(Resource1.file);
                list.Images.Add(Resource1.detail);
                list.Images.Add(Resource1.construct);
                treeView1.ImageList = list;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void CommandMenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (aDoc == null) return;
            aDoc.SendStringToExecute(clickedItem.Text + "\n", true, false, true);

        }
        private void DirBuild(string sDir, TreeNode root, string fileMask)
        {            
            try
            {
                //folders in the root
                foreach (string dir in Directory.GetDirectories(sDir))
                {
                    TreeNode nd = root.Nodes.Add(dir.Replace(sDir + "\\", ""));
                    nd.Tag = "Folder";                    
                    DirBuild(dir, nd, fileMask);
                }

                //files in the root                
                foreach (string file in Directory.GetFiles(sDir, fileMask))
                {
                    string shortFile = file.Replace(sDir + "\\", "");
                    TreeNode fileNd = root.Nodes.Add(shortFile);
                    fileNd.Tag = "File";
                    fileNd.ImageIndex = 2;
                    fileNd.SelectedImageIndex = 2;

                    //calc md5
                    string md5string = "";
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(file))
                        {
                            md5string = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                    }
                    FilesTable.Add(file, md5string);

                    if (FileMD5DetsTable.Contains(file))
                    {
                        HashFilesTable obj = FileMD5DetsTable[file] as HashFilesTable;
                        string hash1 = obj.hash;
                        if (hash1 == md5string)
                        {
                            //fill nodes
                            fileNd.Nodes.AddRange(obj.nodes.ToArray());
                        }
                        else
                        {
                            //remove not correct key
                            FileMD5DetsTable.Remove(file);
                            //add key and read file
                            HashFilesTable NewObj = ReadFile(file, md5string);
                            FileMD5DetsTable.Add(file, NewObj);
                            fileNd.Nodes.AddRange(NewObj.nodes.ToArray());
                        }

                    }
                    else
                    {
                        //add key and read file
                        HashFilesTable NewObj = ReadFile(file, md5string);
                        FileMD5DetsTable.Add(file, NewObj);
                        fileNd.Nodes.AddRange(NewObj.nodes.ToArray());
                    }

                    //foreach (DictionaryEntry pair in FilesTable)
                    //{
                    //    string key2 = pair.Key as string;
                    //    string hash2 = pair.Value as string;

                    //    if (FileMD5DetsTable.Contains(key2))
                    //    {
                    //        HashFilesTable obj = FileMD5DetsTable[key2] as HashFilesTable;
                    //        string hash1 = obj.hash;
                    //        if (hash1 == hash2)
                    //        {
                    //            //do nothing
                    //        }
                    //        else
                    //        {
                    //            //remove not correct key
                    //            FileMD5DetsTable.Remove(key2);
                    //            //add key and read file
                    //            FileMD5DetsTable.Add(pair.Key, ReadFile(key2, hash2));
                    //        }

                    //    }
                    //    else
                    //    {
                    //        //add key and read file
                    //        FileMD5DetsTable.Add(pair.Key, ReadFile(key2, hash2));
                    //    }

                    //}
                    ////fast refresh
                    //bool needToread = true;
                    //if (FileMD5Table.Contains(file))
                    //{      
                    //    string md5str=FileMD5Table[file] as string;
                    //    if (md5str == md5string)
                    //    {
                    //        needToread = false; //don't read file, load it!
                    //    }
                    //    else
                    //    {
                    //        //Файл изменился
                    //        FileMD5Table.Remove(file);
                    //        FileDtlTable.Remove(file);
                    //    }
                    //}
                    //if(needToread)
                    //{
                    //    //read tree nodes in collection
                    //    CSVReader rdr = new CSVReader(file);
                    //    DataTable TDetails = rdr.GetDataTable('\t');

                    //    List<TreeNode> nodes = new List<TreeNode>();
                    //    foreach (DataRow row in TDetails.Rows)
                    //    {
                    //        string detail = row["ПОЛНОЕ_ИМЯ"].ToString();
                    //        TreeNode dtlNode = new TreeNode(detail);                            
                    //        try
                    //        {
                    //            string href1 = row["HREF1"].ToString();
                    //            if (href1 != "")
                    //            {
                    //                ContextMenu mnu = new ContextMenu(new MenuItem[] { new MenuItem(href1, 
                    //                new System.EventHandler(DetailContext_Click))});
                    //                dtlNode.ContextMenu = mnu;

                    //                string href2 = row["HREF2"].ToString();
                    //                if (href2 != "")
                    //                {
                    //                    mnu.MenuItems.Add(new MenuItem(href2, new System.EventHandler(DetailContext_Click)));
                    //                }
                    //            }
                    //        }
                    //        catch (ArgumentException)
                    //        {

                    //        }
                            
                    //        dtlNode.Tag = "Detail";
                    //        dtlNode.ImageIndex = 3;
                    //        dtlNode.SelectedImageIndex = 3;
                    //        nodes.Add(dtlNode);
                    //    }
                    //    //fill hashtables
                    //    FileMD5Table.Add(file, md5string);
                    //    FileDtlTable.Add(file, nodes);
                    //}
                    //List<TreeNode> TreeList = FileDtlTable[file] as List<TreeNode>;
                    //fileNd.Nodes.AddRange(TreeList.ToArray());                    
                }//foreach file                
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private HashFilesTable ReadFile(string file, string hash)
        {
            //Предполагаем, что файл и хэш сумма уже проверены!

            HashFilesTable resObj = new HashFilesTable();
            resObj.hash = hash;

            CSVReader rdr = new CSVReader(file);
            DataTable TDetails = rdr.GetDataTable('\t');
            resObj.table = TDetails.Copy();

            List<TreeNode> nodes = resObj.nodes;
            foreach (DataRow row in TDetails.Rows)
            {
                string detail = row["ПОЛНОЕ_ИМЯ"].ToString();
                TreeNode dtlNode = new TreeNode(detail);

                List<MenuItem> mnitems = new List<MenuItem>();
                if(CheckFields(row, new string[] {"HREF1"}))
                {                   
                    mnitems.Add(new MenuItem(row["HREF1"].ToString(),new System.EventHandler(DetailContext_Click)));
                    if (CheckFields(row, new string[] { "HREF2" }))
                    {
                        mnitems.Add(new MenuItem(row["HREF2"].ToString(), new System.EventHandler(DetailContext_Click)));
                    }
                }                

                //Если деталь - провод с набором параметров для механического расчета
                //добавляем в контекстное меню возможность создания слоя провода
                if (CheckProvod(row))
                {
                    MenuItem itm = new MenuItem("Cоздать слой", new System.EventHandler(CreateLayer_Click));
                    itm.Tag = detail;
                    mnitems.Add(itm);
                }
                //Add contex menus
                if (mnitems.Count > 0)
                {
                    ContextMenu mnu = new ContextMenu(mnitems.ToArray());
                    dtlNode.ContextMenu = mnu;
                }
                dtlNode.Tag = "Detail";
                dtlNode.ImageIndex = 3;
                dtlNode.SelectedImageIndex = 3;
                nodes.Add(dtlNode);
            }
            return resObj;
        }

        private bool CheckProvod(DataRow row)
        {
            string[] fields = { "МАССА","D","S","E","ALPHA","SIGMA_R","SIGMA_MAX",
                                  "SIGMA_SRED"};
            return CheckFields(row, fields);
        }

        public static bool CheckFields(DataRow row, string[] fields)
        {
            foreach (string field in fields)
            {
                if (!row.Table.Columns.Contains(field)) return false;
                string val = row[field].ToString();
                if (val == "") return false;
            }
            return true;
        }

        private void ConstructsBuild(string sDir, TreeNode root, string fileMask)
        {
            try
            {
                //folders in the root
                foreach (string dir in Directory.GetDirectories(sDir))
                {
                    TreeNode nd = root.Nodes.Add(dir.Replace(sDir + "\\", ""));
                    nd.Tag = "Folder";
                    ConstructsBuild(dir, nd, fileMask);
                }

                //files in the root
                foreach (string file in Directory.GetFiles(sDir, fileMask))
                { 
                    try
                    {                       
                        string fileName = file.Replace(sDir + "\\", "");
                        TreeNode fileNd = root.Nodes.Add(fileName);
                        ContextMenu mnu = new ContextMenu();
                        //ContextMenu mnu = new ContextMenu(new MenuItem [] { new MenuItem("Edit", 
                        //    new System.EventHandler(ConstrEditContext_Click))});
                        fileNd.ContextMenu = mnu;
                        
                        CConstruct constr = new CConstruct();
                        constr.OpenFile(file);                       
                        DataRow[] RefRows = constr._fields.Select("Поле='HREF'");
                        foreach (DataRow row in RefRows)
                        {
                            string href = row["Значение"].ToString();
                            mnu.MenuItems.Add(new MenuItem(href, new System.EventHandler(ConstrHrefContex_Click)));
                        }                       

                        fileNd.Tag = "Construct";
                        fileNd.ImageIndex = 4;
                        fileNd.SelectedImageIndex = 4;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveTreeState(TreeNode root, Hashtable stateTable)
        {
            try
            {
                foreach (TreeNode nd in root.Nodes)
                {
                    if (nd.Nodes.Count>0)
                    {
                        //!stateTable.Contains(nd.FullPath) 
                        stateTable.Add(nd.FullPath, nd.IsExpanded);
                        SaveTreeState(nd, stateTable);
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RestoreTreeState(TreeNode root, Hashtable stateTable)
        {
            try
            {
                foreach (TreeNode nd in root.Nodes)
                {
                    if (nd.Nodes.Count > 0 && stateTable.Contains(nd.FullPath))
                    {
                        bool expand =(bool) stateTable[nd.FullPath] ;
                        if (expand) nd.Expand();
                        RestoreTreeState(nd, stateTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void RefreshTree()
        {
            try
            {
                if (this.treeView1 == null) return;
                if (this.treeView1.Nodes == null) return;

                Hashtable treestate = new Hashtable();
                if (this.treeView1.Nodes.Count>0)
                {
                    //Save tree state 
                    treestate.Add(this.treeView1.Nodes[0].FullPath, this.treeView1.Nodes[0].IsExpanded);
                    SaveTreeState(this.treeView1.Nodes[0], treestate);
                    
                }

                this.treeView1.Nodes.Clear();
                //root
                TreeNode root = this.treeView1.Nodes.Add(databaseFolderName);
                //details
                TreeNode dtlNode = root.Nodes.Add("Details");

                FilesTable.Clear();
                DirBuild(db_path + "\\Details", dtlNode, "*.txt");
                //fast refresh 
                //cycle2
                List<string> removeKeys = new List<string>();
                foreach (DictionaryEntry pair in FileMD5DetsTable)
                {                    
                    //remove old files
                    if (!FilesTable.Contains(pair.Key))
                    {
                        removeKeys.Add(pair.Key as string);                        
                    }
                }
                //cycle 3
                foreach (string key in removeKeys)
                {
                    FileMD5DetsTable.Remove(key);
                }

                //constructs
                TreeNode cnstrNode = root.Nodes.Add("Constructs");
                ConstructsBuild(db_path + "\\Constructs", cnstrNode, "*.xml");

                if (treestate.Count>0)
                {
                    //Save tree state   
                    bool expand = (bool)treestate[this.treeView1.Nodes[0].FullPath];
                    if (expand) this.treeView1.Nodes[0].Expand();
                    RestoreTreeState(this.treeView1.Nodes[0], treestate);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            RefreshTree();          
        }


        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode clickedNode = treeView1.SelectedNode;
                if (clickedNode == null) return;
                if (clickedNode.Tag == null) return;
                if (clickedNode.Tag.ToString() == "Detail")
                {                    
                    string FilePath = db_path.Replace(databaseFolderName, "") + clickedNode.Parent.FullPath;
                    string DtlName = clickedNode.Text;
                    TableCSVEditForm frm = new TableCSVEditForm(FilePath, DtlName);
                    DialogResult res = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
                    if (res == DialogResult.Yes) this.RefreshTree();
                }
                if (clickedNode.Tag.ToString() == "Construct")
                {
                    ConstrEditForm frm = new ConstrEditForm(db_path, databaseFolderName);                    
                    string FilePath = db_path.Replace(databaseFolderName, "") + clickedNode.FullPath;
                    frm.XmlFilePath = FilePath;
                    frm.Text = FilePath;
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(frm);
                }
            }            
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //DateTime tm1 = DateTime.Now;
            RefreshTree();
            //DateTime tm2 = DateTime.Now;
            //MessageBox.Show((tm2 - tm1).TotalMilliseconds.ToString());
        }

        private void treeView1_MouseHover(object sender, EventArgs e)
        {
            //imgFrm.Show();
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode nd = e.Item as TreeNode;
            if (nd==null) return;
            if (nd.Tag == null) return;
            string tag=nd.Tag.ToString();
            if (!(tag == "Detail" || tag == "Construct" || tag == "File")) return;
            DoDragDrop(nd, DragDropEffects.Copy);

            DataObject myDataObject = new DataObject("TreeNode", nd);
            Autodesk.AutoCAD.ApplicationServices.
          Application.DoDragDrop(this.treeView1, myDataObject, DragDropEffects.Copy, new MyDropTarget());
        }

        private void DetailContext_Click(object sender, EventArgs e)
        {
            if (m_rightSelectedNode == null) return;
            if (m_rightSelectedNode.Tag.ToString() == "Detail")
            {
                MenuItem itm = (MenuItem)sender;
                string href = itm.Text;
                if(href.StartsWith("\\"))
                {
                    string FilePath = db_path.Replace(databaseFolderName, "") + 
                        m_rightSelectedNode.Parent.Parent.FullPath;
                    href = FilePath + href;
                }
                Process.Start(href);
            }
        }

        private void CreateLayer_Click(object sender, EventArgs e)
        {        
            MenuItem itm = (MenuItem)sender;
            string layer = itm.Tag as string;
            Random rnd1 = new Random();
            int rndcolor = rnd1.Next(0, 255);
            vl_utility.VL_commands.MakeLayer(layer, "Continious", Convert.ToInt16(rndcolor), true);
            //Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //if (aDoc == null) return;

        }
        
        private void ConstrHrefContex_Click(object sender, EventArgs e)
        {
            string href=string.Empty;
            try
            {
                if (m_rightSelectedNode == null) return;
                if (m_rightSelectedNode.Tag.ToString() == "Construct")
                {
                    MenuItem itm = (MenuItem)sender;
                    href = itm.Text;
                    //if (href.StartsWith("\\"))
                    {
                        string FilePath = db_path.Replace(databaseFolderName, "") +
                            m_rightSelectedNode.Parent.FullPath;
                        //string FilePath = db_path.Replace(databaseFolderName, "") + m_rightSelectedNode.FullPath;
                        href = FilePath + "\\" + href;
                        if (!File.Exists(href)) throw new FileNotFoundException();
                    }
                    Process.Start(href);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл " + href +" не найден");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
		        Point p = new Point(e.X, e.Y);

		        // Get the node that the user has clicked.
		        TreeNode node= treeView1.GetNodeAt(p);
                if (node != null)
                {
                    m_rightSelectedNode = node;
                }
            }
        }

        private void toolStripMenuCollapse_Click(object sender, EventArgs e)
        {
            this.treeView1.Nodes[0].Collapse();
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                TreeNode nd = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                if (nd == null) return;                
                string tag = nd.Tag.ToString();
                if (tag == "Detail")
                {
                    ////string FilePath = db_path.Replace(databaseFolderName, "") + nd.Parent.FullPath;
                    //string DtlName = nd.Text;
                    //DataRow row = m_cnstr._details.Rows.Add(DtlName, "1");
                    //ReloadTables();
                }
                if (tag == "Construct")
                {
                    string source = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                    string destin = source;
                    while (File.Exists(destin))
                    {
                        destin = destin.Replace(".xml", "_копия.xml");
                    }                    
                    File.Copy(source, destin, false);
                    RefreshTree();
                }
                if (tag == "File")
                {
                    string source = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                    string destin = source;
                    while (File.Exists(destin))
                    {
                        destin = destin.Replace(".txt", "_копия.txt");
                    }
                    File.Copy(source, destin, false);
                    RefreshTree();
                }
            }
            catch (Exception ex)
            {                
                MessageBox.Show(ex.ToString());
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    TreeNode nd = treeView1.SelectedNode;
                    string tag = nd.Tag.ToString();
                    if (tag == "Construct" | tag == "File")
                    {
                        string file = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                        DialogResult res = MessageBox.Show("Удалить файл \"" + nd.Text + "\" ?",
                            "vl_utility", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (res == DialogResult.OK)
                        {
                            File.Delete(file);
                            nd.Remove();
                        }
                    }
                }

                if (e.KeyCode == Keys.F2)
                {
                    TreeNode nd = treeView1.SelectedNode;
                    string tag = nd.Tag.ToString();
                    if (tag == "Construct" | tag == "File")
                    {
                        string file = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                        InputBox frm = new InputBox();
                        frm.SetLabel("Введите новое имя файла: ");
                        frm.SetText(Path.GetFileNameWithoutExtension(file));
                        DialogResult res = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
                        if (res == DialogResult.OK)
                        {
                            string oldName = Path.GetFileNameWithoutExtension(file);
                            string newFile = file.Replace(oldName, frm.GetText());
                            File.Move(file, newFile);
                            RefreshTree();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void treeView1_MouseLeave(object sender, EventArgs e)
        {
            //imgFrm.Hide();
        }

        private void toolStripTextFind_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {               
                if (e.KeyData == Keys.Enter)
                {
                    //Enter key is down

                    //Capture the text
                    if (sender is TextBox)
                    {
                        TextBox txb = (TextBox)sender;
                        MessageBox.Show(txb.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripTextFind_EnterKeyDown(object sender, EventArgs e)
        {
            string FullName = this.toolStripTextFind.Text;
            vl_utility.VlineForms.DbSearchForm frm = new vl_utility.VlineForms.DbSearchForm(FullName);
            
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
            if (frm.DialogResult == DialogResult.OK)
            {
                string DtlName = frm.retDtlFullName;
                string FilePath = frm.retDtlPath;
                Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (aDoc == null) return;
                FilePath = db_path + FilePath;
                
                ////Посылаем команду о вставке детали
                aDoc.SendStringToExecute(
                    "_vl_detail " + "\"" + FilePath + "\"" + "\n" + "\"" + DtlName + "\"" + "\n", true, false, true);
               
            } 
                                    
        }
              
    }

    public class HashFilesTable
    {
        public string hash = "";
        public List<TreeNode> nodes = new List<TreeNode>();
        public DataTable table = new DataTable();
        public HashFilesTable() { }
    }


}
