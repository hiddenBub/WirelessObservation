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

namespace WirelessObservation.View
{
    /// <summary>
    /// ScrollList.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollList : UserControl
    {
        public ScrollList()
        {
            InitializeComponent();
            this.Loaded += ScrollList_Loaded;
        }

        private void ScrollList_Loaded(object sender, RoutedEventArgs e)
        {
            stacktt.Children.Clear();
            for (int index = 0; index < ShowItemCount; index++)
            {
                TextBlock text = new TextBlock() {
                    Height = ItemHeight
                };
                stacktt.Children.Add(text);
            }
            RefreshData();
        }
        public void Reload(List<string> datasource)
        {
            
            startIndex = 0;
            DataSource = datasource;
            RefreshData();
        }

        public List<string> DataSource
        {
            get { return (List<string>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(List<string>), typeof(ScrollList), new PropertyMetadata(new List<string>()));
        public string SelectData
        {
            get { return (string)GetValue(SelectDataProperty); }
            set { SetValue(SelectDataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SelectData.  This enables animation, styling, binding, etc... 
        public static readonly DependencyProperty SelectDataProperty =
            DependencyProperty.Register("SelectData", typeof(string), typeof(ScrollList), new PropertyMetadata(""));

        public int ItemHeight
        {
            get { return (int)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ItemHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(int), typeof(ScrollList), new PropertyMetadata(20));
        int ShowItemCount
        {
            get { return (int)ActualHeight / ItemHeight; }
        }

        int startIndex = 0;
        /// <summary>
        /// 鼠标滚动时控件逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tt_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine("TimeStap={0} Delta={1}", e.Timestamp, e.Delta);
            if (DataSource.Count == 0) return;
            // 滚轮上滚
            if (e.Delta > 0)
            {
                // 上滚超界
                startIndex++;
            }
            // 滚轮下滚
            else
            {
                startIndex--;
            }
            // 数据下部越界重设数据起始
            if (startIndex > DataSource.Count)
            {
                startIndex = 0;
            }
            // 数据上部越界重设数据起始
            else if (startIndex < 0)
            {
                startIndex = DataSource.Count - 1;
            }
            RefreshData();
        }

        /// <summary>
        /// 重设控件显示区域的数据
        /// </summary>
        private void RefreshData()
        {
            if (DataSource.Count > 0)
            {
                int count = 0;
                foreach (var item in stacktt.Children)
                {
                    // 根据起始位置重设控件显示区域
                    (item as TextBlock).Text = DataSource[((startIndex + count) % DataSource.Count)].ToString();
                    // 自增
                    count += 1;
                }
                TextBlock selectText = (TextBlock)VisualTreeHelper.GetChild(stacktt, ShowItemCount / 2);
                if (ShowItemCount % 2 != 0)
                {
                    selectText = (TextBlock)VisualTreeHelper.GetChild(stacktt, Convert.ToInt32(Math.Ceiling((double)(ShowItemCount / 2))));
                }
                SelectData = selectText.Text;
            }
        }
    }
}
