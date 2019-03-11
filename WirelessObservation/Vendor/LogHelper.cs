using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Vendor
{
    class LogHelper
    {
        static string AppLogPath = AppDomain.CurrentDomain.BaseDirectory + "log/";

        public static void WriteLog(Exception ex,string additional="")
        {
            if (!System.IO.Directory.Exists(AppLogPath))
                System.IO.Directory.CreateDirectory(AppLogPath);
            StringBuilder sb = new StringBuilder("");
            string currentTime = System.DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
            if (ex != null)
            {
                sb.Append("\n\r");
                sb.Append(currentTime + "\n\r");
                sb.Append(ex.Message + "\n\r");
                sb.Append(ex.GetType() + "\n\r");
                sb.Append(ex.Source + "\n\r");
                sb.Append(ex.TargetSite + "\n\r");
                sb.Append(ex.StackTrace + "\n\r");
                sb.Append(additional + "\n\r");
            }
            System.IO.File.AppendAllText(AppLogPath + System.DateTime.Now.ToString("yyyy-MM-dd") + ".log", sb.ToString());
        }
    }
}
