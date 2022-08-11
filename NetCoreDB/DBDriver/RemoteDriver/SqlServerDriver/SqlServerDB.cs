using Microsoft.Data.SqlClient;
using NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool;
using NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool.ConnectionPoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.SqlServerDriver
{
   public  class SqlServerDB : IDBDriver
    {
        static string conn_str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
        ConnectPool.ConnectionPoolManager.ConnectionPool connectionPool = null;
        public int DATA_BASE_TYPE { get; set; }
        public string ApplkKeyUser { get; set; }
        #region  数据库类型
        private string db_type;
        public string DB_TYPE
        {
            get { return db_type; }
            set
            {
                db_type = "SqlServer";
            }
        }
        #endregion



        public SqlServerDB()
        {
            GetConstructDBConnect();
        }

        public SqlServerDB(string Constr)
        {
            GetConstructDBConnect(Constr);
        }

        public SqlServerDB(string Constr, int data_base_type)
        {
            DATA_BASE_TYPE = data_base_type;
            GetConstructDBConnect(Constr);
        }
        public SqlServerDB(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master")
        {
            GetConstructDBConnect(ip,database_user_name,database_user_password,port,database_name);
        }

        #region  获取连接字符串
        public DbConnection connection;
        public DbTransaction dbTransaction;

        private DbConnection GetConstructDBConnect()
        {
            ApplkKeyUser = "remote_sqlserver";
            // connection = GetSqlServerConnection.instance;
            connectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc, 5, 1, DATA_BASE_TYPE);
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

        private DbConnection GetConstructDBConnect(string Constr)
        {

            ApplkKeyUser = "remote_sqlserver";
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
            ApplkKeyUser = "remote_sqlserver";
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

      
        public DbTransaction my_dbTransaction;//事物对象管理
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




        #region 批量导入
        /// <summary>
        /// /  批量导入
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="sourceDataTable">数据表</param>
        public void DataBulkCopyByDataTable(string dataTableName, DataTable sourceDataTable)
        {
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection.ConnectionString, SqlBulkCopyOptions.Default))
                {
                    try
                    {
                        sqlBulkCopy.DestinationTableName = dataTableName;
                        sqlBulkCopy.BatchSize = sourceDataTable.Rows.Count;
                        sqlBulkCopy.WriteToServer(sourceDataTable);
                        sqlBulkCopy.Close();
                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
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
        public decimal ExcuteForNewID(string sql, Hashtable args)
        {
            decimal r = 0;
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        // my_dbTransaction = dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        // cmd.Transaction = (MySqlTransaction)my_dbTransaction;
                        cmd.Transaction = (SqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = (decimal)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
                
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
        public int Execute(string sql, Hashtable args)
        {
            int r = 0;
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        //my_dbTransaction = dbTransaction;
                        //my_dbTransaction = con.BeginTransaction();
                        // cmd.Transaction = (MySqlTransaction)my_dbTransaction;
                        cmd.Transaction = (SqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;

                }
            }

            if (isConn == false)
            {
                connection.Close();
                connection = null;
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
        public int ExecuteProc(string sql, Hashtable args)
        {
            int r = 0;
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                try
                {

                    if (dbTransaction != null)
                    {
                        //my_dbTransaction = dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        //cmd.Transaction = (MySqlTransaction)my_dbTransaction;
                        cmd.Transaction = (SqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgsProc(sql, args, cmd);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                    r = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
            
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
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;

                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
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
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        // adapter.Dispose();
                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();
                        // GetMySqlConnectionPool.getPool().closeConnection((MySqlConnection)con);
                    }

                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                connection.Close();
                connection = null;
            }
            return aryl;

        }
        #endregion

        #region  SQL 查询语句 List<Hashtable>
        /// <summary>
        ///  SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>动态List</returns>
        public List<Hashtable> SelectList(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = DataTable2List(data.Tables[0]);
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)connection);
                    }
                }
                catch (Exception ex)
                {
                    connection.Close();
                    connection = null;
                    throw;
                }
            }

            if (isConn == false)
            {
                connection.Close();
                connection = null;
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
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        tb = data.Tables[0];
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)connection);
                    }
                }
                catch
                {
                    connection.Close();
                    connection = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
            }
            return tb;
        }
        #endregion


        #region 查询返回DataTable 带事务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        public DataTable SelectDataTable(string sql, Hashtable args,DbTransaction transaction)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            System.Data.DataTable tb = new System.Data.DataTable();
            // data.EnforceConstraints = false; 
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction();
                    cmd.Transaction = (SqlTransaction)transaction;
                    dbTransaction = cmd.Transaction;
                }
                else
                {
                    cmd.Transaction = (SqlTransaction)transaction;

                }
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(data);
                        adapter.Dispose();
                        tb = data.Tables[0];
                        cmd.Parameters.Clear();
                    }
                }
                catch
                {
                    connection.Close();
                    connection = null;
                    throw;
                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
            }
            GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)connection);
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
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                Microsoft.Data.SqlClient.SqlDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new SqlDataAdapter())
                {
                    try
                    {
                        adapter.SelectCommand = cmd;
                        adapter.Fill(data);
                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)connection);
                    }
                    catch (Exception ex)
                    {
                        connection.Close();
                        connection = null;
                        throw;

                    }
                }
            }
            if (isConn == false)
            {
                connection.Close();
                connection = null;
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
            // int i = 1;
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                string key = m.Value;

                Object value = args[key];
                if (value == null)
                {
                    value = args[key.Substring(1)];
                }
                if (value == null) value = DBNull.Value;

                cmd.Parameters.Add(new SqlParameter(key, value));
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
            cmd.CommandText = sql;
            foreach (string str in args.Keys)
            {
                string key = "@" + str;

                Object value = args[str];
                if (value == null)
                {
                    value = args[str];
                }
                if (value == null) value = DBNull.Value;

                cmd.Parameters.Add(new SqlParameter(key, value));
            }
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


        #region  把DataTable转换成ArrayListList<Hashtable>
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        public List<Hashtable> DataTable2List(System.Data.DataTable data)
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

        #region 存储过程把表作为变量来传递
        /// <summary>
        /// 存储过程把表作为变量来传递
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetSpecialArgsProc(string sql, Hashtable args, IDbCommand cmd)
        {
            cmd.CommandText = sql;
            SqlParameter sp = null;
            foreach (string str in args.Keys)
            {
                string key = "@" + str;

                Object value = args[str];
                if (value == null)
                {
                    value = args[str];
                }
                if (value == null) value = DBNull.Value;
                sp = new SqlParameter(key, value);//超级重要的
               // sp.Value = value;
                cmd.Parameters.Add(sp);
            }

        }
        #endregion


        #region 获取连接状态
        public DbConnection GetDBConnection()
        {
            if (connection == null)
            {
                connection = GetSqlServerConnection.instance;
            }
            return connection;
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnectionStr)
        {
            if (connection == null)
            {
                GetSqlServerConnection.conStr = ConnectionStr;
                connection = GetSqlServerConnection.instance;
            }
            return connection;
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
            if (connection == null) connection.Open();
            //数据库是关闭的
            if (connection.State == ConnectionState.Closed) connection.Open();
            //网络中断
            if (connection.State == ConnectionState.Broken) connection.Open();
        }
        #endregion

        #region  增删改执行SQL带参数 自带事务
        public int Execute(string sql, Hashtable args, DbTransaction transaction)
        {
            int r = 0;
            connection = GetLastDBConnection(ApplkKeyUser);
            bool isConn = connection != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)connection))
            {
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction();
                    cmd.Transaction = (SqlTransaction)transaction;
                    dbTransaction = cmd.Transaction;
                }
                else
                {
                    cmd.Transaction= (SqlTransaction)transaction;
                }
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                     transaction.Rollback();
                    connection.Close();
                    connection = null;
                    throw;

                }
            }

            if (isConn == false)
            {
                connection.Close();
                connection = null;
            }
            GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)connection);
            return r;
        }
        #endregion


        #region 获取事务
        public DbTransaction GetDbTransaction()
        {
            if (dbTransaction == null)
            {
                DbConnection conn = GetLastDBConnection(ApplkKeyUser);
                dbTransaction = conn.BeginTransaction();
            }
            else
            {
                if (dbTransaction.Connection == null)
                {
                    DbConnection conn = GetLastDBConnection(ApplkKeyUser);
                    dbTransaction = conn.BeginTransaction();
                }
            
            }
            return dbTransaction;
        }
        #endregion

        public List<Hashtable> SelectList(string sql, Hashtable args, DbTransaction dbTransaction)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;
            DbConnection con = GetLastDBConnection(ApplkKeyUser);
            bool isConn = con != null;
            SqlDataAdapter adapter = null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        aryl = DataTable2List(data.Tables[0]);
                        cmd.Parameters.Clear();
                        // connectionPool.DisposeConnection(ApplyUserKey);
                        // GetMySqlConnectionPool.getPool().closeConnection((MySqlConnection)con);
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
            GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
            return aryl;
        }

        public DataTable SelectDataTablePecialProc(string sql, Hashtable args)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            // data.EnforceConstraints = false; 
            DataTable aryl = new DataTable();
            DbConnection con = GetLastDBConnection(ApplkKeyUser);
            bool isConn = con != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                Microsoft.Data.SqlClient.SqlDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new SqlDataAdapter())
                {
                    try
                    {
                        adapter.SelectCommand = cmd;
                        adapter.Fill(data);
                        aryl = data.Tables[0];
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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

        public int ChangeDataBase(string database)
        {
            throw new NotImplementedException();
        }

        public List<Hashtable> SelectList(string sql, Hashtable args, int IsNormalSmallOrBig = 0)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;
            DbConnection con = GetLastDBConnection(ApplkKeyUser);
            bool isConn = con != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        //adapter.Dispose();
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

        public int ExecuteProc(string sql, Hashtable args, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }

        public decimal ExcuteForNewID(string sql, Hashtable args, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }

        public List<Hashtable> DataTable2List(DataTable data, int IsNormalSmallOrBig = 0)
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
            return array; ;
        }

        public List<T> SelectListModel<T>(string sql, Hashtable args) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Hashtable SelectPageList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            throw new NotImplementedException();
        }

        public ArrayList SelectPageArrayList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectPageDataTable(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            throw new NotImplementedException();
        }

        public List<Hashtable> SelectListForIn(string sql, Hashtable args, string InIds, int IsNormalSmallOrBig = 0)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectDataTableForIn(string sql, Hashtable args, string InIds)
        {
            throw new NotImplementedException();
        }
    }
}
