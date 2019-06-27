﻿using System;
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
using Microsoft.Win32;
using WirelessObservation.Vendor;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using WirelessObservation.Entity;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using CefSharp;
using CefSharp.Wpf;

namespace WirelessObservation.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        #region 字段

        

        private IController controller;
       
        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// 采集切换标识
        /// </summary>
        private static bool isGather = false;

        /// <summary>
        /// 是否使用历史数据标识
        /// </summary>
        private static bool isHistory = false;

        private int intervalCount = 0;

        /// <summary>
        /// 表格数据集
        /// </summary>
        private static List<WindProfileRadarEntity> chartData = new List<WindProfileRadarEntity>();

        private static DateTime startTime;

        /// <summary>
        /// 表格时间轴数组
        /// </summary>
        private static string[] timeAxis;
        private static DateTime endTime;

        /// <summary>
        /// 是否处于采集状态
        /// </summary>
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

        /// <summary>
        /// 串口数据原型
        /// </summary>
        public string Prototype = string.Empty;

        /// <summary>
        /// 表格数据
        /// </summary>
        public static List<WindProfileRadarEntity> ChartData { get => chartData; set => chartData = value; }
        public static string[] TimeAxis { get => timeAxis; set => timeAxis = value; }
        public static DateTime StartTime { get => startTime; set => startTime = value; }
        public static DateTime EndTime { get => endTime; set => endTime = value; }
        public DispatcherTimer DispatcherTimer { get => dispatcherTimer; set => dispatcherTimer = value; }
        public int IntervalCount { get => intervalCount; set => intervalCount = value; }
        public static bool IsHistory { get => isHistory; set => isHistory = value; }






        /// <summary>
        /// 浏览器实例
        /// </summary>
        ChromiumWebBrowser browser;

        #endregion

        #region 界面功能

        public MainWindow()
        {
            InitializeComponent();
            string str = string.Empty;
            try
            {
                // 非开发模式隐藏掉非必要的按钮
                if (!App.Setting.Systemd.DevMode)
                {
                    CollectBtn.Visibility = Visibility.Collapsed;
                    ResetData.Visibility = Visibility.Collapsed;
                    test.Visibility = Visibility.Collapsed;
                }
                controller = new IController(this);
                
                // 开始拉取数据
                StartGather();
                
                // 创建浏览器对象
                browser = new ChromiumWebBrowser();

                // 设置浏览器浏览的html文件
                string HtmlPath = StringHelper.Encoding(Environment.CurrentDirectory + "\\View\\web\\echarts.html");
                LogHelper.WriteLog(new Exception("string exception"), HtmlPath);

                browser.Address = HtmlPath;

                // 将浏览器对象加入chart容器中
                Chart.Children.Add(browser);
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;//新cefsharp绑定需要优先申明
                
                // 设置异步的JS-C#委托事件
                browser.RegisterAsyncJsObject("boud", new JsEvent(), new BindingOptions() { CamelCaseJavascriptNames = false, });
                
                // 增加缩放事件
                this.SizeChanged += new SizeChangedEventHandler(MainWindow_Resize);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            
            
            // 处理上次结束之后到本次未格式化数据

        }



        /// <summary>
        /// 缩放界面时的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Resize(object sender, System.EventArgs e)
        {
            try
            {
                // 最大化及非初始化扩大页面时重载浏览器实例
                if (this.WindowState == WindowState.Maximized)
                {
                    browser.GetBrowser().Reload();
                }
                else if (this.IsLoaded == true && this.WindowState == WindowState.Normal)
                {
                    browser.GetBrowser().Reload();
                }
                
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            
        }

        

        /// <summary>
        /// Set controller
        /// </summary>
        /// <param name="controller"></param>
        public void SetController(IController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// 点击开始采集按钮的的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GatherCB_Checked(object sender, RoutedEventArgs e)
        {
            //browser.ShowDevTools();
            //string name = Vendor.ComPort.GetComName("CH340");
            //string name = ComPort.GetComName("USB Serial Port");
            string name = App.Setting.Systemd.ComPort;
            if (name != "")
            {
                Console.WriteLine(name);
                controller.OpenSerialPort(name, "9600",
                    "8", "One", "None",
                    "None");
            }
        }

        /// <summary>
        /// 点击采集设置按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 点击结束采集按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GatherCB_Unchecked(object sender, RoutedEventArgs e)
        {
            // 只有串口打开处于采集状态时才关闭串口
            if (IsGather)
            {
                controller.CloseSerialPort();
            }
            else
            {
                IntervalCount = 0;
                DispatcherTimer.Stop();
                DispatcherTimer.Tick -= new EventHandler(DispatcherTimer_Tick);
                this.GatherCB.Content = "开始采集";
            }
            
        }

        /// <summary>
        /// 点击导出数据的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (IsGather)
            //{
            //    MessageBox.Show("数据采集中，无法导出数据，请先停止采集", "提示");
            //    return;
            //}
            //Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            //// 初始化文件夹有设置则使用初始化文件夹
            //if (App.Setting.Data.StorePath != "")
            //{
            //    saveFileDialog.InitialDirectory = App.Setting.Data.StorePath;
            //}
            //saveFileDialog.Filter = "CSV文件 | *.csv|所有文件|*.*";
            //saveFileDialog.ShowDialog();
            
            //// 获取文件构成的各部分字符串
            //string rd = saveFileDialog.FileName;
            //if (rd != "")
            //{
            //    string fileName = System.IO.Path.GetFileNameWithoutExtension(rd);
            //    string extension = "csv";
            //    string path = System.IO.Path.GetDirectoryName(rd);
            //    // 存储数据至配置
            //    App.Setting.Data.StorePath = path;
            //    // 存储至文件
            //    Vendor.XmlHelper.SerializeToXml(App.SettingPath, App.Setting);
            //    System.IO.StreamWriter sw = new System.IO.StreamWriter(path + "\\" + fileName + "." + extension, false, Encoding.UTF8);
            //    List<string> header = new List<string>
            //    {
            //        "\"" + string.Join("\",\"", new string[] { "记录数","时间","风速", "风向"}) + "\"",
            //        "\"" + string.Join("\",\"", new string[] { "RN", "TS", "m/s","°" }) + "\"",
            //    };
            //    // 将文件头中所有数据写入文件
            //    foreach (string str in header)
            //    {
            //        // 写入一整行
            //        sw.WriteLine(str);
            //    }
            //    int min = int.MaxValue;
            //    for (int index = 0; index < SeriesCollection.Count; index++)
            //    {
            //        min = Math.Min(SeriesCollection[index].Values.Count, min);
            //    }

            //    //遍历当前数据并写入文件
            //    for (int i = 0; i < min; i++)
            //    {
            //        string data = (i + 1).ToString() + ",\"" + Labels[(int)i] + "\"," + SeriesCollection[0].Values[i] + "," + SeriesCollection[1].Values[i];
            //        sw.WriteLine(data);
            //    }

            //    // 关闭文件
            //    sw.Close();
            //}
            
        }

        /// <summary>
        /// 点击历史数据按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsGather || GatherCB.Content== "结束采集")
            {
                MessageBox.Show("数据采集中，无法获取数据，请先停止采集", "提示");
                return;
            }
            HistoryData historyData = new HistoryData();
            historyData.ShowDialog();
            if (historyData.DialogResult == true)
            {
                var query = (from f in Directory.GetFiles(App.Setting.Data.StorePath, "*.json")
                             let fi = new FileInfo(f)
                             orderby fi.Name ascending
                             where fi.Name.CompareTo(DateFormat(StartTime, "yyyyMMdd.json")) >= 0 && fi.Name.CompareTo(DateFormat(EndTime.AddDays(1), "yyyyMMdd.json")) < 0
                             select fi.FullName);
                string[] flies = query.ToArray();
                ChartData.Clear();
                for (int i = 0;i < flies.Length; i++)
                {
                    if (File.Exists(flies[i]))
                    {
                        // 读取所有内容
                        StreamReader jsonReader = new StreamReader(flies[i], Encoding.UTF8);
                        string json = jsonReader.ReadToEnd().TrimEnd(new char[] { '\r', '\n' });
                        jsonReader.Close();
                        if (!string.IsNullOrEmpty(json))  // 将数据存入数组中
                        {
                            
                             ChartData.AddRange(JsonConvert.DeserializeObject<List<WindProfileRadarEntity>>(json));
                        }
                        
                    }
                }
                SetTimeAxis(145);
                IsHistory = true;
                browser.ExecuteScriptAsync("SetChart()");

            }
        }

        /// <summary>
        /// 点击帮助按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            string des = "当前版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n" +
                "首次进入软件后请先设置雷达数据文件夹，点击 文件->设置数据文件位置 ，设置数据文件存储位置\r\n" +
                "缓存数据默认存储在软件安装目录下json文件夹内，更换位置请选择 文件->设置数据存储位置\r\n" +
                //"(1) 使用本软件请确认已插入无线接收模块\r\n" +
                "(1) 点击\"开始采集\"开始本次采集\r\n" +
                "(2) 点击\"结束采集\"结束本次采集\r\n" +
                //"(4) 点击\"导出数据\"导出本次采集数据，导出数据只能在结束采集后使用\r\n" +
                "(3) 点击\"历史数据\"可根据历史缓存的数据进行查看，历史数据只能在结束采集后使用\r\n" +
                "如有疑问请联系：wangwei@topflagtec.com 王玮\r\n\r\n" +
                "北京旗云创科科技有限责任公司" +
                "BEIJING TOP FLAG TECHNOLOGY CO., LTD\r\n" +
                "地址：北京市海淀区中关村南三街6号中科资源大厦（100080）\r\n" +
                "电话：010 - 6142 - 6159，传真：010 - 6072 - 0351\r\n" +
                "网址：www.topflagtec.com\r\n";
            MessageBox.Show(des, "帮助", MessageBoxButton.OK);
        }

        /// <summary>
        /// 点击测试按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void test_Click(object sender, RoutedEventArgs e)
        {
            SFTPHelper sftp = new SFTPHelper("192.168.24.218", "root", "111111");
            sftp.Connect();
            var filelist = sftp.GetFileList("/home","sh");
            var fl = sftp.GetFileSize("/home/item.sh");
            FileInfo fi = new FileInfo(@"C:\Users\Mloong\Desktop\test\item.sh");
            long size = fi.Length;
            sftp.Disconnect();

            
            
            //// 显示开发工具
            //browser.ShowDevTools();
            //long offset = App.Setting.Systemd.FileOffest;
            //string recently = App.Setting.Systemd.RecentlyFile;
            //string dataPath = App.Setting.Data.DataPath;
            //int count = 0;
            //FTPEntity ftp = new FTPEntity("118.190.202.172", "ftpuser", "111111", 6000);
            //DateTime recentTime = new DateTime();
            //IFormatProvider ifp = new System.Globalization.CultureInfo("zh-CN", true);
            //DateTime.TryParseExact(recently, "yyyyMMdd", ifp, System.Globalization.DateTimeStyles.None, out recentTime);
            //string dir = "/beishida";
            //SFTPHelper sftp = new SFTPHelper("118.190.202.172", "ftpuser", "273678@ROOT");
            //sftp.Connect();
            //sftp.Get("/home/download/Data-Dumper-2.173.tar.gz", @"E:\");
            //sftp.Disconnect();
            //var ftp = new Entity.FTPEntity("118.190.202.172", "ftpuser", "111111", 6000);
            //long a = FTPHelper.GetFileSize(ftp, @"/beishida/20190421.dat");
            //FileInfo file = new FileInfo(@"D:\C#\WirelessObservation\WirelessObservation\bin\Debug\source\20190421.dat");
            //long b = file.Length;
            //List<string> a = FTPHelper.ListFiles(ftp, "beishida");
            //foreach (string str in a)
            //{
            //    FTPHelper.Download(ftp, "/beishida/" + str, App.Setting.Data.DataPath, str);
            //}
            //Console.WriteLine(new FileInfo(@"E:\weatherdata-GFP1041J001-20190423AM.txt").Length);
            //Console.WriteLine(a);
        }

        /// <summary>
        /// 设置数据文件存储位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDataPosBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isGather == true)
            {
                MessageBox.Show("采集数据时无法更改数据文件存放位置", "提示");
                return;
            }
            // 调起文件夹选择窗口
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,                          //设置为选择文件夹
                InitialDirectory = App.Setting.Data.DataPath    // 初始化打开文件夹
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            
            App.Setting.Data.DataPath = dialog.FileName;
            StartGather();
            browser.GetBrowser().Reload();
        }

        /// <summary>
        /// 设置数据存储路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetStorePosBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isGather == true)
            {
                MessageBox.Show("采集数据时无法更改数据文件存放位置", "提示");
                return;
            }

            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,                          // 设置为选择文件夹
                InitialDirectory = App.Setting.Data.StorePath   // 设置初始化文件夹
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            string storePath = dialog.FileName;
            SettingHelper.SetOutPutFilePos(storePath);
        }

        /// <summary>
        /// 重置数据按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetData_Click(object sender, RoutedEventArgs e)
        {
            if (IsGather)
            {
                MessageBox.Show("采集数据中，无法更改数据文件存放位置", "提示");
                return;
            }
            MessageBoxResult mbr = MessageBox.Show("是否确定重置已采集到的数据？\r\n已采集到的数据有可能丢失！", "请确认！", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                SettingHelper.ResetData();
                // 重置数据完成，提示弹窗
                MessageBox.Show("数据已重置请重新采集数据", "提示");
            }
        }

        /// <summary>
        /// 关闭程序时的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsGather)
            {
                MessageBoxResult a = MessageBox.Show("程序正在采集中，是否结束采集并退出程序", "警告", MessageBoxButton.YesNoCancel);
                if (a == MessageBoxResult.Yes)
                {
                    controller.CloseSerialPort();
                    XmlHelper.SerializeToXml(App.SettingPath, App.Setting);

                    string jsonFile = App.Setting.Data.StorePath + "\\" + DateFormat(DateTime.UtcNow,"yyyyMMdd.json");
                    // json字符串
                    string jsonData = JsonConvert.SerializeObject(ChartData);
                    // 避免因为文件存在导致的冲突
                    if (File.Exists(jsonFile)) File.Delete(jsonFile);
                    // 以生成文件的方式写数据

                    StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8);
                    // 将数据写入
                    sw.WriteLine(jsonData);
                    sw.Close();
                    Environment.Exit(0);
                    //System.Diagnostics.Process tt = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
                    //tt.Kill();
                }
                else
                {
                    e.Cancel = true;
                    Console.WriteLine(e.ToString());
                    
                }
                
            }
            else
            {
                XmlHelper.SerializeToXml(App.SettingPath, App.Setting);
                string jsonFile = App.Setting.Data.StorePath + "\\" + DateFormat(DateTime.UtcNow, "yyyyMMdd.json");
                // json字符串
                string jsonData = JsonConvert.SerializeObject(ChartData);
                // 避免因为文件存在导致的冲突
                if (File.Exists(jsonFile)) File.Delete(jsonFile);
                // 以生成文件的方式写数据

                StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8);
                // 将数据写入
                sw.WriteLine(jsonData);
                sw.Close();
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// 定时器回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (App.Setting.Systemd.Download)
            {
                SyncFiles();
            }
            
            // 当前时间为与数据监听间隔吻合并且串口数据原型返回完整数据
            DateTime lastWriteTime = App.Setting.Systemd.LastModify;
            string recentlyFile = App.Setting.Systemd.RecentlyFile;
            long fileOffest = App.Setting.Systemd.FileOffest;
            DateTime now = DateTime.UtcNow;
            DateTime nowMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            string datFile = App.Setting.Data.DataPath + string.Format("\\{0:yyyyMMdd}.dat", now);
            if (File.Exists(datFile))
            {
                FileInfo fi = new FileInfo(datFile);
                DateTime Ooclock = now.Date;
                SetTitle(now);
                // 每天零时将数据清空并缓存数据至本地json文件待调用

                if (now.Equals(Ooclock) && ChartData.Count > 0)
                {
                    string jsonFile = App.Setting.Data.StorePath + "\\" + fi.Name + ".json";
                    // json字符串
                    string jsonData = JsonConvert.SerializeObject(ChartData);
                    // 避免因为文件存在导致的冲突
                    if (File.Exists(jsonFile)) File.Delete(jsonFile);
                    // 以生成文件的方式写数据

                    StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8);
                    try
                    {
                        sw.WriteLine(jsonData);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(ex);
                    }
                    finally
                    {
                        sw.Close();
                    }
                    // 将数据写入
                    
                   
                    ChartData.Clear();
                    App.Setting.Systemd.FileOffest = 0;
                    fileOffest = 0;
                }


                // 当前时间为与数据监听间隔吻合并且串口数据原型返回完整数据
                if (fi.Length > fileOffest)
                {

                    // 创建雷达数据操作对象
                    Model.WindProfileRadar radar = new Model.WindProfileRadar(fi.FullName, App.Setting.Systemd.FileOffest);
                    // 接收是否到达文件结尾
                    bool eof = false;

                    while (!eof)
                    {
                        List<WindProfileRadarEntity> t = radar.GetSectionData(out eof);
                        if (t != null)
                        {
                            // 将数据合并至ChartData数组
                            ChartData = ChartData.Union(t).ToList();
                        }
                    }
                    radar.SR.Close();
                    lastWriteTime = fi.LastWriteTimeUtc;
                    recentlyFile = DateFormat(lastWriteTime, "yyyyMMdd");
                    fileOffest = fi.Length;

                    // 读取本次需读取的雷达数据
                    browser.ExecuteScriptAsync("SetChart()");
                }
                else if (!fi.Exists)
                {
                    lastWriteTime = nowMinute;
                    recentlyFile = DateFormat(lastWriteTime, "yyyyMMdd");
                    fileOffest = 0;
                }

                App.Setting.Systemd.LastModify = lastWriteTime;
                App.Setting.Systemd.RecentlyFile = recentlyFile;
                App.Setting.Systemd.FileOffest = fileOffest;
            }
            
            if (IntervalCount > 8640)
            {
                // 解决长时间运行时计时器出现异常的状况
                DispatcherTimer.Stop();
                DispatcherTimer.Tick -= new EventHandler(DispatcherTimer_Tick);


                DispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                DispatcherTimer.Start();
                IntervalCount = 0;
            }
            IntervalCount++;
        }

        #endregion

        #region 应用待调用方法

        /// <summary>
        /// 打开串口监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenComEvent(Object sender, SerialPortEventArgs e)
        {
            //if (this.CheckAccess())
            //{
            //    //this.Dispatcher.Invoke(new Action<Object, SerialPortEventArgs>(OpenComEvent), sender, e);
            //    return;
            //}


            if (e.isOpend)  //Open successfully
            {
                GatherCB.Content = "结束采集";      // 变更按钮显示
                                                // 现在时间
                //startTime = DateTime.Now;
                IsGather = true;
                StartGather();
                browser.ExecuteScriptAsync("SetChart()");
            }
            else    //Open failed
            {
                MessageBoxResult dr = MessageBox.Show("未检测到相应串口数据，是否直接采集雷达数据", "请确认", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    StartGather();
                    browser.ExecuteScriptAsync("SetChart()");
                    DispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                    DispatcherTimer.Start();
                    GatherCB.Content = "结束采集";
                }
                else
                {
                    GatherCB.IsChecked = false;
                }
                //GatherCB.IsChecked = false;
               
            }
            
        }

        /// <summary>
        /// 关闭串口监听事件
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
            try
            {
                if (!e.isOpend) //close successfully
                {
                    
                    isGather = false;
                    this.GatherCB.Dispatcher.Invoke(
                                new Action(
                                    delegate
                                    {
                                        this.GatherCB.Content = "开始采集";
                                    }

                                    ));
                    //GatherCB.Content = "开始采集";

                }
            }
            catch(Exception ex)
            {
                
            }
            
        }
       
        /// <summary>
        /// 
        /// </summary>
        private void StartGather()
        {
            try
            {
                IsHistory = false;
                if (App.Setting.Systemd.Download)
                {
                    SyncFiles();
                }
                string mod = "";
                // 格式化最后数据转化日期
                if (string.IsNullOrEmpty(App.Setting.Systemd.RecentlyFile))
                {
                    mod = "19700101";
                }
                else
                {
                    Regex datePartten = new Regex(@"^(\d{4})(\d{2})(\d{2})$");
                    mod = datePartten.Match(App.Setting.Systemd.RecentlyFile).Success ? App.Setting.Systemd.RecentlyFile : "19700101";
                   
                }
                
                //DateTime last = DateTime.ParseExact(mod, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                DateTime last = DateTime.ParseExact(mod, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                if (last.CompareTo(DateTime.UtcNow) > 0) throw new Exception("time out of range");
                DateTime now = DateTime.UtcNow;
                SetTitle(now);
                // 设置表格时间轴
                SetTimeAxis();
                
                // 符合规则的正则表达式
                Regex reg = new Regex(@"^(\d{4})(\d{2})(\d{2})\.dat$");

                // 获取指定文件夹下以last起始至现在所有的文件
                var query = (from f in Directory.GetFiles(App.Setting.Data.DataPath, "*.dat")
                             let fi = new FileInfo(f)
                             orderby fi.LastWriteTime ascending
                             where fi.Name.CompareTo(mod + ".dat") >= 0 && fi.Name.CompareTo(DateFormat(now, "yyyyMMdd") + ".dat") <= 0
                             select fi.FullName);

                string[] flies = query.ToArray();
                string lastFile = string.Empty;
                for (int i = 0; i < flies.Length; i++)
                {
                    string filestr = flies[i];
                    // filename.ext
                    string filename = System.IO.Path.GetFileName(filestr);
                    // 清空数据集中数据，待本次填充
                    if (ChartData != null) ChartData.Clear();
                    // 匹配成功将文件移动至缓存文件夹
                    if (reg.Match(filename).Success)
                    {
                        lastFile = App.Setting.Data.DataPath + "\\" + filename;
                        string jsonFile = App.Setting.Data.StorePath + "\\" + System.IO.Path.GetFileNameWithoutExtension(filestr) + ".json";
                        
                        // 雷达数据文件存在
                        if (File.Exists(lastFile))
                        {
                            // 创建雷达数据操作对象
                            Model.WindProfileRadar radar = new Model.WindProfileRadar(lastFile, 0);
                            // 接收是否到达文件结尾
                            bool eof = false;

                            int j = 0;
                            while (!eof)
                            {
                                List<WindProfileRadarEntity> t = radar.GetSectionData(out eof);
                                if (t != null)
                                {
                                    // 将数据合并至ChartData数组
                                    ChartData = ChartData.Union(t).ToList();
                                    j++;
                                }
                            }
                            radar.SR.Close();
                        }
                        if (File.Exists(jsonFile))
                        {
                            StreamReader sr = new StreamReader(jsonFile, Encoding.UTF8);
                            try
                            {
                                string jsonString = sr.ReadToEnd();
                                List<WindProfileRadarEntity> jsonData = JsonConvert.DeserializeObject<List<WindProfileRadarEntity>>(jsonString);
                                List<WindProfileRadarEntity> temp = jsonData.FindAll((WindProfileRadarEntity entity) => entity.Alt == 0);
                                ChartData = ChartData.Union(temp).ToList();
                            }
                            catch(Exception ex)
                            {
                                LogHelper.WriteLog(ex);
                            }
                            finally
                            {
                                sr.Close();
                            }
                           
                            
                        }
                        if (ChartData.Count > 0)
                        {
                            string jsonData = JsonConvert.SerializeObject(ChartData);
                            // 避免因为文件存在导致的冲突
                            if (File.Exists(jsonFile)) File.Delete(jsonFile);
                            // 以生成文件的方式写数据

                            StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8);
                            try
                            {
                                sw.WriteLine(jsonData);

                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteLog(ex);
                            }
                            finally
                            {
                                sw.Close();

                            }
                        }

                    }
                }
                // ********************************************
                // *在导入数据后将最后更改的数据信息写入配置中*
                // *待下次继续拉取数据                        *
                // ********************************************
                if (lastFile != string.Empty)
                {
                    // 获取最后拉取数据的时间
                    FileInfo info = new FileInfo(lastFile);
                    // 获取最后拉取文件的时间
                    DateTime dt = info.LastWriteTime;
                    SetTitle(dt);
                    // 设置配置
                    App.Setting.Systemd.LastModify = dt;
                    App.Setting.Systemd.RecentlyFile = DateFormat(dt, "yyyyMMdd");
                    App.Setting.Systemd.FileOffest = info.Length;
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            
        }

        /// <summary>
        /// 串口接收数据的监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            // 线程监听鉴权
            if (this.CheckAccess())
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
            try
            {
                if (App.Setting.Systemd.Download)
                {
                    SyncFiles();
                }
                // 接收串口数据
                string recived = Encoding.Default.GetString(e.receivedBytes);
                // 追加至数据原型中
                if (!string.IsNullOrEmpty(recived)) Prototype += recived;

                DateTime lastWriteTime = App.Setting.Systemd.LastModify;
                string recentlyFile = App.Setting.Systemd.RecentlyFile;
                long fileOffest = App.Setting.Systemd.FileOffest;
                // 现在时间
                DateTime now = DateTime.UtcNow; 
                DateTime nowMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute,0);
                FileInfo fi = new FileInfo(App.Setting.Data.DataPath + "\\" + string.Format("{0:yyyyMMdd}.dat", now));
                DateTime Ooclock = now.Date;
                SetTitle(now);

                // 每天零时将数据清空并缓存数据至本地json文件待调用

                if (now.Equals(Ooclock) && ChartData.Count > 0)
                {
                    string jsonFile = App.Setting.Data.StorePath + "\\" + fi.Name + ".json";
                    // json字符串
                    string jsonData = JsonConvert.SerializeObject(ChartData);
                    // 避免因为文件存在导致的冲突
                    if (File.Exists(jsonFile)) File.Delete(jsonFile);
                    // 以生成文件的方式写数据

                    StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8);

                    try
                    {
                        // 将数据写入
                        sw.WriteLine(jsonData);
                    }
                    catch(Exception  ex)
                    {
                        LogHelper.WriteLog(ex);
                    }
                    finally
                    {
                        sw.Close();
                    }

                    ChartData.Clear();
                    App.Setting.Systemd.FileOffest = 0;
                    fileOffest = 0;
                }
                List<string> split = new List<string>();
                if (Prototype.IndexOfAny(new char[] { '\r', '\n' }) >= 0)
                {
                    // 数据不以$开始时，数据不包含,时丢弃数据
                    if (Prototype.IndexOf("$") != 0 || !Prototype.Contains(","))
                    {
                        throw new Exception("");
                    }
                    // 获取被截断的数据
                    split = Prototype.Split(new char[] { ',', '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    // 正确的数据应该分割出来是5段字符串数组
                    if (split.Count != 5)
                    {
                        throw new Exception("");
                    }
                    ////int lowestAlt = (int)ChartData.Min(X => X.Alt);
                    //bool chartResult = !ChartData.Exists((WindProfileRadarEntity entity) => entity.Alt == 0 && entity.TimeStamp.Equals(dataTime));

                    //// 截取出有用数据断
                    //split = split.Skip(2).Take(2).ToList();

                    //if (double.TryParse(split[0], out double ws)
                    //        && int.TryParse(split[1], out int wd)
                    //        && chartResult)
                    //{
                    //    ChartData.Add(new WindProfileRadarEntity(0, (ws * 100), wd, dataTime));
                    //}
                    //////////////////////数据可用性检查通过///////////////////////////
                }

                // 当前时间为与数据监听间隔吻合并且串口数据原型返回完整数据
                if (fi.Exists && fi.Length > fileOffest)
                {
                    DateTime dataTime = new DateTime();


                    // 雷达文件存在并且当前数据长度大于上次采集

                    // 创建雷达数据操作对象
                    Model.WindProfileRadar radar = new Model.WindProfileRadar(fi.FullName, App.Setting.Systemd.FileOffest);
                    // 接收是否到达文件结尾
                    bool eof = false;

                    while (!eof)
                    {
                        List<WindProfileRadarEntity> t = radar.GetSectionData(out eof);
                        dataTime = t[0].TimeStamp;
                        if (t != null)
                        {
                            // 将数据合并至ChartData数组
                            ChartData = ChartData.Union(t).ToList();
                        }
                    }
                    radar.SR.Close();
                    // 检查数据集中是否有高度为0时间与当前PA-XS数据相同的数据没有true 有false
                    bool chartResult = !ChartData.Exists((WindProfileRadarEntity entity) => entity.Alt == 0 && entity.TimeStamp.Equals(dataTime));

                    // 截取出有用数据断
                    split = split.Skip(2).Take(2).ToList();

                    if (double.TryParse(split[0], out double ws)
                            && int.TryParse(split[1], out int wd)
                            && chartResult)
                    {
                        ChartData.Add(new WindProfileRadarEntity(0, (ws * 100), wd, dataTime));
                        Prototype = string.Empty;
                    }
                    lastWriteTime = fi.LastWriteTimeUtc;
                    recentlyFile = DateFormat(lastWriteTime, "yyyyMMdd");
                    fileOffest = fi.Length;
                }
                else if (!fi.Exists)
                {
                    lastWriteTime = nowMinute;
                    recentlyFile = DateFormat(lastWriteTime, "yyyyMMdd");
                    fileOffest = 0;
                }
                
                    // 读取本次需读取的雷达数据
                    browser.ExecuteScriptAsync("SetChart()");
                
                App.Setting.Systemd.LastModify = lastWriteTime;
                App.Setting.Systemd.RecentlyFile = recentlyFile;
                App.Setting.Systemd.FileOffest = fileOffest;
            }
            catch(Exception exc)
            {
                Prototype = string.Empty;
                LogHelper.WriteLog(exc);
                return;
            }
        }

        /// <summary>
        /// 设置表格时间轴项目数组
        /// </summary>
        public static void SetTimeAxis(int timeRange = 144)
        {
            List<String> temporary = new List<string>();
            if (timeRange > 144)
            {
                DateTime Ooclock = StartTime.Date;
                DateTime tomorrow = EndTime.Date.AddDays(1);
                while (Ooclock.CompareTo(tomorrow) < 0)
                {
                    temporary.Add(DateFormat(Ooclock, "dd日 HH:mm"));
                    int interval = (int)App.Setting.Collect.Interval;
                    Ooclock = Ooclock.AddSeconds(interval);
                }
            }
            else
            {
                DateTime Ooclock = DateTime.UtcNow.Date;
                DateTime tomorrow = Ooclock.AddDays(1);
                while (Ooclock.CompareTo(tomorrow) < 0)
                {
                    temporary.Add(DateFormat(Ooclock, "HH:mm"));
                    int interval = (int)App.Setting.Collect.Interval;
                    Ooclock = Ooclock.AddSeconds(interval);
                }
            }
            
            
            timeAxis = temporary.ToArray();
        }

        /// <summary>
        /// 设置表格标题时间
        /// </summary>
        public static void SetTitle(DateTime time)
        {
            StartTime = time.Date;
        }

        

        /// <summary>
        /// 格式化日期字符串
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DateFormat(DateTime time, string format = "yyyy-MM-dd HH：mm：ss")
        {
            return time.ToString(format);
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

        #region SyncFiles
        /// <summary>
        /// 同步服务器及本地文件
        /// </summary>
        /// <returns></returns>
        public int SyncFiles()
        {
            long offset = App.Setting.Systemd.FileOffest;
            string recently = App.Setting.Systemd.RecentlyFile;
            string dataPath = App.Setting.Data.DataPath;
            string host = App.Setting.Systemd.Hostname;
            string name = App.Setting.Systemd.Username;
            string pass = App.Setting.Systemd.Password;
            string remotePath = App.Setting.Systemd.RemotePath;
            int count = 0;
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(pass)) return 0;
            SFTPHelper sftp = new SFTPHelper(host, name, pass);
            sftp.Connect();
            try
            {
                if (sftp.Connected)
                {
                    
                    if (string.IsNullOrEmpty(recently))
                    {
                        var filelist = sftp.GetFileList(remotePath, "dat");
                        foreach (string file in filelist)
                        {
                            sftp.Get(remotePath + "/" + file, dataPath + "\\" + file);
                            count++;
                        }
                        
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(recently) || recently.CompareTo("20190101") < 0) return 0;
                        IFormatProvider ifp = new System.Globalization.CultureInfo("zh-CN", true);
                        ;
                        if (DateTime.TryParseExact(recently, "yyyyMMdd", ifp, System.Globalization.DateTimeStyles.None, out DateTime recentTime))
                        {
                            
                            var Ftpfiles = sftp.GetFileList(remotePath, "dat");
                            while (recentTime.CompareTo(DateTime.UtcNow.Date) <= 0)
                            {
                                string file = DateFormat(recentTime, "yyyyMMdd") + ".dat";

                                string res = Ftpfiles.Find(s => s.Equals(file));
                                if (string.IsNullOrEmpty(res))
                                {
                                    recentTime = recentTime.AddDays(1);
                                }

                                long ftpfile = sftp.GetFileSize(remotePath + "/" + file);
                                
                                FileInfo fi = new FileInfo(dataPath + "\\" + file);
                                if (!fi.Exists || offset < ftpfile)
                                {
                                    sftp.Get(remotePath + "/" + file, dataPath + "\\" + file);
                                }
                                count++;
                                recentTime = recentTime.AddDays(1);
                            }
                        }


                    }
                }
                
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            finally
            {
                sftp.Disconnect();
            }

            
            return count;

        }
        #endregion

        #endregion



    }

    public class JsEvent
    {
        private string messageText = "111";
        private List<String> xSeries = new List<string>();
        private DateTime title = new DateTime();
        List<List<List<string>>> res = new List<List<List<string>>>();
        
        public void ShowTest(string aaa)
        {
            MessageBox.Show("this in c#.\n\r" + MessageText);
        }
        public void ShowTestArg(string ss)
        {
            MessageBox.Show("收到Js参数的调用\n\r" + ss);
        }
        public List<string> GetXSeries()
        {
            return XSeries;
        }

        public String GetTitle()
        {
            string title = string.Empty;
            if (MainWindow.IsHistory)
            {
                title = MainWindow.StartTime.ToLongDateString().ToString() + "至" + MainWindow.EndTime.ToLongDateString().ToString();
            }
            else
            {
                title = MainWindow.StartTime.ToLongDateString().ToString();
            }
            return title;
        }

        
        public string MessageText { get => messageText; set => messageText = value; }
        public List<string> XSeries { get => xSeries; set => xSeries = value; }
        public DateTime Title { get => title; set => title = value; }

        public List<List<List<string>>> GetData()
        {
            
            XSeries = MainWindow.TimeAxis.ToList();
            int[] threshold = new int[] { 0, 1, 2, 4, 6, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 60, 999 };
            
            List<List<List<string>>> res = new List<List<List<string>>>();
            try
            {
                
                for (int x = 0; x < threshold.Length - 1; x++)
                {
                    res.Add(new List<List<string>>());
                }

                foreach (WindProfileRadarEntity wpre in MainWindow.ChartData)
                {
                    int index = 0;
                    for (int j = 0; j < threshold.Length; j++)
                    {
                        if ((wpre.Speed / 100) <= threshold[j])
                        {
                            index = j - 1;
                            break;
                        }
                    }
                    string speedRange = string.Format("{0:D}-{1:D}", threshold[index], threshold[index + 1]);
                    string timeStamp = MainWindow.DateFormat(wpre.TimeStamp, "HH时mm分");
                    string dir = WindDirection.GetFormatDir(wpre.Direction);
                    DateTime temp = MainWindow.ChartData.Min(s => s.TimeStamp).Date;

                    // [x,y,dir,speedRange]
                    // 当前数据有相同风速数据进入数组
                    DateTime Ooclock = temp;
                    TimeSpan ts = wpre.TimeStamp - Ooclock;

                    res[index].Add(new List<string> { (ts.TotalSeconds / App.Setting.Collect.Interval).ToString(), wpre.Alt.ToString(),
                            wpre.Direction.ToString(), speedRange,
                            (wpre.Speed / 100).ToString(), MainWindow.DateFormat(wpre.TimeStamp, "HH时mm分"),
                            dir
                        });
                }


            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(ex);
            }
            return res;
        }
    }
}
