using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;

namespace vl_utility
{
    /// <summary>
    /// Класс исключений пробный
    /// </summary>
    public class MyException : Exception
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message"></param>
        public MyException(string message):base(message)
        {
            
        }
    }
    /// <summary>
    /// Класс для работы с базами данных в формате csv - текст с разделителями
    /// </summary>
    public class CSVReader
    {
        private string _filename;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="filename">путь к файлу</param>
        public CSVReader(string filename)
        {
            _filename = filename;
        }
        /// <summary>
        /// Поиск значения поля по базе данных
        /// </summary>
        /// <param name="baseFld">имя известного поля</param>
        /// <param name="value">известное значение</param>
        /// <param name="searchFld">искомое поле</param>
        /// <param name="separator">разделитель в файле</param>
        /// <returns></returns>
        public string QueryByValue(string baseFld, string value, string searchFld, char separator)
        {
            string res = "";
            using (StreamReader sr = new StreamReader(_filename, Encoding.UTF8))
            {

                string first_line;
                int baseInt = -1; int searchInt = -1;
                first_line = sr.ReadLine();
                string[] fields = first_line.Split(separator);  //Разделитель в CVS файле.
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == baseFld) baseInt = i;
                    if (fields[i] == searchFld) searchInt = i;
                }
                if (baseInt == -1) throw new IndexOutOfRangeException("Поле " + baseFld + " не найдено в файле " + _filename);
                if (searchInt == -1) throw new IndexOutOfRangeException("Поле " + searchFld + " не найдено в файле " + _filename);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fieldVals = line.Split(separator);  //Разделитель в CVS файле.
                    if (fieldVals[baseInt] == value)
                    {
                        res = fieldVals[searchInt];
                        sr.Close();
                        return res;
                    }
                }
                sr.Close();
                if (res == "") throw new IndexOutOfRangeException("Значение поля " + searchFld + " для " + baseFld + "=" + value + " не найдено в файле " + _filename);
            }
            return res;
        }
        /// <summary>
        /// Заполняет комбобокс значениями поля из БД
        /// </summary>
        /// <param name="baseFld">имя поля</param>
        /// <param name="combo">заполняемый комбобокс</param>
        /// <param name="separator">Разделитель в CVS файле</param>
        public void FillCombo(string baseFld, System.Windows.Forms.ComboBox combo, char separator)
        {
            using (StreamReader sr = new StreamReader(_filename, Encoding.UTF8))
            {
                combo.Items.Clear();

                string first_line;
                int baseInt = -1;
                first_line = sr.ReadLine();
                string[] fields = first_line.Split(separator);  //Разделитель в CVS файле.
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == baseFld) baseInt = i;                   
                }
                if (baseInt == -1) throw new IndexOutOfRangeException("Поле " + baseFld + " не найдено в файле " + _filename);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fieldVals = line.Split(separator);  //Разделитель в CVS файле.
                    combo.Items.Add(fieldVals[baseInt]);                  
                }
                sr.Close();

            }

        }

        /// <summary>
        /// Возвращет список значений поля
        /// </summary>
        /// <param name="baseFld">поле</param>
        /// <param name="list">заполняемый массив</param>
        /// <param name="separator">Разделитель в CVS файле</param>
        public void GetArray(string baseFld, List<string> list, char separator)
        {
            using (StreamReader sr = new StreamReader(_filename, Encoding.UTF8))
            {

                list.Clear();
                
                string first_line;
                int baseInt = -1;
                first_line = sr.ReadLine();
                string[] fields = first_line.Split(separator);  //Разделитель в CVS файле.
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == baseFld) baseInt = i;
                }
                if (baseInt == -1) throw new IndexOutOfRangeException("Поле " + baseFld + " не найдено в файле " + _filename);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fieldVals = line.Split(separator);  //Разделитель в CVS файле.
                    list.Add(fieldVals[baseInt]);                    
                }
                sr.Close();

            }
        }

        public DataTable GetDataTable(char separator)
        {            
            DataTable _table = new DataTable();
            using (StreamReader sr = new StreamReader(_filename, Encoding.UTF8))
            {
                //string[] lines= File.ReadAllLines(_filename, Encoding.UTF8);
                //string[] fields = lines[0].Split(separator);
                //for (int i = 0; i < fields.Length; i++)
                //{
                //    _table.Columns.Add(fields[i]);
                //}
                //for (int i = 1; i < lines.Length; i++)
                //{
                //    string[] fieldVals = lines[i].Split(separator);
                //    DataRow row = _table.NewRow();
                //    for (int j = 0; j < fields.Length; j++)
                //    {
                //        row[j] = fieldVals[j];                        
                //    }
                //    _table.Rows.Add(row);
                //}

                string first_line;
                first_line = sr.ReadLine();
                string[] fields = first_line.Split(separator);  //Разделитель в CVS файле.
                for (int i = 0; i < fields.Length; i++)
                {
                    _table.Columns.Add(fields[i]);
                }
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fieldVals = line.Split(separator);  //Разделитель в CVS файле.

                    DataRow row = _table.NewRow();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        row[i] = fieldVals[i];
                    }
                    _table.Rows.Add(row);
                }
                sr.Close(); 
            }
            return _table;
        }

        public void GetHashTable(string baseFld, string value, Hashtable table, char separator)
        {
            table.Clear();            
            using (StreamReader sr = new StreamReader(_filename, Encoding.UTF8))
            {
                string first_line;
                int baseInt = -1;
                first_line = sr.ReadLine();
                string[] fields = first_line.Split(separator);  //Разделитель в CVS файле.
                for (int i = 0; i < fields.Length; i++)
                {                    
                    if (fields[i] == baseFld) baseInt = i;                    
                }
                if (baseInt == -1) throw new IndexOutOfRangeException("Поле " + baseFld + " не найдено в файле " + _filename);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fieldVals = line.Split(separator);  //Разделитель в CVS файле.
                    if (fieldVals[baseInt] == value)
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            table.Add(fields[i], fieldVals[i]);
                        }
                        sr.Close();
                        return;
                    }
                }
                sr.Close();                
            }            
        }
    }
}
