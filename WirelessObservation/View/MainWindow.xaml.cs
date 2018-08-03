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
        private List<List<decimal>> DataTemp = new List<List<decimal>>();
        private IController controller;
        private int sendBytesCount = 0;
        private int receiveBytesCount = 0;
        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

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
            }

            if ((bool)GatherCB.IsChecked)
            {
                controller.OpenSerialPort(name, "9600",
                    "8", "One", "None",
                    "None");
            }
            else
            {
                controller.CloseSerialPort();
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {

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

                dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);     // 增加分发事件
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);                   // 设置循环时间

                dispatcherTimer.Start();                                    // 开启循环发送接收数据

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
                //AddStringToCheckBox("开始采集",GatherCB);      // 变更按钮显示

                dispatcherTimer.Stop();             // 停止计时器
                dispatcherTimer.Tick -= new EventHandler(DispatcherTimer_Tick);     // 移除分发事件
            }
        }

        /// <summary>
        /// Display received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            if (CheckAccess())
            {
                try
                {
                    Dispatcher.Invoke(new Action<Object, SerialPortEventArgs>(ComReceiveDataEvent), sender, e);
                }
                catch (Exception)
                {
                    //disable form destroy exception
                }
                return;
            }
            string prototype = Encoding.Default.GetString(e.receivedBytes);
            List<string> split = prototype.Split(new char[] { ',', '=' }).ToList();
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            split = split.Skip(2).Take(2).ToList();
            split.Add(now);
            string str = string.Join(",", split.ToArray());
            Console.WriteLine(str);
        }

        /// <summary>
        /// 定时器回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //try
            //{
            //    // 获取数据
            //    string dataPath = Setting.Gather.DataPath;
            //    FileStream fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read);

            //    StringBuilder sb = new StringBuilder();
            //    long newPos = InverseReadRow(fs, fs.Length, ref sb);


            //    // 存储当前行
            //    List<string> a = new List<string>
            //{
            //    sb.ToString()
            //};

            //    // 设置分割字符
            //    char[] sp = { ',', '"', '\r', '\n' };
            //    // 存储数据型
            //    string[] datas = a[0].Split(sp, StringSplitOptions.RemoveEmptyEntries);
            //    // 取得X轴中最后一个时间戳
            //    string lastTimeStamp = labels[0][(labels[0].Count) - 1];
            //    // 将数据中的时间取出
            //    DateTime dateTime = Convert.ToDateTime(datas[0]);


            //    if (labels[0].Count > 1 && DataTemp.Count > 0)
            //    {
            //        if (dateTime.Second == DataTemp[0].Count)
            //        {
            //            return;
            //        }
            //        int length = 0;

            //        if (dateTime.Second != 0 && dateTime.Second < DataTemp[0].Count)
            //        {
            //            string before = string.Format("{0:g}", dateTime.AddMinutes(-1));
            //            before += ":" + DataTemp[0].Count;
            //            length = Math.Abs(dateTime.CompareTo(Convert.ToDateTime(before)));
            //        }
            //        else if (dateTime.Second - 1 > DataTemp[0].Count)
            //        {
            //            length = dateTime.Second - 1 - DataTemp[0].Count;
            //        }
            //        for (int key = 0; key < length; key++)
            //        {
            //            sb.Clear();
            //            newPos = InverseReadRow(fs, newPos, ref sb);
            //            a.Add(sb.ToString());

            //        }

            //        a.Reverse();
            //    }
            //    fs.Close();

            //    for (int i = 0; i < a.Count; i++)
            //    {
            //        datas = a[i].Split(sp, StringSplitOptions.RemoveEmptyEntries);
            //        string con = a[i] + "|";
            //        if (DataTemp.Count > 0) con += DataTemp[0].Count + "-";
            //        if (datas.Length > 0) con += datas[0] + ",";
            //        Console.WriteLine(con + dateTime.Second);
            //        string[] split = datas[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //        string date = split[0];
            //        DateTime earlist = Convert.ToDateTime(date + " 05:00:00");
            //        DateTime latest = Convert.ToDateTime(date + " 19:00:00");
            //        if (DateTime.Compare(Convert.ToDateTime(datas[0]), earlist) < 0 || DateTime.Compare(Convert.ToDateTime(datas[0]), latest) > 0) return;

            //        // 当前数据中的时间与X轴的计量点中坐标轴时间较大时
            //        if (DateTime.Compare(Convert.ToDateTime(datas[0]), Convert.ToDateTime(lastTimeStamp)) <= 0)
            //        {
            //            for (int di = 2; di < datas.Length; di++)
            //            {
            //                if (!decimal.TryParse(datas[di], out decimal x))
            //                {
            //                    x = 0;
            //                }
            //                int scIndex = di - 2;

            //                // 将取得的数据存入dataTemp
            //                DataTemp = AddSonItem<Decimal>(DataTemp, x, scIndex);

            //            }

            //            // 获取临时数据存储数组项目数量                                
            //            int count = DataTemp.Count;
            //            double[] avg = new double[datas.Length - 2];
            //            for (int dli = 0; dli < count; dli++)
            //            {
            //                // 使用均值方法获取均值
            //                avg[dli] = GetAvg(DataTemp[dli]);
            //                // 数据列中当前列的数量大于0 并且dataTemp中对应数据多于1的时候将数据列中的该点移除
            //                if (SeriesCollection[0][dli].Values.Count > 0 && DataTemp[dli].Count > 1)
            //                {
            //                    SeriesCollection[0][dli].Values.RemoveAt(SeriesCollection[0][dli].Values.Count - 1);
            //                }
            //                // 添加当前的点进入数组
            //                SeriesCollection[0][dli].Values.Add(avg[dli]);
            //            }

            //            // 当两个相等时获取
            //            if (DateTime.Compare(Convert.ToDateTime(datas[0]), Convert.ToDateTime(lastTimeStamp)) == 0)
            //            {
            //                // 将新的坐标轴时间加入Labels数组

            //                Labels[0].Add(Convert.ToDateTime(lastTimeStamp).AddSeconds(60).ToString());

            //                // 行数据数组
            //                string[] column = new string[datas.Length];
            //                // 写数据
            //                column[0] = "\"" + datas[0] + "\"";
            //                // 数据编号
            //                column[1] = spendTime.ToString();
            //                // 遍历数据列
            //                for (int key = 0; key < avg.Length; key++)
            //                {
            //                    column[key + 2] = avg[key].ToString();
            //                }
            //                // 将数据数组接合为字符串
            //                string line = string.Join(",", column) + Environment.NewLine;
            //                // 将字符串转换为byte型数据
            //                byte[] by = Encoding.UTF8.GetBytes(line);
            //                // 获取当前需要操作的文件名
            //                string fn = GetFileName(DataStorage, DataType.SourceData, (DateTime)GatherTimer[0], null);
            //                // 以追加写方式打开文件流
            //                FileStream fileStream = new FileStream(fn, FileMode.Append, FileAccess.Write);
            //                // 写数据
            //                fileStream.Write(by, 0, by.Length);
            //                // 关闭文件流
            //                fileStream.Close();
            //                spendTime++;
            //                // 清理数据缓存，准备下次数据接入
            //                DataTemp.Clear();
            //            }

            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "提示");
            //}


        }


        
    }
}
