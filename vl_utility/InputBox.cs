using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace vl_utility
{
    partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
            this.Text = "InputBox";       
        }
        public string GetText()
        {
            return this.textBox1.Text;
        }
        public void SetLabel(string text)
        {
            this.label1.Text = text;
        }
        public void SetText(string text)
        {
            this.textBox1.Text = text;
        }
    }
}
