using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WirelessObservation
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 系统变量

        /*路径设置*/
        /// <summary>
        /// 程序数据路径
        /// </summary>
        public static string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TopFlagTec\\SolarCalibration";

        /// <summary>
        /// 文档路径
        /// </summary>
        public static string DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Solar Calibration";

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string SettingPath = ProgramData + "\\Setting.xml";

        /// <summary>
        /// 采集指针路径
        /// </summary>
        public static string GatherPath = ProgramData + "\\Gather.txt";

        /// <summary>
        /// 数据存储路径
        /// </summary>
        public static string DataStoragePath = DocumentPath + "\\DataStorage";

        /*系统设置*/
        public static string Setting = Vendor.XmlHelper.DeserializeFromXml<>(SettingPath);

        #endregion

    }
}
