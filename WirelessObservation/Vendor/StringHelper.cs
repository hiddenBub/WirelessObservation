using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Vendor
{
    class StringHelper
    {
        /// <summary>
        /// 将指定类型的数据添加至数组中，
        /// </summary>
        /// <typeparam name="T">数组中指定的类型</typeparam>
        /// <param name="variable">数组变量</param>
        /// <param name="item">数组中需要更改的数据</param>
        /// /// <param name="index">数组中需要更改的索引</param>
        /// <returns></returns>
        public static List<T> AddItem<T>(List<T> variable, T item, int index)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > index)
            {
                // 更改该项目
                variable[index] = item;
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(item);
            }
            // 将数组返回
            return variable;
        }

        public static List<T> AddItem<T>(List<T> variable, T item)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > 0)
            {
                // 更改该项目
                variable[0] = item;
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(item);
            }
            // 将数组返回
            return variable;
        }

        public static List<List<T>> AddSonItem<T>(List<List<T>> variable, T sonItem, int index)
        {
            /*判断数组的长度*/
            // 如果数组长度大于需要更改的索引值，则说明该索引存在
            if (variable.Count > index)
            {
                // 更改该项目
                variable[index].Add(sonItem);
            }
            // 小于则说明该索引不存在
            else
            {
                // 添加该索引至数组
                variable.Add(new List<T> { sonItem });
            }
            // 将数组返回
            return variable;
        }
    }
}
