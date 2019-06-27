using System;
using System.Xml.Serialization;


namespace WirelessObservation.Entity
{
    [XmlRoot(ElementName = "Setting")]
    [Serializable]
    public class Setting
    {
        //[XmlIgnore]
        //public object Parent;

        [XmlElement(ElementName = "Collect")]
        public Collect Collect { get; set; }

        //[XmlElement(ElementName = "Port")]
        //public Port Port { get; set; }

        [XmlElement(ElementName = "Data")]
        public Data Data { get; set; }

        [XmlElement(ElementName = "Systemd")]
        public Systemd Systemd { get; set; }
    }

    public class Systemd
    {
        [XmlElement(ElementName = "RecentlyFile")]    // 最近操作的文件
        private string recentlyFile = "";
        public string RecentlyFile {
            get => recentlyFile;
            set {
                recentlyFile = value.Contains(".") ? value.Split(new char['.'], StringSplitOptions.RemoveEmptyEntries)[0] : value;
            }
        }

        [XmlElement(ElementName = "FileOffest")]    // 文件偏移量
        private long fileOffset = 0;
        public long FileOffest { get => fileOffset; set => fileOffset = value; }

        [XmlElement(ElementName = "LastModify")]    // 文件更改时间
        private DateTime lastModify = new DateTime();
        public DateTime LastModify { get => lastModify; set => lastModify = value; }

        [XmlElement(ElementName = "ComPort")]    // 接收数据使用的COM口
        private string comPort = "";
        public string ComPort { get => comPort; set => comPort = value; }

        [XmlElement(ElementName = "DevMode")]    // 是否为开发模式
        private bool devMode = false;
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
    }
    

    public class Data
    {
        //[XmlIgnore]
        [XmlElement(ElementName = "DataPath")]  // 数据存储位置
        private string dataPath;

        public string DataPath { get => dataPath; set => dataPath = value; }

        [XmlElement (ElementName = "StorePath")]    // 输出数据存储位置
        private string storePath;

        public string StorePath { get => storePath; set => storePath = value; }

        

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

    public class Collect
    {
        /// <summary>
        /// 输入/采集频率 unit:second
        /// </summary>
        [XmlElement(ElementName = "Interval")]
        public int? Interval { get; set; }

        [XmlElement(ElementName = "SodarUrl")]    // 输出数据存储位置
        private string sodarUrl;

        public string SodarUrl { get => sodarUrl; set => sodarUrl = value; }

        ///// <summary>
        ///// 输出/显示频率
        ///// </summary>
        //[XmlElement(ElementName = "Output")]
        //public int? Output { get; set; }

    }

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
