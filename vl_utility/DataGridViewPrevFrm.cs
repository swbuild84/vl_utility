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
    public partial class DataGridViewPrevFrm : Form
    {
        public DataGridViewPrevFrm()
        {
            InitializeComponent();
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            try
            {
                int irow = dataGridView1.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;

                DataTable table=dataGridView1.DataSource as DataTable;
                if(table!=null)
                {

                    DataRow selectedRow = table.Rows[irow];
                    DataRow prevRow = table.Rows[irow-1];
                    object selVal = selectedRow[0];
                    object prevVal = prevRow[0];
                    if ((selVal.ToString() != "") && (prevVal.ToString() != ""))//не выходим за границы раздела
                    {
                        object tmpVal = prevVal;
                        prevRow[0] = selVal;
                        selectedRow[0] = tmpVal;

                        DataRow newRow = table.NewRow();
                        newRow.ItemArray = selectedRow.ItemArray; // copy data
                        table.Rows.Remove(selectedRow);
                        table.Rows.InsertAt(newRow, irow-1);
                        dataGridView1.CurrentCell = dataGridView1.Rows[irow - 1].Cells[icol];
                        
                    }                    
                }
            }
            catch (Exception)
            {               
                
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            try
            {
                int irow = dataGridView1.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;

                DataTable table = dataGridView1.DataSource as DataTable;
                if (table != null)
                {

                    DataRow selectedRow = table.Rows[irow];
                    DataRow nextRow = table.Rows[irow + 1];
                    object selVal = selectedRow[0];
                    object nextVal = nextRow[0];
                    if ((selVal.ToString() != "") && (nextVal.ToString() != ""))//не выходим за границы раздела
                    {
                        object tmpVal = selVal;
                        selectedRow[0] = nextVal;
                        nextRow[0] = tmpVal;

                        DataRow newRow = table.NewRow();
                        newRow.ItemArray = selectedRow.ItemArray; // copy data
                        table.Rows.Remove(selectedRow);
                        table.Rows.InsertAt(newRow, irow + 1);
                        dataGridView1.CurrentCell = dataGridView1.Rows[irow + 1].Cells[icol];
                        //dataGridView1.Rows[irow-1].Cells[icol].Selected = true;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                int irow = dataGridView1.CurrentCell.RowIndex;
                int icol = dataGridView1.CurrentCell.ColumnIndex;

                DataTable table = dataGridView1.DataSource as DataTable;
                if (table != null)
                {

                    DataRow selectedRow = table.Rows[irow];
                    DataRow nextRow = table.Rows[irow + 1];
                    object selVal = selectedRow[0];
                    object nextVal = nextRow[0];
                    //if ((selVal.ToString() != "") && (nextVal.ToString() != ""))//не выходим за границы раздела
                    {
                        //object tmpVal = selVal;
                        //selectedRow[0] = nextVal;
                        //nextRow[0] = tmpVal;

                        DataRow newRow = table.NewRow();
                        //newRow.ItemArray = selectedRow.ItemArray; // copy data
                        //table.Rows.Remove(selectedRow);
                        table.Rows.InsertAt(newRow, irow + 1);
                        dataGridView1.CurrentCell = dataGridView1.Rows[irow + 1].Cells[icol];
                        //dataGridView1.Rows[irow-1].Cells[icol].Selected = true;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            int irow = dataGridView1.CurrentCell.RowIndex;
            int icol = dataGridView1.CurrentCell.ColumnIndex;

            DataTable table = dataGridView1.DataSource as DataTable;
            if (table != null)
            {
                DataRow selectedRow = table.Rows[irow];
                table.Rows.Remove(selectedRow);
            }
        }
    }
}
