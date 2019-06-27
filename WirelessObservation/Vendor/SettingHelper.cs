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
        /// <summary>
        /// 设置输出json文件存储位置
        /// </summary>
        /// <param name="storePath"></param>
        static public void SetOutPutFilePos(string storePath)
        {
            Regex reg = new Regex(@"^(\d{4})(\d{2})(\d{2})\.json$");
            var files = (from f in Directory.GetFiles(App.Setting.Data.StorePath, "*.json")
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
            App.Setting.Data.StorePath = storePath;
        }


        static public void ResetData()
        {
            // 获取指定文件夹下以last起始至现在所有的文件
            var query = (from f in Directory.GetFiles(App.Setting.Data.DataPath, "*.json")
                         let fi = new FileInfo(f)
                         select fi.FullName);

            //string[] flies = query.ToArray();
            foreach (String Flie in query.ToArray())
            {
                File.Delete(Flie);
            }

            Setting setting = new Setting
            {
                Collect = new Collect
                {
                    Interval = 600,

                },
                Systemd = new Systemd
                {
                    RecentlyFile = "",
                    FileOffest = 0,
                    LastModify = new DateTime(),
                }
            };
            // 更新程序内使用的配置实体
            App.Setting.Collect.Interval = 600;
            App.Setting.Systemd.RecentlyFile = "";
            App.Setting.Systemd.LastModify = new DateTime();
            App.Setting.Systemd.FileOffest = 0;
        }
    }
}
