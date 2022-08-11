using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Npgsql;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver
{
    public class NetDBTransation : IDisposable
    {

        //DBC_Customer db = new DBC_Customer();
 
        public string SQL_TYPE { get; set; }

        public DbTransaction dbTransaction { get; set; }


        public NetDBTransation()
        {
            string DB_TYPE = GetDBTYype.instance;
            SQL_TYPE = DB_TYPE;
            #region   获取DbTransaction
            //dbTransaction=db.GetDbTransaction();
            #endregion 
        }

        public void TransationRollback(DbTransaction dbTransaction)
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Rollback();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Rollback();
                }
            }
        }
        public void TransationCommit(DbTransaction dbTransaction)
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Commit();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Commit();
                }
            }
        }


        public void TransationRollback()
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Rollback();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Rollback();
                }
            }
        }
        public void TransationCommit()
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Commit();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Commit();
                }
            }
        }



        public void Rollback()
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Rollback();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Rollback();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Rollback();
                }
            }
        }
        public void Complete()
        {
            if (dbTransaction.Connection != null)
            {
                if (SQL_TYPE == "SqlServer")
                {
                    SqlTransaction trans = (SqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "MySql")
                {

                    MySqlTransaction trans = (MySqlTransaction)dbTransaction;
                    trans.Commit();
                }
                else if (SQL_TYPE == "Oracle")
                {
                    OracleTransaction trans = (OracleTransaction)dbTransaction;
                    trans.Commit();

                }
                else if (SQL_TYPE == "PostgreSql")
                {
                    NpgsqlTransaction trans = (NpgsqlTransaction)dbTransaction;
                    trans.Commit();
                }
            }
        }






        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                   // db = null;
                    dbTransaction = null;
                }
                // Dispose UNMANAGED resources (like P/Invoke functions)
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            disposed = true;
        }


        ~NetDBTransation()
        {
            Dispose(false);
        }



    }
}
