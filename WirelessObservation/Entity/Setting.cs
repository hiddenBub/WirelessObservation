using System;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    [XmlRoot(ElementName = "Setting")]
    [Serializable]
    public class Setting
    {
        //[XmlIgnore]
        //public object Parent;

        //[XmlElement(ElementName = "Collect")]
        //public Collect Collect { get; set; }

        [XmlElement(ElementName = "Port")]
        public Port Port { get; set; }

        //[XmlElement(ElementName = "Data")]
        //public Data Data { get; set; }
    }

    public class Port
    {
        //[XmlIgnore]
        // public Setting Parent = new Setting();
        [XmlElement(ElementName = "PortName")]
        public string PortName { get; set; }

        [XmlElement(ElementName = "Baud")]
        public int Baud { get; set; }

        [XmlElement(ElementName = "Data")]
        public int Data { get; set; }

        [XmlElement(ElementName = "Stop")]
        public int Stop { get; set; }

        [XmlElement(ElementName = "Parity")]
        public int Parity { get; set; }

        [XmlArray(ElementName = "PjList")]
        public ObservableCollection<Pj> PjList { get; set; }
    }

    public class Pj
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        [XmlAttribute(AttributeName = "Pid")]
        public int Pid { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 经度值
        /// </summary>
        [XmlAttribute(AttributeName = "Lng")]
        public double Lng { get; set; }

        /// <summary>
        /// 纬度值
        /// </summary>
        [XmlAttribute(AttributeName = "Lat")]
        public double Lat { get; set; }
    }

    public class Gather
    {
        // [XmlIgnore]
        // public Setting Parent = new Setting();

        /// <summary>
        /// 数据存储路径
        /// </summary>
        [XmlElement(ElementName = "DataPath")]
        public string DataPath { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        [XmlArray(ElementName = "ColumnList")]
        public ObservableCollection<Column> ColumnList { get; set; }
    }

    public class Column : INotifyPropertyChanged
    {
        //[XmlIgnore]
        //public Gather Parent = new Gather();
        /// <summary>
        /// 索引
        /// </summary>
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
        /// <summary>
        /// 行名称
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        /// <summary>
        /// 灵敏度
        /// </summary>
        [XmlAttribute(AttributeName = "Sensitivity")]
        public double Sensitivity { get; set; }

        /// <summary>
        /// 采集频率
        /// </summary>
        [XmlAttribute(AttributeName = "Frequency")]
        public int Frequency { get; set; }
        /// <summary>
        /// 映射至源数据的ID
        /// </summary>
        [XmlAttribute(AttributeName = "Shadow")]
        public string Shadow { get; set; }

        /// <summary>
        /// 结果集方法
        /// </summary>
        [XmlAttribute(AttributeName = "Method")]
        public string Method { get; set; }



        #region 属性更改通知
        public event PropertyChangedEventHandler PropertyChanged;
        private void Changed(string PropertyName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        #endregion

        #region 构造函数重载

        public Column() { }
        public Column(int index, string name)
        {
            Index = index;
            Name = name;
        }
        #endregion
    }
}
