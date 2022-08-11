using MySql.Data.MySqlClient;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool.ConnectionPoolManager;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.MySqlDriver
{
    public class MySqlDB : IDBDriver
    {
        private static object obj = new object();
        private static object obj2 = new object();
        // public static ConnectPool.ConnectionPoolManager.ConnectionPool ConnectionPool = null;
        static string conn_str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
        public static ConnectPool.ConnectionPoolManager.ConnectionPool connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc, 10, 5);
        //{
        //    get
        //    {
        //        if (ConnectionPool == null)
        //        {
        //            lock (obj)
        //            {
        //                if (ConnectionPool == null)
        //                {
        //                    ConnectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc, 10, 5);
        //                }
        //            }
        //        }
        //        return ConnectionPool;
        //    }
        //}
        //ConnectPool.ConnectionPoolManager.ConnectionPool connectionPool=
        public string ApplkKeyUser { get; set; }

        #region  数据库类型
        private string db_type;
        public string DB_TYPE
        {
            get { return db_type; }
            set
            {
                db_type = "MySql";
            }
        }
        #endregion

       

        public MySqlDB(string key = null)
        {
            ApplkKeyUser = "steve";
            //第一次连接池未开启
            if (connectionPool.State == PoolState.UnInitialize)
            {
                connectionPool.StartServices();
                Myconnection = connectionPool.GetConnectionFormPool(ApplkKeyUser, ConnectionLevel.High);
            }
            else
            {
                //连接池正在运行
                if (connectionPool.State == PoolState.Run)
                {
                    //找出没有正在被使用的连接
                    var conn_in_using_list = connectionPool.al_All.Where(x => x.IsUsing == false).OrderBy(Y => Y.CreateTime).ToList();
                    if (conn_in_using_list.Count > 0)
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        int index = r.Next(1, conn_in_using_list.Count);
                        Myconnection = conn_in_using_list[index - 1].Connection;
                        conn_in_using_list[0].IsUsing = true;
                    }
                    else
                    {
                        //连接被使用过，归还给连接池，但是状态是打开的
                        Myconnection = connectionPool.al_All[0].Connection;

                    }
                }
                //停止连接池
                else if (connectionPool.State == PoolState.Stop)
                {

                }
                //初始化连接池
                else if (connectionPool.State == PoolState.Initialize)
                {

                }

            }
            if (Myconnection.State == ConnectionState.Closed)
            {
                Myconnection.Open();
            }
        }
        public MySqlDB(string constr, string key)
        {
           

        }



        #region  获取连接字符串
        public DbConnection Myconnection=null;//数据库连接对象
        //public DbTransaction my_dbTransaction;//事物对象管理

        public  DbTransaction my_dbTransaction = null;

        #endregion


        public DbConnection GetLastDBConnection(string key)
        {
               DbConnection Myconnection = null;
                //找出没有正在被使用的连接
                var conn_in_using_list = connectionPool.al_All.Where(x => x.IsUsing == false).OrderBy(Y => Y.CreateTime).ToList();
                if (conn_in_using_list.Count > 0)
                {
                    Random r = new Random(Guid.NewGuid().GetHashCode());
                    int index = r.Next(1, conn_in_using_list.Count);
                    Myconnection = conn_in_using_list[index-1].Connection;
                    conn_in_using_list[0].IsUsing = true;
                }
                else
                {
                    //连接被使用过，归还给连接池，但是状态是打开的
                    Myconnection= connectionPool.al_All[0].Connection;
                 
                }
     
            if (Myconnection.State == ConnectionState.Closed)
            {
                Myconnection.Open();
            }
            if (my_dbTransaction == null)
            {
                my_dbTransaction = GetDbTransaction();    //开始获取事务
            }
            return Myconnection;
        }

        #region 用完之后归还连接池
        public void ReleaseConnect()
        {
            var conn_in_using_list = connectionPool.al_All.Where(x => x.IsUsing == true).FirstOrDefault();
            if (conn_in_using_list != null)
            {
                conn_in_using_list.IsUsing = false;
            }
        }
        #endregion 



        #region 批量导入
        /// <summary>
        /// /  批量导入
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="sourceDataTable">数据表</param>
        public void DataBulkCopyByDataTable(string rootPath, DataTable sourceDataTable)
        {

            if (string.IsNullOrEmpty(sourceDataTable.TableName)) throw new Exception("请给DataTable的TableName属性附上表名称");
            if (sourceDataTable.Rows.Count == 0) throw new Exception("记录数为0");
            int insertCount = 0;
            string filPath = string.Format("{0}{1}.csv", rootPath, sourceDataTable.TableName);//要上传的文件
            DataTableToCsv(sourceDataTable, filPath);


            SFtp sFtp = sFtp = new SFtp("47.100.109.211", 22, "wf", "wf");

            sFtp.Put(filPath, sourceDataTable.TableName + ".csv");

            try
            {
                MySqlBulkLoader bulk = new MySqlBulkLoader((MySqlConnection)Myconnection)
                {
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n",
                    FileName = @"D:\sftp_server\data\" + sourceDataTable.TableName + ".csv",
                    NumberOfLinesToSkip = 0,
                    TableName = sourceDataTable.TableName,
                };
                bulk.Columns.AddRange(sourceDataTable.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList());
                insertCount = bulk.Load();
                // sFtp.Delete(rmfp);
                File.Delete(filPath);
                sFtp.Delete(sourceDataTable.TableName + ".csv");

            }
            catch (Exception ex)
            {
                File.Delete(filPath);
                sFtp.Delete(sourceDataTable.TableName + ".csv");
                throw ex;

            }
        }
        #endregion

        #region 获取自动增长的ID值
        /// <summary>
        /// 获取自动增长的ID值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>decimal</returns>
        public decimal ExcuteForNewID(string sql, Hashtable args, DbTransaction dbTransaction = null)
        {
            decimal r = 0;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                try
                {
                    if (dbTransaction != null)
                    {

                        cmd.Transaction = (MySqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = (decimal)cmd.ExecuteScalar();
                    // GetMySqlConnectionPool.getPool().closeConnection((MySqlConnection)con);
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            return r;
        }
        #endregion

        #region  增删改执行SQL带参数
        /// <summary>
        ///  增删改执行SQL带参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>INT</returns>
        public int Execute(string sql, Hashtable args, DbTransaction dbTransaction = null)
        {
            int r = 0;

            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;

            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                cmd.CommandTimeout = 120;
                try
                {
                    if (dbTransaction != null)
                    {

                        cmd.Transaction = (MySqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    return -1;

                }
            }

            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
       
            ReleaseConnect();
            return r;
        }
        #endregion

        #region 存过过程新增，删除，更新
        /// <summary>
        ///  存过过程新增，删除，更新
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>INT</returns>
        public int ExecuteProc(string sql, Hashtable args, DbTransaction dbTransaction = null)
        {
            int r = 0;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                try
                {

                    if (dbTransaction != null)
                    {

                        cmd.Transaction = (MySqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgsProc(sql, args, cmd);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                    r = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            return r;
        }
        #endregion 

        #region  获取集合总个数
        /// <summary>
        /// 获取集合总个数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>object</returns>
        public object GetTotalRecordWithParameter(string sql, Hashtable args)
        {
            object r = null;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            return r;
        }
        #endregion

        #region SQL 查询语句
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        public ArrayList Select(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            ArrayList aryl = null;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new MySqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);

                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();

                    }
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            return aryl;

        }
        #endregion

        #region  SQL 查询语句 List<Hashtable>
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        public List<Hashtable> SelectList(string sql, Hashtable args, int IsNormalSmallOrBig = 0)
        {
          //  ConnectionStruct Myconnection;
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;

            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;

            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                cmd.CommandTimeout = 6000;
                if (args != null) SetArgs(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new MySqlDataAdapter(cmd))
                    {
                       
                        adapter.Fill(data);
                        aryl = DataTable2List(data.Tables[0], IsNormalSmallOrBig);
                        cmd.Parameters.Clear();
                        data.Clear();
                        data.Dispose();

                    }

                    //分页获取总个数
                    if (args != null)
                    {
                        if (args["COUNT_SQL"] != null)
                        {
                            if (!String.IsNullOrEmpty((args["COUNT_SQL"].ToString())))
                            {
                                String SQL = args["COUNT_SQL"].ToString();
                                DataTable TB = SelectDataTable(SQL, args);
                                if (TB != null && TB.Rows.Count > 0)
                                {
                                    if (aryl.Count > 0)
                                    {
                                        Hashtable HC = (Hashtable)aryl[0];
                                        HC["TOTAL_COUNT"] = Convert.ToUInt32(TB.Rows[0]["COUNT"].ToString());
                                    }
                                }
                            }

                        }
                    }




                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            ReleaseConnect();
            return aryl;

        }
        #endregion

        #region 查询返回DataTable
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        public DataTable SelectDataTable(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            System.Data.DataTable tb = new System.Data.DataTable("sb");
            //string ApplyUserKey = args["APPLY_USER_KEY"].ToString();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            // using (Myconnection)
            // {
            //Myconnection.Open();
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new MySqlDataAdapter(cmd))
                    {
                        data.Tables.Add(tb);
                        adapter.Fill(data.Tables["sb"]);
                        cmd.Parameters.Clear();
                        cmd.Dispose();

                    }
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            // }
            ReleaseConnect();
            return data.Tables["sb"];
        }
        #endregion

        #region 查询特殊存储过程【参数是表】
        /// <summary>
        /// 特殊存储过程【参数是表】
        /// </summary>
        /// <param name="sql">存储过程名</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        public ArrayList SelectsPecialProc(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            // data.EnforceConstraints = false; 
            ArrayList aryl = null;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new MySqlDataAdapter())
                {
                    try
                    {
                        adapter.SelectCommand = cmd;
                        adapter.Fill(data);
                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();
                    }
                    catch (Exception ex)
                    {
                        Myconnection.Close();
                        Myconnection = null;
                        throw;

                    }
                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            return aryl;
        }
        #endregion

        #region SQL语句参数
        /// <summary>
        ///  SQL语句参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetArgs(string sql, Hashtable args, IDbCommand cmd)
        {
            System.Text.RegularExpressions.MatchCollection ms = System.Text.RegularExpressions.Regex.Matches(sql, @"@\w+");
            System.Text.RegularExpressions.MatchCollection msk = System.Text.RegularExpressions.Regex.Matches(sql, @"!\w+");
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                string newKey = "?" + key.Substring(1);
                sql = sql.Replace(key, newKey);

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new MySqlParameter(newKey, value));
            }
            foreach (System.Text.RegularExpressions.Match m in msk)
            {
                string key = m.Value;
                string newKey = "@" + key.Substring(1);
                sql = sql.Replace(key, newKey);
            }

            cmd.CommandText = sql;
        }
        #endregion

        #region 存储过程参数私有
        /// <summary>
        ///  存储过程参数私有
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetArgsProc(string sql, Hashtable args, IDbCommand cmd)
        {
            System.Text.RegularExpressions.MatchCollection ms = System.Text.RegularExpressions.Regex.Matches(sql, @"@\w+");
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                string newKey = "?" + key.Substring(1);
                sql = sql.Replace(key, newKey);

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new MySqlParameter(newKey, value));
            }
            cmd.CommandText = sql;
        }
        #endregion

        #region  把DataTable转换成ArrayList
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        public ArrayList DataTable2ArrayList(DataTable data)
        {
            ArrayList array = new ArrayList();
            Hashtable record = null;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                System.Data.DataRow row = data.Rows[i];
                record = new NoSortHashtable();
                for (int j = 0; j < data.Columns.Count; j++)
                {

                    object cellValue = row[j];
                    if (cellValue.GetType() == typeof(DBNull))
                    {
                        cellValue = null;
                    }
                    record[data.Columns[j].ColumnName.ToUpper()] = cellValue;

                }
                array.Add(record);
            }

            return array;
        }
        #endregion

        #region 存储过程把表作为变量来传递
        /// <summary>
        /// 存储过程把表作为变量来传递
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetSpecialArgsProc(string sql, Hashtable args, IDbCommand cmd)
        {
            System.Text.RegularExpressions.MatchCollection ms = System.Text.RegularExpressions.Regex.Matches(sql, @"@\w+");
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                string newKey = "?" + key.Substring(1);
                sql = sql.Replace(key, newKey);

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new MySqlParameter(newKey, value));
            }
            cmd.CommandText = sql;
        }
        #endregion

        #region  把DataTable转换成ArrayListList<Hashtable>
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        public List<Hashtable> DataTable2List(System.Data.DataTable data, int IsNormalSmallOrBig = 0)
        {
            List<Hashtable> array = new List<Hashtable>();
            Hashtable record = null;
            if (data.Rows.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    System.Data.DataRow row = data.Rows[i];
                    record = new NoSortHashtable();
                    for (int j = 0; j < data.Columns.Count; j++)
                    {

                        object cellValue = row[j];
                        if (cellValue.GetType() == typeof(DBNull))
                        {
                            cellValue = null;
                        }
                        else
                        {
                            if (cellValue.GetType() == typeof(MySql.Data.Types.MySqlDateTime))
                            {
                                cellValue = ((MySql.Data.Types.MySqlDateTime)(cellValue)).Value;
                            }
                        }
                        if (IsNormalSmallOrBig == 1)//小写
                            record[data.Columns[j].ColumnName.ToLower()] = cellValue;
                        else if (IsNormalSmallOrBig == 2)//大写
                            record[data.Columns[j].ColumnName.ToUpper()] = cellValue;
                        else
                            record[data.Columns[j].ColumnName] = cellValue;


                    }
                    array.Add(record);
                }
            }
            //else
            //{
            //    record = new NoSortHashtable();
            //    foreach (DataColumn col in data.Columns)
            //    {
            //        if (col.DataType == typeof(String))
            //            record[col.ColumnName] = " ";
            //        if (col.DataType == typeof(int))
            //            record[col.ColumnName] = 0;
            //        if (col.DataType == typeof(Char))
            //            record[col.ColumnName] = " ";

            //    }

            //    array.Add(record);

            //}
            return array;
        }
        #endregion

        #region 打开链接
        public void OpenConnection()
        {
            Myconnection.Open();
        }
        #endregion

        #region 关闭连接
        public void CloseConnection()
        {
            Myconnection.Close();
        }
        #endregion

        #region 判断数据库连接关闭，空或者网络中断原因，在打开数据库连接
        public void ReOpenConnection()
        {
            //没有Myconnection这个对象
            if (Myconnection == null)
            {
                string conn_str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
                connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.None);
                connectionPool.StartServices();
                Myconnection = connectionPool.GetConnectionFormPool("steve", ConnectionLevel.High);
            };
            //数据库是关闭的
            if (Myconnection.State == ConnectionState.Closed) Myconnection.Open();
            //网络中断
            if (Myconnection.State == ConnectionState.Broken) Myconnection.Open();
        }
        #endregion
        #region 获取连接状态
        public DbConnection GetDBConnection()
        {
            return Myconnection;
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnectionStr)
        {
            return Myconnection;
        }
        #endregion



        #region 获取事务
        public DbTransaction GetDbTransaction()
        {


            if (my_dbTransaction == null)
            {
                my_dbTransaction = Myconnection.BeginTransaction();
            }
            return my_dbTransaction;
        }
        #endregion



        public DataTable SelectDataTablePecialProc(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            // data.EnforceConstraints = false; 
            DataTable aryl = new DataTable();
            //DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;
            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new MySqlDataAdapter())
                {
                    try
                    {
                        adapter.SelectCommand = cmd;
                        adapter.Fill(data);
                        aryl = data.Tables[0];
                        cmd.Parameters.Clear();
                        // GetMySqlConnectionPool.getPool().closeConnection((MySqlConnection)con);
                    }
                    catch (Exception ex)
                    {
                        Myconnection.Close();
                        Myconnection = null;
                        throw;

                    }
                }
            }
            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            ReleaseConnect();
            return aryl;
        }

        #region 切换同一种数据库的不同数据库
        public int ChangeDataBase(string database)
        {
            DbConnection con = GetLastDBConnection(ApplkKeyUser);
            con.ChangeDatabase(database);
            return 0;


        }
        #endregion

        #region 把DataTable转成CSV
        private void DataTableToCsv(DataTable table, string fileName)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(row[colum].ToString());
                }
                sb.AppendLine();
            }
            File.WriteAllText(fileName, sb.ToString());
        }
        #endregion



        #region 将DataTable数据源转换成实体类
        /// <summary>
        /// 将DataTable数据源转换成实体类
        /// </summary>
        /// <typeparam name="T">t</typeparam>
        /// <param name="dt">数据表</param>
        /// <returns>List<T></returns>
        public List<T> ConvertToModel<T>(DataTable dt) where T : class, new()
        {
            List<T> ts = new List<T>();// 定义集合
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                PropertyInfo[] propertys = t.GetType().GetProperties();// 获得此模型的公共属性
                foreach (PropertyInfo pi in propertys)
                {
                    if (dt.Columns.Contains(pi.Name))
                    {
                        if (!pi.CanWrite) continue;
                        var value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            if (pi.PropertyType.FullName.Contains("System.Nullable"))
                            {
                                pi.SetValue(t, Convert.ChangeType(value, (Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType)), null);
                            }
                            else
                            {
                                switch (pi.PropertyType.FullName)
                                {
                                    case "System.Decimal":
                                        pi.SetValue(t, decimal.Parse(value.ToString()), null);
                                        break;
                                    case "System.String":
                                        pi.SetValue(t, value.ToString(), null);
                                        break;
                                    case "System.Char":
                                        pi.SetValue(t, Convert.ToChar(value), null);
                                        break;
                                    case "System.Guid":
                                        pi.SetValue(t, value, null);
                                        break;
                                    case "System.Int16":
                                        pi.SetValue(t, Convert.ToInt16(value), null);
                                        break;
                                    case "System.Int32":
                                        pi.SetValue(t, int.Parse(value.ToString()), null);
                                        break;
                                    case "System.Int64":
                                        pi.SetValue(t, Convert.ToInt64(value), null);
                                        break;
                                    case "System.Byte[]":
                                        pi.SetValue(t, Convert.ToByte(value), null);
                                        break;
                                    case "System.Boolean":
                                        pi.SetValue(t, Convert.ToBoolean(value), null);
                                        break;
                                    case "System.Double":
                                        pi.SetValue(t, Convert.ToDouble(value.ToString()), null);
                                        break;
                                    case "System.DateTime":
                                        pi.SetValue(t, value ?? Convert.ToDateTime(value), null);
                                        break;
                                    default:
                                        throw new Exception("类型不匹配:" + pi.PropertyType.FullName);
                                }
                            }
                        }
                    }
                }
                ts.Add(t);
            }
            return ts;
        }
        #endregion

        #region  查询对象
        public List<T> SelectListModel<T>(string sql, Hashtable args) where T : class, new()
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<T> aryl = null;
            // DbConnection con = GetDBConnect();
            Myconnection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = Myconnection != null;

            using (MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Myconnection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                MySqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(data);
                        // adapter.Dispose();
                        aryl = ConvertToModel<T>(data.Tables[0]);
                        cmd.Parameters.Clear();
                        // GetMySqlConnectionPool.getPool().closeConnection((MySqlConnection)con);
                    }
                }
                catch (Exception ex)
                {
                    Myconnection.Close();
                    Myconnection = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                Myconnection.Close();
                Myconnection = null;
            }
            ReleaseConnect();
            return aryl;
        }
        #endregion


        #region 分页部分
        public Hashtable SelectPageList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {




            // SELECT @rownum:= @rownum + 1 as rownum,WF.* FROM(SELECT @rownum:= 0) r,({ 0 }) WF where 1 = 1 order by rownum LIMIT { 1},{ 2}



            Hashtable hashtable = null;
            hashtable = new Hashtable();
            try
            {
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                int pageindex = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                page_sql = string.Format("SELECT !rownum:= !rownum + 1 as rownum,WF.* FROM(SELECT !rownum:= 0) r,({0}) WF where 1 = 1 order by rownum LIMIT {1},{2}", sql, pageindex, page_size);
                //page_sql = string.Format("SELECT WF.* FROM (SELECT rownum= 0) r,({0}) WF where 1 = 1 order by rownum LIMIT {1},{2}", sql, pageindex, page_size);
                List<Hashtable> list_page = SelectList(page_sql, arg, 2);
                if (!string.IsNullOrEmpty(order_str))
                {
                    //排序字段
                    string[] property = order_str.Split(',');
                    foreach (string str_order in property)
                    {
                        list_page.OrderBy(x => x[str_order].ToString()).AsQueryable();
                    }
                }
                hashtable["TOTAL_COUNT"] = total_count;
                hashtable["List"] = list_page;
            }
            catch (Exception ex)
            {


            }
            ReleaseConnect();
            return hashtable;
        }

        public ArrayList SelectPageArrayList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            ArrayList list = null;
            list = new ArrayList();
            try
            {
                int pageindex = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                if (string.IsNullOrEmpty(order_str))
                    page_sql = string.Format("SELECT !rownum:= !rownum + 1 as rownum,WF.* FROM(SELECT !rownum:= 0) r,({0}) WF where 1 = 1 order by rownum LIMIT {1},{2}", sql, pageindex, page_size);
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}", t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());

                    page_sql = string.Format("SELECT !rownum:= !rownum + 1 as rownum,WF.* FROM(SELECT !rownum:= 0) r,({0}) WF where 1 = 1 order by {3} LIMIT {1},{2}", sql, pageindex, page_size, wf_order);
                }

                list = Select(page_sql, arg);
                Hashtable hash = null;
                hash = new Hashtable();
                hash["TOTAL_COUNT"] = total_count;
                list.Add(hash);


            }
            catch (Exception ex)
            {


            }
            ReleaseConnect();
            return list;
        }

        public DataTable SelectPageDataTable(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            DataTable dataTable = null;
            dataTable = new DataTable();
            try
            {
                int pageindex = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                if (string.IsNullOrEmpty(order_str))
                    page_sql = string.Format("SELECT !rownum:= !rownum + 1 as rownum,WF.*,{3} AS TOTAL_COUNT  FROM(SELECT !rownum:= 0) r,({0}) WF where 1 = 1 order by rownum LIMIT {1},{2}", sql, pageindex, page_size, total_count);
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}", t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());
                    page_sql = string.Format("SELECT !rownum:= !rownum + 1 as rownum,WF.*,{4} AS TOTAL_COUNT FROM(SELECT !rownum:= 0) r,({0}) WF where 1 = 1 order by {3} LIMIT {1},{2}", sql, pageindex, page_size, wf_order, total_count);
                }
                dataTable = SelectDataTable(page_sql, arg);
            }
            catch (Exception ex)
            {


            }
            return dataTable;
        }
        #endregion


        #region 查询In小分队 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <param name="InIds"></param>
        /// <param name="IsNormalSmallOrBig"></param>
        /// <returns></returns>
        public List<Hashtable> SelectListForIn(string sql, Hashtable args, string InIds, int IsNormalSmallOrBig = 0)
        {
            var pattern = @" \*\s+from\s+[\w\[\]]*\.?[\w\[\]]*\.?\[?(\b\w+)\]?[\r\n\s]*";
            var table_name = Regex.Match(sql, pattern).Groups[1].Value;//获取表名
            string pkc = GetPkc(table_name);//获取主键
            string last_select_sql = string.Empty;
            if (!string.IsNullOrEmpty(pkc))
            {
                List<string> list = new List<string>(InIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                string inion_sql = string.Empty;
                int count = 1;
                foreach (string guid in list)
                {
                    if (count == list.Count)
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID  ", guid);
                    }
                    else
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID  ", guid) + " UNION  ";
                    }
                    count++;
                }
                last_select_sql = string.Format(" SELECT A.* FROM ({0}) A INNER JOIN ({1}) B ON A.{2}=B.GUID ", sql, inion_sql, pkc);
            }
            else//主键没有查出来
            {
                string IDS = LocalDBCommon.StringToList(InIds);
                if (sql.Contains("where") || sql.Contains("WHERE"))
                {
                    last_select_sql = string.Format(" {0} AND {1} IN ({2})  ", sql, pkc, IDS);
                }
                else
                {
                    last_select_sql = string.Format(" {0} WHERE {1} IN ({2})  ", sql, pkc, IDS);
                }
            }
            List<Hashtable> li = SelectList(last_select_sql, args);
            return li;
        }

        public DataTable SelectDataTableForIn(string sql, Hashtable args, string InIds)
        {
            var pattern = @" \*\s+from\s+[\w\[\]]*\.?[\w\[\]]*\.?\[?(\b\w+)\]?[\r\n\s]*";
            var table_name = Regex.Match(sql, pattern).Groups[1].Value;//获取表名
            string pkc = GetPkc(table_name);//获取主键
            string last_select_sql = string.Empty;
            if (!string.IsNullOrEmpty(pkc))
            {
                List<string> list = new List<string>(InIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                string inion_sql = string.Empty;
                int count = 1;
                foreach (string guid in list)
                {
                    if (count == list.Count)
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID  ", guid);
                    }
                    else
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID  ", guid) + " UNION  ";
                    }
                    count++;
                }
                last_select_sql = string.Format(" SELECT A.* FROM ({0}) A INNER JOIN ({1}) B ON A.{2}=B.GUID ", sql, inion_sql, pkc);
            }
            else//主键没有查出来
            {
                string IDS = LocalDBCommon.StringToList(InIds);
                if (sql.Contains("where") || sql.Contains("WHERE"))
                {
                    last_select_sql = string.Format(" {0} AND {1} IN {2}  ", sql, pkc, IDS);
                }
                else
                {
                    last_select_sql = string.Format(" {0} WHERE {1} IN {2}  ", sql, pkc, IDS);
                }
            }

            DataTable dataTable = SelectDataTable(last_select_sql, args);

            return dataTable;
        }
        #endregion


        #region  获取主键
        public string GetPkc(string table_name)
        {
            string pk = string.Empty;
            Hashtable hashtable = null;
            hashtable = new Hashtable();
            hashtable["TABLE_NAME"] = table_name;
            string sql_pk = @" SELECT
	                                    COLUMN_NAME AS PKC
                                    FROM
	                                    INFORMATION_SCHEMA. COLUMNS
                                    WHERE
	                                    TABLE_NAME=@TABLE_NAME
                                    AND COLUMN_KEY = 'PRI'; ";
            DataTable dataTable = SelectDataTable(sql_pk, hashtable);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                pk = dataTable.Rows[0]["PKC"].ToString();
            }

            return pk;
        }


        #endregion

        public DbTransaction CloseDbTransaction()
        {
            Myconnection.BeginTransaction().Dispose();
            my_dbTransaction = null;
            return my_dbTransaction;

        }

        #region 提交事务
        public void CommitTransaction()
        {
            my_dbTransaction.Commit();
            my_dbTransaction = null;
        }
        #endregion

        #region 回滚事务
        public void RollbackTransaction()
        {
            my_dbTransaction.Rollback();
            my_dbTransaction = null;
        }
        #endregion 
    }
}
