﻿using Autofac;
using MemoryCacheModule.InMemoryCache;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.MySqlDriver;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.OracleDriver;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.PostgreSqlDriver;
using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.SqlServerDriver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver
{
  public  class DBC_ContainerBuilder
    {
        IDBDriver dBDriver = null;
        public string SQL_TYPE { get; set; }

        public DbTransaction dbTransaction;



        public DBC_ContainerBuilder(string constr, string key = null)
        {
            string DB_TYPE = GetDBTYype.instance;
            SQL_TYPE = DB_TYPE;
            switch (DB_TYPE)
            {
                case "SqlServer":
                    dBDriver = new SqlServerDB();
                    break;
                case "MySql":
                    dBDriver = new MySqlDB(constr, key);
                    break;
                case "Oracle":
                    dBDriver = new OracleDB(constr);
                    break;
                case "PostgreSql":
                    dBDriver = new PostgreSqlDB();
                    break;
            }
        }


        public Task<List<Hashtable>> SelectListAsync(string sql, Hashtable args)
        {
            List<Hashtable> arrayList = dBDriver.SelectList(sql, args);
            return Task.FromResult(arrayList);
        }

        #region SQL 查询语句
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        public ArrayList Select(string sql, Hashtable args)
        {
            ArrayList arrayList = dBDriver.Select(sql, args);
            return arrayList;
        }
        #endregion

        #region SQL 查询语句
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        public List<Hashtable> SelectList(string sql, Hashtable args, int IsNormalSmallOrBig = 0)
        {
            List<Hashtable> arrayList = dBDriver.SelectList(sql, args, IsNormalSmallOrBig);
            return arrayList;
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
            ArrayList arrayList = dBDriver.SelectsPecialProc(sql, args);
            return arrayList;
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
            int result = dBDriver.Execute(sql, args, dbTransaction);
            return result;
        }
        #endregion


        #region  增删改执行SQL带参数 带事务
        /// <summary>
        ///  增删改执行SQL带参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>INT</returns>
        //public int Execute(string sql, Hashtable args, DbTransaction dbTransaction)
        //{
        //    int result = dBDriver.Execute(sql, args, dbTransaction);
        //    return result;
        //}
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
            int result = dBDriver.ExecuteProc(sql, args, dbTransaction);
            return result;
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
            object result = dBDriver.GetTotalRecordWithParameter(sql, args);
            return result;
        }
        #endregion

        #region 获取自动增长的ID值
        /// <summary>
        ///  获取自动增长的ID值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>decimal</returns>
        public decimal ExcuteForNewID(string sql, Hashtable args, DbTransaction dbTransaction = null)
        {
            decimal result = dBDriver.ExcuteForNewID(sql, args, dbTransaction);
            return result;
        }
        #endregion

        #region 查询返回DataTable
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        //public DataTable SelectDataTable(string sql, Hashtable args, DbTransaction dbTransaction)
        //{
        //    DataTable result = dBDriver.SelectDataTable(sql, args,dbTransaction);
        //    return result;
        //}
        #endregion

        #region 查询返回DataTable 带事务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        public DataTable SelectDataTable(string sql, Hashtable args)
        {
            DataTable result = dBDriver.SelectDataTable(sql, args);
            return result;
        }
        #endregion

        #region 批量导入
        /// <summary>
        /// 批量导入
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="sourceDataTable">数据表</param>
        public void DataBulkCopyByDataTable(string rootPath, System.Data.DataTable sourceDataTable)
        {
            dBDriver.DataBulkCopyByDataTable(rootPath, sourceDataTable);
        }
        #endregion

        #region  SQL语句参数
        /// <summary>
        /// SQL语句参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetArgs(string sql, Hashtable args, System.Data.IDbCommand cmd)
        {
            dBDriver.SetArgs(sql, args, cmd);
        }
        #endregion

        #region 存储过程参数私有
        /// <summary>
        ///  存储过程参数私有
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetArgsProc(string sql, Hashtable args, System.Data.IDbCommand cmd)
        {
            dBDriver.SetArgsProc(sql, args, cmd);
        }
        #endregion

        #region 存储过程把表作为变量来传递
        /// <summary>
        /// 存储过程把表作为变量来传递
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        public void SetSpecialArgsProc(string sql, Hashtable args, System.Data.IDbCommand cmd)
        {
            dBDriver.SetSpecialArgsProc(sql, args, cmd);
        }
        #endregion

        #region  把DataTable转换成ArrayList
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        public ArrayList DataTable2ArrayList(System.Data.DataTable data)
        {
            ArrayList arrayList = dBDriver.DataTable2ArrayList(data);
            return arrayList;
        }
        #endregion

        #region  把DataTable转换成List<Hashtable>
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        public List<Hashtable> DataTable2List(System.Data.DataTable data)
        {
            List<Hashtable> arrayList = dBDriver.DataTable2List(data);
            return arrayList;
        }
        #endregion

        #region 打开链接
        public void OpenConnection()
        {
            dBDriver.OpenConnection();
        }
        #endregion

        #region 关闭连接
        public void CloseConnection()
        {
            dBDriver.CloseConnection();
        }
        #endregion

        #region 判断数据库连接关闭，空或者网络中断原因，在打开数据库连接
        public void ReOpenConnection()
        {
            dBDriver.ReOpenConnection();
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection()
        {
            return dBDriver.GetDBConnection();
        }
        #endregion

        #region 获取连接状态
        public DbConnection GetDBConnection(string ConnStr)
        {
            return dBDriver.GetDBConnection(ConnStr);
        }
        #endregion


        #region 获取事务
        public DbTransaction GetDbTransaction()
        {
            dbTransaction = dBDriver.GetDbTransaction();
            return dBDriver.GetDbTransaction();
        }
        #endregion


        #region 切换同一种数据库的不同数据库
        public int ChangeDataBase(string database)
        {
            return dBDriver.ChangeDataBase(database);

        }
        #endregion 

        public DataTable SelectDataTablePecialProc(string sql, Hashtable args)
        {
            DataTable arrayList = dBDriver.SelectDataTablePecialProc(sql, args);
            return arrayList;
        }


        public List<T> SelectListModel<T>(string sql, Hashtable args) where T : class, new()
        {
            return dBDriver.SelectListModel<T>(sql, args);
        }


        #region 分页小分队
        public Hashtable SelectPageList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            return dBDriver.SelectPageList(sql, page_index, page_size, arg, order_str);
        }
        public ArrayList SelectPageArrayList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            return dBDriver.SelectPageArrayList(sql, page_index, page_size, arg, order_str);
        }

        public DataTable SelectPageDataTable(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null)
        {
            return dBDriver.SelectPageDataTable(sql, page_index, page_size, arg, order_str);
        }
        #endregion

        #region  查询In小分队 
        public List<Hashtable> SelectListForIn(string sql, Hashtable args, string InIds, int IsNormalSmallOrBig = 0)
        {
            return dBDriver.SelectListForIn(sql, args, InIds, IsNormalSmallOrBig);
        }

        public DataTable SelectDataTableForIn(string sql, Hashtable args, string InIds)
        {
            return dBDriver.SelectDataTableForIn(sql, args, InIds);
        }
        #endregion

    }
}
