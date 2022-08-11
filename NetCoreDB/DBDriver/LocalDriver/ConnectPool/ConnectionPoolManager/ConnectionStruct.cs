using MySql.Data.MySqlClient;
using System;

using System.Data;
using System.Data.Common;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver.ConnectPool.ConnectionPoolManager
{
    /// <summary>
    /// 连接池状态
    /// </summary>
    public enum PoolState
    {
        /// <summary>
        /// 刚刚创建的对象，表示该对象未被调用过StartSeivice方法。
        /// </summary>
        UnInitialize,
        /// <summary>
        /// 初始化中，该状态下服务正在按照参数初始化连接池。
        /// </summary>
        Initialize,
        /// <summary>
        /// 运行中
        /// </summary>
        Run,
        /// <summary>
        /// 停止状态
        /// </summary>
        Stop
    }

    /// <summary>
    /// 要申请连接的级别
    /// </summary>
    public enum ConnectionLevel
    {
        /// <summary>
        /// 独占方式，分配全新的连接资源，并且该连接资源在本次使用释放回连接池之前不能在分配出去。如果连接池只能分配引用记数类型连接资源则该级别将产生一个异常，标志连接池资源耗尽
        /// </summary>
        ReadOnly,
        /// <summary>
        /// 优先级-高，分配全新的连接资源，不使用引用记数技术。注：此级别不保证在分配后该连接资源后，仍然保持独立占有资源，若想独立占有资源请使用ReadOnely
        /// </summary>
        High,
        /// <summary>
        /// 优先级-中，适当应用引用记数技术分配连接
        /// </summary>
        None,
        /// <summary>
        /// 优先级-底，尽可能使用引用记数技术分配连接
        /// </summary>
        Bottom
    }

    /// <summary>
    /// 连接类型
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// ODBC 数据源
        /// </summary>
        Odbc,
        /// <summary>
        /// OLE DB 数据源
        /// </summary>
        OleDb,
        /// <summary>
        /// SqlServer 数据库连接
        /// </summary>
        SqlClient,
        /// <summary>
        /// 默认（无分配）
        /// </summary>
        None
    }

    /// <summary>
    /// 连接池中的一个连接类型
    /// </summary>
    public class ConnectionStruct : IDisposable
    {
        private bool _enable = true;//是否失效
        private bool _isCanAllot = true;//表示该连接是否可以被分配
        private bool _isUsing = false;//是否正在被使用中
        private DateTime _createTime = DateTime.Now;//创建时间
        private int _useDegree = 0;//被使用次数
        private int _repeatNow = 0;//当前连接被重复引用多少
        private bool _isRepeat = true;//连接是否可以被重复引用，当被分配出去的连接可能使用事务时，该属性被标识为true
        private ConnectionType _connType = ConnectionType.None;//连接类型
        private DbConnection _connection = null;//连接对象
        private object _obj = null;//连接附带的信息
       

        /// <summary>
        /// 连接池中的连接
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="connType">连接类型</param>
        public ConnectionStruct(DbConnection conn, ConnectionType connType)
        {
            _createTime = DateTime.Now;
            _connection = conn;
            _connType = connType;
        }

        /// <summary>
        /// 连接池中的连接
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="connType">连接类型</param>
        /// <param name="dt">连接创建时间</param>
        public ConnectionStruct(DbConnection conn, ConnectionType connType, DateTime time)
        {
            _createTime = time;
            _connection = conn;
            _connType = connType;
        }

        #region 属性部分
        /// <summary>
        /// 是否失效；false表示失效
        /// </summary>
        public bool Enable
        {
            get { return _enable; }
        }

        /// <summary>
        /// 表示该连接是否可以被分配
        /// </summary>
        public bool IsCanAllot
        {
            get { return _isCanAllot; }
            set { _isCanAllot = value; }
        }

        /// <summary>
        /// 是否正在被使用中
        /// </summary>
        public bool IsUsing
        {
            get { return _isUsing; }
            set { _isUsing = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _createTime; }
        }

        /// <summary>
        /// 被使用次数
        /// </summary>
        public int UseDegree
        {
            get { return _useDegree; }
        }

        /// <summary>
        /// 当前连接被重复引用多少
        /// </summary>
        public int RepeatNow
        {
            get { return _repeatNow; }
        }

        /// <summary>
        /// 连接是否可以被重复引用
        /// </summary>
        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { _isRepeat = value; }
        }

        /// <summary>
        /// 连接类型
        /// </summary> 
        public ConnectionType ConnType
        {
            get { return _connType; }
        }

        /// <summary>
        /// 连接对象
        /// </summary>
        public DbConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// 连接附带的信息
        /// </summary>
        public object Obj
        {
            get { return _obj; }
            set { _obj = value; }
        }

        /// <summary>
        /// 数据库连接状态
        /// </summary>
        public ConnectionState State
        {
            get { return _connection.State; }
        }
        #endregion

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// 关闭数据库连接 
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// 无条件将连接设置为失效
        /// </summary>
        public void SetConnectionLost()
        {
            _enable = false;
            _isCanAllot = false;
        }

        /// <summary>
        /// 被分配出去，线程安全的
        /// </summary>
        public void Repeat()
        {
            lock (this)
            {
                if (_enable == false)//连接是否可用
                {
                   // throw new ResLostnExecption();//连接资源已经失效
                }
                if (_isCanAllot == false)//是否可以被分配
                { 
                
                }                 //连接资源不可以被分配
                if (_isUsing == true && _isRepeat == false)
                    throw new AllotAndRepeatExecption();//连接资源已经被分配并且不允许重复引用
                _repeatNow++;//引用记数+1
                _useDegree++;//被使用次数+1
                _isUsing = true;//被使用
            }
        }

        /// <summary>
        /// 被释放回来，线程安全的
        /// </summary>
        public void Remove()
        {
            lock (this)
            {
                if (_enable == false)//连接可用
                    throw new ResLostnExecption();//连接资源已经失效
                if (_repeatNow == 0)
                    throw new RepeatIsZeroExecption();//引用记数已经为0
                _repeatNow--;//引用记数-1
                if (_repeatNow == 0)
                    _isUsing = false;//未使用
                else
                    _isUsing = true;//使用中
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _enable = false;
            _connection.Close();
            _connection = null;
        }
    }
}