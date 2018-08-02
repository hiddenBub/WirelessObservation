using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WirelessObservation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
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

        #endregion



        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
