
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.SqlServerDriver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.SQLiteDriver
{
   public  partial class SQLiteDB : IDBDriver
    {
        #region  数据库类型
        private string db_type;
        public string DB_TYPE
        {
            get { return db_type; }
            set
            {
                db_type = "SQLite";
            }
        }
        #endregion

        #region  获取连接字符串
        public DbConnection connection;

        private DbConnection GetDBConnect()
        {
            connection = GetSqlServerConnection.instance;
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
        public void DataBulkCopyByDataTable(string dataTableName, DataTable sourceDataTable)
        {


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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = (decimal)cmd.ExecuteScalar();
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
        public int Execute(string sql, Hashtable args)
        {
            int r = 0;
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                try
                {
                    if (args != null) SetArgs(sql, args, cmd);
                    r = cmd.ExecuteNonQuery();
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
        public int ExecuteProc(string sql, Hashtable args)
        {
            int r = 0;
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                try
                {
                    if (args != null) SetArgsProc(sql, args, cmd);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                    r = cmd.ExecuteNonQuery();
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
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
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
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SQLiteDataAdapter adapter = null;
                try
                {
                    using (adapter = new SQLiteDataAdapter(cmd))
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
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SQLiteDataAdapter adapter = null;
                try
                {
                    using (adapter = new SQLiteDataAdapter(cmd))
                    {

                        adapter.Fill(data);
                        adapter.Dispose();
                        aryl = DataTable2List(data.Tables[0]);
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

        #region SQL 查询语句 List<T>
        /// <summary>
        ///  SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>List<T></returns>
        public List<T> SelectListModel<T>(string sql, Hashtable args) where T : class, new()
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            List<T> aryl = null;
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SQLiteDataAdapter adapter = null;

                try
                {

                    using (adapter = new SQLiteDataAdapter(cmd))
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
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                if (args != null) SetArgs(sql, args, cmd);
                SQLiteDataAdapter adapter = null;
                try
                {
                    using (adapter = new SQLiteDataAdapter(cmd))
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


        #region 查询返回DataTable 带事务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        public DataTable SelectDataTable(string sql, Hashtable args, DbTransaction transaction)
        {
            System.Data.DataSet data = null;
            data = new System.Data.DataSet();
            System.Data.DataTable tb = new System.Data.DataTable();
            // data.EnforceConstraints = false; 
            DbConnection con = GetDBConnect();
            bool isConn = con != null;
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {
                transaction = con.BeginTransaction();
                cmd.Transaction = (SQLiteTransaction)transaction;
                if (args != null) SetArgs(sql, args, cmd);
                SQLiteDataAdapter adapter = null;
                try
                {
                    using (adapter = new SQLiteDataAdapter(cmd))
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
            using (SQLiteCommand cmd = new SQLiteCommand(sql, (SQLiteConnection)con))
            {//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象  
                if (args != null) SetSpecialArgsProc(sql, args, cmd);
                SQLiteDataAdapter adapter = null;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程  
                using (adapter = new SQLiteDataAdapter())
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

                cmd.Parameters.Add(new SQLiteParameter(key, value));
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

                cmd.Parameters.Add(new SQLiteParameter(key, value));
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
            cmd.CommandText = sql;
            SQLiteParameter sp = null;
            foreach (string str in args.Keys)
            {
                string key = "@" + str;

                Object value = args[str];
                if (value == null)
                {
                    value = args[str];
                }
                if (value == null) value = DBNull.Value;
                sp = new SQLiteParameter(key, System.Data.SqlDbType.Structured);//超级重要的
                sp.Value = value;
                cmd.Parameters.Add(sp);
            }
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

        #region 获取连接状态
        public DbConnection GetDBConnection()
        {
            return connection;
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnectionStr)
        {
            return connection;
        }
        #endregion

        #region  增删改执行SQL带参数 自带事务
        public int Execute(string sql, Hashtable args, DbTransaction transaction)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 获取事务
        public DbTransaction GetDbTransaction()
        {
            throw new NotImplementedException();
        }
        #endregion

        public List<Hashtable> SelectList(string sql, Hashtable args, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectDataTablePecialProc(string sql, Hashtable args)
        {
            throw new NotImplementedException();
        }

        public int ChangeDataBase(string database)
        {
            throw new NotImplementedException();
        }

        public List<Hashtable> SelectList(string sql, Hashtable args, int IsNormalSmallOrBig = 0)
        {
            throw new NotImplementedException();
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
