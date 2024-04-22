using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System.Configuration;
using ChrisForbes.DataStructures;
using vl_utility.VlineForms;


[assembly: CommandClass(typeof(vl_utility.VL_commands))]
namespace vl_utility
{
    public class MyDropTarget : Autodesk.AutoCAD.Windows.DropTarget
    {
        private const string databaseFolderName = "Database";
        public override void OnDrop(DragEventArgs e)
        {
            try
            {
                #region CONFIG

                Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

                if (config.AppSettings.Settings["DatabasePath"] == null)
                {
                    return;
                }
                string db_path = config.AppSettings.Settings["DatabasePath"].Value;
                #endregion

                if (e.Data.GetDataPresent("TreeNode"))
                {
                    object o = e.Data.GetData("TreeNode");
                    TreeNode nd = o as TreeNode;
                    if (nd != null)
                    {
                        Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                        if (aDoc == null) return;

                        string tag = nd.Tag.ToString();
                        if (tag == "Detail")
                        {
                            string FilePath = db_path.Replace(databaseFolderName, "") + nd.Parent.FullPath;
                            string DtlName = nd.Text;
                            //Посылаем команду о вставке детали
                            aDoc.SendStringToExecute(
                                "_vl_detail " + "\"" + FilePath + "\"" + "\n" + "\"" + DtlName + "\"" + "\n", true, false, true);
                        }
                        if (tag == "Construct")
                        {
                            string FilePath = db_path.Replace(databaseFolderName, "") + nd.FullPath;
                            //Посылаем команду о вставке конструкции
                            aDoc.SendStringToExecute(
                                "_vl_construct " + "\"" + FilePath + "\"" + "\n", true, false, true);
                        }
                    }
                }
            }
            catch (System.InvalidOperationException)
            {
                //do nothing
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

    public class VL_commands : IExtensionApplication
    {
        private const string databaseFolderName = "Database";
        private static string db_path;
        private static string RazdPriortyFilePath;
        private static string _TemplatePath;

        //const attributes required
        string[] attributes = { "ПОЛНОЕ_ИМЯ", "КОЛИЧЕСТВО", "ЕД_ИЗМ","ТИП", "КОД_ОБОРУДОВАНИЯ", 
                                  "ЗАВОД_ИЗГОТОВИТЕЛЬ", "РАЗДЕЛ", "МАССА", "НАИМЕНОВАНИЕ" , 
                                  "ПРИМЕЧАНИЕ"};
        //панель с базой
        protected static Autodesk.AutoCAD.Windows.PaletteSet m_ps = null;
        public static UserControl1 m_usrform = null; 
        //панель свойств конструкций


        public const double PI = 3.1415926535897931;

        public void Initialize()
        {

            // location update
            Assembly assem = Assembly.GetExecutingAssembly();
            string name = assem.ManifestModule.Name;
            string appPath = assem.Location.Replace(name, "");
            //db_path = appPath + databaseFolderName;
            RazdPriortyFilePath = appPath + "разделы.csv";            

            #region CONFIG
            try
            {                
                string inputDBPath = "";
                Configuration config = null;
                try
                {
                    config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                    inputDBPath = config.AppSettings.Settings["DatabasePath"].Value;                    
                }
                catch (System.Exception)
                {
                    string DefaultPath = appPath + databaseFolderName;
                    inputDBPath = DefaultPath;                    
                    config.AppSettings.Settings.Add("DatabasePath", DefaultPath);
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
               
                if(!(Directory.Exists(inputDBPath)))
                {
                    DialogResult res = MessageBox.Show("Файл настроек vl_utility.dll.config \nНе найдена база данных: " + inputDBPath + "\n" +
                        "Установить путь по умолчанию?", "vl_utility.dll", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (res == DialogResult.Yes)
                    {
                        config.AppSettings.Settings.Remove("DatabasePath");
                        //Set default value
                        string DefaultPath = appPath + databaseFolderName;
                        config.AppSettings.Settings.Add("DatabasePath", DefaultPath);
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                    else
                    {
                        MessageBox.Show("Работа программы невозможна", "vl_utility.dll", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                db_path = config.AppSettings.Settings["DatabasePath"].Value;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            _TemplatePath = db_path.Replace(databaseFolderName, "Template\\");
            ed.WriteMessage("TemplatePath "+ _TemplatePath + "\n");
            
            #endregion

            #region PALETTE
            if (m_ps == null)
            {
                m_ps = new Autodesk.AutoCAD.Windows.PaletteSet("Vl_Palette",
                new Guid("{4B4D7C68-CC7E-4883-9AF6-D3D981975906}"));
                m_usrform = new UserControl1();
                m_usrform.db_path = db_path;
                m_ps.Add("Имя вкладки", m_usrform);
                m_ps.Size = new System.Drawing.Size(187, 350);
                m_ps.MinimumSize = new System.Drawing.Size(187, 350);
                m_ps.Dock = Autodesk.AutoCAD.Windows.DockSides.Left;
                m_ps.Location = new System.Drawing.Point(0, 0);
                m_ps.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
                m_ps.Visible = true;
                //m_usrform.treeView1.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeView1_NodeMouseDoubleClick);                               
            }
            #endregion

            #region EVENTHANDLERS
            DocumentCollection dm = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            dm.DocumentCreated += new DocumentCollectionEventHandler(OnDocumentCreated);
            foreach (Document doc in dm)
            {
                doc.Editor.PointMonitor +=
                  new PointMonitorEventHandler(OnMonitorPoint);
                doc.ImpliedSelectionChanged += new EventHandler(OnDoc_ImpliedSelectionChanged);                
            }
            //Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            //if (aDoc == null) return;
            //aDoc.SendStringToExecute("vl_paletteshow" + "\n", true, false, true);
            #endregion

            
            ed.WriteMessage("vl_situation - создание тахеосъемки \n geo_export - экспорт геоточек \n");           

            DemandLoading.RegistryUpdate.RegisterForDemandLoading();

            ed.WriteMessage(this.GetType().Module.Name + " загружен и добавлен в автозагрузку\n");
            ed.WriteMessage("Для удаления из автозагрузки используйте команду VL_REMAUTO\n");
        }        

        void OnDoc_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptSelectionResult selectionResult = ed.SelectImplied();
            if (selectionResult.Status == PromptStatus.OK)
            {
                Transaction tr = doc.TransactionManager.StartTransaction();
                try
                {
                    ObjectId[] objIds = selectionResult.Value.GetObjectIds();
                    foreach (ObjectId objId in objIds)
                    {
                        DBObject obj = tr.GetObject(objId, OpenMode.ForRead);
                        //Entity ent = (Entity)obj;
                        //// This time access the properties directly
                        //ed.WriteMessage("\nType:        " + ent.GetType().ToString());
                        //obj.Dispose();
                    }
                    // Although no changes were made, use Commit()
                    // as this is much quicker than rolling back
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                    tr.Abort();
                }
            }
        }

        public void Terminate()
        {
            try
            {
                DocumentCollection dm = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                if (dm != null)
                {
                    Editor ed = dm.MdiActiveDocument.Editor;
                    ed.PointMonitor -= new PointMonitorEventHandler(OnMonitorPoint);                    
                }
            }
            catch (System.Exception)
            {
                // The editor may no longer
                // be available on unload
            }
        }
        
        //private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    //TreeNode clickedNode = e.Node;
        //    TreeNode clickedNode = m_usrform.treeView1.SelectedNode;
        //    if (clickedNode.Tag == null) return;            
        //    Document aDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //    if (aDoc == null) return;

        //    if (clickedNode.Tag.ToString() == "Detail")
        //    {
        //        string FilePath = db_path.Replace(databaseFolderName, "") + clickedNode.Parent.FullPath;
        //        string DtlName = clickedNode.Text;

        //        //Посылаем команду о вставке детали
        //        aDoc.SendStringToExecute(
        //            "_vl_detail " + "\"" + FilePath + "\"" + "\n" + "\"" + DtlName + "\"" + "\n", true, false, true);
        //    }

        //    if (clickedNode.Tag.ToString() == "Construct")
        //    {
        //        string FilePath = db_path.Replace(databaseFolderName, "") + clickedNode.FullPath;
        //        //Посылаем команду о вставке конструкции
        //        aDoc.SendStringToExecute(
        //            "_vl_construct " + "\"" + FilePath + "\"" + "\n", true, false, true);
        //    }
        //}

        

        [CommandMethod("VL_PALETTE")]
        public void cmdPaletteshow()
        {
            if (m_ps == null) return;
            m_ps.Visible = true;
            //if (m_ps.Visible)
            //{
            //    m_ps.Visible = false;
            //}
            //else
            //{
            //    m_ps.Visible = true;
            //}
        }

        [CommandMethod("VL_PALETTEHIDE")]
        public void cmdPalettehide()
        {
            if (m_ps == null) return;
            m_ps.Visible = false;
        }

        [CommandMethod("VL_REMAUTO")]
        public void cmdRemoveAuto()
        {
            DemandLoading.RegistryUpdate.UnregisterForDemandLoading();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(this.GetType().Module.Name + " удален из автозагрузки\n");
        }

        //[CommandMethod("pldim")]
        //public void cmdPLineDimSpeed()
        //{
        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    Database db = HostApplicationServices.WorkingDatabase;
        //    TypedValue[] values = new TypedValue[] { new TypedValue(0, "LWPOLYLINE") };
        //    SelectionFilter filter = new SelectionFilter(values);
        //    PromptSelectionOptions opts = new PromptSelectionOptions();

        //    //opts.MessageForRemoval = "\nMust be a type of Block!";
        //    opts.MessageForAdding = "\nSelect a polyline: ";
        //    //opts.PrepareOptionalDetails = true;
        //    //opts.SingleOnly = true;
        //    //opts.SinglePickInSpace = true;
        //    //opts.AllowDuplicates = false;
        //    PromptSelectionResult res = default(PromptSelectionResult);
        //    res = ed.GetSelection(opts, filter);
        //    if (res.Status != PromptStatus.OK)
        //        return;
        //    try
        //    {
        //        using (Transaction tr = db.TransactionManager.StartTransaction())
        //        {
        //            BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //            BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);                    
        //            foreach (SelectedObject selPl in res.Value)
        //            {
        //                Polyline pline = tr.GetObject(selPl.ObjectId, OpenMode.ForRead, false) as Polyline;
        //                for (int i = 1; i < pline.NumberOfVertices; i++)
        //                {
        //                    Point2d pnt1 = pline.GetPoint2dAt(i - 1);
        //                    Point2d pnt2 = pline.GetPoint2dAt(i);
        //                    AlignedDimension drdim = new AlignedDimension(new Point3d(pnt1.X, pnt1.Y, 0),
        //                        new Point3d(pnt2.X, pnt2.Y, 0), new Point3d(pnt1.X, pnt1.Y, 0), "", ObjectId.Null);
        //                    acBlkTblRec.AppendEntity(drdim);
        //                    tr.AddNewlyCreatedDBObject(drdim, true);
        //                }

        //            }
        //            tr.Commit();
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        ed.WriteMessage("Error: " + ex.Message + "\n" + ex.StackTrace);
        //    }

        //}


        [CommandMethod("vl_situation")]
        public void cmdVl_situation()
        {
            Point3d AnchorPnt;
            double StartAngle;
            double stationH;
            double stationAbs;
            bool clockwise = true;
            ObjectId blkID;

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            SituationForm frm = new SituationForm();
            DialogResult res = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
            if (!(res == DialogResult.OK)) return;

            //Get station point                        
            PromptPointOptions prPntOpt = new PromptPointOptions("Укажите точку станции: \n");
            PromptPointResult prPntRes = ed.GetPoint(prPntOpt);
            AnchorPnt = prPntRes.Value;

            //Get orientation
            PromptAngleOptions angOpt = new PromptAngleOptions("Укажите направление нуля: \n");
            angOpt.UseBasePoint = true;
            angOpt.BasePoint = AnchorPnt;
            PromptDoubleResult dblRes = ed.GetAngle(angOpt);
            StartAngle = dblRes.Value;

            //Get angle direction
            PromptKeywordOptions kwrdOpt = new PromptKeywordOptions("Направление отсчета по горизонтальному кругу: \n");
            kwrdOpt.AllowNone = true;
            kwrdOpt.Keywords.Add("1 - по часовой");
            kwrdOpt.Keywords.Add("2 - против часовой");
            kwrdOpt.Keywords.Default = "1 - по часовой";
            PromptResult prRes = ed.GetKeywords(kwrdOpt);
            if (!(prRes.Status == PromptStatus.OK)) return;
            if (prRes.StringResult == "1") clockwise = true;
            if (prRes.StringResult == "2") clockwise = false;
            //Get station height            
            dblRes = ed.GetDouble("Высота станции, см: \n");
            stationH = dblRes.Value;
            //Get station height            
            dblRes = ed.GetDouble("Отметка станции, м: \n");
            stationAbs = dblRes.Value;

            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = dbCurrent.TransactionManager;
            using (Transaction tr = tm.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite);
                if (bt.Has("TAHEO_POINT"))
                {
                    blkID = bt["TAHEO_POINT"];
                }
                else
                {
                    //add block
                    BlockTableRecord BlkTblRec = new BlockTableRecord();
                    BlkTblRec.Name = "TAHEO_POINT";
                    BlkTblRec.Origin = new Point3d(0, 0, 0);
                    bt.UpgradeOpen();
                    blkID = bt.Add(BlkTblRec);
                    tr.AddNewlyCreatedDBObject(BlkTblRec, true);
                    //Add point
                    DBPoint pnt = new DBPoint(new Point3d(0, 0, 0));
                    BlkTblRec.AppendEntity(pnt);
                    tr.AddNewlyCreatedDBObject(pnt, true);

                    //add attribute "Number"
                    AttributeDefinition attDef = new AttributeDefinition(new Point3d(0, 3, 0), "", "NUMBER", "NUMBER",
                         Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
                    BlkTblRec.AppendEntity(attDef);
                    tr.AddNewlyCreatedDBObject(attDef, true);

                    //add attribute "DESCRIPTION"
                    AttributeDefinition attDef2 = new AttributeDefinition(new Point3d(0, -3, 0), "", "DESCRIPTION", "DESCRIPTION",
                         Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
                    BlkTblRec.AppendEntity(attDef2);
                    tr.AddNewlyCreatedDBObject(attDef2, true);

                    //add attribute "HEIGHT"
                    AttributeDefinition attDef3 = new AttributeDefinition(new Point3d(-12, 0, 0), "", "HEIGHT", "HEIGHT",
                         Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
                    BlkTblRec.AppendEntity(attDef3);
                    tr.AddNewlyCreatedDBObject(attDef3, true);
                }
                //Add layer UNPLOTTED
                ObjectId unplotLayer = MakeLayer("UNPLOTTED", "Continious", 9, false);

                //Add Blocks TaheoPoint
                string number = null;
                string descr = null;
                double angleGor = 0;
                double angleVer = 0;
                double hr = 0;
                double lenght = 0;
                Group group = new Group(); //группа для объектов

                for (int i = 0; i <= frm.DataGridView1.Rows.Count - 2; i++)
                {
                    //get rows point attributes from gridView
                    number = frm.DataGridView1.Rows[i].Cells[0].Value.ToString();
                    descr = frm.DataGridView1.Rows[i].Cells[1].Value.ToString();
                    //отсчет по рейке см
                    hr = System.Convert.ToDouble(frm.DataGridView1.Rows[i].Cells[2].Value);
                    //горизонтальный угол
                    angleGor = DegMinSecToRadians(frm.DataGridView1.Rows[i].Cells[3].Value.ToString());
                    //вертикальный угол
                    angleVer = DegMinSecToRadians(frm.DataGridView1.Rows[i].Cells[4].Value.ToString());
                    //длина измеренная
                    string tmp = frm.DataGridView1.Rows[i].Cells[5].Value.ToString();
                    if (tmp.Contains(",")) tmp = tmp.Replace(",", ".");
                    lenght = System.Convert.ToDouble(tmp);

                    //Calculations
                    //Проложение горизонтальное
                    double Lgor = lenght * Math.Sin(angleVer);
                    //РАзность высот, см
                    double deltaH = stationH + lenght * Math.Cos(angleVer) * 100 - hr;//Ед. изм. - сантиметры
                    //Абсолютная отметка точки, м 
                    double Habs = stationAbs + deltaH / 100;
                    //

                    Point3d curPnt;
                    if (clockwise)
                        curPnt = acgePolar(AnchorPnt, StartAngle - angleGor, Lgor);
                    else curPnt = acgePolar(AnchorPnt, StartAngle + angleGor, Lgor);
                    //Draw unplotted line
                    BlockTableRecord btCur = (BlockTableRecord)tr.GetObject(dbCurrent.CurrentSpaceId, OpenMode.ForWrite);

                    Line ln = new Line(AnchorPnt, curPnt);
                    ln.LayerId = unplotLayer;
                    btCur.AppendEntity(ln);
                    tr.AddNewlyCreatedDBObject(ln, true);
                    group.Append(ln.Id);
                    //Insert Block Reference
                    BlockReference bRef = new BlockReference(curPnt, blkID);
                    btCur.AppendEntity(bRef);
                    tr.AddNewlyCreatedDBObject(bRef, true);
                    group.Append(bRef.Id);
                    //Add attributes
                    InsertBlockAttibuteRef(bRef, "NUMBER", number);
                    InsertBlockAttibuteRef(bRef, "DESCRIPTION", descr);
                    InsertBlockAttibuteRef(bRef, "HEIGHT", string.Format("{0:0.00}", Habs));
                }
                DBDictionary dbDict = (DBDictionary)tr.GetObject(dbCurrent.GroupDictionaryId, OpenMode.ForWrite);
                int grpNum = 1;
                while (dbDict.Contains(grpNum.ToString())) grpNum++;
                dbDict.SetAt(grpNum.ToString(), group);
                tr.AddNewlyCreatedDBObject(group, true);

                tr.Commit();
                //Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("ddstyle", 
            }



        }

        [CommandMethod("geo_export")]
        public void cmdGeo_export()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions selOpt = new PromptSelectionOptions();
            selOpt.MessageForAdding = "Select taheo points";
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), new TypedValue((int)DxfCode.BlockName, "TAHEO_POINT") };
            SelectionFilter sfilter = new SelectionFilter(values);
            selOpt.AllowDuplicates = false;
            PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
            if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
            ObjectId[] objIds = sset.Value.GetObjectIds();

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            DialogResult dRes = dlg.ShowDialog();
            if (!(dRes == DialogResult.OK)) return;
            string filePath = dlg.FileName;
            StreamWriter swriter = new StreamWriter(filePath, false, System.Text.Encoding.GetEncoding(1251));

            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = dbCurrent.TransactionManager;
            using (Transaction tr = tm.StartTransaction())
            {
                foreach (ObjectId id in objIds)
                {
                    BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                    string strOut = "";
                    AttributeCollection attcol = bRef.AttributeCollection;
                    foreach (ObjectId att in attcol)
                    {
                        AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                        if (atRef.Tag == "NUMBER")
                        {
                            strOut += atRef.TextString;
                            strOut += (" ");
                        }

                    }
                    strOut += bRef.Position.X;
                    strOut += " ";
                    strOut += bRef.Position.Y;
                    strOut += " ";
                    foreach (ObjectId att in attcol)
                    {
                        AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                        if (atRef.Tag == "HEIGHT")
                        {
                            strOut += atRef.TextString;
                            strOut += " ";
                        }

                    }
                    foreach (ObjectId att in attcol)
                    {
                        AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                        if (atRef.Tag == "DESCRIPTION")
                        {
                            strOut += "\"";
                            strOut += atRef.TextString;
                            strOut += "\"";
                        }

                    }
                    swriter.WriteLine(strOut);

                }
            }
            swriter.Close();


        }

        //[CommandMethod("mins")]
        //public void cmdMultiInsert()
        //{
        //    var w = Clipboard.GetData(DataFormats.Text);//Читаем
        //    string str = w.ToString();
        //    str = str.Replace("\n", "");
        //    string[] lines;
        //    lines = str.Split('\r');
        //    List<string> array = new List<string>();
        //    foreach (string s in lines)
        //    {
        //        string[] cols;
        //        cols = s.Split('\t');
        //        array.AddRange(cols);
        //    }
        //    array.Remove("");

        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    Database db = HostApplicationServices.WorkingDatabase;

        //    foreach (string s in array)
        //    {
        //        PromptEntityOptions opt = new PromptEntityOptions("Select text, mtext or dimension");
        //        opt.SetRejectMessage("Selected object not a text, mtext or dimension");
        //        opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.MText), false);
        //        //opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.AlignedDimension), false);
        //        opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.DBText), false);
        //        opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.Dimension), false);

        //        PromptEntityResult res = ed.GetEntity(opt);
        //        if (res.Status != PromptStatus.OK) return;
        //        try
        //        {
        //            using (Transaction tr = db.TransactionManager.StartTransaction())
        //            {
        //                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //                DBObject obj = tr.GetObject(res.ObjectId, OpenMode.ForWrite, false);
        //                if (obj.GetType() == typeof(Autodesk.AutoCAD.DatabaseServices.MText))
        //                {
        //                    MText mtxt = obj as MText;
        //                    mtxt.Contents = s;
        //                }
        //                if (obj.GetType() == typeof(Autodesk.AutoCAD.DatabaseServices.DBText))
        //                {
        //                    DBText txt = obj as DBText;
        //                    txt.TextString = s;
        //                }
        //                if (obj is Autodesk.AutoCAD.DatabaseServices.Dimension)
        //                {
        //                    Dimension dim = obj as Dimension;
        //                    dim.DimensionText = s;
        //                }

        //                tr.Commit();
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            ed.WriteMessage("Error: " + ex.Message + "\n" + ex.StackTrace);
        //        }
        //    }
        //}

        [CommandMethod("calc", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void cmdCalc()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            PromptSelectionOptions opt = new PromptSelectionOptions();
            PromptSelectionResult res;
            res = ed.GetSelection(opt);
            if (res.Status != PromptStatus.OK) return;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    double sum = 0;
                    string s = "";
                    foreach (SelectedObject selPl in res.Value)
                    {
                        DBObject obj = tr.GetObject(selPl.ObjectId, OpenMode.ForRead, false);
                        if (obj.GetType() == typeof(Autodesk.AutoCAD.DatabaseServices.MText))
                        {
                            MText mtxt = obj as MText;
                            s = mtxt.Text;
                            s = s.Replace(',', '.');
                            sum += Convert.ToDouble(s);
                        }
                        if (obj.GetType() == typeof(Autodesk.AutoCAD.DatabaseServices.DBText))
                        {
                            DBText txt = obj as DBText;
                            s = txt.TextString;
                            s = s.Replace(',', '.');
                            sum += Convert.ToDouble(s);
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Dimension)
                        {
                            Dimension dim = obj as Dimension;
                            if (dim.DimensionText == "")
                            {
                                sum += dim.Measurement;
                            }
                            else
                            {
                                s = dim.DimensionText;
                                s = s.Replace(',', '.');
                                sum += Convert.ToDouble(s);
                            }
                        }

                    }
                    tr.Commit();
                    ed.WriteMessage(sum.ToString());
                }
            }

            catch (System.Exception ex)
            {
                ed.WriteMessage("Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        [CommandMethod("vl_detail")]
        public void cmdDetail()
        {
            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.Editor;

            string DBPath, DtlName;

            PromptResult res = ed.GetString("Enter path to database: ");
            if (res.Status != PromptStatus.OK) return;
            DBPath = res.StringResult;

            res = ed.GetString("Enter name of detail: ");
            if (res.Status != PromptStatus.OK) return;
            DtlName = res.StringResult;

            //Количество
            PromptDoubleResult DblRes = ed.GetDouble("Введите количество деталей: ");
            if (DblRes.Status != PromptStatus.OK) return;
            double qty = DblRes.Value;

            //get all fields datail
            Hashtable DtlFields = new Hashtable();
            CSVReader rdr = new CSVReader(DBPath);
            try
            {
                rdr.GetHashTable(attributes[0], DtlName, DtlFields, '\t');
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            DtlFields.Add("КОЛИЧЕСТВО", qty);

            //Если имя блока задано в детали
            string blkName = "";
            if (DtlFields.Contains("BLOCKNAME"))
            {
                try
                {
                    blkName = DtlFields["BLOCKNAME"].ToString();
                    this.ImportBlock(_TemplatePath + "1.dwg", blkName);                    
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    //Если такого блока нет в шаблоне
                    if (ex.ErrorStatus == ErrorStatus.InvalidBlockName)
                    {
                        
                    }
                    MessageBox.Show(ex.ToString());
                }
            }
            try
            {
                BlockReference br = this.BlockJig(blkName, DtlFields);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return;

            //ObjectId blkID;
            //Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = dbCurrent.TransactionManager;
            //using (Transaction tr = tm.StartTransaction())
            //{
            //    BlockTable bt = (BlockTable)tr.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite);
            //    if (bt.Has("DETAIL_W"))
            //    {
            //        blkID = bt["DETAIL_W"];
            //    }
            //    else
            //    {
            //        //add block

            //        BlockTableRecord BlkTblRec = new BlockTableRecord();
            //        BlkTblRec.Name = "DETAIL_W";
            //        BlkTblRec.Origin = new Point3d(0, 0, 0);
            //        bt.UpgradeOpen();
            //        blkID = bt.Add(BlkTblRec);
            //        tr.AddNewlyCreatedDBObject(BlkTblRec, true);

            //        //add attributes
            //        int x = 0;
            //        foreach (string attr in attributes)
            //        {
            //            AttributeDefinition attDef = new AttributeDefinition(new Point3d(x, 0, 0), "", attr, attr,
            //                Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
            //            BlkTblRec.AppendEntity(attDef);
            //            tr.AddNewlyCreatedDBObject(attDef, true);
            //            x += 30;
            //        }
            //    }

            //    //Get point  
            //    Point3d AnchorPnt;
            //    PromptPointOptions prPntOpt = new PromptPointOptions("Укажите точку: \n");
            //    PromptPointResult prPntRes = ed.GetPoint(prPntOpt);
            //    AnchorPnt = prPntRes.Value;
            //    //Insert Block Reference
            //    BlockTableRecord btCur = (BlockTableRecord)tr.GetObject(dbCurrent.CurrentSpaceId, OpenMode.ForWrite);
            //    BlockReference bRef = new BlockReference(AnchorPnt, blkID);
            //    btCur.AppendEntity(bRef);
            //    tr.AddNewlyCreatedDBObject(bRef, true);
            //    //Add attributes
            //    foreach (string attr in attributes)
            //    {
            //        string val = "";
            //        if (DtlFields.ContainsKey(attr))
            //        {
            //            val = DtlFields[attr].ToString();
            //        }
            //        InsertBlockAttibuteRef(bRef, attr, val);
            //    }
            //    tr.Commit();
            //}
        }

        [CommandMethod("vl_construct")]
        public void cmdConstruct()
        {
            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.Editor;

            string FilePath;

            PromptResult res = ed.GetString("Enter path to xml file: ");
            if (res.Status != PromptStatus.OK) return;
            FilePath = res.StringResult;

            CConstruct constr = new CConstruct();
            string blkName = "";

            try
            {
                constr.OpenFile(FilePath);
                DataRow[] selrow = constr._fields.Select("Поле='BLOCKNAME'");
                if (selrow.Length != 1) blkName = "constrDef";
                else
                {
                    blkName = selrow[0]["Значение"].ToString();                    
                }
                ImportBlock(_TemplatePath + "1.dwg", blkName);

                string xmlString = constr.SaveToString();
                Hashtable attrs = new Hashtable();
                attrs.Add("XML_CONSTRUCT", xmlString);
                BlockReference br = this.BlockJig(blkName, attrs);
                               
            }

            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                //Если такого блока нет в шаблоне
                if (ex.ErrorStatus == ErrorStatus.InvalidBlockName)
                {

                }
                MessageBox.Show(ex.Message);
            } 

            catch (System.Exception)
            {
                
                throw;
            }
        }

        [CommandMethod("vl_cedit", CommandFlags.UsePickSet)]
        public void cmdConstructEdit()
        { 
            try
            {
                Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
                    DocumentManager.MdiActiveDocument;
                Editor ed = activeDoc.Editor;
                PromptSelectionResult result = ed.GetSelection();
                if (result.Status == PromptStatus.OK)
                {
                    SelectionSet ss = result.Value;
                    foreach (SelectedObject so in ss)
                    {
                        Database db = activeDoc.Database;
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            BlockReference bRef = (BlockReference)tr.GetObject(so.ObjectId, OpenMode.ForRead);
                            if (bRef != null)
                            {
                                CConstruct constr = GetConstructFromRef(bRef);
                                ConstrEditForm frm = new ConstrEditForm(db_path, databaseFolderName);
                                frm.Construct = constr;
                                frm.BlockRefId = bRef.Id;
                                Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(frm);
                            }
                        }
                    }
                }                
            }
            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error",  MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        [CommandMethod("vl_num")]
        public void cmdNumbering()
        {
            try
            {
                #region SELECTING
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptIntegerResult res = ed.GetInteger("\nВведите начальный номер опоры: ");
                if (!(res.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
                int number = res.Value;

                PromptSelectionOptions selOpt = new PromptSelectionOptions();
                selOpt.MessageForAdding = "Выберите опоры: ";
                TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
                SelectionFilter sfilter = new SelectionFilter(values);
                selOpt.AllowDuplicates = false;
                PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
                if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
                #endregion

                ObjectId[] objIds = sset.Value.GetObjectIds();
                Database dbCurrent = HostApplicationServices.WorkingDatabase;

                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {

                    foreach (ObjectId id in objIds)
                    {
                        BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForWrite);
                        CConstruct constr = GetConstructFromRef(bRef);
                        if (constr != null)
                        {
                            if (constr.IsOpora())
                            {
                                constr.SetField("НОМЕР", number.ToString());
                                AttributeCollection attcol = bRef.AttributeCollection;
                                foreach (ObjectId att in attcol)
                                {
                                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForWrite);
                                    if (atRef.Tag == "XML_CONSTRUCT")
                                    {
                                        atRef.TextString = constr.SaveToString();
                                        number++;
                                    }
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }         
        }

        [CommandMethod("vl_index")]
        public void cmdIndexing()
        {
            #region SELECTING
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions selOpt = new PromptSelectionOptions();
            selOpt.MessageForAdding = "Выберите опоры: ";
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
            SelectionFilter sfilter = new SelectionFilter(values);
            selOpt.AllowDuplicates = false;
            PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
            if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
            #endregion            
            int number = 1;
            ObjectId[] objIds = sset.Value.GetObjectIds();
            Database dbCurrent = HostApplicationServices.WorkingDatabase;

            using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
            {

                foreach (ObjectId id in objIds)
                {
                    BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForWrite);
                    CConstruct constr = GetConstructFromRef(bRef);
                    if(constr!=null)
                    {
                        if (constr.IsOpora())
                        {
                            constr.SetField("ИНДЕКС", number.ToString());
                            AttributeCollection attcol = bRef.AttributeCollection;
                            foreach (ObjectId att in attcol)
                            {
                                AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForWrite);
                                if (atRef.Tag == "XML_CONSTRUCT")
                                {
                                    atRef.TextString = constr.SaveToString();
                                    number++;
                                }                                
                            }
                        }
                    }
                }
                tr.Commit();
            }
        }

        [CommandMethod("vl_poopor")]
        public void cmdPoopor()
        {
            string A4FirstLayName = "poopA4First";
            //string A4NextLayName = "poopA4Next";
            string A3FirstLayName = "poopA3First";
            //string A3NextLayName = "poopA3Next";

            #region SELECTING
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions selOpt = new PromptSelectionOptions();
            selOpt.MessageForAdding = "Выберите опоры: ";
            TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
            SelectionFilter sfilter = new SelectionFilter(values);
            selOpt.AllowDuplicates = false;
            PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
            if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }

            //Prompt format layout
            bool formatA4 = true;
            PromptKeywordOptions pko = new PromptKeywordOptions("\nФормат спецификации : ");
            pko.AllowNone = true;
            pko.Keywords.Add("A3");
            pko.Keywords.Add("A4");
            pko.Keywords.Default = "A4";
            PromptResult pkr = ed.GetKeywords(pko);
            if (pkr.Status != PromptStatus.OK) return;
            else
            {
                if (pkr.StringResult == "A3") formatA4 = false;
            }
            //End Prompt format layout

            ObjectId[] objIds = sset.Value.GetObjectIds();
            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            #endregion SELECTING

            #region FILL_TABLE

            System.Data.DataTable table = new System.Data.DataTable();
            List<CConstruct> cnstrList = new List<CConstruct>();
            foreach (string attr in attributes)
            {
                table.Columns.Add(attr, typeof(string));
            }

            using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in objIds)
                {
                    BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                    AttributeCollection attcol = bRef.AttributeCollection;

                    bool fnd = false;   //найден ли результат
                    foreach (ObjectId att in attcol)
                    {
                        if (fnd) break;
                        AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);

                        switch (atRef.Tag)
                        {                            
                            case "XML_CONSTRUCT":
                                CConstruct constr = new CConstruct();
                                constr.ReadFromString(atRef.TextString);
                                //add only constructs with name опора
                                if (constr.IsOpora())
                                {
                                    cnstrList.Add(constr);
                                    foreach (DataRow drow in constr._details.Rows)
                                    {
                                        string dtl = drow["Наименование_детали"].ToString();
                                        double qty1 = double.Parse(drow["Количество"].ToString());
                                        DataRow row2 = GetDetail(dtl);
                                        if (row2 != null)
                                        {
                                            table.ImportRow(row2);
                                            DataRow NewRow2 = table.Rows[table.Rows.Count - 1];
                                            NewRow2["КОЛИЧЕСТВО"] = qty1;
                                        }

                                    }
                                    fnd = true;
                                }
                                break;
                                
                            default:
                                break;
                        }
                    }
                }
            }
            #endregion FILL_TABLE

            #region GROUP_AND_SORT_TABLE
            //Сгруппированная таблица
            System.Data.DataTable grouped = table.Clone();

            var groupe = from row in table.AsEnumerable()
                         group row by row["ПОЛНОЕ_ИМЯ"] into g
                         select new { Name = g.Key, Rows = g };

            foreach (var g in groupe)
            {
                double sum = 0;
                DataRow expRow = null;
                foreach (DataRow row in g.Rows)
                {
                    sum += MyConvert.ToDouble(row["КОЛИЧЕСТВО"].ToString());
                    expRow = row;
                }
                expRow["КОЛИЧЕСТВО"] = sum.ToString();
                grouped.ImportRow(expRow);
            }

            //Сортировка по разделам
            //чтение приоритета разделов
            CSVReader rdr = new CSVReader(RazdPriortyFilePath);
            List<string> razdels = new List<string>();
            rdr.GetArray("Наименование_раздела", razdels, ';');

            //назначение приоритета

            System.Data.DataColumn column = new System.Data.DataColumn("Order", typeof(Int32));
            grouped.Columns.Add(column);
            foreach (DataRow row in grouped.Rows)
            {
                string razd = row["РАЗДЕЛ"].ToString();
                if (razdels.Contains(razd))
                {
                    int index = razdels.IndexOf(razd);
                    row["Order"] = index;
                }
                else
                {
                    row["Order"] = int.MaxValue;
                }
            }
            //сортировка по разделам 
            DataView dv = new DataView(grouped);
            dv.Sort = "Order, Раздел ASC";

            try
            {
                CConstructComparer myComparer = new CConstructComparer();
                cnstrList.Sort(myComparer);
            }
            catch (System.Exception ex)
            {

                DialogResult res = MessageBox.Show(null, "Не указаны индексы опор.\nСоставить ведомость в порядке выбора опор?\n" + 
                    ex.Message, "Поопорная ведомость", MessageBoxButtons.OKCancel,MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (res == DialogResult.Cancel) return;
            }

            //fill new out DataTable            
            System.Data.DataTable tblOut = new System.Data.DataTable();
            //полное имя | обозначение | наименование | кол-во на опору
            for (int i = 0; i < cnstrList.Count + 3; i++)
            {
                tblOut.Columns.Add();
            }
            //Пустая строка для номера опоры
            tblOut.Rows.Add();
            tblOut.Rows[0][2] = "Номер опоры";
            {
                int crow = 1;   //current row                
                string razd = "";
                for (int i = 0; i < dv.Count; i++)
                {
                    string razdTable = dv[i]["РАЗДЕЛ"].ToString();
                    if (razd != razdTable)
                    {
                        //add razdel name
                        razd = razdTable;
                        tblOut.Rows.Add();
                        tblOut.Rows[crow][2] = razd;
                        crow++;
                    }
                    tblOut.Rows.Add();
                    tblOut.Rows[crow][0] = dv[i]["ПОЛНОЕ_ИМЯ"].ToString();
                    tblOut.Rows[crow][1] = dv[i]["НАИМЕНОВАНИЕ"].ToString();
                    tblOut.Rows[crow][2] = dv[i]["ТИП"].ToString(); 
                    crow++;
                }
            }

            //опрос списка и заполнение количества
            for (int i = 0; i < cnstrList.Count; i++)
            {
                //Номер опоры
                tblOut.Rows[0][i + 3] = cnstrList[i].GetField("НОМЕР");
                for (int j = 1; j < tblOut.Rows.Count; j++)
                {
                    string fullName = tblOut.Rows[j][0].ToString();
                    double cnt = cnstrList[i].GetDetailCount(fullName);
                    if (cnt > 0)
                    {
                        //Количество деталей из этой строки
                        tblOut.Rows[j][i + 3] = cnt;
                    }
                }
            }
            tblOut.Columns.RemoveAt(0);
            #endregion GROUP_AND_SORT_TABLE
            #region FILL_LAYOUT
            if (formatA4)
            {
                Transaction trans = dbCurrent.TransactionManager.StartTransaction();
                {
                    try
                    {
                        //string outName = ImportLayout(_TemplatePath + "1.dwg", A4FirstLayName);
                        //Layout lt = GetLayout(outName);
                        Layout lt = ImportLayoutWithOrWithoutReplace(_TemplatePath + "1.dwg", A4FirstLayName);
                        string outName = lt.LayoutName;
                        ObjectId tblID = GetFirstTable(lt);
                        Table tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                        int nrows = tbl.Rows.Count;
                        int tblrow = 1;

                        for (int i = 0; i < tblOut.Rows.Count; i++)
                        {
                            if (tblrow == nrows)
                            {
                                throw new System.Exception("Не хватает строк в таблице");
                                //new layout 
                                //tbl.GenerateLayout();
                                //outName = ImportLayout(_TemplatePath + "1.dwg", A4NextLayName);
                                //lt = GetLayout(outName);
                                //tblID = GetFirstTable(lt);
                                //tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                                //nrows = tbl.Rows.Count;
                                //tblrow = 1;
                            }
                            for (int j = 0; (j < tbl.Columns.Count) & (j<tblOut.Columns.Count); j++)
                            {
                                tbl.Cells[tblrow, j].TextString = tblOut.Rows[i][j].ToString();
                            }                            
                            tblrow++;
                        }
                    }
                    finally
                    {
                        trans.Commit();
                    }
                }
            }
            else
                //A3
            {
                Transaction trans = dbCurrent.TransactionManager.StartTransaction();
                {
                    try
                    {
                        //string outName = ImportLayout(_TemplatePath + "1.dwg", A3FirstLayName);
                        //Layout lt = GetLayout(outName);
                        Layout lt = ImportLayoutWithOrWithoutReplace(_TemplatePath + "1.dwg", A3FirstLayName);
                        string outName = lt.LayoutName;
                        ObjectId tblID = GetFirstTable(lt);
                        Table tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                        int nrows = tbl.Rows.Count;
                        int tblrow = 1;

                        for (int i = 0; i < tblOut.Rows.Count; i++)
                        {
                            if (tblrow == nrows)
                            {
                                throw new System.Exception("Не хватает строк в таблице");
                                //new layout 
                                //tbl.GenerateLayout();
                                //outName = ImportLayout(_TemplatePath + "1.dwg", A3NextLayName);
                                //lt = GetLayout(outName);
                                //tblID = GetFirstTable(lt);
                                //tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                                //nrows = tbl.Rows.Count;
                                //tblrow = 1;
                            }
                            for (int j = 0; (j < tbl.Columns.Count) & (j < tblOut.Columns.Count); j++)
                            {
                                tbl.Cells[tblrow, j].TextString = tblOut.Rows[i][j].ToString();
                            }  
                            tblrow++;
                        }
                    }
                    finally
                    {
                        trans.Commit();
                    }
                }
            }
            #endregion FILL_LAYOUT
        }

        /// <summary>
        /// Составляет спецификацию формата А3/А4 для деталей и конструкций
        /// </summary>
        [CommandMethod("vl_specification")]
        public void cmdSpecification()
        {
            try
            {
                string A4FirstLayName = "specA4First";
                string A4NextLayName = "specA4Next";
                string A3FirstLayName = "specA3First";
                string A3NextLayName = "specA3Next";

                #region SELECTING
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptSelectionOptions selOpt = new PromptSelectionOptions();
                selOpt.MessageForAdding = "Выберите позиции: ";
                TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
                SelectionFilter sfilter = new SelectionFilter(values);
                selOpt.AllowDuplicates = false;
                PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
                if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }

                //Prompt format layout
                bool formatA4 = true;
                PromptKeywordOptions pko = new PromptKeywordOptions("\nФормат спецификации : ");
                pko.AllowNone = true;
                pko.Keywords.Add("A3");
                pko.Keywords.Add("A4");
                pko.Keywords.Default = "A4";
                PromptResult pkr = ed.GetKeywords(pko);
                if (pkr.Status != PromptStatus.OK) return;
                else
                {
                    if (pkr.StringResult == "A3") formatA4 = false;
                }
                //End Prompt format layout

                ObjectId[] objIds = sset.Value.GetObjectIds();
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                #endregion SELECTING

                #region FILL_TABLE

                System.Data.DataTable table = new System.Data.DataTable();

                foreach (string attr in attributes)
                {
                    table.Columns.Add(attr, typeof(string));
                }

                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in objIds)
                    {
                        BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                        AttributeCollection attcol = bRef.AttributeCollection;

                        bool fnd = false;   //найден ли результат
                        foreach (ObjectId att in attcol)
                        {
                            if (fnd) break;
                            AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);

                            switch (atRef.Tag)
                            {
                                case "ПОЛНОЕ_ИМЯ":
                                    DataRow row = GetDetail(atRef.TextString);
                                    //copy row without quantity
                                    table.ImportRow(row);
                                    DataRow NewRow = table.Rows[table.Rows.Count - 1];
                                    //второй проход по аттрибутам блока для нахождения количества
                                    foreach (ObjectId att2 in attcol)
                                    {
                                        AttributeReference atRef2 = (AttributeReference)tr.GetObject(att2, OpenMode.ForRead);
                                        switch (atRef2.Tag)
                                        {
                                            case "ПОЛНОЕ_ИМЯ":
                                                break;
                                            case "КОЛИЧЕСТВО":
                                                double qty = double.Parse(atRef2.TextString.Replace(',', '.'));
                                                NewRow["КОЛИЧЕСТВО"] = qty;
                                                fnd = true;
                                                break;
                                            default:
                                                if (attributes.Contains(atRef2.Tag))
                                                {
                                                    NewRow[atRef2.Tag] = atRef2.TextString;
                                                }
                                                break;
                                        }
                                    }
                                    break;


                                case "XML_CONSTRUCT":
                                    CConstruct constr = new CConstruct();
                                    constr.ReadFromString(atRef.TextString);
                                    foreach (DataRow drow in constr._details.Rows)
                                    {
                                        string dtl = drow["Наименование_детали"].ToString();
                                        double qty1 = double.Parse(drow["Количество"].ToString().Replace(',', '.'));
                                        DataRow row2 = GetDetail(dtl);
                                        if (row2 != null)
                                        {
                                            table.ImportRow(row2);
                                            DataRow NewRow2 = table.Rows[table.Rows.Count - 1];
                                            NewRow2["КОЛИЧЕСТВО"] = qty1;
                                        }

                                    }
                                    foreach (DataRow Crow in constr._constructs.Rows)
                                    {
                                        string xmlString = Crow["XmlString"].ToString();
                                        CConstruct inConstr = new CConstruct();
                                        inConstr.ReadFromString(xmlString);
                                        double qtyConstr = double.Parse(Crow["Количество"].ToString().Replace(',', '.'));

                                        foreach (DataRow drow in inConstr._details.Rows)
                                        {
                                            string dtl = drow["Наименование_детали"].ToString();
                                            double qty1 = double.Parse(drow["Количество"].ToString().Replace(',', '.'));
                                            DataRow row2 = GetDetail(dtl);
                                            if (row2 != null)
                                            {
                                                table.ImportRow(row2);
                                                DataRow NewRow2 = table.Rows[table.Rows.Count - 1];
                                                NewRow2["КОЛИЧЕСТВО"] = qty1 * qtyConstr;
                                            }

                                        }
                                    }
                                    fnd = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                #endregion FILL_TABLE

                #region GROUP_AND_SORT_TABLE
                //Сгруппированная таблица с суммарным количеством
                System.Data.DataTable grouped = table.Clone();

                var groupe = from row in table.AsEnumerable()
                             group row by new { Name = row["ПОЛНОЕ_ИМЯ"], Razdel = row["РАЗДЕЛ"] } into g
                             select new { Name = g.Key, Rows = g };

                foreach (var g in groupe)
                {
                    double sum = 0;
                    DataRow expRow = null;
                    foreach (DataRow row in g.Rows)
                    {
                        sum += MyConvert.ToDouble(row["КОЛИЧЕСТВО"].ToString());
                        expRow = row;
                    }
                    expRow["КОЛИЧЕСТВО"] = sum.ToString();
                    grouped.ImportRow(expRow);
                }

                //Сортировка по разделам
                //чтение приоритета разделов
                CSVReader rdr = new CSVReader(RazdPriortyFilePath);
                List<string> razdels = new List<string>();
                rdr.GetArray("Наименование_раздела", razdels, ';');

                //назначение приоритета

                System.Data.DataColumn column = new System.Data.DataColumn("Order", typeof(Int32));
                grouped.Columns.Add(column);
                foreach (DataRow row in grouped.Rows)
                {
                    string razd = row["РАЗДЕЛ"].ToString();
                    if (razdels.Contains(razd))
                    {
                        int index = razdels.IndexOf(razd);
                        row["Order"] = index;
                    }
                    else
                    {
                        row["Order"] = int.MaxValue;
                    }
                }
                //сортировка по разделам 
                DataView dv = new DataView(grouped);
                dv.Sort = "Order, Раздел ASC";

                //fill new out DataTable
                System.Data.DataTable tblOut = new System.Data.DataTable();
                for (int i = 0; i < 9; i++)
                {
                    tblOut.Columns.Add();
                }

                {
                    int crow = 0;   //current row
                    int cntr = 1;   //counter detail 
                    string razd = "";
                    for (int i = 0; i < dv.Count; i++)
                    {
                        string razdTable = dv[i]["РАЗДЕЛ"].ToString();
                        if (razd != razdTable)
                        {
                            //add razdel name
                            razd = razdTable;
                            tblOut.Rows.Add();
                            tblOut.Rows[crow][1] = razd;
                            crow++;
                        }
                        tblOut.Rows.Add();
                        tblOut.Rows[crow][0] = cntr.ToString();
                        tblOut.Rows[crow][1] = dv[i]["НАИМЕНОВАНИЕ"].ToString();
                        tblOut.Rows[crow][2] = dv[i]["ТИП"].ToString();
                        tblOut.Rows[crow][3] = dv[i]["КОД_ОБОРУДОВАНИЯ"].ToString();
                        tblOut.Rows[crow][4] = dv[i]["ЗАВОД_ИЗГОТОВИТЕЛЬ"].ToString();
                        tblOut.Rows[crow][5] = dv[i]["ЕД_ИЗМ"].ToString();
                        tblOut.Rows[crow][6] = dv[i]["КОЛИЧЕСТВО"].ToString().Replace('.', ',');
                        tblOut.Rows[crow][7] = dv[i]["МАССА"].ToString();
                        tblOut.Rows[crow][8] = dv[i]["ПРИМЕЧАНИЕ"].ToString();
                        cntr++;
                        crow++;
                    }
                }
                #endregion GROUP_AND_SORT_TABLE

                DataGridViewPrevFrm frm = new DataGridViewPrevFrm();
                frm.dataGridView1.DataSource = tblOut;
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);


                #region FILL_LAYOUT

                //add and fill acad tables in layouts 
                if (formatA4)
                {
                    Transaction trans = dbCurrent.TransactionManager.StartTransaction();
                    {
                        try
                        {
                            //string outName = ImportLayout(_TemplatePath + "1.dwg", A4FirstLayName);
                            //Layout lt = GetLayout(outName);
                            Layout lt = ImportLayoutWithOrWithoutReplace(_TemplatePath + "1.dwg", A4FirstLayName);
                            string outName = lt.LayoutName;
                            ObjectId tblID = GetFirstTable(lt);
                            Table tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                            int nrows = tbl.Rows.Count;
                            int tblrow = 1;

                            for (int i = 0; i < tblOut.Rows.Count; i++)
                            {
                                if (tblrow == nrows)
                                {
                                    //new layout 
                                    //tbl.GenerateLayout();
                                    outName = ImportLayout(_TemplatePath + "1.dwg", A4NextLayName);
                                    lt = GetLayout(outName);
                                    tblID = GetFirstTable(lt);
                                    tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                                    nrows = tbl.Rows.Count;
                                    tblrow = 1;
                                }
                                for (int j = 0; j < 3; j++)
                                {
                                    tbl.Cells[tblrow, j].TextString = tblOut.Rows[i][j].ToString();
                                }
                                for (int j = 5; j < 9; j++)
                                {
                                    tbl.Cells[tblrow, j - 2].TextString = tblOut.Rows[i][j].ToString();
                                }
                                tblrow++;
                            }
                        }
                        finally
                        {
                            trans.Commit();
                        }
                    }
                }
                else
                {
                    Transaction trans = dbCurrent.TransactionManager.StartTransaction();
                    {
                        try
                        {
                            //string outName = ImportLayout(_TemplatePath + "1.dwg", A3FirstLayName);
                            //Layout lt = GetLayout(outName);
                            Layout lt = ImportLayoutWithOrWithoutReplace(_TemplatePath + "1.dwg", A3FirstLayName);
                            string outName = lt.LayoutName;

                            ObjectId tblID = GetFirstTable(lt);
                            Table tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                            int nrows = tbl.Rows.Count;
                            int tblrow = 1;

                            for (int i = 0; i < tblOut.Rows.Count; i++)
                            {
                                if (tblrow == nrows)
                                {
                                    //new layout 
                                    //tbl.GenerateLayout();
                                    outName = ImportLayout(_TemplatePath + "1.dwg", A3NextLayName);
                                    lt = GetLayout(outName);
                                    tblID = GetFirstTable(lt);
                                    tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                                    nrows = tbl.Rows.Count;
                                    tblrow = 1;
                                }
                                for (int j = 0; j < 9; j++)
                                {
                                    tbl.Cells[tblrow, j].TextString = tblOut.Rows[i][j].ToString();
                                }
                                tblrow++;
                            }
                        }
                        finally
                        {
                            trans.Commit();
                        }
                    }
                }
                #endregion FILL_LAYOUT
            }
            catch (System.Exception ex )
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private ObjectId GetFirstTable(Layout lt)
        {
            ObjectId tblID = ObjectId.Null;
            Database db = HostApplicationServices.WorkingDatabase;            
            Transaction trans = db.TransactionManager.StartTransaction();
            try
            {
                BlockTableRecord layoutTableRecord = (BlockTableRecord)lt.
                    Database.TransactionManager.GetObject(lt.BlockTableRecordId, OpenMode.ForWrite, false);
                foreach (ObjectId entityId in layoutTableRecord)
                {
                    object acadObject = lt.Database.TransactionManager.
                        GetObject(entityId, OpenMode.ForRead, false);
                    Table tbl = acadObject as Table;
                    if (tbl != null)
                    {
                        tblID = tbl.ObjectId;
                        break;
                    }
                }
            }
            finally
            {
                trans.Commit();                
            }
            return tblID;
        }
        private double DegMinSecToRadians(string angleDegSec)
        {
            try
            {
                double res, deg, min, sec;
                string[] strAr = new string[3];

                if (angleDegSec.Contains(","))
                {
                    string[] tmpAr = angleDegSec.Split(',');
                    if (!(tmpAr.Length == 3))
                    {
                        throw new System.Exception("Неверный ввод градусной меры");
                    }
                    strAr[0] = tmpAr[0];
                    strAr[1] = tmpAr[1];
                    strAr[2] = tmpAr[2];
                }
                if (angleDegSec.Contains("."))
                {
                    string[] tmpAr = angleDegSec.Split('.');
                    if (!(tmpAr.Length == 3))
                    {
                        throw new System.Exception("Неверный ввод градусной меры");
                    }
                    strAr[0] = tmpAr[0];
                    strAr[1] = tmpAr[1];
                    strAr[2] = tmpAr[2];
                }
                deg = System.Convert.ToDouble(strAr[0]);
                min = System.Convert.ToDouble(strAr[1]);
                sec = System.Convert.ToDouble(strAr[2]);
                res = PI / 180 * (deg + min / 60 + sec / 3600);
                return res;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return 0;
            }

        }

        [CommandMethod("VL_MATCHPR")]
        public void cmdMatchProperties()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            PromptEntityOptions opt = new PromptEntityOptions("Select a source blockreference: \n");
            opt.SetRejectMessage("Selected object not a blockreference...\n");
            opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.BlockReference), false);            

            PromptEntityResult res = ed.GetEntity(opt);
            if (res.Status != PromptStatus.OK) return;
            string xmlString = "";
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    DBObject obj = tr.GetObject(res.ObjectId, OpenMode.ForWrite, false);
                    bool fnd = false;   //найден ли результат
                    if (obj.GetType() == typeof(Autodesk.AutoCAD.DatabaseServices.BlockReference))
                    {
                        BlockReference bRef = obj as BlockReference;
                        AttributeCollection attcol = bRef.AttributeCollection;
                        
                        foreach (ObjectId att in attcol)
                        {
                            if (fnd) break;
                            AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);

                            switch (atRef.Tag)
                            {
                                case "XML_CONSTRUCT":
                                    fnd = true;
                                    xmlString = atRef.TextString;   
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    if (fnd)
                    {                        
                        PromptSelectionOptions selOpt = new PromptSelectionOptions();
                        selOpt.MessageForAdding = "Select a destination: \n";
                        TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
                        SelectionFilter sfilter = new SelectionFilter(values);
                        selOpt.AllowDuplicates = false;
                        PromptSelectionResult sset = ed.GetSelection(selOpt, sfilter);
                        if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
                        ObjectId[] objIds = sset.Value.GetObjectIds();
                        foreach (ObjectId id in objIds)
                        {
                            fnd = false;
                            BlockReference bRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            AttributeCollection attcol = bRef.AttributeCollection;
                            foreach (ObjectId att in attcol)
                            {
                                if (fnd) break;
                                AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForWrite);

                                switch (atRef.Tag)
                                {
                                    case "XML_CONSTRUCT":
                                        atRef.TextString = xmlString;
                                        fnd = true;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static ObjectId MakeLayer(string LayerName, string ltypeName, short layerCol, bool toPlot)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            DocumentLock dloc = doc.LockDocument();
            Editor ed = doc.Editor;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
            using (Transaction tr = tm.StartTransaction())
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
                if (lt.Has(LayerName))
                {
                    return lt[LayerName];
                }

                LayerTableRecord ltr = new LayerTableRecord();
                ltr.Name = LayerName;
                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, layerCol);
                ObjectId ltypeID;
                LinetypeTable ltt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForWrite);
                if (ltt.Has(ltypeName))
                {
                    ltypeID = ltt[ltypeName];
                }
                else
                {
                    LinetypeTableRecord ltype = new LinetypeTableRecord();
                    ltype.Name = ltypeName;
                    ltt.Add(ltype);
                    tr.AddNewlyCreatedDBObject(ltype, true);
                    ltypeID = ltype.ObjectId;
                }
                ltr.LinetypeObjectId = ltypeID;
                ltr.IsPlottable = toPlot;
                lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                ed.Regen();
                tr.Commit();
                dloc.Dispose();
                return lt[LayerName];
            }



        }

        //[CommandMethod("ZmC")]
        //static public void ZoomCenter()
        //{
        //    // Center the view at 5,5,0
        //    Zoom(new Point3d(), new Point3d(), new Point3d(5, 5, 0), 1);
        //}

        public static void Zoom(Point3d pMin, Point3d pMax, Point3d pCenter, double dFactor)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if (acCurDb.TileMode == true)
            {
                if (pMin.Equals(new Point3d()) == true &&
                    pMax.Equals(new Point3d()) == true)
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist,
                                                   acView.ViewDirection,
                                                   acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        pMin = new Point3d(pCenter.X - (acView.Width / 2),
                                           pCenter.Y - (acView.Height / 2), 0);

                        pMax = new Point3d((acView.Width / 2) + pCenter.X,
                                           (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line acLine = new Line(pMin, pMax))
                    {
                        eExtents = new Extents3d(acLine.Bounds.Value.MinPoint,
                                                 acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 0)
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                                                 ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 0)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }

        public static void ViewEntityPos(ObjectId id)
        {
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                //ed.UpdateTiledViewportsInDatabase();                
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                    Extents3d entityExtent = ent.GeometricExtents;

                    ViewportTable vpTbl = tr.GetObject(
                                                        db.ViewportTableId,
                                                        OpenMode.ForRead
                                                      ) as ViewportTable;

                    ViewportTableRecord viewportTableRec
                           = tr.GetObject(vpTbl["*Active"], OpenMode.ForWrite)
                                                       as ViewportTableRecord;

                    Matrix3d matWCS2DCS
                       = Matrix3d.PlaneToWorld(viewportTableRec.ViewDirection);

                    matWCS2DCS = Matrix3d.Displacement(
                        viewportTableRec.Target - Point3d.Origin) * matWCS2DCS;

                    matWCS2DCS = Matrix3d.Rotation
                                                (
                                                 -viewportTableRec.ViewTwist,
                                                 viewportTableRec.ViewDirection,
                                                 viewportTableRec.Target
                                                 ) * matWCS2DCS;

                    matWCS2DCS = matWCS2DCS.Inverse();

                    entityExtent.TransformBy(matWCS2DCS);

                    Point2d center = new Point2d(
                    (entityExtent.MaxPoint.X + entityExtent.MinPoint.X) * 0.5,
                    (entityExtent.MaxPoint.Y + entityExtent.MinPoint.Y) * 0.5);
                    //new
                    doc.Editor.Regen();
                    ent.Highlight();
                    Zoom(new Point3d(), new Point3d(), new Point3d(center.X, center.Y, 0), 1);                    
                    
                    //viewportTableRec.CenterPoint = center;                    
                    tr.Commit();
                }
                //ed.UpdateTiledViewportsFromDatabase();
                
                
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// Inserts all attributreferences
        /// </summary>
        /// <param name="blkRef">Blockreference to append the attributes</param>
        /// <param name="strAttributeTag">The tag to insert the <paramref name="strAttributeText"/></param>
        /// <param name="strAttributeText">The textstring for <paramref name="strAttributeTag"/></param>
        public static void InsertBlockAttibuteRef(BlockReference blkRef, string strAttributeTag, string strAttributeText)
        {
            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = dbCurrent.TransactionManager;
            using (Transaction tr = tm.StartTransaction())
            {
                BlockTableRecord btAttRec = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                foreach (ObjectId idAtt in btAttRec)
                {
                    Entity ent = (Entity)tr.GetObject(idAtt, OpenMode.ForRead);
                    if (ent is AttributeDefinition)
                    {
                        AttributeDefinition attDef = (AttributeDefinition)ent;
                        AttributeReference attRef = new AttributeReference();
                        attRef.SetAttributeFromBlock(attDef, blkRef.BlockTransform);
                        if (attRef.Tag == strAttributeTag)
                        {
                            attRef.TextString = strAttributeText;
                            ObjectId idTemp = blkRef.AttributeCollection.AppendAttribute(attRef);
                            tr.AddNewlyCreatedDBObject(attRef, true);
                        }
                    }
                }
                tr.Commit();
            }
        }

        private Point3d acgePolar(Point3d pt, double angle, double dist)
        {
            return pt + Vector3d.XAxis.RotateBy(angle, Vector3d.ZAxis) * dist;
        }

        public string ImportLayout(string fileName, string layoutName)
        {
            string retString = "";
            DocumentLock oLock = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.LockDocument();
            Database dbSource = new Database(false, false);
            Database db = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    dbSource.ReadDwgFile(fileName, System.IO.FileShare.Read, true, null);
                    ObjectId idDbdSource = dbSource.LayoutDictionaryId;
                    DBDictionary dbdLayout = (DBDictionary)trans.GetObject(idDbdSource, OpenMode.ForRead, false, false);
                    ObjectId idLayout;

                    ObjectIdCollection idc = new ObjectIdCollection();
                    //Ищем имя листа в исходном файле
                    foreach (DictionaryEntry deLayout in dbdLayout)
                    {
                        string curLayName = deLayout.Key.ToString().ToLower();
                        

                        if (curLayName == layoutName.ToLower())
                        {
                            idLayout = (ObjectId)deLayout.Value;
                            Layout ltr = (Layout)trans.GetObject(idLayout, OpenMode.ForWrite);

                            string copyLayName = layoutName;
                            int i = 1;
                            while (CheckLayout(copyLayName))
                            {
                                copyLayName = layoutName + "_" + i.ToString();
                                ltr.LayoutName = copyLayName;
                                i += 1;
                            }
                            idc.Add(idLayout);
                            retString = copyLayName;
                            break;
                        }
                    }

                    IdMapping im = new IdMapping();
                    db.WblockCloneObjects(idc, db.LayoutDictionaryId, im, DuplicateRecordCloning.Ignore, false);
                    trans.Commit();
                    return retString;
                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return retString;
            }
            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return retString;
            }
            finally
            {

                dbSource.Dispose();
                oLock.Dispose();
            }
        }

        private Layout ImportLayoutWithOrWithoutReplace(string fileName, string layoutName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (CheckLayout(layoutName))
            {
                DialogResult res = MessageBox.Show("Лист " + layoutName + " существует. Заменить?", 
                    "vl_utility", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    DocumentLock oLock = Autodesk.AutoCAD.ApplicationServices.Application.
                        DocumentManager.MdiActiveDocument.LockDocument();
                    Database dbSource = HostApplicationServices.WorkingDatabase;
                    try
                    {
                        using (Transaction trans = dbSource.TransactionManager.StartTransaction())
                        {                            
                            DBDictionary dbdLayout = (DBDictionary)trans.GetObject(dbSource.LayoutDictionaryId,
                                OpenMode.ForRead, false, false);
                            
                            foreach (DictionaryEntry deLayout in dbdLayout)
                            {
                                string curLayName = deLayout.Key.ToString().ToLower();
                                if (curLayName == layoutName.ToLower())
                                {
                                    LayoutManager.Current.DeleteLayout(layoutName); // Delete layout.
                                }
                            }
                            trans.Commit();
                        } 
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    catch (System.Exception ex)
                    {

                        MessageBox.Show(ex.ToString(), "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    finally
                    {
                        dbSource.Dispose();
                        oLock.Dispose();
                    }                    
                }
            }
            string outName = ImportLayout(fileName, layoutName);
            ed.Regen();   // Updates AutoCAD GUI to relect changes.
            return GetLayout(outName);            
        }

        public static bool CheckLayout(string layoutName)
        {
            DocumentLock oLock = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.LockDocument();
            Database dbSource = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Transaction trans = dbSource.TransactionManager.StartTransaction())
                {
                    ObjectId idDbdSource = dbSource.LayoutDictionaryId;
                    DBDictionary dbdLayout = (DBDictionary)trans.GetObject(idDbdSource, OpenMode.ForRead, false, false);


                    ObjectIdCollection idc = new ObjectIdCollection();
                    foreach (DictionaryEntry deLayout in dbdLayout)
                    {
                        string curLayName = deLayout.Key.ToString().ToLower();
                        if (curLayName == layoutName.ToLower())
                        {
                            return true;
                        }
                    }

                    trans.Commit();
                }
                return false;

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {

                dbSource.Dispose();
                oLock.Dispose();
            }
        }

        public Layout GetLayout(string layoutName)
        {
            DocumentLock oLock = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument.LockDocument();
            Database dbSource = HostApplicationServices.WorkingDatabase;
            try
            {
                using (Transaction trans = dbSource.TransactionManager.StartTransaction())
                {
                    ObjectId idDbdSource = dbSource.LayoutDictionaryId;
                    DBDictionary dbdLayout = (DBDictionary)trans.GetObject(idDbdSource, OpenMode.ForRead, false, false);


                    ObjectIdCollection idc = new ObjectIdCollection();
                    foreach (DictionaryEntry deLayout in dbdLayout)
                    {
                        string curLayName = deLayout.Key.ToString().ToLower();
                        if (curLayName == layoutName.ToLower())
                        {
                            ObjectId idLayout = (ObjectId)deLayout.Value;
                            return (Layout)trans.GetObject(idLayout, OpenMode.ForRead);
                        }
                    }

                    trans.Commit();
                }
                return null;

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {

                dbSource.Dispose();
                oLock.Dispose();
            }
        }

        public static DataRow GetDetail(string FullName)
        {
            DataRow retRow = null;
            int fnd = 0;

            foreach (DictionaryEntry pair in m_usrform.FileMD5DetsTable)
            {
                System.Data.DataTable tbl = (pair.Value as HashFilesTable).table;
                string search = "ПОЛНОЕ_ИМЯ='" + FullName + "'";
                DataRow[] rows = tbl.Select(search);
                if (rows.Length == 1)
                {
                    retRow = rows[0];
                }
                fnd += rows.Length;
            }
            if (fnd != 1) MessageBox.Show("Деталь " + FullName + " встечается в базе " + fnd.ToString() + " раз.");
            return retRow;
            
            //foreach (System.Data.DataTable tbl in _dbTables)
            //{
            //    string search = "ПОЛНОЕ_ИМЯ='" + FullName + "'";
            //    DataRow[] rows = tbl.Select(search);
            //    if (rows.Length == 1)
            //    {
            //        retRow = rows[0];                    
            //    }
            //    fnd += rows.Length;
            //}
            //if(fnd!=1) MessageBox.Show("Деталь " + FullName + " встечается в базе " + fnd.ToString() + " раз.");
            //return retRow;
        }

        //private Hashtable GetDetail(string FullName)
        //{
        //    Hashtable htbl = null;
        //    int finds = 0;
        //    foreach (System.Data.DataTable tbl in _dbTables)
        //    {
        //        foreach (DataRow row in tbl.Rows)
        //        {
        //            if (row["Полное_Имя"].ToString() == FullName)
        //            {
        //                finds += 1;
        //                if (finds == 1)
        //                {
        //                    htbl = new Hashtable();
        //                    for (int i = 0; i < tbl.Columns.Count; i++)
        //                    {
        //                        htbl.Add(tbl.Columns[i].ColumnName, row[i].ToString());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //if (finds!=1) MessageBox.Show("Деталь " + FullName + " встречается " + finds.ToString() + " раз");
        //    return htbl;
        //}

        [CommandMethod("CBL")]
        public void CombineBlocksIntoLibrary()
        {

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database destDb = doc.Database;
            // Get name of folder from which to load and import blocks
            PromptResult pr = ed.GetString("\nEnter the folder of source drawings: ");
            if (pr.Status != PromptStatus.OK) return;
            string pathName = pr.StringResult;
            // Check the folder exists
            if (!Directory.Exists(pathName))
            {
                ed.WriteMessage("\nDirectory does not exist: {0}", pathName);
                return;
            }
            // Get the names of our DWG files in that folder
            string[] fileNames = Directory.GetFiles(pathName, "*.dwg");
            // A counter for the files we've imported
            int imported = 0, failed = 0;
            // For each file in our list
            foreach (string fileName in fileNames)
            {
                // Double-check we have a DWG file (probably unnecessary)
                if (fileName.EndsWith(".dwg",StringComparison.InvariantCultureIgnoreCase))
                {
                    // Catch exceptions at the file level to allow skipping
                    try
                    {
                        // Create a source database to load the DWG into
                        using (Database db = new Database(false, true))
                        {
                            // Read the DWG into our side database
                            db.ReadDwgFile(fileName, FileShare.Read, true, "");
                            // Create a list of block identifiers (will only
                            // contain one entry, the modelspace ObjectId)
                            ObjectIdCollection ids = new ObjectIdCollection();
                            // Start a transaction on the source database
                            Transaction tr = db.TransactionManager.StartTransaction();
                            using (tr)
                            {
                                // Open the block table
                                BlockTable bt =(BlockTable)tr.GetObject(db.BlockTableId,OpenMode.ForRead);
                                // Add modelspace to list of blocks to import
                                ids.Add(bt[BlockTableRecord.ModelSpace]);
                                // Committing is cheaper than aborting
                                tr.Commit();
                            }
                            // Copy our modelspace block from the source to
                            // destination database
                            // (will also copy required, referenced objects)
                            IdMapping im = new IdMapping();
                            db.WblockCloneObjects(ids, destDb.BlockTableId,im,
                                DuplicateRecordCloning.MangleName,false);
                            // Start a transaction on the destination database
                            Transaction tr2 =destDb.TransactionManager.StartTransaction();

                            using (tr2)
                            {
                                // Work through the results of the WblockClone
                                foreach (IdPair ip in im)
                                {
                                    // Open each new destination object, checking for
                                    // BlockTableRecords
                                    BlockTableRecord btr = tr2.GetObject(ip.Value, OpenMode.ForRead)
                                      as BlockTableRecord;
                                    if (btr != null)
                                    {
                                        // If the name starts with the modelspace string
                                        if ( btr.Name.StartsWith(BlockTableRecord.ModelSpace,
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Get write access to it and change the name
                                            // to that of the source drawing
                                            btr.UpgradeOpen();
                                            btr.Name = Path.GetFileNameWithoutExtension(fileName);
                                        }
                                    }
                                }
                                // We need to commit, as we've made changes
                                tr2.Commit();
                            }
                            // Print message and increment imported block counter
                            ed.WriteMessage("\nImported from \"{0}\".", fileName);
                            imported++;
                        }
                    }

                    catch (System.Exception ex)
                    {
                        ed.WriteMessage("\nProblem importing \"{0}\": {1} - file skipped.",
                          fileName, ex.Message);
                        failed++;
                    }
                }
            }
            ed.WriteMessage("\nImported block definitions from {0} files{1} in " +
              "\"{2}\" into the current drawing.", imported,
              failed > 0 ? " (" + failed + " failed)" : "", pathName);
        }

        public void ImportBlock(string sourceFileName, string BlockName)
        {
            DocumentCollection dm =
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Editor ed = dm.MdiActiveDocument.Editor;
            Database destDb = dm.MdiActiveDocument.Database;
            Database sourceDb = new Database(false, true);            
            try
            {
                // Read the DWG into a side database
                sourceDb.ReadDwgFile(sourceFileName,
                                    System.IO.FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                ObjectIdCollection blockIds = new ObjectIdCollection();
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm =
                  sourceDb.TransactionManager;

                using (Transaction myT = tm.StartTransaction())
                {
                    // Open the block table
                    BlockTable bt = (BlockTable)tm.GetObject(sourceDb.BlockTableId,
                                                OpenMode.ForRead, false);
                    if (!bt.Has(BlockName))
                    {
                        throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidBlockName,
                            "\nBlock \"" + BlockName + "\" not found in \"" + sourceFileName + "\"\n");                                              
                    }
                    BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockName], 
                        OpenMode.ForRead, false);

                    if (!btr.IsAnonymous && !btr.IsLayout) blockIds.Add(btr.ObjectId);
                    btr.Dispose(); 
                }

                // Copy blocks from source to destination database

                IdMapping mapping = new IdMapping();

                sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping,
                    DuplicateRecordCloning.Ignore, false);
            }

            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                throw ex;
            }
            sourceDb.Dispose();
        }
        
        private BlockReference BlockJig(string BlockName, Hashtable HAttributes)
        {
            Document doc =
              Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            BlockReference br = null;
           
            Transaction tr =
              doc.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt =
                  (BlockTable)tr.GetObject(
                    db.BlockTableId,
                    OpenMode.ForRead
                  );

                if (!bt.Has(BlockName))
                {
                    throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidBlockName,
                      "\nBlock \"" + BlockName + "\" not found.");                    
                }

                BlockTableRecord space =
                  (BlockTableRecord)tr.GetObject(
                    db.CurrentSpaceId,
                    OpenMode.ForWrite
                  );

                BlockTableRecord btr =
                  (BlockTableRecord)tr.GetObject(
                    bt[BlockName],
                    OpenMode.ForRead);

                // Block needs to be inserted to current space before
                // being able to append attribute to it

                br = new BlockReference(new Point3d(), btr.ObjectId);
                space.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);

                Dictionary<ObjectId, AttInfo> attInfo =
                  new Dictionary<ObjectId, AttInfo>();

                if (btr.HasAttributeDefinitions)
                {
                    foreach (ObjectId id in btr)
                    {
                        DBObject obj =
                          tr.GetObject(id, OpenMode.ForRead);
                        AttributeDefinition ad =
                          obj as AttributeDefinition;

                        if (ad != null && !ad.Constant)
                        {
                            AttributeReference ar =
                              new AttributeReference();

                            ar.SetAttributeFromBlock(ad, br.BlockTransform);
                            ar.Position =
                              ad.Position.TransformBy(br.BlockTransform);

                            if (ad.Justify != AttachmentPoint.BaseLeft)
                            {
                                ar.AlignmentPoint =
                                  ad.AlignmentPoint.TransformBy(br.BlockTransform);
                            }
                            if (ar.IsMTextAttribute)
                            {
                                ar.UpdateMTextAttribute();
                            }

                            if (HAttributes != null && HAttributes.Contains(ad.Tag))
                            {
                                ar.TextString = HAttributes[ad.Tag].ToString();
                                
                            }
                            else
                            {
                                ar.TextString = ad.TextString;
                            }

                            ObjectId arId =
                              br.AttributeCollection.AppendAttribute(ar);
                            tr.AddNewlyCreatedDBObject(ar, true);

                            // Initialize our dictionary with the ObjectId of
                            // the attribute reference + attribute definition info

                            attInfo.Add(
                              arId,
                              new AttInfo(
                                ad.Position,
                                ad.AlignmentPoint,
                                ad.Justify != AttachmentPoint.BaseLeft
                              )
                            );
                        }
                    }
                }
                // Run the jig

                BlockJig myJig = new BlockJig(tr, br, attInfo);

                if (myJig.Run() != PromptStatus.OK)
                    return null;

                // Commit changes if user accepted, otherwise discard
                tr.Commit();

            }
            return br;
        }

        private void OnDocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            e.Document.Editor.PointMonitor += new PointMonitorEventHandler(OnMonitorPoint);
            e.Document.ImpliedSelectionChanged += new EventHandler(OnDoc_ImpliedSelectionChanged);            
        }
        

        private void OnMonitorPoint(object sender, PointMonitorEventArgs e)
        {
            try
            {
                FullSubentityPath[] paths = e.Context.GetPickedEntities();
                if (paths.Length == 1)
                {
                    string tooltext = "";
                    Editor ed = (Editor)sender;
                    Document doc = ed.Document;
                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        ObjectId[] ids = paths[0].GetObjectIds();
                        ObjectId id = ids[0];
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        if (obj != null)
                        {
                            //if BlockReference
                            BlockReference bref = obj as BlockReference;
                            if (bref != null)
                            {
                                AttributeCollection attcol = bref.AttributeCollection;
                                foreach (ObjectId att in attcol)
                                {
                                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                                    if (atRef.Tag == "XML_CONSTRUCT")
                                    {
                                        CConstruct constr = new CConstruct();
                                        constr.ReadFromString(atRef.TextString);
                                        foreach (DataRow row in constr._fields.Rows)
                                        {
                                            tooltext += row[0].ToString() + "->" + row[1].ToString() + "\n";
                                        }
                                        foreach (DataRow row in constr._details.Rows)
                                        {
                                            tooltext += row[0].ToString() + "->" + row[1].ToString() + " шт.\n";
                                        }
                                        foreach (DataRow row in constr._constructs.Rows)
                                        {
                                            tooltext += row[0].ToString() + "->" + row[1].ToString() + " шт.\n";
                                        }
                                    }
                                }
                            }

                            //if Polyline
                            {
                                Polyline pl = obj as Polyline;
                                if (pl != null)
                                {
                                    
                                    //double len = pl.GetDistAtPoint(e.Context.ObjectSnappedPoint);
                                    //tooltext += len.ToString("0.00");                                
                                }
                            }
                        }
                        tr.Commit();
                    }
                    if (tooltext != "") e.AppendToolTipText(tooltext);
                }
            }
            catch (CConstructReadException)
            {
                //do nothing
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        [CommandMethod("gredit", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void cmdGroupEdit()
        {
            try
            {
                #region SELECTING
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptSelectionResult sset;
                sset = ed.SelectImplied();

                if (sset.Status == PromptStatus.Error)
                {
                    PromptSelectionOptions selOpt = new PromptSelectionOptions();
                    selOpt.MessageForAdding = "Выберите опоры: ";
                    TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
                    SelectionFilter sfilter = new SelectionFilter(values);
                    selOpt.AllowDuplicates = false;
                    sset = ed.GetSelection(selOpt, sfilter);
                    if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
                }

                

                ObjectId[] objIds = sset.Value.GetObjectIds();
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                #endregion SELECTING

                #region FILL_TABLE

                List<CConstruct> cnstrList = new List<CConstruct>();
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in objIds)
                    {
                        BlockReference bRef = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                        if (bRef != null)
                        {
                            bRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            CConstruct constr = GetConstructFromRef(bRef);
                            if (constr != null)
                            {
                                constr.bRef = bRef;
                                cnstrList.Add(constr);
                            }
                        }
                    }
                }
                //Sort by index
                try
                {
                    CConstructComparer myComparer = new CConstructComparer();
                    cnstrList.Sort(myComparer);
                }
                catch (System.Exception ex)
                {
                }

                System.Data.DataTable tblFields = new System.Data.DataTable();
                tblFields.Columns.Add("Поле");    //first column
                List<string> fields = new List<string>();

                System.Data.DataTable tblDetails = new System.Data.DataTable();
                tblDetails.Columns.Add("Наименование детали");    //first column
                List<string> details = new List<string>();

                List<string> cnstrNames = new List<string>();   //список имен конструкций
                List<string> cnstrXmls = new List<string>();    //xml список конструкций в конструкциях

                System.Data.DataTable tblConstr = new System.Data.DataTable();
                tblConstr.Columns.Add("Наименование");    //first column                
 
                int icol = 0;   //column index

                foreach (CConstruct constr in cnstrList)
                {
                    icol += 1;
                    //fields
                    tblFields.Columns.Add(icol.ToString());
                    foreach (DataRow row in constr._fields.Rows)
                    {
                        string name = row[0].ToString();
                        string val = row[1].ToString();
                        if (fields.Contains(name))
                        {
                            int irow = fields.IndexOf(name);
                            tblFields.Rows[irow][icol] = val;
                        }
                        else
                        {
                            fields.Add(name);
                            DataRow newRow = tblFields.NewRow();
                            newRow[0] = name;
                            newRow[icol] = val;
                            tblFields.Rows.Add(newRow);
                        }

                    }

                    //details
                    tblDetails.Columns.Add(icol.ToString());
                    foreach (DataRow row in constr._details.Rows)
                    {
                        string name = row[0].ToString();
                        string val = row[1].ToString();
                        if (details.Contains(name))
                        {
                            int irow = details.IndexOf(name);
                            tblDetails.Rows[irow][icol] = val;
                        }
                        else
                        {
                            details.Add(name);
                            DataRow newRow = tblDetails.NewRow();
                            newRow[0] = name;
                            newRow[icol] = val;
                            tblDetails.Rows.Add(newRow);
                        }
                    }
                    //constructs
                    tblConstr.Columns.Add(icol.ToString());
                    foreach (DataRow row in constr._constructs.Rows)
                    {
                        string name = row[0].ToString();
                        string qty = row[1].ToString();
                        string xml = row[2].ToString();

                        bool fnd = false;
                        int i;
                        for (i = 0; i < cnstrNames.Count; i++)
                        {
                            if (cnstrNames[i] == name && cnstrXmls[i] == xml) { fnd = true; break; }                              
                        }
                        if (fnd) { tblConstr.Rows[i][icol] = qty; }
                        else
                        {
                            cnstrNames.Add(name);
                            cnstrXmls.Add(xml);

                            DataRow newRow = tblConstr.NewRow();
                            newRow[0] = name;
                            newRow[icol] = qty;
                            tblConstr.Rows.Add(newRow);
                        }
                        
                    }
                }
                #endregion FILL_TABLE
                GroupConstrEditForm frm = new GroupConstrEditForm(tblFields, tblDetails, tblConstr, cnstrList, cnstrXmls);
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        [CommandMethod("calc04")]
        public void cmdCalc_04()
        {
            #region SELECTING
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions selOpt = new PromptSelectionOptions();
            selOpt.MessageForAdding = "Выберите объекты: ";
            //TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
            //SelectionFilter sfilter = new SelectionFilter(values);
            selOpt.AllowDuplicates = false;
            PromptSelectionResult sset = ed.GetSelection(selOpt);
            if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }

            PromptEntityResult eres = ed.GetEntity("Выберите вершину дерева: ");
            if (!(eres.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
            #endregion SELECTING
            #region INPUT GRAPH
            //Graph<CConstruct, CProvod> graph = new Graph<CConstruct, CProvod>();
            //CConstruct rootConstr = null;
            Graph<MultiJoint04, CProlet> graph = new Graph<MultiJoint04, CProlet>();
            MultiJoint04 nodeJoint = new MultiJoint04();

            Database dbCurrent = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
            {
                BlockReference bRefHEAD = (BlockReference)tr.GetObject(eres.ObjectId, OpenMode.ForRead);
                if (bRefHEAD == null) { ed.WriteMessage("Head is'nt BlockReference"); return; }
                ObjectId[] objIds = sset.Value.GetObjectIds();
                var list = new List<ObjectId>(objIds);
                if (!list.Contains(bRefHEAD.Id)) { ed.WriteMessage("Head is'nt in Selection Set"); return; }
                list.Remove(bRefHEAD.Id);
                
                //read from BlockReference
                Joint04 jnt = new Joint04();
                AttributeCollection attcol = bRefHEAD.AttributeCollection;
                foreach (ObjectId att in attcol)
                {
                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                    //Считываем данные об источнике нагрузки
                    switch (atRef.Tag)
                    {
                        case "НАГРУЗКА":
                            jnt.Load = double.Parse(atRef.TextString);
                            break;
                        case "НАПРЯЖЕНИЕ":
                            jnt.U = double.Parse(atRef.TextString);
                            break;
                        case "COS":
                            jnt.Cos = double.Parse(atRef.TextString);
                            break;
                        case "КОЛИЧЕСТВО":
                            jnt.QtyLoad = int.Parse(atRef.TextString);
                            break;
                        default:
                            break;
                    }
                    
                    //if (atRef.Tag == "XML_CONSTRUCT")
                    //{
                    //    rootConstr = new CConstruct();
                    //    rootConstr.ReadFromString(atRef.TextString);
                    //    graph.Add(rootConstr);  //add construct
                    //}
                }
                nodeJoint.joints.Add(jnt);
                graph.Add(nodeJoint);
                GraphInRecElectrical_04(graph, list, bRefHEAD.Position, nodeJoint);
                //GraphInRecMechanical(graph, list, bRefHEAD.Position, rootConstr);
            }
            #endregion
            System.Data.DataTable calcTable = new System.Data.DataTable();
            CalcElectrical_04(graph, nodeJoint, calcTable, null);
            //string[] colnames = new string[] { "Полный путь", "Номера опор", "Потери участка dU(i), В", "Потери участка dU(i), %",
            //"Сумма потерь dU, В", "Сумма потерь dU, %"};
            //for (int i = 0; i < colnames.Length; i++) { calcTable.Columns.Add(colnames[i]); }            
            //RecursiveCalc(graph, rootConstr, calcTable, null);
        }

        [CommandMethod("calclow")]
        public void cmdCalc_LowVotage()
        {
            LowVltgCalcFrm frm = new LowVltgCalcFrm(db_path, databaseFolderName);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(frm);
        }

        [CommandMethod("calcmech")]
        public void cmdCalc_Mechanical()
        {
            try
            {
                #region SELECTING
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptSelectionOptions selOpt = new PromptSelectionOptions();
                selOpt.MessageForAdding = "Выберите объекты: ";
                //TypedValue[] values = { new TypedValue((int)DxfCode.Start, "INSERT"), /*new TypedValue((int)DxfCode.BlockName, "DETAIL_W")*/ };
                //SelectionFilter sfilter = new SelectionFilter(values);
                selOpt.AllowDuplicates = false;
                PromptSelectionResult sset = ed.GetSelection(selOpt);
                if (!(sset.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }

                PromptEntityResult eres = ed.GetEntity("Выберите вершину дерева: ");
                if (!(eres.Status == PromptStatus.OK)) { ed.WriteMessage("Programm was cancelled"); return; }
                #endregion SELECTING

                #region INPUT GRAPH AND CALC
                Graph<CConstruct, CProlet> graph = new Graph<CConstruct, CProlet>();
                CConstruct rootConstr = null;
                VlineLib.CVLine line = new VlineLib.CVLine();
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    BlockReference bRefHEAD = (BlockReference)tr.GetObject(eres.ObjectId, OpenMode.ForRead);
                    if (bRefHEAD == null) { ed.WriteMessage("Head is'nt BlockReference"); return; }
                    ObjectId[] objIds = sset.Value.GetObjectIds();
                    var list = new List<ObjectId>(objIds);
                    if (!list.Contains(bRefHEAD.Id)) { ed.WriteMessage("Head is'nt in Selection Set"); return; }
                    list.Remove(bRefHEAD.Id);

                    rootConstr = GetConstructFromRef(bRefHEAD);
                    if (rootConstr == null) { ed.WriteMessage("Head is'nt in Construct"); return; }
                    graph.Add(rootConstr);  //add construct    
                    //1. Ввод климатических условий
                    InputKlimatDlg dlg = new InputKlimatDlg(db_path);
                    dlg.Line = line;
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(dlg);
                    //2. Чтение графа
                    GraphInRecMechanical(graph, list, bRefHEAD.Position, rootConstr);
                    //3. перевод графа в анкерные пролеты
                    CalcMechanical(graph, rootConstr, line, new List<VlineLib.COpora>(), null);
                    //4. Ввод типов местности

                    line.Calc();
                    foreach (string msg in line.Reports)
                    {
                        ed.WriteMessage(msg);
                    }
                }
                #endregion

                #region FILL TABLE
                System.Data.DataTable tblOutProves = new System.Data.DataTable();
                System.Data.DataTable tblOutTiagen = new System.Data.DataTable();
                for (int i = 0; i < 18; i++)
                {
                    tblOutProves.Columns.Add();
                    tblOutTiagen.Columns.Add();
                }
                foreach (VlineLib.CAnkerStage stg in line._ankerstgs)
                {
                    DataRow row = tblOutProves.Rows.Add();                                        
                    VlineLib.COpora firstOp = stg.Opors[0];
                    int ubound = stg.Opors.Count - 1;
                    VlineLib.COpora lastOP = stg.Opors[ubound];
                    row[0] = firstOp.Number + "-" + lastOP.Number;
                    double len = lastOP.Piket - firstOp.Piket;
                    row[1] = stg.Provod.Name;
                    row[2] = len.ToString("0.00"); 
                    row[3] = stg.Lpr.ToString("0.00");
                    row[4] = stg.provSigmaMax.Val;
                    row[5] = stg.provSigmaE.Val;
                    row[6] = stg.GetProlet(1).ToString("0.00");
                    row[7] = stg.Opors[0].Number + "-" + stg.Opors[1].Number;
                    System.Data.DataTable TblProv = stg.GetProvisTable(1);                    
                    for (int i = 1; i < 11; i++)
                    {
                        row[7 + i] = TblProv.Rows[1][i];
                    }

                    DataRow rowT = tblOutTiagen.Rows.Add();
                    rowT[0] = firstOp.Number + "-" + lastOP.Number;
                    rowT[1] = stg.Provod.Name;
                    rowT[2] = len.ToString("0.00");
                    rowT[3] = stg.Lpr.ToString("0.00");

                    for (int i = 1; i < 11; i++)
                    {
                        rowT[7 + i] = TblProv.Rows[3][i];
                    }

                    for (int i = 1; i < stg.Opors.Count-1; i++)
                    {
                        row = tblOutProves.Rows.Add();
                        row[6] = stg.GetProlet(i + 1).ToString("0.00");
                        row[7] = stg.Opors[i].Number + "-" + stg.Opors[i+1].Number;
                        TblProv = stg.GetProvisTable(i+1);
                        for (int j = 1; j < 11; j++)
                        {
                            row[7 + j] = TblProv.Rows[1][j];
                        }
                    }
                }                
                #endregion

                #region FILL LAYOUT
                Transaction trans = dbCurrent.TransactionManager.StartTransaction();
                {
                    try
                    {
                        Layout lt = ImportLayoutWithOrWithoutReplace(_TemplatePath + "1.dwg", "Тяжения");
                        string outName = lt.LayoutName;

                        ObjectId tblID = GetFirstTable(lt);
                        Table tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;

                        //ЗАполняем температуры
                        System.Data.DataTable TblProv = line._ankerstgs[0].GetProvisTable(1);
                        for (int i = 1; i < 11; i++)
                        {
                            string res = "";
                            double v = Convert.ToDouble( TblProv.Rows[0][i] );
                            if (v > 0) res = "+" + v.ToString();
                            if (v <= 0) res = v.ToString();
                            tbl.Cells[1, i+7].TextString = res;
                        }

                        //Заполняем стрелы провеса
                        int nrows = tbl.Rows.Count;
                        int tblrow = 3;

                        for (int i = 0; i < tblOutProves.Rows.Count; i++)
                        {
                            //if (tblrow == nrows)
                            //{
                            //    //new layout 
                            //    //tbl.GenerateLayout();
                            //    outName = ImportLayout(_TemplatePath + "1.dwg", A3NextLayName);
                            //    lt = GetLayout(outName);
                            //    tblID = GetFirstTable(lt);
                            //    tbl = lt.Database.TransactionManager.GetObject(tblID, OpenMode.ForWrite, false) as Table;
                            //    nrows = tbl.Rows.Count;
                            //    tblrow = 1;
                            //}
                            for (int j = 0; j < 18; j++)
                            {
                                tbl.Cells[tblrow, j].TextString = tblOutProves.Rows[i][j].ToString();
                            }
                            tblrow++;
                        }

                        tblrow = 24;
                        for (int i = 0; i < tblOutTiagen.Rows.Count; i++)
                        {
                            for (int j = 0; j < 18; j++)
                            {
                                tbl.Cells[tblrow, j].TextString = tblOutTiagen.Rows[i][j].ToString();
                            }
                            tblrow++;
                        }

                    }
                    finally
                    {
                        trans.Commit();
                    }
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //recursive input graph function for mechanical calc
        private void GraphInRecMechanical(Graph<CConstruct, CProlet> graph, List<ObjectId> ids, Point3d startPnt, CConstruct start)
        {
            try
            {
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {                    
                    foreach (ObjectId id in ids)
                    {
                        Polyline pline = tr.GetObject(id, OpenMode.ForRead) as Polyline;
                        if (pline != null)
                        {
                            Point3d nextPnt = new Point3d();
                            if (pline.StartPoint == startPnt)
                            {
                                nextPnt = pline.EndPoint;
                            }
                            if (pline.EndPoint == startPnt)
                            {
                                nextPnt = pline.StartPoint;
                            }
                            //edge
                            CProlet prov = new CProlet();
                            prov.Lenght = pline.Length;
                            prov.ProvodType = pline.Layer;

                            //node
                            foreach (ObjectId refid in ids)
                            {
                                BlockReference childRef = tr.GetObject(refid, OpenMode.ForRead) as BlockReference;
                                if (childRef != null)
                                {
                                    if (childRef.Position == nextPnt)
                                    {                                        
                                        //read cconstruct from BlockReference                                        
                                        CConstruct childConstr = GetConstructFromRef(childRef);
                                        if (childConstr == null) throw new System.Exception("Не удалось прочитать объект CConstruct");
                                        if (childConstr.IsOpora())
                                        {
                                            graph.Add(childConstr);
                                            graph.AddEdge(start, childConstr, prov);

                                            List<ObjectId> newlist = new List<ObjectId>(ids);
                                            newlist.Remove(id);
                                            newlist.Remove(refid);
                                            GraphInRecMechanical(graph, newlist, nextPnt, childConstr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //recursive input graph function for  calc 0,4 кВ
        private void GraphInRecElectrical_04(Graph<MultiJoint04, CProlet> graph, List<ObjectId> ids, Point3d startPnt, MultiJoint04 start)
        {
            try
            {
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in ids)
                    {
                        Polyline pline = tr.GetObject(id, OpenMode.ForRead) as Polyline;
                        if (pline != null)
                        {
                            Point3d nextPnt = new Point3d();
                            bool fnd = false;
                            if (pline.StartPoint == startPnt)
                            {
                                nextPnt = pline.EndPoint;
                                fnd = true;
                            }
                            if (pline.EndPoint == startPnt)
                            {
                                nextPnt = pline.StartPoint;
                                fnd = true;
                            }
                            if (fnd)
                            {
                                //list witout previous items
                                List<ObjectId> newlist = new List<ObjectId>(ids);
                                newlist.Remove(id);
                                //edge
                                CProlet prov = new CProlet();
                                prov.Lenght = pline.Length;
                                prov.ProvodType = pline.Layer;

                                //node 
                                MultiJoint04 nodeJoint = new MultiJoint04();


                                foreach (ObjectId refid in ids)
                                {
                                    BlockReference childRef = tr.GetObject(refid, OpenMode.ForRead) as BlockReference;
                                    if (childRef != null)
                                    {
                                        if (childRef.Position == nextPnt)
                                        {
                                            CConstruct constr = GetConstructFromRef(childRef);
                                            if (constr != null)
                                            {
                                                nodeJoint.construct = constr;
                                            }
                                            else
                                            {
                                                Joint04 jnt = new Joint04();                                                
                                                AttributeCollection attcol = childRef.AttributeCollection;
                                                foreach (ObjectId att in attcol)
                                                {
                                                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                                                    switch (atRef.Tag)
                                                    {
                                                        case "НАГРУЗКА":
                                                            jnt.Load = double.Parse(atRef.TextString);
                                                            break;
                                                        case "НАПРЯЖЕНИЕ":
                                                            jnt.U = double.Parse(atRef.TextString);
                                                            break;
                                                        case "COS":
                                                            jnt.Cos = double.Parse(atRef.TextString);
                                                            break;
                                                        case "КОЛИЧЕСТВО":
                                                            jnt.QtyLoad = int.Parse(atRef.TextString);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                nodeJoint.joints.Add(jnt);
                                            }
                                            newlist.Remove(refid);
                                        }
                                    }
                                }
                                graph.Add(nodeJoint);
                                graph.AddEdge(start, nodeJoint, prov);
                                GraphInRecElectrical_04(graph, newlist, nextPnt, nodeJoint);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void CalcElectrical_04(Graph<MultiJoint04, CProlet> graph, MultiJoint04 start, System.Data.DataTable table, DataRow prevRow)
        {
            //string fullPath, prolet;            
            //double dUi, dUiPercent, dU, dUPercent;

            //if (prevRow == null)    //first calc
            //{
            //    dUi = 0;
            //    dUiPercent = 0;
            //    dU = 0;
            //    dUPercent = 0;
            //}
            //else
            //{

            //}
            foreach (Edge<MultiJoint04, CProlet> edg in graph.GetEdgesFrom(start))
            {
                CalcElectrical_04(graph, edg.End, table, null);
                //string newPath = path;                
                //newPath += "-" + edg.End.ToString();
                //MessageBox.Show(newPath);
                //RecursiveCalc(graph, edg.End, newPath);
            }
        }

        private void CalcMechanical(Graph<CConstruct, CProlet> graph, CConstruct start, VlineLib.CVLine line, List<VlineLib.COpora> opors,
            CProlet prevProlet)
        {             
            if (opors == null) throw new ArgumentNullException("opors");
            VlineLib.COpora op = start.ToOpora();
            if (opors.Count > 0)
            {
                VlineLib.COpora prevOp = opors.Last();
                op.Piket = prevOp.Piket + prevProlet.Lenght;

                if (op.Anker)
                {
                    //закрыли анкерный пролет
                    opors.Add(op);
                    line.AddAnkerStage(opors);
                    int last = line._ankerstgs.Count - 1;

                    DataRow row = GetDetail(prevProlet.ProvodType);
                    if (row == null) throw new ArgumentNullException("Такого провода нет в базе данных - " + prevProlet.ProvodType);
                    //Init provod from DB
                    VlineLib.CProvod prov = new VlineLib.CProvod();
                    prov.Name = prevProlet.ProvodType;
                    prov.Diam = MyConvert.ToDouble(row["D"].ToString());
                    prov.Area = MyConvert.ToDouble(row["S"].ToString());
                    prov.Emodule = MyConvert.ToDouble(row["E"].ToString());
                    prov.Alpha = MyConvert.ToDouble(row["ALPHA"].ToString());
                    prov.Weight = MyConvert.ToDouble(row["МАССА"].ToString());
                    prov.SigmaR = MyConvert.ToDouble(row["SIGMA_R"].ToString());
                    prov.SigmaSred = MyConvert.ToDouble(row["SIGMA_SRED"].ToString());
                    prov.SigmaMax = MyConvert.ToDouble(row["SIGMA_MAX"].ToString());                    
                    line._ankerstgs[last].Provod = prov;
                    //ПЕРЕДЕЛАТЬ ОБЯЗАТЕЛЬНО!
                    line._ankerstgs[last].MestnType = 1;      
                }
            }
            

            foreach (Edge<CConstruct, CProlet> edg in graph.GetEdgesFrom(start))
            {
                if (op.Anker)
                { 
                    //открыли анкерный пролет
                    opors = new List<VlineLib.COpora>();
                    opors.Add(op);
                }
                else//промежутка
                {                    
                    opors.Add(op);
                }  
                //edg.Data.Type;
                CalcMechanical(graph, edg.End, line, opors, edg.Data);                
            }
        }

        /// <summary>
        /// Получить объект конструкции из экземпляра блока
        /// </summary>
        /// <param name="bRef">экземпляр блока</param>
        /// <returns>объект конструкции</returns>
        private CConstruct GetConstructFromRef(BlockReference bRef)
        {
            try
            {
                CConstruct resConstr = null;
                Database dbCurrent = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {                    
                    //read cconstruct from BlockReference
                    AttributeCollection attcol = bRef.AttributeCollection;
                    foreach (ObjectId att in attcol)
                    {
                        AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForRead);
                        if (atRef.Tag == "XML_CONSTRUCT")
                        {
                            resConstr = new CConstruct();
                            resConstr.ReadFromString(atRef.TextString);
                        }
                    }
                }
                return resConstr;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
     
}
