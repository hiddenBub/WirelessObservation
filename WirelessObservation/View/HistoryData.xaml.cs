using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WirelessObservation.View;
using WirelessObservation;
using Loya.Dameer;

namespace WirelessObservation
{
    /// <summary>
    /// HistoryData.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryData : Window
    {

        private DateTime start = new DateTime();
        private DateTime end = new DateTime();
        private string[] dataList;

        public DateTime Start { get => start; set => start = value; }
        public DateTime End { get => end; set => end = value; }
        public string[] DataList { get => dataList; set => dataList = value; }
        public HistoryData()
        {
            InitializeComponent();

            try
            {
            /*****************************时间轴起始显示事件***************************/
                // 注册失去焦点事件
                StartTime.LostFocus += new RoutedEventHandler (TimeBar_LostFocus);
                // 注册按键抬起事件
                StartTime.PreviewKeyUp += new KeyEventHandler(TimeBar_KeyUp);
            /************************************************************************/
            /*****************************时间轴结束显示事件***************************/
                // 注册失去焦点事件
                EndTime.LostFocus += new RoutedEventHandler(TimeBar_LostFocus);
                // 注册按键抬起事件
                EndTime.PreviewKeyUp += new KeyEventHandler(TimeBar_KeyUp);
                /************************************************************************/
                string file = (from f in Directory.GetFiles(App.Setting.Data.StorePath, "*.json")
                                      let fi = new FileInfo(f)
                                      select fi.Name).Min();
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                //DateTime earlist = (from f in Directory.GetFiles(App.Setting.Data.StorePath, "*.json")
                //let fi = new FileInfo(f)
                //select fi.LastWriteTime).Min();
                DateTime earlist = DateTime.ParseExact(fileName, "yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en"));
                //earlist = earlist.Date;
                StartTime.Value = earlist.Date;
                EndTime.Value = DateTime.Now.Date;
                //FileList.DataSource = GetDataFiles(earlist, DateTime.Now).ToList();
                //string dataFile = App.DataStoragePath + "\\source.dat" ;
                //if (File.Exists(dataFile))
                //{
                //    //StartTime.TextInput += new TextCompositionEventHandler(StartChange);
                //    //StartTime.PreviewTextInput += new TextCompositionEventHandler(StartChange);

                //    DataList = File.ReadAllLines(dataFile, Encoding.UTF8);
                //    string[] datBody = Vendor.Customize.GetDatBody(DataList);
                //    string[] firstLine = datBody[0].Split(new char[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    Start = Convert.ToDateTime(firstLine[1]);
                //    string[] lastLine = datBody[datBody.Length - 1].Split(new char[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    End = Convert.ToDateTime(lastLine[1]);
                //    StartTime.Value = Start;
                //    EndTime.Value = End;
                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }
        }

        private string[] GetDataFiles(DateTime start, DateTime end)
        {
            string[] files = (from f in Directory.GetFiles(App.Setting.Data.StorePath, "*.json")
                              let fi = new FileInfo(f)
                              orderby fi.LastWriteTime ascending
                              where fi.LastWriteTime.CompareTo(start) >= 0 && fi.LastWriteTime.CompareTo(end) < 0
                              select fi.Name).ToArray();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^(\d{4})(\d{2})(\d{2})\.json$");
            List<string> res = new List<string>();
            foreach (string file in files)
            {
                System.Text.RegularExpressions.Match match = reg.Match(file);

                // 匹配成功将文件移动至缓存文件夹
                if (match.Success)
                {
                    res.Add(file);
                }
            }
            return res.ToArray();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (StartTime.Value != null)
                {
                    //string selected = System.IO.Path.GetFileNameWithoutExtension(FileList.SelectData);
                    

                    if (EndTime.Value != null && Convert.ToDateTime(StartTime.Value).CompareTo((DateTime)EndTime.Value) <= 0)
                    {
                        MainWindow.StartTime = (DateTime)StartTime.Value;
                        MainWindow.EndTime = (DateTime)EndTime.Value;
                    }
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }

        }

        private void TimeBar_LostFocus(object sender, RoutedEventArgs e)
        {
            Dameer Dammer = (Dameer)sender;
            var source = Convert.ToDateTime(e.Source.ToString());
            ChangeTimeRange(Dammer, source);
        }

        private void TimeBar_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.Key ==Key.Enter)
            {
                Dameer Dammer = (Dameer)sender;
                var source = Convert.ToDateTime(e.Source.ToString());
                ChangeTimeRange(Dammer,source);
            }
            else if(e.Key == Key.Back)
            {

            }
        }

        private void TimeBar_Changed(object sender,RoutedEventArgs e)
        {
            var source = e.Source.ToString();
            if (source != "0001/1/1 0:00:00" && DateTime.TryParse(source,out DateTime dt) && dt.CompareTo(new DateTime(2000,1,1)) > 0 )
            {
                Dameer Dammer = (Dameer) sender;
                if (Dammer.IsLoaded)
                {
                    Dammer.Value = dt;
                    ChangeTimeRange(Dammer, dt);
                }
                
            }
        }

        /// <summary>
        /// 更改文件时间筛选范围
        /// </summary>
        private void ChangeTimeRange(Dameer control,DateTime time)
        {
            if (time.CompareTo(Start) > 0 && time.CompareTo(DateTime.Now) <= 0)
            {
                control.Value = time;
                List<string> dataSet =  GetDataFiles(Convert.ToDateTime(StartTime.Value), Convert.ToDateTime(EndTime.Value)).ToList();
                //FileList.Reload(dataSet);
            }
        }

        /// <summary>
        /// 点击取消按钮的逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        
        

        
    }
}
