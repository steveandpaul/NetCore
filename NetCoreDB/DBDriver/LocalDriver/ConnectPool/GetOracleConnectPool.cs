using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool
{
  public  class GetOracleConnectPool
    {
        private static GetOracleConnectPool cpool = null;//池管理对象
        private static GetOracleConnectPool Customer_cpool = null;//池管理对象
        private static Object objlock = typeof(GetOracleConnectPool);//池管理对象实例
        private int size = 5;//池中连接数
        private int useCount = 0;//已经使用的连接数
        private ArrayList pool = null;//连接保存的集合
        private  String ConnectionStr = "";//连接字符串
        public static string constr;

        #region 构造函数
        //无参
        public GetOracleConnectPool()
        {
            ConnectionStr = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
            constr = ConnectionStr;
            //创建可用连接的集合
            pool = new ArrayList();
        }

        //有参--直接传入连接字符串(不友好)
        public GetOracleConnectPool(string Constr)
        {
            //数据库连接字符串
            constr = Constr;
            //创建可用连接的集合
            pool = new ArrayList();
        }

        //有参--参数传入(友好)
        public GetOracleConnectPool(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            //数据库连接字符串
            string sql_constr = AppConfigurtaionServices.Configuration["OutConnection:Oracle"].ToString();
            ConnectionStr = string.Format(sql_constr, ip, database_name, database_user_name, database_user_password);
            //创建可用连接的集合
            pool = new ArrayList();
        }
        #endregion 

        #region 创建获取连接池对象
        //无参数
        public static GetOracleConnectPool getPool()
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetOracleConnectPool();
                }
                return cpool;
            }
        }
        //有参
        public static GetOracleConnectPool getPool(string ConStr)
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetOracleConnectPool(ConStr);
                }
                return cpool;
            }
        }

        public static GetOracleConnectPool getPool(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            lock (objlock)
            {
                if (cpool == null)
                {
                    cpool = new GetOracleConnectPool(ip, database_user_name, database_user_password, port, database_name);
                }
                string sql_constr = AppConfigurtaionServices.Configuration["OutConnection:Oracle"].ToString();
                constr = string.Format(sql_constr, ip, database_name, database_user_name, database_user_password);
                return cpool;
            }
        }
        #endregion

        #region 获取池中的连接
        public OracleConnection getConnection()
        {
            lock (pool)
            {
                OracleConnection tmp = null;
                //可用连接数量大于0
                if (pool.Count > 0)
                {
                    //取第一个可用连接
                    tmp = (OracleConnection)pool[0];
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
                return tmp;
            }
        }
        #endregion

        #region 创建连接
        private OracleConnection CreateConnection(OracleConnection tmp)
        {
            //创建连接
            //using (OracleConnection conn = new OracleConnection(constr))
            //{
            if(tmp==null)
                tmp = new OracleConnection(constr);
              //  bool connectSuccess = false;
               // Thread t = new Thread(delegate ()
               // {
                   // try
                   // {
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
               // tmp = conn;
            //}
           
           
            return tmp;
        }
        #endregion

        #region 关闭连接,加连接回到池中
        public void closeConnection(OracleConnection con)
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
        private bool isUserful(OracleConnection con)
        {

            bool bf = true;
            string[] array = con.ConnectionString.Split(';');
            foreach (string str in array)
            {
                if (!constr.Contains(str))
                {
                    bf = false;
                    break;
                }
            
            }

            if (bf==false)
            {
                con.Close();
                con = null;
                con = new OracleConnection(constr);
            }
            //主要用于不同用户
            bool result = true;
            if (con != null)
            {
                string sql = " select   1 from dual ";//随便执行对数据库操作
                OracleCommand cmd = new OracleCommand(sql, con);
                try
                {
                    cmd.ExecuteScalar().ToString();
                    con.Close();
                   // con.Dispose();
                }
                catch(Exception ex)
                {
                    result = false;
                }

            }
            return result;
        }
        #endregion

    }
}
