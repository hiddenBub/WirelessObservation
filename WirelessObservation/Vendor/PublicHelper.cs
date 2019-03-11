using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Vendor
{
    public class PublicHelper
    {
        public static bool IsChineseSimple()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN";
        }

    }
}
