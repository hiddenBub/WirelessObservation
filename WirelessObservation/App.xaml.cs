using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WirelessObservation.Entity;

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
        public static Setting Setting;

        #endregion

        /// <summary>
        /// 程序起始点
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;      // 设置程序中断模式


            // 初始化文件夹
            if (!System.IO.Directory.Exists(ProgramData)) System.IO.Directory.CreateDirectory(ProgramData);
            if (!System.IO.Directory.Exists(DocumentPath)) System.IO.Directory.CreateDirectory(DocumentPath);
            if (!System.IO.Directory.Exists(DataStoragePath)) System.IO.Directory.CreateDirectory(DocumentPath);
            if (!System.IO.File.Exists(SettingPath))
            {
                Setting setting = new Setting
                {
                    Collect = new Collect
                    {
                        Input = 1,
                        Output = 1,
                    },
                };
                Vendor.XmlHelper.SerializeToXml(SettingPath, setting);
            }
            Setting = Vendor.XmlHelper.DeserializeFromXml<Setting>(SettingPath);

            if (!System.IO.File.Exists(DataStoragePath + "\\source.dat"))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(DataStoragePath + "\\source.dat", false, System.Text.Encoding.UTF8);
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

                // 关闭文件
                sw.Close();
            }
        }

    }
}
