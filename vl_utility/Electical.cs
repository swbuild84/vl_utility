using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vl_utility
{
    public class MultiJoint04 : IEquatable<MultiJoint04>
    {
        public List<Joint04> joints=new List<Joint04>();
        public CConstruct construct;
        public bool Equals(MultiJoint04 other)
        {
            if (other == null)
                return false;
            return ReferenceEquals(this, other);
        }
    }

    public class Joint04 : IEquatable<Joint04>
    {
        //double _resistance;

        double _load;
        /// <summary>
        /// Нагрузка в узле
        /// </summary>
        public double Load
        {
            get { return _load; }
            set { _load = value; }
        }
        double _cos;
        /// <summary>
        /// Коэф. сощности нагрузки
        /// </summary>
        public double Cos
        {
            get { return _cos; }
            set { _cos = value; }
        }
        
        int _qtyLoad;

        /// <summary>
        /// Кол-во нагрузок
        /// </summary>
        public int QtyLoad
        {
            get { return _qtyLoad; }
            set { _qtyLoad = value; }
        }
        double _U;
        /// <summary>
        /// Напряжение нагрузки - 0,22 или 0,38 кВ
        /// </summary>
        public double U
        {
            get { return _U; }
            set { _U = value; }
        }

        double _TransPower;
        /// <summary>
        /// Мощность трансформатора
        /// </summary>
        public double TransPower
        {
            get { return _TransPower; }
            set { _TransPower = value; }
        }

        int _transShem;
        /// <summary>
        /// Схема соединения обмоток
        /// </summary>
        public int TransShem
        {
            get { return _transShem; }
            set { _transShem = value; }
        }

        public bool Equals(Joint04 other)
        {
            if (other == null)
                return false;
            return ReferenceEquals(this, other);
        }
    }
}
