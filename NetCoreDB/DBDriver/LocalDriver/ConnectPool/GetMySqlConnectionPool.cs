using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool
{
   public class GetMySqlConnectionPool
    {
        private static GetMySqlConnectionPool cpool = null;//池管理对象
        private static GetMySqlConnectionPool Customer_cpool = null;//池管理对象
        private static Object objlock = typeof(GetMySqlConnectionPool);//池管理对象实例
        private int size = 5;//池中连接数
        private int useCount = 0;//已经使用的连接数
        private  static ArrayList pool = null;//连接保存的集合
        private String ConnectionStr = "";//连接字符串
        public static  string constr;
        MemoryCacheModule.InMemoryCache.MemoryCacheHelper memoryCacheHelper = null;



        #region  必要字段
          public string DataBase { get; set; }
          public string Flag { get; set; }

          public string Ip { get; set; }
        #endregion 




        #region 构造函数
        public GetMySqlConnectionPool()
        {
            //数据库连接字符串
            ConnectionStr = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
            constr = ConnectionStr;
            //创建可用连接的集合
            if (pool == null)
            {
                pool = new ArrayList();
            }
        }


        public GetMySqlConnectionPool(string Constr)
        {
            int count = 0;
            //数据库连接字符串
            constr = Constr;
            //创建可用连接的集合
            if (pool == null)
            {
                pool = new ArrayList();
            }

            for (int i = 0; i < pool.Count; i++)
            {
                var connItem = (MySqlConnection)pool[i];
                if (constr.Contains(connItem.Database))
                {
                    pool.Add(connItem);
                    count++;
                    break;
                }
            }

            if (count == 0)
            {
                MySqlConnection mySqlConnection = null;
                using (mySqlConnection = new MySqlConnection(constr))
                {
                    pool.Add(mySqlConnection);
                }
            }

        }


        public GetMySqlConnectionPool(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            //数据库连接字符串  server={0};userid={1};password={2};database={3}
            string sql_constr = AppConfigurtaionServices.Configuration["OutConnection:MySql"].ToString();
            ConnectionStr = string.Format(sql_constr, ip, database_user_name, database_user_password, database_name);
            constr = ConnectionStr;
            //创建可用连接的集合
            if (pool == null)
            {
                pool = new ArrayList();
            }
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
                if (string.IsNullOrEmpty(cpool.ConnectionStr))//!= constr
                {
                    string str=AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
                    cpool.ConnectionStr = str;
                    if (cpool.ConnectionStr != constr)
                    {
                        cpool = null;
                        cpool = new GetMySqlConnectionPool(str);
                    }
                   
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
                if (cpool.ConnectionStr != ConStr)
                {
                    cpool = null;
                    cpool = new GetMySqlConnectionPool(ConStr);
                }
                return cpool;
            }
        }


        public static GetMySqlConnectionPool getPool(string ConStr,string flag)
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetMySqlConnectionPool(ConStr);
                }
                if (cpool.ConnectionStr != ConStr)
                {
                    cpool = null;
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
                string sql_constr = AppConfigurtaionServices.Configuration["OutConnection:MySql"].ToString();
                //constr = string.Format(sql_constr, ip, database_name, database_user_name, database_user_password);
                constr = string.Format(sql_constr, ip, database_user_name, database_user_password, database_name);

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
                    //取第一个可用连接[根据连接的数据库来做判断]
                    foreach (var item in pool)
                    {
                        var connItem = (MySqlConnection)item;
                        if (constr.Contains(connItem.Database))
                        {
                            tmp = connItem;
                            break;
                        }
                       

                    }
                    //在可用连接中移除此链接
                    if (pool.Count >5)
                    {
                        pool.Remove(tmp);
                    }
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
                            pool.Add(tmp);

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

               // pool.Remove(tmp);
                return tmp;
            }
        }
        #endregion

        #region 创建连接
        private MySqlConnection CreateConnection(MySqlConnection tmp)
        {
            if (tmp == null)
                using (tmp = new MySqlConnection(constr))
                {
                    //tmp.Open();
                    //可用的连接数加上一个
                    useCount++;
                }
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
                    if (pool.Count < 5)
                    {
                        pool.Add(con);
                    }
                }
            }
        }
        #endregion


        #region 目的保证所创连接成功,测试池中连接
        private bool isUserful(MySqlConnection con)
        {
            //主要用于不同用户
            bool result = true;
            try
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    if (con.State == ConnectionState.Connecting)
                    {
                        con.Close();
                        con.Open();
                    }
                    if (con.State == ConnectionState.Broken)
                    {
                        con.Close();
                        con.Open();
                    }
                    //if (con.State == ConnectionState.Open)
                    //{
                    //    con.Close();
                    //    con = null;
                    //    con=CreateConnection(con);
                    //}
                    //string sql = "select 1";//随便执行对数据库操作
                    //using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    //{
                    //    try
                    //    {
                    //        cmd.ExecuteScalar().ToString();
                    //        con.Close();
                    //        //con.Dispose();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        result = false;
                    //    }
                    //}

                    con.Close();
                    con.Dispose();

                }
            }
            catch (Exception ex)
            {

                result = false;
            }
            return result;
        }
        #endregion



    }
}
