using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Vendor
{
    class Customize
    {
        public enum DataType
        {
            None,
            SourceData,
            CalibrationData,
        };

        public string dateFormat = "yyyy-MM-dd HH:mm:ss";

        public static string[] GetDatHeader(string[] dataList)
        {
            return dataList.Take(2).ToArray();
        }

        public static string[] GetDatBody(string[] dataList)
        {
            return dataList.Skip(2).ToArray();
        }

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static string GetFileName(string RootPath, DataType type, DateTime startTime, DateTime? endTime)
        {
            // 生成字符串
            StringBuilder sb = new StringBuilder();

            // 依据数据类型生成文件名
            switch (type)
            {
                case DataType.SourceData:
                    sb.Append("_" + DataType.SourceData.ToString());
                    break;
                case DataType.CalibrationData:
                    sb.Append("_" + DataType.CalibrationData.ToString());
                    break;
            }
            sb.Append("_" + DateFormat(startTime));
            // 判断是否传入了结束时间
            if (endTime != null) sb.Append("_" + DateFormat((DateTime)endTime));
            sb.Append(".dat");
            sb.Insert(0, RootPath + "\\");
            // 返回文件名
            return sb.ToString();
        }

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DateFormat(DateTime time, string format = "yyyy-MM-dd HH：mm：ss")
        {
            return time.ToString(format);
        }

    }
}
