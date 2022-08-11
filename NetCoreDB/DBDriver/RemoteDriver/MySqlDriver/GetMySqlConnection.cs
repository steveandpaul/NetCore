using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.MySqlDriver
{
  public  partial class GetMySqlConnection
    {
        private static MySqlConnection con = null;
        private static object obj = new object();
        public static string conStr { get; set; }
        ///
        /// 定义公共静态属性instance，外部调用
        ///
        public static MySqlConnection instance
        {
            get
            {
                // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
                // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
                // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
                // 双重锁定只需要一句判断就可以了
                if (con == null)
                {
                    lock (obj)
                    {
                        if (con == null)
                        {


                            if (!string.IsNullOrEmpty(conStr))
                            {
                                //var str = ConfigurationManager.ConnectionStrings["DBServer"].ConnectionString;//获取配置文件中的数据库连接字符串
                                con = new MySqlConnection(conStr);//实例化
                            }
                            else
                            {
                                var str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;//获取配置文件中的数据库连接字符串
                                con = new MySqlConnection(str);//实例化

                            }
                         
                            try
                            {
                                con.Open();
                            }
                            catch (Exception e)
                            {
                                return null;
                            }
                        }
                    }
                }
                return con;
            }

        }
    }
}

