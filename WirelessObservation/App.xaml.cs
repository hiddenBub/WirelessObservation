using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IWshRuntimeLibrary;
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
        public static string ProjectName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;
        /// <summary>
        /// 程序数据路径
        /// </summary>
        public static string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TopFlagTec\\" + ProjectName;

        /// <summary>
        /// 文档路径
        /// </summary>
        public static string DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TopFlagTec\\" + ProjectName;

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
            if (!Directory.Exists(ProgramData)) Directory.CreateDirectory(ProgramData);
            if (!Directory.Exists(DocumentPath)) Directory.CreateDirectory(DocumentPath);
            if (!Directory.Exists(DataStoragePath)) Directory.CreateDirectory(DataStoragePath);
            if (!System.IO.File.Exists(SettingPath))
            {

                var desktop = Vendor.Shortcut.GetDeskDir();


                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                WshShell shell = new WshShell();

                string shotcutName = "无线采集系统.lnk";
                string shortcutAddress = Path.Combine(desktopPath, shotcutName);
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "无线采集系统";
                //shortcut.Hotkey = "Ctrl+Shift+N";
                shortcut.TargetPath = AppDomain.CurrentDomain.BaseDirectory + ProjectName + ".exe";
                shortcut.Save();

                
                Setting setting = new Setting
                {
                    Collect = new Collect
                    {
                        Input = 1,
                        Output = 1,
                    },
                    Data = new Data
                    {
                        StoragePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    }
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
