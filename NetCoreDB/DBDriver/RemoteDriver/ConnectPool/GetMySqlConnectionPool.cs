using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool
{
   public class GetMySqlConnectionPool
    {
        private static GetMySqlConnectionPool cpool = null;//池管理对象
        private static GetMySqlConnectionPool Customer_cpool = null;//池管理对象
        private static Object objlock = typeof(GetMySqlConnectionPool);//池管理对象实例
        private int size = 1;//池中连接数
        private int useCount = 0;//已经使用的连接数
        private ArrayList pool = null;//连接保存的集合
        private  String ConnectionStr = "";//连接字符串
        public static string constr;
        MemoryCacheModule.InMemoryCache.MemoryCacheHelper memoryCacheHelper = null;

        //public GetMySqlConnectionPool()
        //{
        //    //数据库连接字符串
        //    ConnectionStr = "server=localhost;User ID=root;Password=123456;database=test;";
        //    //创建可用连接的集合
        //    pool = new ArrayList();
        //}



        #region 构造函数
        //无参
        public GetMySqlConnectionPool()
        {
            //数据库连接字符串
            String STR = ConnectionStr;
           // ConnectionStr = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
            //创建可用连接的集合
            pool = new ArrayList();
        }

        //有参--直接传入连接字符串(不友好)
        public GetMySqlConnectionPool(string Constr)
        {
            //数据库连接字符串
            ConnectionStr = Constr;
            //创建可用连接的集合
            pool = new ArrayList();
        }

        //有参--参数传入(友好)
        public GetMySqlConnectionPool(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            //数据库连接字符串  server={0};userid={1};password={2};database={3}
            string sql_constr = "server={0};port={3};userid={1};password={2};database={4};Charset=UTF8;Allow Zero Datetime=True; Pooling=true; Max Pool Size=500;sslmode=none;Allow User Variables=True";
         


            ConnectionStr = string.Format(sql_constr, ip, database_user_name, database_user_password, port, database_name);
          
            //创建可用连接的集合
            pool = new ArrayList();
        }
        #endregion 

        #region 创建获取连接池对象
        //无参数
        public static GetMySqlConnectionPool getPool()
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetMySqlConnectionPool();
                }
                return cpool;
            }
        }
        //有参
        public static GetMySqlConnectionPool getPool(string ConStr)
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetMySqlConnectionPool(ConStr);
                }
                return cpool;
            }
        }

        public static GetMySqlConnectionPool getPool(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetMySqlConnectionPool(ip, database_user_name, database_user_password, port, database_name);
                }
                string sql_constr = "server={0};port={3};userid={1};password={2};database={4};Charset=UTF8;Allow Zero Datetime=True; Pooling=true; Max Pool Size=500;sslmode=none;Allow User Variables=True";
                //constr = string.Format(sql_constr, ip, database_name, database_user_name, database_user_password);



               



                constr = string.Format(sql_constr, ip, database_user_name, database_user_password, port, database_name);

                return cpool;
            }
        }

        #endregion

        #region 获取池中的连接
        public MySqlConnection getConnection()
        {
            lock (pool)
            {
                MySqlConnection tmp = null;
                //可用连接数量大于0
                if (pool.Count > 0)
                {
                    //取第一个可用连接
                    tmp = (MySqlConnection)pool[0];
                    //在可用连接中移除此链接
                    pool.RemoveAt(0);
                    //不成功
                    if (!isUserful(tmp))
                    {
                        //可用的连接数据已去掉一个
                        useCount--;
                        tmp = getConnection();
                    }
                }
                else
                {
                    //可使用的连接小于连接数量
                    if (useCount <= size)
                    {
                        try
                        {
                            //创建连接
                            tmp = CreateConnection(tmp);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                //连接为null
                if (tmp == null)
                {
                    //达到最大连接数递归调用获取连接否则创建新连接
                    if (useCount <= size)
                    {
                        tmp = getConnection();
                    }
                    else
                    {
                        tmp = CreateConnection(tmp);
                    }
                }
                return tmp;
            }
        }
        #endregion

        #region 创建连接
        private MySqlConnection CreateConnection(MySqlConnection tmp)
        {
            //创建连接
            MySqlConnection conn = new MySqlConnection(constr);
           // {
                //bool connectSuccess = false;
               // Thread t = new Thread(delegate ()
               // {
                    //try
                    //{
                       // conn.Open();
                       // connectSuccess = true;
                   // }
                   // catch
                   // {
                   // }
               // });
               // t.Start();
                //可用的连接数加上一个
                useCount++;
                tmp = conn;
           // }
           
           
            return tmp;
        }
        #endregion

        #region 关闭连接,加连接回到池中
        public void closeConnection(MySqlConnection con)
        {
            lock (pool)
            {
                if (con != null)
                {
                    //将连接添加在连接池中
                    pool.Add(con);
                }
            }
        }
        #endregion


        #region 目的保证所创连接成功,测试池中连接
        private bool isUserful(MySqlConnection con)
        {
            //这里为了其他数据库库调用方便，采用实例化
            bool bf = true;
            memoryCacheHelper = new MemoryCacheModule.InMemoryCache.MemoryCacheHelper();
            DataBaseInfo dataBaseInfo = null;
             dataBaseInfo= memoryCacheHelper.GetCacheItem<DataBaseInfo>("mysqlDB", delegate ()
            {
                return dataBaseInfo;
            }, null, null);
            if (dataBaseInfo.IP !=con.DataSource && dataBaseInfo.DB_Name != con.Database)
            {
                bf = false;
            }



            //bool bf = true;
            //string[] array = con.ConnectionString.Split(';');
            //foreach (string str in array)
            //{
            //    if (!constr.Contains(str))
            //    {
            //        bf = false;
            //        break;
            //    }

            //}

            if (bf == false)
            {
                con.Close();
                con = null;
                con = new MySqlConnection(constr);
            }
            //主要用于不同用户
            bool result = true;
            if (con != null)
            {
                string sql = "select 1";//随便执行对数据库操作
                MySqlCommand cmd = new MySqlCommand(sql, con);
                try
                {
                    cmd.ExecuteScalar().ToString();
                    con.Close();
                    con.Dispose();
                }
                catch
                {
                    result = false;
                }

            }
            return result;
        }
        #endregion



    }
}
