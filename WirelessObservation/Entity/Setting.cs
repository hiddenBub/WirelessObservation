using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace WirelessObservation.Entity
{
    [XmlRoot(ElementName = "Setting")]
    [Serializable]
    public class Setting
    {
        //[XmlIgnore]
        //public object Parent;
        [XmlElement(ElementName = "File")]
        private Files files = new Files { Alias = "文件", Name = "Files", Sort = 1};
        public Files Files { get => files; set => files = value; }

        [XmlElement(ElementName = "Collect")]
        private Collect collect = new Collect { Alias = "采集", Name = "Collect",Sort = 2 };
        public Collect Collect { get => collect; set => collect = value; }

        //[XmlElement(ElementName = "Port")]
        //public Port Port { get; set; }

        [XmlElement(ElementName = "Systemd")]
        private Systemd systemd = new Systemd {  Name = "Systemd", Alias = "系统", Sort = 999, IsShow = false};
        public Systemd Systemd { get => systemd; set => systemd = value; }
    }

    public class OptionItem : IComparable
    {
        [XmlAttribute(AttributeName = "Name")]      // Option name, generally is used to set name of components
        private string name = "";
        public string Name { get => name; set => name = value; }

        [XmlAttribute(AttributeName = "Sort")]      // Sort of components, use ascent
        private int sort = 1;
        public int Sort { get => sort; set => sort = value; }

        [XmlAttribute(AttributeName = "Alias")]     // Option alias, generally is used to set name of option display
        private string alias = "";
        public string Alias { get => alias; set => alias = value; }

        [XmlAttribute(AttributeName = "IsShow")]    // set this option show in the set window or not
        private bool isShow = true;
        public bool IsShow { get => isShow; set => isShow = value; }

        [XmlAttribute(AttributeName = "NodeType")] // text => textBox, radio => radioSelectOption, multiple => checkBox, select => selectBox
        private string nodeType = "node";
        public string NodeType { get => nodeType; set => nodeType = value; }

        [XmlAttribute(AttributeName = "Options")]   // if nodetype is not text or node, this parameter is used to saved  Alternative values
        private string[] options = new string[] { };
        public string[] Options { get => options; set => options = value; }

        [XmlAttribute(AttributeName = "Values")]    // save the select value
        private string values = "";
        public string Values { get => values; set => values = value; }

        [XmlAttribute(AttributeName = "Prefix")]    // prefix of value such as description
        private string prefix = "";
        public string Prefix { get => prefix; set => prefix = value; }

        [XmlAttribute(AttributeName = "Suffix")]    // suffix of value such as unit
        private string suffix = "";
        public string Suffix { get => suffix; set => suffix = value; }

        [XmlAttribute(AttributeName = "Height")]    // suffix of value such as unit
        private int height = 20;
        public int Height { get => height; set => height = value; }

        /// <summary>
        /// 实现接口中的方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            OptionItem option = obj as OptionItem;
            //因为int32实现了接口IComparable，那么int也有CompareTo方法，直接调用该方法就行
            return this.sort.CompareTo(option.sort);
        }
    }

    public class Files : OptionItem
    {
        
        [XmlElement(ElementName = "OptionItems")]
        private OptionItem[] optionItems = new OptionItem[] {
            new OptionItem
            {
                Name = "DataPath",
                Alias = "源数据存储位置",
                NodeType = "text",
                IsShow = true,
                Values = Environment.CurrentDirectory + "\\source",
            },
            new OptionItem
            {
                Name = "StorePath",
                Alias = "输出数据存储位置",
                Sort = 2,
                NodeType = "text",
                IsShow = true,
                Values = Environment.CurrentDirectory + "\\data"
            },
            new OptionItem
            {
                Name = "TempPath",
                Alias = "缓存数据存储位置",
                Sort = 3,
                NodeType = "text",
                IsShow = true,
                Values =  Environment.CurrentDirectory + "\\temp",
            },
            new OptionItem
            {
                Name = "DataType",
                Alias = "数据输出格式",
                Sort = 4,
                NodeType = "multiple",
                IsShow = true,
                Options = new string[] {"json","csv","dat","raw"},
                Values = "json,csv"
            }
        };
        public OptionItem[] OptionItems { get => optionItems; set => optionItems = value; }

        public string DataPath { get => optionItems[0].Values; set => optionItems[0].Values = value; }
        public string StorePath { get => optionItems[1].Values; set => optionItems[1].Values = value; }
        public string TempPath { get => optionItems[2].Values; set => optionItems[2].Values = value; }
        public string DataType { get => optionItems[3].Values; set => optionItems[3].Values = value; }
    }

    public class Collect : OptionItem
    {
        [XmlElement(ElementName = "OptionItems")]
        private OptionItem[] optionItems = new OptionItem[] {
            new OptionItem
            {
                Name = "Interval",
                Alias = "源数据更新时间",
                Sort = 1,
                NodeType = "text",
                IsShow = false,
                Values = "60",
                Suffix = "秒（不支持小数）"
            },
            new OptionItem
            {
                Name = "ComPort",
                Alias = "COM口名称",
                Sort = 2,
                NodeType = "text",
                IsShow = true,
                Values = "CH340",
                Suffix = "USB-SERIAL CH340（COM5）可以填写 5 或者 CH340 以及 USB-SERIAL CH340"
            },
            new OptionItem
            {
                Name = "Baud",
                Alias = "波特率",
                Sort = 3,
                NodeType = "select",
                IsShow = true,
                Options = new string[] {"4800","9600","19200","38400","57600","115200"},
                Values =  "9600",
            },
            new OptionItem
            {
                Name = "DataBit",
                Alias = "数据位",
                Sort = 4,
                NodeType = "select",
                IsShow = true,
                Options = new string[] {"7","8"},
                Values =  "8",
            } ,
            new OptionItem
            {
                Name = "StopBit",
                Alias = "停止位",
                Sort = 5,
                NodeType = "select",
                IsShow = true,
                Options = new string[] {"One","OnePointFive","Two"},
                Values =  "One",
            },
            new OptionItem
            {
                Name = "Parity",
                Alias = "校验位",
                Sort = 6,
                NodeType = "select",
                IsShow = true,
                Options = new string[] {"None","Even","Mark","Odd","Space"},
                Values =  "None",
            },
            new OptionItem
            {
                Name = "HandShake",
                Alias = "握手",
                Sort = 7,
                NodeType = "select",
                IsShow = true,
                Options = new string[] {"None","XOnXOff", "RequestToSend", "RequestToSendXOnXOff",},
                Values =  "None",
            },
            new OptionItem
            {
                Name = "InitHeight",
                Alias = "数据初始高度",
                Sort = 8,
                NodeType = "text",
                IsShow = true,
                Values = "10",
            },
        };
        public OptionItem[] OptionItems { get => optionItems; set => optionItems = value; }
        
        public int Interval { get => int.Parse(optionItems[0].Values); set => optionItems[0].Values = value.ToString(); }
        
        public string ComPort { get => optionItems[1].Values; set => optionItems[1].Values = value; }

        public int Baud { get => int.Parse(optionItems[2].Values); set => optionItems[2].Values = value.ToString(); }
       
        public string DataBit { get => optionItems[3].Values; set => optionItems[3].Values = value; }
        
        public string StopBit { get => optionItems[4].Values; set => optionItems[4].Values = value; }

        public string Parity { get => optionItems[5].Values; set => optionItems[5].Values = value; }

        public string HandShake { get => optionItems[6].Values; set => optionItems[6].Values = value; }

        public double InitHeight { get => Convert.ToDouble(optionItems[7].Values); set => optionItems[7].Values = value.ToString(); }

    }




    public class Systemd : OptionItem
    { 

        [XmlElement(ElementName = "RecentlyFile")]    // 最近操作的文件
        private string recentlyFile = "";
        public string RecentlyFile
        {
            get => recentlyFile;
            set
            {
                recentlyFile = value.Contains(".") ? value.Split(new char['.'], StringSplitOptions.RemoveEmptyEntries)[0] : value;
            }
        }

        [XmlElement(ElementName = "FileOffest")]    // 文件偏移量
        private long fileOffset = 0;
        public long FileOffest { get => fileOffset; set => fileOffset = value; }

        [XmlElement(ElementName = "LastModify")]    // 文件更改时间
        private DateTime lastModify = new DateTime();
        public DateTime LastModify { get => lastModify; set => lastModify = value; }

        [XmlElement(ElementName = "DevMode")]    // 是否为开发模式
        private bool devMode = true;
        public bool DevMode { get => devMode; set => devMode = value; }

        [XmlElement(ElementName = "Download")]  // 是否启用FTP下载
        private bool download = false;
        public bool Download { get => download; set => download = value; }

        [XmlElement(ElementName = "Hostname")]  // FTP主机IP或域名
        private string hostname = "";
        public string Hostname { get => hostname; set => hostname = value; }

        [XmlElement(ElementName = "Username")]  // FTP用户名称
        private string username = "";
        public string Username { get => username; set => username = value; }

        [XmlElement(ElementName = "Password")]  // FTP主机密码
        private string password = "";
        public string Password { get => password; set => password = value; }

        [XmlElement(ElementName = "Port")]      // 端口
        private int port = 21;
        public int Port { get => port; set => port = value; }

        [XmlElement(ElementName = "RemotePath")]
        private string remotePath = "";
        public string RemotePath { get => remotePath; set => remotePath = value; }


        [XmlElement(ElementName = "UtcTime")]   // 是否使用格林威治时间
        private bool utcTime = true;
        public bool UtcTime { get => utcTime; set => utcTime = value; }

        [XmlElement(ElementName = "HtmlFile")]   // 网页文件位置
        private string htmlFile = Environment.CurrentDirectory + "\\View\\web\\echarts.html";
        public string HtmlFile { get => htmlFile; set => htmlFile = value; }
    }
    

   

    //public class Port
    //{
    //    //[XmlIgnore]
    //    // public Setting Parent = new Setting();
    //    [XmlElement(ElementName = "PortName")]
    //    public string PortName { get; set; }

    //    [XmlElement(ElementName = "Baud")]
    //    public int Baud { get; set; }

    //    [XmlElement(ElementName = "Data")]
    //    public int Data { get; set; }

    //    [XmlElement(ElementName = "Stop")]
    //    public int Stop { get; set; }

    //    [XmlElement(ElementName = "Parity")]
    //    public int Parity { get; set; }

    //    [XmlArray(ElementName = "PjList")]
    //    public ObservableCollection<Pj> PjList { get; set; }
    //}

    

    //public class Gather
    //{
    //    // [XmlIgnore]
    //    // public Setting Parent = new Setting();

    //    /// <summary>
    //    /// 数据存储路径
    //    /// </summary>
    //    [XmlElement(ElementName = "DataPath")]
    //    public string DataPath { get; set; }

    //    /// <summary>
    //    /// 数据列表
    //    /// </summary>
    //    [XmlArray(ElementName = "ColumnList")]
    //    public ObservableCollection<Column> ColumnList { get; set; }
    //}

    //public class Column : INotifyPropertyChanged
    //{
    //    //[XmlIgnore]
    //    //public Gather Parent = new Gather();
    //    /// <summary>
    //    /// 索引
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Index")]
    //    public int Index { get; set; }
    //    /// <summary>
    //    /// 行名称
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Name")]
    //    public string Name { get; set; }
    //    /// <summary>
    //    /// 灵敏度
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Sensitivity")]
    //    public double Sensitivity { get; set; }

    //    /// <summary>
    //    /// 采集频率
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Frequency")]
    //    public int Frequency { get; set; }
    //    /// <summary>
    //    /// 映射至源数据的ID
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Shadow")]
    //    public string Shadow { get; set; }

    //    /// <summary>
    //    /// 结果集方法
    //    /// </summary>
    //    [XmlAttribute(AttributeName = "Method")]
    //    public string Method { get; set; }



    //    #region 属性更改通知
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void Changed(string PropertyName)
    //    {
    //        if (this.PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
    //    }
    //    #endregion

    //    #region 构造函数重载

    //    public Column() { }
    //    public Column(int index, string name)
    //    {
    //        Index = index;
    //        Name = name;
    //    }
    //    #endregion
    //}
}
