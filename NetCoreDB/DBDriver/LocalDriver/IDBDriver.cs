using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver
{
    public interface IDBDriver
    {
        #region SQL 查询语句
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        ArrayList Select(string sql, Hashtable args);
        #endregion

        #region SQL 查询语句
        /// <summary>
        /// SQL 查询语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        List<Hashtable> SelectList(string sql, Hashtable args,int IsNormalSmallOrBig=0);
        #endregion



        //#region SQL 查询语句 事务
        ///// <summary>
        ///// SQL 查询语句
        ///// </summary>
        ///// <param name="sql">SQL语句</param>
        ///// <param name="args">参数</param>
        ///// <returns>ArrayList动态数组</returns>
        //List<Hashtable> SelectList(string sql, Hashtable args, DbTransaction dbTransaction);
        //#endregion



        #region 查询特殊存储过程【参数是表】
        /// <summary>
        /// 特殊存储过程【参数是表】
        /// </summary>
        /// <param name="sql">存储过程名</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        ArrayList SelectsPecialProc(string sql, Hashtable args);
        #endregion




        #region 查询特殊存储过程 返回DataTable
        /// <summary>
        /// 特殊存储过程【参数是表】
        /// </summary>
        /// <param name="sql">存储过程名</param>
        /// <param name="args">参数</param>
        /// <returns>ArrayList动态数组</returns>
        DataTable SelectDataTablePecialProc(string sql, Hashtable args);
        #endregion





        #region  增删改执行SQL带参数
        /// <summary>
        ///  增删改执行SQL带参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>INT</returns>
        int Execute(string sql, Hashtable args, DbTransaction dbTransaction);
        #endregion



        //#region  增删改执行SQL带参数
        ///// <summary>
        /////  增删改执行SQL带参数
        ///// </summary>
        ///// <param name="sql">SQL语句</param>
        ///// <param name="args">参数</param>
        ///// <returns>INT</returns>
        //int Execute(string sql, Hashtable args,DbTransaction transaction);
        //#endregion




        #region 存过过程新增，删除，更新
        /// <summary>
        ///  存过过程新增，删除，更新
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>INT</returns>
        public int ExecuteProc(string sql, Hashtable args, DbTransaction dbTransaction);
        #endregion

        #region  获取集合总个数
        /// <summary>
        /// 获取集合总个数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>object</returns>
        object GetTotalRecordWithParameter(string sql, Hashtable args);
        #endregion

        #region 获取自动增长的ID值
        /// <summary>
        ///  获取自动增长的ID值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>decimal</returns>
        decimal ExcuteForNewID(string sql, Hashtable args, DbTransaction dbTransaction);
        #endregion

        #region 查询返回DataTable
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <returns>DataTable</returns>
        DataTable SelectDataTable(string sql, Hashtable args);
        #endregion



        //#region 查询返回DataTable 带事务
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sql">SQL语句</param>
        ///// <param name="args">参数</param>
        ///// <returns>DataTable</returns>
        //DataTable SelectDataTable(string sql, Hashtable args, DbTransaction dbTransaction);
        //#endregion


        #region 批量导入
        /// <summary>
        /// 批量导入
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="sourceDataTable">数据表</param>
        void DataBulkCopyByDataTable(string rootPath, System.Data.DataTable sourceDataTable);
        #endregion

        #region  SQL语句参数
        /// <summary>
        /// SQL语句参数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        void SetArgs(string sql, Hashtable args, System.Data.IDbCommand cmd);
        #endregion

        #region 存储过程参数私有
        /// <summary>
        ///  存储过程参数私有
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        void SetArgsProc(string sql, Hashtable args, System.Data.IDbCommand cmd);
        #endregion

        #region 存储过程把表作为变量来传递
        /// <summary>
        /// 存储过程把表作为变量来传递
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="args">参数</param>
        /// <param name="cmd">命令Command</param>
        void SetSpecialArgsProc(string sql, Hashtable args, System.Data.IDbCommand cmd);
        #endregion

        #region  把DataTable转换成ArrayList
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        ArrayList DataTable2ArrayList(System.Data.DataTable data);
        #endregion

        #region  把DataTable转换成List<Hashtable>
        /// <summary>
        /// 把DataTable转换成ArrayList
        /// </summary>
        /// <param name="data">DataTable</param>
        /// <returns>ArrayList</returns>
        List<Hashtable> DataTable2List(System.Data.DataTable data,int IsNormalSmallOrBig=0);
        #endregion

        #region 获取连接状态
        DbConnection GetDBConnection();
        #endregion


        #region 获取连接状态
        DbConnection GetDBConnection(string ConnectionStr);
        #endregion


        #region 打开链接
        void OpenConnection();
        #endregion

        #region 关闭连接
        void CloseConnection();
        #endregion

        #region 判断数据库连接关闭，空或者网络中断原因，在打开数据库连接
        void ReOpenConnection();
        #endregion


        #region 获取事务
        DbTransaction GetDbTransaction();
        #endregion


        #region 关闭事务
        DbTransaction CloseDbTransaction();
        #endregion

        #region 提交事务
        void CommitTransaction();
        #endregion

        #region 回滚事务
        void RollbackTransaction();
        #endregion



        #region  切换数据库
        int ChangeDataBase(string database);

        #endregion




        List<T> SelectListModel<T>(string sql, Hashtable args) where T : class, new();



        #region 分页小分队
        Hashtable SelectPageList(string sql,int page_index,int page_size,Hashtable arg=null,string order_str=null);
        ArrayList SelectPageArrayList(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null);

        DataTable SelectPageDataTable(string sql, int page_index, int page_size, Hashtable arg = null, string order_str = null);
        #endregion


        #region 查询IN
        List<Hashtable> SelectListForIn(string sql, Hashtable args, string InIds, int IsNormalSmallOrBig = 0);

        DataTable SelectDataTableForIn(string sql, Hashtable args, string InIds);
        #endregion 


    }
}
