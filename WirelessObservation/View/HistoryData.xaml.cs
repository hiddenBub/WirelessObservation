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
                string dataFile = App.DataStoragePath + "\\source.dat" ;
                if (File.Exists(dataFile))
                {
                    //StartTime.TextInput += new TextCompositionEventHandler(StartChange);
                    //StartTime.PreviewTextInput += new TextCompositionEventHandler(StartChange);

                    DataList = File.ReadAllLines(dataFile, Encoding.UTF8);
                    string[] datBody = Vendor.Customize.GetDatBody(DataList);
                    string[] firstLine = datBody[0].Split(new char[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    Start = Convert.ToDateTime(firstLine[1]);
                    string[] lastLine = datBody[datBody.Length - 1].Split(new char[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    End = Convert.ToDateTime(lastLine[1]);
                    StartTime.Value = Start;

                    EndTime.Value = End;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (StartTime.Value != null)
                {
                    MainWindow.startTime = (DateTime)StartTime.Value;
                    
                    if (EndTime.Value != null)
                    {
                        MainWindow.endTime = (DateTime)EndTime.Value;
                    }
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
