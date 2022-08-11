using NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool;
using NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool.ConnectionPoolManager;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.OracleDriver
{
  public  class OracleDB : IDBDriver
    {
        static string conn_str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
        ConnectPool.ConnectionPoolManager.ConnectionPool connectionPool = null;
        public string ApplkKeyUser { get; set; }
        public int DATA_BASE_TYPE { get; set; }
        #region  数据库类型
        private string db_type;
        public string DB_TYPE
        {
            get { return db_type; }
            set
            {
                db_type = "Oracle";
            }
        }
        #endregion


        public OracleDB(string key = null)
        {
            GetConstructDBConnect(key);
        }

        public OracleDB(string constr, string key)
        {
            ApplkKeyUser = key;
            if (connectionPool == null)
            {
                connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(constr, ConnectionType.Odbc, 2, 1, DATA_BASE_TYPE);
                connectionPool.StartServices();
                if (!string.IsNullOrEmpty(key))
                {
                    connection = connectionPool.GetConnectionFormPool(ApplkKeyUser, 0);
                }
                else
                {
                    connection = connectionPool.GetConnectionFormPool("remote_oracle", 0);
                }
            }

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public OracleDB(string Constr, int data_base_type)
        {
            DATA_BASE_TYPE = data_base_type;
            GetConstructDBConnect(Constr);
        }

        public OracleDB(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            GetConstructDBConnect(ip, database_user_name, database_user_password, port, database_name);

        }

        public DbConnection GetLastDBConnection(string key)
        {
            if (connection == null)
            {
                connection = connectionPool.GetConnectionFormPool(key, 0);

            }
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            return connection;


        }

        #region  获取连接字符串
        public DbConnection connection;//数据库连接对象
        public DbTransaction my_dbTransaction;//事物对象管理

        private DbConnection GetDBConnect()
        {
            // connection = GetOracleConnection.instance;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                if (connection.State == ConnectionState.Connecting)
                {
                    connection.Close();
                    connection.Open();
                }
                if (connection.State == ConnectionState.Broken)
                {
                    connection.Close();
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                ////connection.Dispose();
                //string sqlcon= connection.ConnectionString;
                //connection = null;
                //GetOracleConnection.conStr = sqlcon;
                //connection = GetOracleConnection.instance;
            }
            return connection;
        }






        private DbConnection GetConstructDBConnect(string Constr)
        {

            ApplkKeyUser = "remote_oracle";
            // connection = GetSqlServerConnection.instance;
            connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(Constr, ConnectionType.Odbc, 5, 1, DATA_BASE_TYPE);
            if (connectionPool != null)
            {

                //connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc,2,1);
                connectionPool.StartServices();

                connection = connectionPool.GetConnectionFormPool(ApplkKeyUser, 0);


                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                if (connection.State == ConnectionState.Connecting)
                {
                    connection.Close();
                    connection.Open();
                }
                if (connection.State == ConnectionState.Broken)
                {
                    connection.Close();
                    connection.Open();
                }
            }
            return connection;

        }

        private DbConnection GetConstructDBConnect(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {



            string sql_constr = "server={0};port={3};userid={1};password={2};database={4};Charset=UTF8;Allow Zero Datetime=True; Pooling=true; Max Pool Size=500;sslmode=none;Allow User Variables=True";
            string CONN = string.Format(sql_constr, ip, database_user_name, database_user_password, port, database_name);
            ApplkKeyUser = "remote_oracle";
            // connection = GetSqlServerConnection.instance;
            connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(CONN, ConnectionType.Odbc, 5, 1, DATA_BASE_TYPE);
            if (connectionPool != null)
            {

                //connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc,2,1);
                connectionPool.StartServices();

                connection = connectionPool.GetConnectionFormPool(ApplkKeyUser, 0);


                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                if (connection.State == ConnectionState.Connecting)
                {
                    connection.Close();
                    connection.Open();
                }
                if (connection.State == ConnectionState.Broken)
                {
                    connection.Close();
                    connection.Open();
                }
            }
            return connection;

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
            OracleConnection oracleConnection = (OracleConnection)connection;
            OracleBulkCopy oracleBulkCopy = new OracleBulkCopy(oracleConnection, OracleBulkCopyOptions.Default);
            oracleBulkCopy.BatchSize = sourceDataTable.Rows.Count;
            oracleBulkCopy.BulkCopyTimeout = 260;
            oracleBulkCopy.DestinationTableName = sourceDataTable.TableName;
            try
            {
                if (oracleConnection.State != ConnectionState.Open)
                {
                    oracleConnection.Open();
                }
                // conn.Open();
                if (sourceDataTable != null && sourceDataTable.Rows.Count != 0)
                {
                    oracleBulkCopy.WriteToServer(sourceDataTable);

                }
            }
            catch (Exception ex)
            {
                // Log.WriteLog(err, ex);
            }
            finally
            {
                oracleConnection.Close();
                if (oracleConnection != null)
                    oracleConnection.Close();
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        // my_dbTransaction = dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        //cmd.Transaction = (OracleTransaction)my_dbTransaction;
                        cmd.Transaction = (OracleTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = (decimal)cmd.ExecuteScalar();
                    GetOracleConnectPool.getPool().closeConnection((OracleConnection)con);
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        //my_dbTransaction = dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        //cmd.Transaction = (OracleTransaction)my_dbTransaction;
                        cmd.Transaction = (OracleTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();
                    GetOracleConnectPool.getPool().closeConnection((OracleConnection)con);
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;

                }
            }

            if (isConn == false)
            {
                con.Close();
                con = null;
            }
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        // my_dbTransaction = dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        // cmd.Transaction = (OracleTransaction)my_dbTransaction;
                        cmd.Transaction = (OracleTransaction)dbTransaction;
                    }
                    if (args != null) SetArgsProc(sql, args, cmd);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                    r = cmd.ExecuteNonQuery();
                    GetOracleConnectPool.getPool().closeConnection((OracleConnection)con);
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                OracleDataAdapter adapter = null;
                try
                {
                    using (adapter = new OracleDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                con.Close();
                con = null;
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
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;
            DbConnection con = GetLastDBConnection(ApplkKeyUser);
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                OracleDataAdapter adapter = null;
                try
                {
                    using (adapter = new OracleDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = DataTable2List(data.Tables[0], IsNormalSmallOrBig);
                        cmd.Parameters.Clear();
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    // con = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                con.Close();
                con = null;
            }
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
            System.Data.DataTable tb = new System.Data.DataTable();
            // data.EnforceConstraints = false; 
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                OracleDataAdapter adapter = null;
                try
                {
                    using (adapter = new OracleDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        tb = data.Tables[0];
                        cmd.Parameters.Clear();
                    }
                }
                catch
                {
                    con.Close();
                    con = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
            }
            return tb;
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                OracleDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new OracleDataAdapter())
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
                        con.Close();
                        con = null;
                        throw;

                    }
                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
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
            int i = 1;
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                //string newKey = "@P" + i++;
                //sql = sql.Replace(key, ":");
                string newKey = ":" + key.Substring(1);
                sql = sql.Replace(key, newKey);

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new OracleParameter(newKey, value));
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
            int i = 1;
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                string newKey = "@P" + i++;
                sql = sql.Replace(key, "?");

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new OracleParameter(newKey, value));
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
                    record[data.Columns[j].ColumnName] = cellValue;

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
            int i = 1;
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;
                string newKey = "@P" + i++;
                sql = sql.Replace(key, "?");

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }

                cmd.Parameters.Add(new OracleParameter(newKey, value));
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
            else
            {
                record = new NoSortHashtable();
                foreach (DataColumn col in data.Columns)
                {
                    if (col.DataType == typeof(String))
                        record[col.ColumnName] = " ";
                    if (col.DataType == typeof(int))
                        record[col.ColumnName] = 0;
                    if (col.DataType == typeof(Char))
                        record[col.ColumnName] = " ";

                }

                array.Add(record);

            }
            return array;
        }
        #endregion

        #region 打开链接
        public void OpenConnection()
        {
            connection.Open();
        }
        #endregion

        #region 关闭连接
        public void CloseConnection()
        {
            connection.Close();
        }
        #endregion

        #region 判断数据库连接关闭，空或者网络中断原因，在打开数据库连接
        public void ReOpenConnection()
        {
            //没有connection这个对象
            if (connection == null) { GetOracleConnectPool.getPool().getConnection(); connection.Open(); };
            //数据库是关闭的
            if (connection.State == ConnectionState.Closed) connection.Open();
            //网络中断
            if (connection.State == ConnectionState.Broken) connection.Open();
        }
        #endregion
        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnectionStr)
        {
            if (connection == null)
            {
                GetOracleConnection.conStr = ConnectionStr;
                connection = GetOracleConnection.instance;
            }
            return connection;
        }
        #endregion
        #region 获取连接状态
        public DbConnection GetDBConnection()
        {
            if (connection == null)
            {
                connection = GetOracleConnection.instance;
            }
            return connection;
        }
        #endregion

        #region 获取事务
        public DbTransaction GetDbTransaction()
        {
            if (my_dbTransaction == null)
            {
                DbConnection conn = GetDBConnect();
                my_dbTransaction = conn.BeginTransaction();
            }
            else
            {
                if (my_dbTransaction.Connection == null)
                {
                    DbConnection conn = GetDBConnect();
                    my_dbTransaction = conn.BeginTransaction();
                }

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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                OracleDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new OracleDataAdapter())
                {
                    try
                    {
                        adapter.SelectCommand = cmd;
                        adapter.Fill(data);
                        aryl = data.Tables[0];
                        cmd.Parameters.Clear();
                        GetOracleConnectPool.getPool().closeConnection((OracleConnection)con);
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        con = null;
                        throw;

                    }
                }
            }
            if (isConn == false)
            {
                con.Close();
                con = null;
            }
            return aryl;
        }

        #region 切换同一种数据库的不同数据库
        public int ChangeDataBase(string database)
        {
            DbConnection con = GetDBConnect();
            con.ChangeDatabase(database);
            return 0;
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (OracleCommand cmd = new OracleCommand(sql, (OracleConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                OracleDataAdapter adapter = null;
                try
                {
                    using (adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = ConvertToModel<T>(data.Tables[0]);
                        cmd.Parameters.Clear();
                        GetOracleConnectPool.getPool().closeConnection((OracleConnection)con);
                    }
                }
                catch (Exception ex)
                {
                    con.Close();
                    con = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                con.Close();
                con = null;
            }
            return aryl;
        }
        #endregion


        #region 分页部分
        public Hashtable SelectPageList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            Hashtable hashtable = null;
            hashtable = new Hashtable();
            try
            {
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                int row_num = page_index * page_size;
                int row_num2 = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                page_sql = string.Format(@" SELECT WF.* FROM (SELECT rownum as ROW_NUM, a.* FROM ({0}) a where rownum <= {1}) WF where WF.ROW_NUM > {2} ", sql, row_num, row_num2);
                IList<Hashtable> list_page = SelectList(page_sql, arg, 2);
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
            return hashtable;
        }

        public ArrayList SelectPageArrayList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            ArrayList list = null;
            list = new ArrayList();
            try
            {
                int row_num = page_index * page_size;
                int row_num2 = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                if (string.IsNullOrEmpty(order_str))
                    page_sql = string.Format(@" SELECT WF.* FROM (SELECT rownum as ROW_NUM, a.* FROM ({0}) a where rownum <= {1}) WF where WF.ROW_NUM > {2} ", sql, row_num, row_num2);
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}", t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());

                    page_sql = string.Format(@" SELECT WF.* FROM (SELECT rownum as ROW_NUM, a.* FROM ({0}) a where rownum <= {1}) WF where WF.ROW_NUM > {2} ORDER BY {3} ", sql, row_num, row_num2, wf_order);
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
            return list;
        }

        public DataTable SelectPageDataTable(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            DataTable dataTable = null;
            dataTable = new DataTable();
            try
            {
                int row_num = page_index * page_size;
                int row_num2 = (page_index - 1) * page_size;
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                if (string.IsNullOrEmpty(order_str))
                    page_sql = string.Format(@" SELECT WF.* FROM (SELECT rownum as ROW_NUM, a.* FROM ({0}) a where rownum <= {1}) WF where WF.ROW_NUM > {2} ", sql, row_num, row_num2);
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}", t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());
                    page_sql = string.Format(@" SELECT WF.* FROM (SELECT rownum as ROW_NUM, a.* FROM ({0}) a where rownum <= {1}) WF where WF.ROW_NUM > {2} ORDER BY {3} ", sql, row_num, row_num2, wf_order);
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
                        inion_sql += string.Format(" SELECT '{0}' AS GUID FROM DUAL  ", guid);
                    }
                    else
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID FROM DUAL  ", guid) + " UNION  ";
                    }
                    count++;
                }
                last_select_sql = string.Format(" SELECT A.* FROM ({0}) A INNER JOIN ({1}) B ON A.\"{2}\"=B.GUID ", sql, inion_sql, pkc);
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
                        inion_sql += string.Format(" SELECT '{0}' AS GUID FROM DUAL  ", guid);
                    }
                    else
                    {
                        inion_sql += string.Format(" SELECT '{0}' AS GUID  FROM DUAL ", guid) + " UNION  ";
                    }
                    count++;
                }
                last_select_sql = string.Format(" SELECT A.* FROM ({0}) A INNER JOIN ({1}) B ON A.\"{2}\"=B.GUID ", sql, inion_sql, pkc);
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
            string sql_pk = @"         Select a.Owner 主键拥有者,
                                                    a.table_name 主键表,
                                                    b.Column_Name PKC,
                                                    b.Constraint_Name 主键名
                                                    From user_Constraints a,
                                                       user_Cons_Columns b
                                                    Where
                                                     a.Constraint_Type = 'P'--P-主键；R-外键；U-唯一约束
                                                    and a.Constraint_Name = b.Constraint_Name

                                                    And a.Owner = b.Owner

                                                    And a.table_name = b.table_name


                                                    And a.table_name= upper(@TABLE_NAME) ";
            DataTable dataTable = SelectDataTable(sql_pk, hashtable);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                pk = dataTable.Rows[0]["PKC"].ToString();
            }

            return pk;
        }
        #endregion


    }
}
