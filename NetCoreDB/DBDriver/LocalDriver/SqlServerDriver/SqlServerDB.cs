using Microsoft.Data.SqlClient;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.SqlServerDriver
{
    public class SqlServerDB : IDBDriver
    {
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

        #region  构造函数无参
        public SqlServerDB()
        {
            GetConstructDBConnect();
        }
        #endregion 


        #region  变量对象
        public DbConnection connection;//数据库连接对象
        public DbTransaction my_dbTransaction;//事物对象管理
        #endregion

        #region 获取连接对象
        private DbConnection GetConstructDBConnect()
        {
            // connection = GetSqlServerConnection.instance;
            if (connection == null)
            {
                connection = GetSqlServerConnectPool.getPool().getConnection();
            }
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
            return connection;
        }
        #endregion

        #region  连接对象---给所有方法
        public DbConnection GetDBConnect()
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
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection.ConnectionString, SqlBulkCopyOptions.Default))
                {
                    try
                    {
                        sqlBulkCopy.DestinationTableName = sourceDataTable.TableName;
                        sqlBulkCopy.BatchSize = sourceDataTable.Rows.Count;
                        //foreach (DataColumn dc in sourceDataTable.Columns)  //传入上述table
                        //{
                        //    sqlBulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);//将table中的列与数据库表这的列一一对应
                        //}
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
        public decimal ExcuteForNewID(string sql, Hashtable args, DbTransaction dbTransaction = null)
        {
            decimal r = 0;
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        my_dbTransaction = dbTransaction;
                        my_dbTransaction = con.BeginTransaction();
                        cmd.Transaction =(SqlTransaction)my_dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = (decimal)cmd.ExecuteScalar();
                    GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                         //dbTransaction= my_dbTransaction;
                        // my_dbTransaction = con.BeginTransaction();
                        cmd.Transaction = (SqlTransaction)dbTransaction;
                    }
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();
                    GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                try
                {
                    if (dbTransaction != null)
                    {
                        my_dbTransaction = dbTransaction;
                        my_dbTransaction = con.BeginTransaction();
                        cmd.Transaction = (SqlTransaction)my_dbTransaction;
                    }
                    if (args != null) SetArgsProc(sql, args, cmd);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                    r = cmd.ExecuteNonQuery();
                    GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                try
                {

                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteScalar();
                    GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SqlDataAdapter adapter = null;
                try
                {
                    using (adapter = new SqlDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = DataTable2ArrayList(data.Tables[0]);
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
        ///  SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>动态List</returns>
        public List<Hashtable> SelectList(string sql, Hashtable args, int IsNormalSmallOrBig = 0)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<Hashtable> aryl = null;
            DbConnection con = GetDBConnect();
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
                        adapter.Dispose();
                        aryl = DataTable2List(data.Tables[0], IsNormalSmallOrBig);
                        cmd.Parameters.Clear();
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
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
                        GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
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
                        aryl = DataTable2ArrayList(data.Tables[0]);
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
                //connection = GetSqlServerConnection.instance;
                connection = GetSqlServerConnectPool.getPool().getConnection();
            }
            return connection;
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnectionStr)
        {
            if (connection == null)
            {
                GetSqlServerConnectPool.getPool().ConnectionStr = ConnectionStr;
                connection = GetSqlServerConnectPool.getPool().getConnection();
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
            if (connection == null) { connection = GetSqlServerConnectPool.getPool().getConnection(); connection.Open(); }
            //数据库是关闭的
            if (connection.State == ConnectionState.Closed) connection.Open();
            //网络中断
            if (connection.State == ConnectionState.Broken) connection.Open();
        }
        #endregion

        #region  增删改执行SQL带参数 自带事务
        //public int Execute(string sql, Hashtable args, DbTransaction transaction)
        //{
        //    int r = 0;
        //    DbConnection con = GetDBConnect();
        //    bool isConn = con != null;
        //    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
        //    {
        //        if (transaction == null)
        //        {
        //            transaction = con.BeginTransaction();
        //            cmd.Transaction = (SqlTransaction)transaction;
        //            dbTransaction = cmd.Transaction;
        //        }
        //        else
        //        {
        //            cmd.Transaction= (SqlTransaction)transaction;
        //        }
        //        try
        //        {
        //            if (args != null) SetArgs(sql, args, cmd);
        //            r = cmd.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //             transaction.Rollback();
        //             con.Close();
        //             con = null;
        //            throw;

        //        }
        //    }

        //    if (isConn == false)
        //    {
        //        con.Close();
        //        con = null;
        //    }
        //    GetSqlServerConnectPool.getPool().closeConnection((SqlConnection)con);
        //    return r;
        // }
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

        #region 切换同一种数据库的不同数据库
        public int ChangeDataBase(string database)
        {
            DbConnection con = GetDBConnect();
            con.ChangeDatabase(database);
            return 0;
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
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                Microsoft.Data.SqlClient.SqlDataAdapter adapter = null;

                try
                {
                    using (adapter =new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = ConvertToModel<T>(data.Tables[0]);
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

        #region 分页部分
        public Hashtable SelectPageList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            Hashtable hashtable = null;
            hashtable = new Hashtable();
            try
            {
                int row_num= ((page_index - 1) * page_size);
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                string page_sql = string.Empty;
                string orderField = "GETDATE()";
                page_sql = string.Format("  SELECT TOP {2} WF.* FROM (SELECT ROW_NUMBER() OVER(ORDER BY {3}) as ROW_NUM,* from ({0}) O) as WF WHERE ROW_NUM > {1} ", sql, row_num, page_size, orderField);
                var list_page = SelectList(page_sql, arg, 2);
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
                int row_num = ((page_index - 1) * page_size);
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                string orderField = "GETDATE()";
                if (string.IsNullOrEmpty(order_str))
                {
                    page_sql = string.Format("  SELECT TOP {2} WF.* FROM (SELECT ROW_NUMBER() OVER(ORDER BY {3}) as ROW_NUM,* from ({0}) O) as WF WHERE ROW_NUM > {1} ", sql, row_num, page_size, orderField);
                }
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    orderField = LIST_ORDER[0];
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}",t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());

                    page_sql = string.Format("  SELECT TOP {2} WF.* FROM (SELECT ROW_NUMBER() OVER(ORDER BY {3}) as ROW_NUM,* from ({0}) O) as WF WHERE ROW_NUM > {1} ORDER BY {4} ", sql, row_num, page_size, orderField,wf_order);
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
                int row_num = ((page_index - 1) * page_size);
                string page_sql = string.Empty;
                int total_count = page_size * 10;//和google一样，只显示10页的链接 当点x页的时候，就显示x + 9页数最大，你可以参考google这种方式
                string orderField = "GETDATE()";
                if (string.IsNullOrEmpty(order_str))
                {
                    page_sql = string.Format("  SELECT TOP {2} WF.*,{4} AS TOTAL_COUNT FROM (SELECT ROW_NUMBER() OVER(ORDER BY {3}) as ROW_NUM,* from ({0}) O) as WF WHERE ROW_NUM > {1} ", sql, row_num, page_size, orderField,total_count);
                }
                else
                {
                    List<string> new_list_order = new List<string>();
                    List<string> LIST_ORDER = new List<string>(order_str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    orderField = LIST_ORDER[0];
                    foreach (string t in LIST_ORDER)
                    {
                        new_list_order.Add(string.Format(" WF.{0}", t));
                    }
                    string wf_order = string.Join(",", new_list_order.ToArray());

                    page_sql = string.Format("  SELECT TOP {2} WF.*,{5} AS TOTAL_COUNT FROM (SELECT ROW_NUMBER() OVER(ORDER BY {3}) as ROW_NUM,* from ({0}) O) as WF WHERE ROW_NUM > {1} ORDER BY {4} ", sql, row_num, page_size, orderField, wf_order,total_count);
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
        ///  此方法只限于主键的查询
        /// </summary>
        /// <param name="sql">只能写SELECT * FROM 表名 或者 SELECT * FROM 表名 WHERE ...</param>
        /// <param name="args">参数 比如 SELECT * FROM A WHERE ID=@ID AND NAME=@NAME  @ID和@NAME是参数 </param>
        /// <param name="InIds">要传的字符串 格式'A0001,A0002,A0003,A004'</param>
        /// <param name="IsNormalSmallOrBig">输出字段的格式 0:和数据库字段保持一致  1:全部小写   2:全部大写   默认是0</param>
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
            string sql_pk = @" SELECT A.COLUMN_NAME AS PKC
                                    FROM  INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE A JOIN
                                    (SELECT * FROM  SYSOBJECTS WHERE XTYPE=N'PK' ) B
                                    on OBJECT_ID(A.CONSTRAINT_NAME)=B.ID
                                    WHERE A.TABLE_NAME=@TABLE_NAME ";
            DataTable dataTable=SelectDataTable(sql_pk, hashtable);
            if (dataTable!=null&& dataTable.Rows.Count>0)
            {
                pk = dataTable.Rows[0]["PKC"].ToString();
            }

            return pk;
        }
        #endregion 


        public DbTransaction CloseDbTransaction()
        {


            connection.BeginTransaction().Dispose();
            my_dbTransaction = null;
            //if (my_dbTransaction == null)
            //{
            //    my_dbTransaction = connection.BeginTransaction();
            //}
            //else
            //{
            //    if (my_dbTransaction.Connection == null)
            //    {
            //        DbConnection conn = GetDBConnect();
            //        string conn_str = AppConfigurtaionServices.Configuration.GetSection("ConnectionStrings:SRVDatabase").Value;
            //        ConnectionPool = new ConnectPool.ConnectionPoolManager.ConnectionPool(conn_str, ConnectionType.Odbc);
            //        ConnectionPool.StartServices();
            //        connection = ConnectionPool.GetConnectionFormPool("steve", ConnectionLevel.Bottom);
            //        my_dbTransaction = connection.BeginTransaction();
            //    }

            //}
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


