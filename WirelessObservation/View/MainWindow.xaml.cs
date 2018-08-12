using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Geared;

namespace WirelessObservation.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        #region 字段

        /// <summary>
        /// 当前窗口是否处于采集中
        /// </summary>
        private bool collecting = false;

        /// <summary>
        /// 数据缓存数组 datatemp
        /// </summary>
        private List<Entity.DataEntity> DataTemp = new List<Entity.DataEntity>();
        private IController controller;
       
        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private static bool isGather = false;
        public static DateTime startTime;
        public static DateTime endTime;
        public static bool IsGather
        {
            get => isGather;
            set
            {
                if (value != false && value != true)
                {
                    isGather = false;
                }
                else
                {
                    isGather = value;
                }
            }
        }

        public CartesianChart CartesianChart { get => cartesianChart; set => cartesianChart = value; }
        public SeriesCollection SeriesCollection { get => seriesCollection; set => seriesCollection = value; }
        public List<string> Labels { get => labels; set => labels = value; }
        public List<Func<double, string>> YFormatter { get => yFormatter; set => yFormatter = value; }

        /// <summary>
        /// 图表LIST
        /// </summary>
        private CartesianChart cartesianChart = new CartesianChart();

        /// <summary>
        /// 图表内数据集LIST
        /// </summary>
        private SeriesCollection seriesCollection = new SeriesCollection();

        /// <summary>
        /// 图标内标签LIST
        /// </summary>
        private List<string> labels = new List<string>();


        /// <summary>
        /// 风速
        /// </summary>
        private List<Func<double, string>> yFormatter = new List<Func<double, string>>();

        #endregion



        public MainWindow()
        {
            InitializeComponent();
            controller = new IController(this);
        }

        /// <summary>
        /// Set controller
        /// </summary>
        /// <param name="controller"></param>
        public void SetController(IController controller)
        {
            this.controller = controller;
        }


        private void GatherCB_Checked(object sender, RoutedEventArgs e)
        {
            string name = Vendor.ComPort.GetComName("CH340");
            if (name != "")
            {
                Console.WriteLine(name);
                controller.OpenSerialPort(name, "9600",
                    "8", "One", "None",
                    "None");
            }
            else
            {
                GatherCB.IsChecked = false;
                MessageBox.Show("未检测到无线接收模块" ,"提示");

                
            }

            
           
        }

        private void GatherCB_Unchecked(object sender, RoutedEventArgs e)
        {
            controller.CloseSerialPort();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsGather)
            {
                MessageBox.Show("数据采集中，无法导出数据，请先停止采集", "提示");
                return;
            }
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            // 初始化文件夹有设置则使用初始化文件夹
            if (App.Setting.Data.StoragePath != "")
            {
                saveFileDialog.InitialDirectory = App.Setting.Data.StoragePath;
            }
            saveFileDialog.Filter = "CSV文件 | *.csv|所有文件|*.*";
            saveFileDialog.ShowDialog();
            
            // 获取文件构成的各部分字符串
            string rd = saveFileDialog.FileName;
            if (rd != "")
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(rd);
                string extension = "csv";
                string path = System.IO.Path.GetDirectoryName(rd);
                // 存储数据至配置
                App.Setting.Data.StoragePath = path;
                // 存储至文件
                Vendor.XmlHelper.SerializeToXml(App.SettingPath, App.Setting);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path + "\\" + fileName + "." + extension, false, Encoding.UTF8);
                List<string> header = new List<string>
                {
                    "\"" + string.Join("\",\"", new string[] { "记录数","时间","风速", "风向"}) + "\"",
                    "\"" + string.Join("\",\"", new string[] { "RN", "TS", "m/s","°" }) + "\"",
                };
                // 将文件头中所有数据写入文件
                foreach (string str in header)
                {
                    // 写入一整行
                    sw.WriteLine(str);
                }
                int min = int.MaxValue;
                for (int index = 0; index < SeriesCollection.Count; index++)
                {
                    min = Math.Min(SeriesCollection[index].Values.Count, min);
                }

                //遍历当前数据并写入文件
                for (int i = 0; i < min; i++)
                {
                    string data = (i + 1).ToString() + ",\"" + Labels[(int)i] + "\"," + SeriesCollection[0].Values[i] + "," + SeriesCollection[1].Values[i];
                    sw.WriteLine(data);
                }

                // 关闭文件
                sw.Close();
            }
            
        }

        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenComEvent(Object sender, SerialPortEventArgs e)
        {
            //if (this.CheckAccess())
            //{
            //    this.Dispatcher.Invoke(new Action<Object, SerialPortEventArgs>(OpenComEvent), sender, e);
            //    return;
            //}


            if (e.isOpend)  //Open successfully
            {
                GatherCB.Content = "结束采集";      // 变更按钮显示
                                                // 现在时间
                startTime = DateTime.Now;
                IsGather = true;
                string[] Datalist = System.IO.File.ReadAllLines(App.DataStoragePath + "\\source.dat");
                List<string> header = Datalist.Take(2).ToList();

                InitChart(header);
                // 采集频率
                int input = (int)App.Setting.Collect.Input;
                string first_temp = ChartStartFormat(startTime, input).ToString("yyyy-MM-dd HH:mm:ss");
                DataTemp = AddItem(DataTemp, new Entity.DataEntity(first_temp));
                // 输出频率

                int output = (int)App.Setting.Collect.Output;
                
                string first_line = ChartStartFormat(startTime, input).ToString("yyyy-MM-dd HH:mm:ss");
                
            }
            else    //Open failed
            {
                MessageBox.Show("无法开启串口");
            }
            
        }

        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseComEvent(Object sender, SerialPortEventArgs e)
        {
            //if (CheckAccess())
            //{
            //    Dispatcher.Invoke(new Action<Object, SerialPortEventArgs>(CloseComEvent), sender, e);
            //    return;
            //}

            if (!e.isOpend) //close successfully
            {
                this.GatherCB.Dispatcher.Invoke(
                            new Action(
                                delegate
                                {
                                    this.GatherCB.Content = "开始采集";
                                }

                                ));
                DataTemp.Clear();
                isGather = false;
            }
        }

        /// <summary>
        /// 初始化图表
        /// </summary>
        /// <param name="datasList"></param>
        /// <param name="isAddLabel"></param>
        private void InitChart(List<string> datasList, bool isAddLabel = true)
        {
            try
            {
                
                    ClearChartElement();

                    // 固定取第二行的数据（此行为列名数据）
                    string title = datasList[0];
                    string[] data = title.Split(new char[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    int count = data.Length;


                    for (int startIndex = 2; startIndex < count; startIndex++)
                    {

                        GLineSeries ls = new GLineSeries
                        {
                            Title = data[startIndex],// 设置集合标题
                            Values = new GearedValues<double> { Quality = Quality.Low},                // 初始化数据集
                            //PointGeometry = DefaultGeometries.None,             // 取消点的图形标注
                            ScalesYAt = startIndex - 2,
                            Fill = Brushes.Transparent,
                            StrokeThickness = .5,
                            PointGeometry = null //use a null geometry when you have many series
                        };

                        if (SeriesCollection.Count > 0)
                        {

                            if (SeriesCollection.Count > startIndex - 2)
                            {
                                seriesCollection[startIndex - 2] = ls;
                            }
                            else
                            {
                                SeriesCollection.Add(ls);
                            }

                        }
                        else
                        {
                            seriesCollection = (new SeriesCollection { });
                            if (SeriesCollection.Count > startIndex - 2)
                            {
                                seriesCollection[startIndex - 2] = ls;
                            }
                            else
                            {
                                SeriesCollection.Add(ls);
                            }
                        }
                        // Y轴的轴标签显示结构
                        YFormatter = AddItem(YFormatter, value => value.ToString("N"), startIndex - 2);
                    }
                    Labels = AddItem(Labels, string.Empty);
                    // 判断是否需要初始化标签
                    if (isAddLabel)
                    {
                        Labels = AddItem(Labels, ChartStartFormat(startTime,(int) App.Setting.Collect.Output).ToString(), 0);
                    }
                   
                    CartesianChart cartesian = new CartesianChart
                    {
                        Series = SeriesCollection,
                        LegendLocation = LegendLocation.Right,
                        Zoom = ZoomingOptions.X,
                        DataTooltip = null,
                        AxisY = new AxesCollection
                {
                    new Axis{
                        Title = "风速，单位：m/s",
                        LabelFormatter = YFormatter[0],
                        Position = AxisPosition.LeftBottom,
                        MinValue = 0

                    },
                    new Axis{
                        Title = "风向，单位：°",
                        LabelFormatter = YFormatter[1],
                        Position = AxisPosition.RightTop,
                        MaxValue = 360,
                        MinValue = 0

                    }
                },
                        AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "时间",
                        Labels = Labels
                    }
                },
                        Hoverable = false,
                        DisableAnimations = true,
                    };
                    CartesianChart = cartesian;

                    // 将图表实例添加至ChartZone这个grid中去
                    Chart.Children.Add(cartesianChart);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }



        }

        private void ClearChartElement()
        {
            // 将chartzone内所有子元素清空
            Chart.Children.Clear();
            // 重置曲线
            SeriesCollection.Clear();
            // 充值图表
            CartesianChart = new CartesianChart() ;
            // 重置Y轴文本格式
            YFormatter.Clear();
            // 重置X轴标签
            Labels.Clear();
        }

        

        /// <summary>
        /// Display received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.CheckAccess())
            {
                try
                {
                    Dispatcher.Invoke(new Action<Object, SerialPortEventArgs>(ComReceiveDataEvent), sender, e);
                }
                catch (System.Exception)
                {
                    //disable form destroy exception
                }
                return;
            }
            try
            {
                // 数据原型
                string prototype = Encoding.Default.GetString(e.receivedBytes);
                // 数据不以$开始时，数据不包含,时丢弃数据
                if (prototype.IndexOf("$") != 0 || !prototype.Contains(","))
                {
                    return;
                }
                // 获取被截断的数据
                List<string> split = prototype.Split(new char[] { ',', '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                // 正确的数据应该分割出来是5段字符串数组
                if (split.Count != 5)
                {
                    return;
                }
                // 现在时间
                DateTime now = DateTime.Now;

                int input   = (int)App.Setting.Collect.Input;
                // 输出频率

                int output  = (int)App.Setting.Collect.Output;
                // 输入时间初始化
                string timeFormat = now.ToString("yyyy-MM-dd HH:mm:ss");
                // 获取文件总行数
                string[] lines = System.IO.File.ReadAllLines(App.DataStoragePath + "\\source.dat");
                // 布置当前数据编号
                int count = lines.Length - 1;
                // 数据头
                List<string> font = new List<string> { count.ToString(), "\"" + timeFormat + "\"" };
                // 截取出有用数据断
                split = split.Skip(2).Take(2).ToList();
                // 插入到数据数组头部
                split.InsertRange(0, font);


                // 取得X轴中最后一个时间戳

                string lastTimeStamp = labels[(labels.Count) - 1];

                DateTime dt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                Console.WriteLine("数据时间差：" + dt.CompareTo(Convert.ToDateTime(lastTimeStamp)));
                // 当前数据中的时间与X轴的计量点中坐标轴时间较大时

                // 获取临时数据存储数组项目数量                                
                int Dcount = DataTemp.Count;
                if (Dcount > 0)
                {
                    // 暂存最后一条缓存数据
                    Entity.DataEntity LastItem = DataTemp[Dcount - 1];
                    Console.WriteLine("标签数量：" + Labels.Count);
                    if (double.TryParse(split[2], out double x)
                        && int.TryParse(split[3], out int y)
                        )
                    {
                        LastItem.WindSpeed = x;
                        LastItem.WindDir = y;
                        SeriesCollection[0].Values.Add(LastItem.WindSpeed);
                        SeriesCollection[1].Values.Add(LastItem.WindDir);

                        // 将数据转换为字符串
                        string str = string.Join(",", split.ToArray());
                        // 
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(App.DataStoragePath + "\\source.dat", true, Encoding.UTF8);

                        // 写入一整行
                        sw.WriteLine(str);


                        // 关闭文件
                        sw.Close();
                    }
                    else
                    {
                        Labels.RemoveAt(Labels.Count - 1);
                    }

                    DataTemp.Clear();
                    Labels.Add(Convert.ToDateTime(lastTimeStamp).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss"));
                    // 数据容量超过3小时即60 * 60 * 3个数据点
                    int capacity = 12 * 60 * 60;
                    if (SeriesCollection[0].Values.Count >= capacity)
                    {
                        SeriesCollection[0].Values.RemoveAt(0);
                        SeriesCollection[1].Values.RemoveAt(0);
                        Labels.RemoveAt(0);
                    }
                    string first_temp = now.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");
                    DataTemp = AddItem(DataTemp, new Entity.DataEntity(first_temp), Dcount);
                }
            }
            catch(Exception exc)
            {
                return;
            }
            
            
        
        }


        public static string DateFormat(DateTime time, string format = "yyyy-MM-dd HH：mm：ss")
        {
            return time.ToString(format);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsGather)
            {
                MessageBoxResult a = MessageBox.Show("程序正在采集中，是否结束采集", "警告", MessageBoxButton.OKCancel);
                if (a == MessageBoxResult.OK)
                {
                    
                }

            }

        }

        /// <summary>
        /// 求指定数组中的所有值的平均值
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Entity.DataEntity GetAvg(List<Entity.DataEntity> arr)
        {
            int count = 0;
            double sumx = 0;
            double sumy = 0;
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i].WindSpeed != 0 && arr[i].WindDir != 0)
                {
                    sumx += arr[i].WindSpeed;
                    sumy += arr[i].WindDir;
                    count++;
                }
                
            }
            if (count != 0)
            {
                return new Entity.DataEntity("", sumx / count, (int)sumy / count);
            }
            else
            {
                return new Entity.DataEntity("", (double)0, (int)0);
            }
            
        }

        private DateTime ChartStartFormat(DateTime startTime, int frequency)
        {
            // 起始时间
            DateTime result = new DateTime();
            if (frequency <= 60 && 60 % frequency == 0)
            {
                // 获取当前周期内的起始时间
                string time_temp = string.Format("{0:g}", startTime);
                time_temp += ":00";
                // 设置初始时间
                DateTime timestamp = Convert.ToDateTime(time_temp);
                // 数据记录开始时间有可能大于当前时间段，将时间对扩大查找至当前分钟内后几段
                int times = 0;
                // 如果当前时间段处于数据时间左侧则将time+1
                while (DateTime.Compare(timestamp.AddSeconds(frequency * times), Convert.ToDateTime(startTime)) < 0) times++;
                // 结果
                result = timestamp.AddSeconds(frequency * times);
            }
            // 采集频率 大于1分钟 但 小于1小时 同时能 整除60 并 被3600整除
            else if (frequency > 60 && frequency <= 60 * 60
                && 3600 % frequency == 0 && frequency % 60 == 0)
            {
                // 获取当前周期内的起始时间
                string time_temp = string.Format("{0:yyyy-MM-dd HH}", startTime);
                time_temp += ":00:00";
                // 设置初始时间
                DateTime timestamp = Convert.ToDateTime(time_temp);
                // 数据记录开始时间有可能大于当前时间段，将时间对扩大查找至当前分钟内后几段
                int times = 0;
                // 如果当前时间段处于数据时间左侧则将time+1
                while (DateTime.Compare(timestamp.AddSeconds(frequency * times), Convert.ToDateTime(startTime)) < 0) times++;
                // 结果
                result = timestamp.AddSeconds(frequency * times);
            }
            // 采集频率 大于1小时 但 小于24小时 同时能 整除60、60*60 并 被24*60*60整除
            else if (frequency > 60 * 60 && frequency <= 24 * 60 * 60
                && (24 * 60 * 60) % frequency == 0 && frequency % 60 * 60 == 0 && frequency % 60 == 0)
            {
                // 获取当前周期内的起始时间
                string time_temp = string.Format("{0:yyyy-MM-dd }", startTime);
                time_temp += "00:00:00";
                // 设置初始时间
                DateTime timestamp = Convert.ToDateTime(time_temp);
                // 数据记录开始时间有可能大于当前时间段，将时间对扩大查找至当前分钟内后几段
                int times = 0;
                // 如果当前时间段处于数据时间左侧则将time+1
                while (DateTime.Compare(timestamp.AddSeconds(frequency * times), Convert.ToDateTime(startTime)) < 0) times++;
                // 结果
                result = timestamp.AddSeconds(frequency * times);
            }

            return result;
        }

        /// <summary>
        /// 将指定类型的数据添加至数组中，
        /// </summary>
        /// <typeparam name="T">数组中指定的类型</typeparam>
        /// <param name="variable">数组变量</param>
        /// <param name="item">数组中需要更改的数据</param>
        /// /// <param name="index">数组中需要更改的索引</param>
        /// <returns></returns>
        public static List<T> AddItem<T>(List<T> variable, T item, int index)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > index)
            {
                // 更改该项目
                variable[index] = item;
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(item);
            }
            // 将数组返回
            return variable;
        }

        public static List<T> AddItem<T>(List<T> variable, T item)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > 0)
            {
                // 更改该项目
                variable[0] = item;
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(item);
            }
            // 将数组返回
            return variable;
        }

        public static List<List<T>> AddSonItem<T>(List<List<T>> variable, T sonItem, int index)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > index)
            {
                // 更改该项目
                variable[index].Add(sonItem);
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(new List<T> { sonItem });
            }
            // 将数组返回
            return variable;
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsGather)
            {
                MessageBox.Show("数据采集中，无法获取数据，请先停止采集", "提示");
                return;
            }
            HistoryData historyData = new HistoryData();
            historyData.ShowDialog();
            if (historyData.DialogResult == true)
            {
                string[] Datalist = System.IO.File.ReadAllLines(App.DataStoragePath + "\\source.dat");
                List<string> header = Datalist.Take(2).ToList();

                InitChart(header);
                //string filepath = GetFileName(DataType.CalibrationData,(DateTime)GatherTimer[0],GatherTimer[1]);





                string[] dataBody = Datalist.Skip(4).ToArray();



                // 遍历源数据，并按照需校准数据进行调整
                foreach (string line in dataBody)
                {
                    // 存储当前行
                    string a = line;
                    // 设置分割字符
                    char[] sp = { ',', '"', '\r', '\n' };
                    // 存储数据型
                    string[] datas = a.Split(sp, StringSplitOptions.RemoveEmptyEntries);
                    // 如果时间数据越界中断循环
                    if (endTime != null && (DateTime.Compare(Convert.ToDateTime(endTime), Convert.ToDateTime(datas[1])) < 0)) break;

                    // 获取临时数据存储数组项目数量                                
                    int count = datas.Length;
                    for (int dli = 2; dli < count; dli++)
                    {
                        if (!decimal.TryParse(datas[dli], out decimal x))
                        {
                            if (dli > 2)
                            {
                                for (var i = dli; i > 2; i--)
                                {
                                    SeriesCollection[i - 2].Values.RemoveAt(SeriesCollection[i - 2].Values.Count - 1);
                                }
                            }
                            break;
                        }
                        SeriesCollection[dli - 2].Values.Add(Convert.ToDouble(datas[dli]));

                    }

                    // 将新的坐标轴时间加入Labels数组

                    Labels.Add(datas[1]);
                }
            }
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            string des = "当前版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n" +
                "(1) 使用本软件请确认已插入无线接收模块\r\n" +
                "(2) 点击\"开始采集\"开始本次采集\r\n" +
                "(3) 点击\"结束采集\"结束本次采集\r\n" +
                "(4) 点击\"导出数据\"导出本次采集数据，导出数据只能在结束采集后使用\r\n" +
                "(5) 点击\"历史数据\"可根据历史缓存的数据进行查看，历史数据只能在结束采集后使用\r\n" +
                "如有疑问请联系：wangwei@topflagtec.com 王玮\r\n\r\n" +
                "北京旗云创科科技有限责任公司" +
                "BEIJING TOP FLAG TECHNOLOGY CO., LTD\r\n" +
                "地址：北京市海淀区中关村南三街6号中科资源大厦（100080）\r\n" +
                "电话：010 - 6142 - 6159，传真：010 - 6072 - 0351\r\n" +
                "网址：www.topflagtec.com\r\n";
            MessageBox.Show(des, "帮助",MessageBoxButton.OK);
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            test t = new test();
            t.ShowDialog();
        }
    }
}
