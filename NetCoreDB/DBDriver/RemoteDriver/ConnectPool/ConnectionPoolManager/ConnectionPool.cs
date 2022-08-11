using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Collections;
using System.Data.Odbc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Npgsql;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver.ConnectPool.ConnectionPoolManager
{
    /// <summary>
    /// 数据库连接池，默认数据库连接方案是ODBC
    /// </summary>
    public class ConnectionPool : IDisposable
    {
        #region 字段与变量
        private int _realFormPool;//连接池中存在的实际连接数(包含失效的连接)
        private int _potentRealFormPool;//连接池中存在的实际连接数(有效的实际连接)
        private int _spareRealFormPool;//空闲的实际连接
        private int _useRealFormPool;//已分配的实际连接
        private int _readOnlyFormPool;//连接池已经分配多少只读连接
        private int _useFormPool;//已经分配出去的连接数
        private int _spareFormPool;//目前可以提供的连接数
        private int _maxConnection;//最大连接数，最大可以创建的连接数目
        private int _minConnection;//最小连接数
        private int _seepConnection;//每次创建连接的连接数
        private int _keepRealConnection;//保留的实际空闲连接，以攻可能出现的ReadOnly使用，当空闲连接不足该数值时，连接池将创建seepConnection个连接
        private int _exist = 20;//每个连接生存期限 20分钟
        private int _maxRepeatDegree = 5;//可以被重复使用次数（引用记数），当连接被重复分配该值所表示的次数时，该连接将不能被分配出去
        //当连接池的连接被分配尽时，连接池会在已经分配出去的连接中，重复分配连接（引用记数）。来缓解连接池压力
        private DateTime _startTime;//服务启动时间
        private string _connString = null;//连接字符串
        private ConnectionType _connType;//连接池连接类型
        private PoolState _poolState;//连接池状态
        //内部对象
        public List<ConnectionStruct> al_All = new List<ConnectionStruct>();//实际连接
        public Hashtable hstUsingConn = new Hashtable();//正在使用的连接
        private System.Timers.Timer time;//监视器记时器

        private Thread threadCreate;//创建线程
        private bool isThreadCheckRun = false;
        private int db_type;
        //private Mutex mUnique = new Mutex();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        public ConnectionPool(string connectionString)
        {
            InitConnectionPool(connectionString, ConnectionType.Odbc, 200, 30, 10, 5, 5);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="connType">数据库连接类型</param>
        public ConnectionPool(string connectionString, ConnectionType connType)
        {
            InitConnectionPool(connectionString, connType, 200, 30, 10, 5, 5);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="connType">数据库连接类型</param>
        /// <param name="maxConnection">最大连接数，最大可以创建的连接数目</param>
        /// <param name="minConnection">最小连接数</param>
        public ConnectionPool(string connectionString, ConnectionType connType, int maxConnection, int minConnection,int db_types)
        {
            db_type = db_types;
            InitConnectionPool(connectionString, connType, maxConnection, minConnection, 10, 5, 5);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="connType">数据库连接类型</param>
        /// <param name="maxConnection">最大连接数，最大可以创建的连接数目</param>
        /// <param name="minConnection">最小连接数</param>
        /// <param name="seepConnection">每次创建连接的连接数</param>
        /// <param name="keepConnection">保留连接数，当空闲连接不足该数值时，连接池将创建seepConnection个连接</param>
        public ConnectionPool(string connectionString, ConnectionType connType, int maxConnection,
            int minConnection, int seepConnection, int keepConnection)
        {
            InitConnectionPool(connectionString, connType, maxConnection, minConnection, seepConnection, keepConnection, 5);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="connType">数据库连接类型</param>
        /// <param name="maxConnection">最大连接数，最大可以创建的连接数目</param>
        /// <param name="minConnection">最小连接数</param>
        /// <param name="seepConnection">每次创建连接的连接数</param>
        /// <param name="keepConnection">保留连接数，当空闲连接不足该数值时，连接池将创建seepConnection个连接</param>
        /// <param name="keepRealConnection">当空闲的实际连接不足该值时创建连接，直到达到最大连接数</param>
        public ConnectionPool(string connectionString, ConnectionType connType, int maxConnection,
            int minConnection, int seepConnection, int keepConnection, int keepRealConnection)
        {
            InitConnectionPool(connectionString, connType, maxConnection, minConnection, seepConnection, keepConnection, keepRealConnection);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="connType">数据库连接类型</param>
        /// <param name="maxConnection">最大连接数，最大可以创建的连接数目</param>
        /// <param name="minConnection">最小连接数</param>
        /// <param name="seepConnection">每次创建连接的连接数</param>
        /// <param name="keepConnection">保留连接数，当空闲连接不足该数值时，连接池将创建seepConnection个连接</param>
        /// <param name="keepRealConnection">当空闲的实际连接不足该值时创建连接，直到达到最大连接数</param>
        protected void InitConnectionPool(string connectionString, ConnectionType connType, int maxConnection,
            int minConnection, int seepConnection, int keepConnection, int keepRealConnection)
        {
            if (connType == ConnectionType.None)
                throw new ConnTypeExecption();//参数不能是None
            _poolState = PoolState.UnInitialize;
            this._connString = connectionString;
            this._connType = connType;
            this._minConnection = minConnection;
            this._seepConnection = seepConnection;
            this._keepRealConnection = keepRealConnection;
            this._maxConnection = maxConnection;
            this.time = new System.Timers.Timer(500);
            this.time.Stop();
            this.time.Elapsed += new System.Timers.ElapsedEventHandler(time_Elapsed);//检查事件
            this.threadCreate = new Thread(new ThreadStart(CreateThreadProcess));//创建连接线程
        }
        #endregion

        #region 属性
        /// <summary>
        /// 连接池服务状态
        /// </summary>
        public PoolState State
        {
            get { return _poolState; }
        }

        /// <summary>
        /// 连接池是否启动，改变该属性将相当于调用StartServices或StopServices方法，
        /// 注：只有连接池处于Run，Stop状态情况下才可以对此属性赋值
        /// </summary>
        public bool Enable
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (_poolState == PoolState.Run || _poolState == PoolState.Stop)
                {
                    if (value == true)
                    {
                        StartServices();
                    }
                    else
                    {
                        StopServices();
                    }
                }
                else
                {
                    throw new SetValueExecption();//只有连接池处于Run，Stop状态情况下才可以对此属性赋值
                }
            }
        }

        /// <summary>
        /// 得到或设置连接类型
        /// </summary>
        public ConnectionType ConnectionType
        {
            get { return _connType; }
            set
            {
                if (_poolState == PoolState.Stop)
                {
                    _connType = value;
                }
                else
                {
                    throw new SetValueExecption();//只有在Stop状态时才可操作
                }
            }
        }

        /// <summary>
        /// 连接池使用的连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return _connString; }
            set
            {
                if (_poolState == PoolState.Stop)
                {
                    _connString = value;
                }
                else
                {
                    throw new SetValueExecption();//只有在Stop状态时才可操作
                }
            }
        }

        /// <summary>
        /// 得到服务器运行时间
        /// </summary>
        public DateTime RunTime
        {
            get
            {
                if (_poolState == PoolState.Stop)
                {
                    return new DateTime(DateTime.Now.Ticks - _startTime.Ticks);
                }
                else
                {
                    return new DateTime(0);
                }
            }
        }

        /// <summary>
        /// 最小连接数
        /// </summary>
        public int MinConnection
        {
            get { return _minConnection; }
            set
            {
                if (value < _maxConnection && value > 0 && value >= _keepRealConnection)
                {
                    _minConnection = value;
                }
                else
                {
                    //参数范围应该在 0~MaxConnection之间，并且应该大于KeepConnection
                    throw new ParameterBoundExecption();
                }
            }
        }

        /// <summary>
        /// 最大连接数，最大可以创建的连接数目
        /// </summary>
        public int MaxConnection
        {
            get { return _maxConnection; }
            set
            {
                if (value >= _minConnection && value > 0)
                {
                    _maxConnection = value;
                }
                else
                {
                    throw new ParameterBoundExecption();//参数范围错误，参数应该大于minConnection
                }
            }
        }

        /// <summary>
        /// 每次创建连接的连接数
        /// </summary>
        public int SeepConnection
        {
            get { return _seepConnection; }
            set
            {
                if (value > 0 && value < _maxConnection)
                {
                    _seepConnection = value;
                }
                else
                {
                    throw new ParameterBoundExecption();//创建连接的步长应大于0，同时小于MaxConnection
                }
            }
        }

        /// <summary>
        /// 保留的实际空闲连接，以攻可能出现的ReadOnly使用
        /// </summary>
        public int KeepRealConnection
        {
            get { return _keepRealConnection; }
            set
            {
                if (value >= 0 && value < _maxConnection)
                {
                    _keepRealConnection = value;
                }
                else
                {
                    throw new ParameterBoundExecption();//保留连接数应大于等于0，同时小于MaxConnection
                }
            }
        }

        /// <summary>
        /// 自动清理连接池的时间间隔
        /// </summary>
        public double Interval
        {
            get { return time.Interval; }
            set { time.Interval = value; }
        }

        /// <summary>
        /// 每个连接生存期限(单位分钟)，默认20分钟
        /// </summary>
        public int Exist
        {
            get { return _exist; }
            set
            {
                if (_poolState == PoolState.Stop)
                {
                    _exist = value;
                }
                else
                {
                    throw new PoolNotStopException();//只有在Stop状态下才可以操作
                }
            }
        }

        /// <summary>
        /// 可以被重复使用次数（引用记数）当连接被重复分配该值所表示的次数时，该连接将不能被分配出去。
        /// 当连接池的连接被分配尽时，连接池会在已经分配出去的连接中，重复分配连接（引用记数）。来缓解连接池压力
        /// </summary>
        public int MaxRepeatDegree
        {
            get { return _maxRepeatDegree; }
            set
            {
                if (value >= 0)
                {
                    _maxRepeatDegree = value;
                }
                else
                {
                    throw new ParameterBoundExecption();//重复引用次数应大于等于0
                }
            }
        }

        /// <summary>
        /// 连接池最多可以提供多少个连接
        /// </summary>
        public int MaxConnectionFormPool
        {
            get { return _maxConnection * _maxRepeatDegree; }
        }

        /// <summary>
        /// 连接池中存在的实际连接数(有效的实际连接)
        /// </summary>
        public int PotentRealFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _potentRealFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 连接池中存在的实际连接数(包含失效的连接)
        /// </summary>
        public int RealFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _realFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 空闲的实际连接
        /// </summary>
        public int SpareRealFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _spareRealFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 已分配的实际连接
        /// </summary>
        public int UseRealFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _useRealFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 连接池已经分配多少只读连接
        /// </summary>
        public int ReadOnlyFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _readOnlyFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 已经分配的连接数
        /// </summary>
        public int UseFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _useFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }

        /// <summary>
        /// 目前可以提供的连接数
        /// </summary>
        public int SpareFormPool
        {
            get
            {
                if (_poolState == PoolState.Run)
                {
                    return _spareFormPool;
                }
                else
                {
                    throw new PoolNotRunException();//连接池处在非运行中
                }
            }
        }
        #endregion

        #region 启动/停止服务
        /// <summary>
        /// 启动服务，线程安全，同步调用
        /// </summary>
        public void StartServices()
        {
            StartServices(false);
        }

        /// <summary>
        /// 启动服务，线程安全
        /// </summary>
        /// <param name="ansy">是否异步调用True为是，异步调用指，用户调用该方法后，无须等待创建结束就可继续做其他操作</param>
        public void StartServices(bool ansy)
        {
            lock (this)
            {
                createThreadMode = 0;//工作模式0
                createThreadProcessRun = true;
                createThreadProcessTemp = _minConnection;
                if (_poolState == PoolState.UnInitialize)
                    threadCreate.Start();
                else if (_poolState == PoolState.Stop)
                    threadCreate.Interrupt();
                else
                    throw new PoolNotStopException();//服务已经运行或者未完全结束
                time.Start();
            }

            if (!ansy)
            {
                //等待可能存在的创建线程结束
                while (threadCreate.ThreadState != ThreadState.WaitSleepJoin)
                {
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        /// 停止服务，线程安全
        /// </summary>
        public void StopServices()
        {
            StopServices(false);
        }

        /// <summary>
        /// 停止服务，线程安全
        /// <param name="needs">是否必须退出；如果指定为false与StartServices()功能相同，如果指定为true。
        /// 将未收回的连接资源关闭，这将是危险的。认为可能你的程序正在使用此资源。</param>
        /// </summary>
        public void StopServices(bool needs)
        {
            lock (this)
            {
                if (_poolState == PoolState.Run)
                {
                    lock (hstUsingConn)
                    {
                        if (needs == true)//必须退出
                        {
                            hstUsingConn.Clear();
                        }
                        else
                        {
                            if (hstUsingConn.Count != 0)
                            {
                                throw new ResCallBackException();//连接池资源未全部回收
                            }
                        }
                    }
                    time.Stop();
                    while (isThreadCheckRun) { Thread.Sleep(50); }//等待timer事件结束
                    createThreadProcessRun = false;
                    //等待可能存在的创建线程结束
                    while (threadCreate.ThreadState != ThreadState.WaitSleepJoin)
                    {
                        Thread.Sleep(50);
                    }

                    lock (al_All)
                    {
                        for (int i = 0; i < al_All.Count; i++)
                            al_All[i].Dispose();
                        al_All.Clear();
                    }
                    _poolState = PoolState.Stop;
                }
                else
                    throw new PoolNotRunException();//服务未启动
            }
            UpdateAttribute();//更新属性
        }

        /// <summary>
        /// 释放[系统自动调用不必关心]
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.StopServices();
                threadCreate.Abort();
            }
            catch (Exception e) { }
        }
        #endregion

        #region 获得/释放连接
        /// <summary>
        /// 在连接池中申请一个连接，使用None级别，线程安全
        /// </summary>
        /// <param name="gui">发起者</param>
        /// <returns>返回申请到的连接</returns>
        public DbConnection GetConnectionFormPool(object key)
        {
            return GetConnectionFormPool(key, ConnectionLevel.None);
        }

        /// <summary>
        /// 在连接池中申请一个连接，线程安全
        /// </summary>
        /// <param name="key">申请者</param>
        /// <param name="connLevel">申请的连接级别</param>
        /// <returns>返回申请到的连接</returns>
        public DbConnection GetConnectionFormPool(object key, ConnectionLevel connLevel)
        {
            lock (this)
            {
                if (_poolState != PoolState.Run)
                    throw new StateException();//服务状态错误
                if (hstUsingConn.Count == MaxConnectionFormPool)
                    throw new PoolFullException();//连接池已经饱和，不能提供连接
                if (hstUsingConn.ContainsKey(key))
                { 
                
                }
                    //throw new KeyExecption();//一个key对象只能申请一个连接

                if (connLevel == ConnectionLevel.ReadOnly)
                    return GetConnectionFormPool_ReadOnly(key);//ReadOnly级别
                else if (connLevel == ConnectionLevel.High)
                    return GetConnectionFormPool_High(key);//High级别
                else if (connLevel == ConnectionLevel.None)
                    return GetConnectionFormPool_None(key);//None级别
                else
                    return GetConnectionFormPool_Bottom(key);//Bottom级别
            }
        }

        /// <summary>
        /// 申请一个连接资源，只读方式，线程安全
        /// </summary>
        /// <param name="key">申请者</param>
        /// <returns>申请到的连接对象</returns>
        protected DbConnection GetConnectionFormPool_ReadOnly(object key)
        {
            ConnectionStruct connStruct = null;
            for (int i = 0; i < al_All.Count; i++)
            {
                connStruct = al_All[i];
                if (connStruct.Enable == false || connStruct.IsCanAllot == false
                    || connStruct.UseDegree == _maxRepeatDegree || connStruct.IsUsing == true)
                    continue;
                return GetConnectionFormPool_Return(key, connStruct, ConnectionLevel.ReadOnly); //返回得到的连接
            }
            return GetConnectionFormPool_Return(key, null, ConnectionLevel.ReadOnly);
        }

        /// <summary>
        /// 申请一个连接资源，优先级-高，线程安全
        /// </summary>
        /// <param name="key">申请者</param>
        /// <returns>申请到的连接对象</returns>
        protected DbConnection GetConnectionFormPool_High(object key)
        {
            ConnectionStruct ConnStruct = null;
            ConnectionStruct ConnStructTemp = null;

            for (int i = 0; i < al_All.Count; i++)
            {
                ConnStructTemp = al_All[i];
                //不可以分配跳出本次循环。
                if (ConnStructTemp.Enable == false || ConnStructTemp.IsCanAllot == false || ConnStructTemp.UseDegree == _maxRepeatDegree)
                {
                    ConnStructTemp = null;
                    continue;
                }
                if (ConnStructTemp.UseDegree == 0)//得到最合适的
                {
                    ConnStruct = ConnStructTemp;
                    break;
                }
                else//不是最合适的放置到最佳选择中
                {
                    if (ConnStruct != null)
                    {
                        if (ConnStructTemp.UseDegree < ConnStruct.UseDegree)
                            //与上一个最佳选择选出一个最佳的放置到ConnStruct中
                            ConnStruct = ConnStructTemp;
                    }
                    else
                        ConnStruct = ConnStructTemp;
                }
            }
            return GetConnectionFormPool_Return(key, ConnStruct, ConnectionLevel.High);//返回最合适的连接
        }

        /// <summary>
        /// 申请一个连接资源，优先级-中，线程安全
        /// </summary>
        /// <param name="key">申请者</param>
        /// <returns>申请到的连接对象</returns>
        protected DbConnection GetConnectionFormPool_None(object key)
        {
            List<ConnectionStruct> al = new List<ConnectionStruct>();
            ConnectionStruct ConnStruct = null;
            for (int i = 0; i < al_All.Count; i++)
            {
                ConnStruct = al_All[i];
                if (ConnStruct.Enable == false || ConnStruct.IsCanAllot == false || ConnStruct.UseDegree == _maxRepeatDegree)//不可以分配跳出本次循环。
                    continue;

                if (ConnStruct.IsCanAllot == true)
                    al.Add(ConnStruct);
            }
            if (al.Count == 0)
                return GetConnectionFormPool_Return(key, null, ConnectionLevel.None);//发出异常
            else
                return GetConnectionFormPool_Return(key, (al[al.Count / 2]), ConnectionLevel.None);//返回连接
        }

        /// <summary>
        /// 申请一个连接资源，优先级-低，线程安全
        /// </summary>
        /// <param name="key">申请者</param>
        /// <returns>申请到的连接对象</returns>
        protected DbConnection GetConnectionFormPool_Bottom(object key)
        {
            ConnectionStruct connStruct = null;
            ConnectionStruct connStructTemp = null;

            for (int i = 0; i < al_All.Count; i++)
            {
                connStructTemp = al_All[i];
                if (connStructTemp.Enable == false || connStructTemp.IsCanAllot == false
                    || connStructTemp.UseDegree == _maxRepeatDegree)//不可以分配跳出本次循环。
                {
                    connStructTemp = null;
                    continue;
                }
                else//不是最合适的放置到最佳选择中
                {
                    if (connStruct != null)
                    {
                        if (connStructTemp.UseDegree > connStruct.UseDegree)
                            //与上一个最佳选择选出一个最佳的放置到ConnStruct中
                            connStruct = connStructTemp;
                    }
                    else
                        connStruct = connStructTemp;
                }
            }
            return GetConnectionFormPool_Return(key, connStruct, ConnectionLevel.Bottom);//返回最合适的连接
        }

        /// <summary>
        /// 返回DbConnection对象，同时做获得连接时的必要操作
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="ConnStruct">ConnectionStruct对象</param>
        /// <param name="connLevel">级别</param>
        /// <param name="readOnly">是否为只读属性</param>
        /// <returns></returns>
        private DbConnection GetConnectionFormPool_Return(object key, ConnectionStruct connStruct, ConnectionLevel connLevel)
        {
            try
            {
                if (connStruct == null)
                {
                    if (al_All.Count > 0)
                    {
                        connStruct = al_All[0];
                    }
                    else
                    {
                       // connStruct


                    }

                }

                //if (connStruct.Enable == false)
                //{
                //    connStruct.Enable = true;
                //}
                   
                connStruct.Repeat();
                hstUsingConn.Add(key, connStruct);
                if (connLevel == ConnectionLevel.ReadOnly)
                {
                    connStruct.IsCanAllot = false;
                    connStruct.IsRepeat = false;
                }
            }
            catch (Exception e)
            {
                //throw new OccasionExecption();//连接资源耗尽，或错误的访问时机。
            }
            finally
            {
                UpdateAttribute();//更新属性
            }
            return connStruct.Connection;
        }

        /// <summary>
        /// 释放申请的数据库连接对象，线程安全
        /// <param name="key">key表示数据库连接申请者</param>
        /// </summary>
        public void DisposeConnection(object key)
        {
            lock (hstUsingConn)
            {
                ConnectionStruct ConnStruct = null;
                if (_poolState == PoolState.Run)
                {
                    if (!hstUsingConn.ContainsKey(key))
                        throw new NotKeyExecption();//无法释放，不存在的key
                    ConnStruct = (ConnectionStruct)hstUsingConn[key];
                    ConnStruct.IsRepeat = true;
                    if (ConnStruct.IsCanAllot == false)
                        if (ConnStruct.Enable == true)
                            ConnStruct.IsCanAllot = true;
                    ConnStruct.Remove();
                    hstUsingConn.Remove(key);
                }
                else
                    throw new PoolNotRunException();//服务未启动
            }
            UpdateAttribute();//更新属性
        }
        #endregion

        #region 私有方法
        private int createThreadMode = 0;//创建线程工作模式
        private int createThreadProcessTemp = 0;//需要创建的连接数
        private bool createThreadProcessRun = false;//是否决定创建线程将继续工作，如果不继续工作则线程会将自己处于阻止状态
        /// <summary>
        /// 创建线程
        /// </summary>
        private void CreateThreadProcess()
        {
            bool join = false;
            int createThreadProcessTemp_inside = createThreadProcessTemp;
            _poolState = PoolState.Initialize;
            while (true)
            {
                join = false;
                _poolState = PoolState.Run;
                if (createThreadProcessRun == false)
                {
                    //遇到终止命令
                    try
                    {
                        threadCreate.Join();
                    }
                    catch (Exception e) { }
                }
                else
                {
                    if (createThreadMode == 0)
                    {
                        //------------------------begin mode  创建模式
                        lock (al_All)//lock类似于synchronized
                        {
                            if (al_All.Count < createThreadProcessTemp_inside)
                                al_All.Add(CreateConnection(_connString, _connType));
                            else
                                join = true;
                        }
                        //------------------------end mode
                    }
                    else if (createThreadMode == 1)
                    {
                        //------------------------begin mode  增加模式
                        lock (al_All)
                        {
                            if (createThreadProcessTemp_inside != 0)
                            {
                                createThreadProcessTemp_inside--;
                                al_All.Add(CreateConnection(_connString, _connType));
                            }
                            else
                                join = true;
                        }
                        //------------------------end mode
                    }
                    else
                        join = true;
                    //-------------------------------------------------------------------------
                    if (join == true)
                    {
                        UpdateAttribute();//更新属性
                        try
                        {
                            createThreadProcessTemp = 0;
                            threadCreate.Join();
                        }
                        catch (Exception e)
                        { createThreadProcessTemp_inside = createThreadProcessTemp; }//得到传入的变量
                    }
                }
            }
        }

        /// <summary>
        /// 检测事件
        /// </summary>
        private void time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectionStruct ConnStruct = null;
            time.Stop();//关闭自己
            isThreadCheckRun = true;
            //如果正在执行创建连接则退出
            if (threadCreate.ThreadState != ThreadState.WaitSleepJoin)
                return;
            //------------------------------------------------------
            lock (al_All)
            {
                //int n = 0;
                for (int i = 0; i < al_All.Count; i++)
                {
                    ConnStruct = al_All[i];
                    TestConnStruct(ConnStruct);//测试
                    if (ConnStruct.Enable == false && ConnStruct.RepeatNow == 0)//没有引用的失效连接
                    {
                        ConnStruct.Close();//关闭它
                        al_All.Remove(ConnStruct);//删除
                    }
                }
            }
            //------------------------------------------------------
            UpdateAttribute();//更新属性

            if (_spareRealFormPool < _keepRealConnection)//保留空闲实际连接数不足
                createThreadProcessTemp = GetNumOf(_realFormPool, _seepConnection, _maxConnection);
            else
                createThreadProcessTemp = 0;
            //if (createThreadProcessTemp != 0)
            //    Console.WriteLine("创建" + createThreadProcessTemp);

            if (createThreadProcessTemp != 0)
            {
                //启动创建线程，工作模式1
                createThreadMode = 1;
                threadCreate.Interrupt();
            }

            isThreadCheckRun = false;
            time.Start();//打开自己
        }

        /// <summary>
        /// 得到当前要增加的量
        /// </summary>
        private int GetNumOf(int nowNum, int seepNum, int maxNum)
        {
            if (maxNum >= nowNum + seepNum)
                return seepNum;
            else
                return maxNum - nowNum;
        }

        /// <summary>
        /// 用指定类型创建连接
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="cte">连接类型</param>
        /// <param name="dt">连接超时时间</param>
        /// <returns>返回创建的连接</returns>
        private ConnectionStruct CreateConnection(string conn, ConnectionType cte)
        {


            // case 0://SqlServer
            //        dBDriver = new SqlServerDB(Constr);
            //break;
            //    case 1://Oracle
            //        dBDriver = new OracleDB(Constr);
            //break;
            //    case 2://MySql
            //        dBDriver = new MySqlDB(Constr);
            //break;
            //    case 3://Mongodb
            //        dBDriver = new PostgreSqlDB();
            //break;
            //    case 4://PostgreSql








            DbConnection db = null;
            if (cte == ConnectionType.Odbc)
            {

                //string DB_TYPE = GetDBTYype.instance;
                switch (db_type)
                {
                    case 0:
                        db = new SqlConnection(conn);
                        break;
                    case 2:
                        db = new MySqlConnection(conn);
                        break;
                    case 1:
                        db = new OracleConnection(conn);
                        break;
                    case 4:
                        db = new NpgsqlConnection(conn);
                        break;
                }
                //db = new OdbcConnection(conn);//ODBC数据源连接
            }
            else if (cte == ConnectionType.OleDb)
            {
                db = new System.Data.OleDb.OleDbConnection(conn);//OLE DB数据连接
            }
            else if (cte == ConnectionType.SqlClient)
            {
                db = new SqlConnection(conn);//SqlServer数据库连接
            }
            ConnectionStruct ConnStruct = new ConnectionStruct(db, cte, DateTime.Now);
            //if (ConnStruct.State == ConnectionState.Closed)
            //{
            //    ConnStruct.Open();
            //}
            return ConnStruct;
        }

        /// <summary>
        /// 测试ConnectionStruct是否过期
        /// </summary>
        /// <param name="ConnStruct">被测试的ConnectionStruct</param>
        private void TestConnStruct(ConnectionStruct ConnStruct)
        {
            //此次被分配出去的连接是否在此次之后失效
            if (ConnStruct.UseDegree == _maxRepeatDegree)
                ConnStruct.SetConnectionLost();//超过使用次数
            if (ConnStruct.CreateTime.AddMinutes(_exist).Ticks <= DateTime.Now.Ticks)
                ConnStruct.SetConnectionLost();//连接超时
            if (ConnStruct.Connection.State == ConnectionState.Closed)
                ConnStruct.SetConnectionLost();//连接被关闭
        }

        /// <summary>
        /// 更新属性
        /// </summary>
        private void UpdateAttribute()
        {
            int temp_readOnlyFormPool = 0;//连接池已经分配多少只读连接
            int temp_potentRealFormPool = 0;//连接池中存在的实际连接数(有效的实际连接)
            int temp_spareRealFormPool = 0;//空闲的实际连接
            int temp_useRealFormPool = 0;//已分配的实际连接
            int temp_spareFormPool = MaxConnectionFormPool;//目前可以提供的连接数
            //---------------------------------
            lock (hstUsingConn)
            {
                _useFormPool = hstUsingConn.Count;
            }
            //---------------------------------
            ConnectionStruct ConnStruct = null;
            //int n = 0;
            lock (al_All)
            {
                _realFormPool = al_All.Count;
                for (int i = 0; i < al_All.Count; i++)
                {
                    ConnStruct = al_All[i];
                    //只读
                    if (ConnStruct.IsCanAllot == false && ConnStruct.IsUsing == true && ConnStruct.IsRepeat == false)
                        temp_readOnlyFormPool++;
                    //有效的实际连接
                    if (ConnStruct.Enable == true)
                        temp_potentRealFormPool++;
                    //空闲的实际连接
                    if (ConnStruct.Enable == true && ConnStruct.IsUsing == false)
                        temp_spareRealFormPool++;
                    //已分配的实际连接
                    if (ConnStruct.IsUsing == true)
                        temp_useRealFormPool++;
                    //目前可以提供的连接数
                    if (ConnStruct.IsCanAllot == true)
                        temp_spareFormPool = temp_spareFormPool - ConnStruct.RepeatNow;
                    else
                        temp_spareFormPool = temp_spareFormPool - _maxRepeatDegree;
                }
            }
            _readOnlyFormPool = temp_readOnlyFormPool;
            _potentRealFormPool = temp_potentRealFormPool;
            _spareRealFormPool = temp_spareRealFormPool;
            _useRealFormPool = temp_useRealFormPool;
            _spareFormPool = temp_spareFormPool;
        }
        #endregion
    }
}
