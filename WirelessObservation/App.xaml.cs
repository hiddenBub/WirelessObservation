﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using IWshRuntimeLibrary;
using System.Reflection;
using WirelessObservation.Entity;
using CefSharp.Wpf;
using CefSharp;

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
            // 首次启动时没有配置文件初始化配置文件及快捷方式
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
                        DataPath = "",
                        StorePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        RecentlyFile = "",
                        FileOffest = 0,
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

        public App()
        {
            //Add Custom assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            //Any CefSharp references have to be in another method with NonInlining
            // attribute so the assembly rolver has time to do it's thing.
            InitializeCefSharp();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            //Perform dependency check to make sure all relevant resources are in our output directory.
            var settings = new CefSettings();
            settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                   Environment.Is64BitProcess ? "x64" : "x86",
                                                   "CefSharp.BrowserSubprocess.exe");

            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        // Required by CefSharp to load the unmanaged dependencies when running using AnyCPU
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return System.IO.File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }

    }
}
