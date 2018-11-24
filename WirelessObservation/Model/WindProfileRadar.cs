using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirelessObservation.Entity;
using WirelessObservation.Vendor;

namespace WirelessObservation.Model
{
    public class WindProfileRadar
    {

        #region 字段
        public StreamReader SR;
        #endregion

        /// <summary>
        /// 重载构造方法
        /// </summary>
        /// <param name="path"></param>
        public WindProfileRadar(string path, long offset)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            SR = new StreamReader(fs);
            FileInfo fileInfo = new FileInfo(path);
            long size = fileInfo.Length;
            //使用StreamReader类来读取文件 
            SR.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public List<WindProfileRadarEntity> GetSectionData(out bool isEOF)
        {
            isEOF = false;
            List<WindProfileRadarEntity> echarts = new List<WindProfileRadarEntity> ();
            List<List<string>> s = new List<List<string>>();
            string partten = @"(GPS\sLAT|GPS\sLONG|T\sIN|T\sOUT)\s+|\s+";
            int i = 0;
            while (!SR.EndOfStream)
            {
                string strLine = SR.ReadLine();
                if (!string.IsNullOrEmpty(strLine))
                {
                    if (strLine == "$") break;
                    string[] lineArray = System.Text.RegularExpressions.Regex.Split(strLine, partten, System.Text.RegularExpressions.RegexOptions.Singleline);
                    foreach (string item in lineArray)
                    {
                        if (!string.IsNullOrEmpty(item)) s = StringHelper.AddSonItem(s, item, i);
                    }
                    i++;
                }
            }
            if (SR.EndOfStream) isEOF = true;
            DateTime timeStamp = new DateTime(Convert.ToInt32(s[3][3]), Convert.ToInt32(s[3][1]), Convert.ToInt32(s[3][2]), Convert.ToInt32(s[3][4]), Convert.ToInt32(s[3][5]), 0);
            List<List<string>> data = s.Skip(9).ToList();
            int j;
            foreach (List<string> d in data)
            {
                if (d[2] == "-9999") continue;
                WindProfileRadarEntity e = new WindProfileRadarEntity(Convert.ToDouble(d[0]), Convert.ToDouble(d[2]), Convert.ToDouble(d[3]),timeStamp);
                echarts.Add(e);
            }
            if (echarts.Count == 0) echarts = null;
            return echarts;
        }
    }
}
