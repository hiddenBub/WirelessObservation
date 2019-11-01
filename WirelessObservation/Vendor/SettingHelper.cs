using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WirelessObservation;
using WirelessObservation.Entity;

namespace WirelessObservation.Vendor
{
    public class SettingHelper
    {
        public static Setting setting = new Setting();
        /// <summary>
        /// 设置输出json文件存储位置
        /// </summary>
        /// <param name="storePath"></param>
        static public void SetOutPutFilePos(string storePath)
        {
            Regex reg = new Regex(@"^(\d{4})(\d{2})(\d{2})\.json$");
            var files = (from f in Directory.GetFiles(setting.Files.StorePath, "*.json")
                         let fi = new FileInfo(f)
                         select fi.FullName).ToArray();
            foreach (string file in files)
            {
                string shortName = Path.GetFileName(file);
                Match match = reg.Match(shortName);
                if (match.Success)
                {
                    try
                    {
                        // 避免冲突删除源文件
                        if (File.Exists(storePath + "\\" + shortName)) File.Delete(storePath + "\\" + shortName);
                        File.Move(file, storePath + "\\" + shortName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }

            }
            setting.Files.StorePath = storePath;
        }


        static public void ResetData()
        {
            // 获取指定文件夹下以last起始至现在所有的文件
            var query = (from f in Directory.GetFiles(setting.Files.StorePath, "*.json")
                         let fi = new FileInfo(f)
                         select fi.FullName);

            //string[] flies = query.ToArray();
            foreach (String Flie in query.ToArray())
            {
                File.Delete(Flie);
            }

            setting = new Setting();
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        public static void Init()
        {
            if (File.Exists(App.SettingPath))
            {
                setting = XmlHelper.DeserializeFromXml<Setting>(App.SettingPath);
            }
            else
            {
                Save();
            }

        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save()
        {
            if (setting.Collect == null)
            {

                Collect collect = new Collect();
                //Data data = new Data();
                Systemd systemd = new Systemd();
                Files files = new Files();
                setting.Collect = collect;
                setting.Files = files;
                //setting.Data = data;
                setting.Systemd = systemd;
            }

            XmlHelper.SerializeToXml(App.SettingPath, setting);
            
        }

    }
}
