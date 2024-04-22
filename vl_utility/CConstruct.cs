using System.Data;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;

namespace vl_utility
{
    public class CConstruct : IEquatable <CConstruct>
    {
        public BlockReference bRef;
        public DataSet ds = new DataSet("DataSet");
        public System.Data.DataTable _fields = new System.Data.DataTable("Fields");
        public System.Data.DataTable _details = new System.Data.DataTable("Details");
        public System.Data.DataTable _constructs = new System.Data.DataTable("Constructs");
        public List<CConstruct> _listConstructs = new List<CConstruct>();   //оригиналы конструкций

        public CConstruct()
        {
            ds.Tables.Add(_fields);
            ds.Tables.Add(_details);
            ds.Tables.Add(_constructs);

            _fields.Columns.Add("Поле");
            _fields.Columns.Add("Значение");

            _fields.Columns["Поле"].ColumnMapping = MappingType.Attribute;
            _fields.Columns["Значение"].ColumnMapping = MappingType.Attribute;

            _details.Columns.Add("Наименование_детали");
            _details.Columns.Add("Количество");

            _details.Columns["Наименование_детали"].ColumnMapping = MappingType.Attribute;
            _details.Columns["Количество"].ColumnMapping = MappingType.Attribute;

            _constructs.Columns.Add("Наименование");
            _constructs.Columns.Add("Количество");
            _constructs.Columns.Add("XmlString");

            _constructs.Columns["Наименование"].ColumnMapping = MappingType.Attribute;
            _constructs.Columns["Количество"].ColumnMapping = MappingType.Attribute;
            _constructs.Columns["XmlString"].ColumnMapping = MappingType.Attribute;            
        }

        public bool Equals(CConstruct other)
        {
            if (other == null)
                return false;
            return ((_details.Equals(other._details)) && (_fields.Equals(other._fields)));
        }

        public override string ToString()
        {
            return this.GetField("НАИМЕНОВАНИЕ")/*+ " "
                + this.GetField("ИНДЕКС")*/;
        }

        public string GetField(string name)
        {
            string search = "Поле='" + name + "'";
            DataRow[] rows = this._fields.Select(search);
            if (rows.Length > 0)
            {
                return rows[0]["Значение"].ToString();
            }
            else return "";
        }

        public bool SetField(string field, string value)
        {
            string search = "Поле='" + field + "'";
            DataRow[] rows = this._fields.Select(search);
            if (rows.Length == 1)
            {
                rows[0]["Значение"] = value;
                return true;
            }
            else return false;
        }

        public double GetDetailCount(string FullName)
        {
            double sum = 0;
            string search = "Наименование_детали='" + FullName + "'";
            DataRow[] rows = this._details.Select(search);
            if (rows.Length > 0)
            {
                foreach (DataRow row in rows)
                {
                    string plus = row["Количество"].ToString();
                    sum +=double.Parse(plus);
                }
            }
            return sum;
        }

        public void SaveToBlockRef()
        {
            if (this.bRef == null) return;
            ObjectId blockRefId = bRef.ObjectId;
            try
            {
                if (blockRefId != ObjectId.Null)
                {
                    //save to construct
                    Document activeDoc = Autodesk.AutoCAD.ApplicationServices.Application.
                       DocumentManager.MdiActiveDocument;
                    Database db = activeDoc.Database;
                    using (DocumentLock docLock = activeDoc.LockDocument())
                    {
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            BlockReference bRefwrite = (BlockReference)tr.GetObject(blockRefId, OpenMode.ForWrite);
                            if (bRefwrite != null)
                            {
                                Autodesk.AutoCAD.DatabaseServices.AttributeCollection attcol = bRefwrite.AttributeCollection;
                                foreach (ObjectId att in attcol)
                                {
                                    AttributeReference atRef = (AttributeReference)tr.GetObject(att, OpenMode.ForWrite);
                                    if (atRef.Tag == "XML_CONSTRUCT")
                                    {
                                        atRef.TextString = this.SaveToString();
                                    }
                                }
                            }
                            tr.Commit();
                        }
                    }
                }
            }

            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        public string SaveToString()
        {
            StringWriter writer = new StringWriter();
            ds.WriteXml(writer);
            string res = writer.ToString();
            writer.Close();
            return res;
        }

        public void ReadFromString(string xmlString)
        {
            try
            {
                //xmlString = xmlString.Replace("\n", "");
                StringReader reader = new StringReader(xmlString);
                ds.ReadXml(reader, XmlReadMode.IgnoreSchema);
            }
            catch (System.Exception ex)
            {                
                throw new CConstructReadException();
            }
        }

        public void SaveToFile(string file)
        {
            //if (File.Exists(file)) File.WriteAllText(file, "", System.Text.Encoding.UTF8);
            ds.WriteXml(file, XmlWriteMode.IgnoreSchema);
            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CConstruct>));            
            //TextWriter wrtr = new StreamWriter(file);
            //xmlSerializer.Serialize(wrtr, _listConstructs);
            //wrtr.Close();
        }
        public void OpenFile(string file)
        {          
            ds.ReadXml(file, XmlReadMode.IgnoreSchema);
        }

        public bool HasConstructs()
        {
            return this._constructs.Rows.Count > 0;
        }

        public bool IsOpora()
        {
            return (this.GetField("ТИП").Contains("опора"));         
        }

        public bool IsAnker()
        {
            return (this.GetField("ТИП").Contains("анкер"));
        }

        /// <summary>
        /// Конвертирует объект конструкции в объект опоры
        /// </summary>
        /// <returns></returns>
        public VlineLib.COpora ToOpora()
        {
            VlineLib.COpora res = new VlineLib.COpora();
            res.Anker = this.IsAnker();
            res.Number = this.GetField("НОМЕР");
            res.Name = this.GetField("НАИМЕНОВАНИЕ");
            res.H1 = MyConvert.ToDouble(this.GetField("H1"));
            res.H2 = MyConvert.ToDouble(this.GetField("H2"));
            res.H3 = MyConvert.ToDouble(this.GetField("H3"));
            res.Htros = MyConvert.ToDouble(this.GetField("Htros"));
            res.Otmetka = MyConvert.ToDouble(this.GetField("Otmetka"));
            res.Piket = MyConvert.ToDouble(this.GetField("Piket"));
            res.Description = this.GetField("Description");
            return res;
        }

    }

    public class CConstructComparer : IComparer<CConstruct>       
    {
        public int Compare(CConstruct x, CConstruct y)
        {
            //Сортируем конструкции по индексу            
            int xIndex = System.Convert.ToInt32(x.GetField("ИНДЕКС"));
            int yIndex = System.Convert.ToInt32(y.GetField("ИНДЕКС"));
            return xIndex.CompareTo(yIndex);
        }
    }
    public class CConstructReadException : System.Exception
    {
        public CConstructReadException()
        {
        }

        public CConstructReadException(string message)
            : base(message)
        {            
        }

        public CConstructReadException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

    }
}
