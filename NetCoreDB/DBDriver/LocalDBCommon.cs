using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreDBModule.NetCoreDB.DBDriver
{
  public  class LocalDBCommon
    {
        #region  把字符串变成'xxxxx','yyyyyy'
        public static string StringToList(string aa)
        {
            string bb1 = "(";
            if (!string.IsNullOrEmpty(aa.Trim()))
            {

                string[] bb = aa.Split(new string[] { "\r\n", ",", ";", "* " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < bb.Length; i++)
                {
                    if (!bb1.Contains(bb[i]))//去掉重复的輸入值
                    {
                        bb1 += "'" + bb[i] + "',";
                    }
                }
            }
            bb1 = bb1.Substring(0, bb1.LastIndexOf(",")) + ")";
            return bb1;
        }
        #endregion
    }
}
