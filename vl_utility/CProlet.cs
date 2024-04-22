using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vl_utility
{
    public class CProlet
    {
        private string _provodType;

        public string ProvodType
        {
            get { return _provodType; }
            set { _provodType = value; }
        }

        private double _lenght;

        public double Lenght
        {
            get { return _lenght; }
            set { _lenght = value; }
        }       
    }
}
