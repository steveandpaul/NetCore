using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver
{
  public  class GetDBTYype
    {
        private static string con_type = null;
        private static object obj = new object();
        ///
        /// 定义公共静态属性instance，外部调用
        ///
        public static string instance
        {
            get
            {
                // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
                // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
                // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
                // 双重锁定只需要一句判断就可以了

                if (con_type == null)
                {
                    lock (obj)
                    {
                        if (con_type == null)
                        {
                            //读取一级配置节点配置 AppConfigurtaionServices.Configuration["ServiceUrl"];
                            //读取二级子节点配置  AppConfigurtaionServices.Configuration["Appsettings:SystemName"];
                            // con_type = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SQL_TYPE").Value;



                            con_type = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SQL_TYPE").Value;//获取配置文件中的数据库连接字符串
                           
                        }
                    }
                }
                return con_type;
            }

        }

    }
}
