using MemoryCacheModule.InMemoryCache;

using Autofac;
using System;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver
{
    public class MyContainerBuilder
    {
       // MemoryCacheHelper memoryCacheHelper = new MemoryCacheHelper();
        public DBC_Customer GetMyContainer(string constr)
        {
            ContainerBuilder cb = new ContainerBuilder();
            IContainer Container;
            cb.RegisterType<DBC_Customer>();//////////////////.SingleInstance();
            Container = cb.Build();
            var scope = Container.BeginLifetimeScope();
            var GG = scope.Resolve<DBC_Customer>(
                     new NamedParameter[] { new NamedParameter("Constr", constr) }
                    );
            return GG;
        }




        public DBC_Customer GetMyContainer(string constr,int db_type)
        {
            ContainerBuilder cb = new ContainerBuilder();
            IContainer Container;
            cb.RegisterType<DBC_Customer>();//////////////////.SingleInstance();
            Container = cb.Build();
            var scope = Container.BeginLifetimeScope();
            var GG = scope.Resolve<DBC_Customer>(
                     new NamedParameter[] { 
                             new NamedParameter("Constr", constr),
                             new NamedParameter("db_type", db_type) }
                    );
            return GG;
        }





        public DBC_Customer GetMyContainer(string ip, string database_user_name, string database_user_password, string port = "", string database_name = "master", string sql_type = "sqlserver")
        {
            //string Unikey = string.Format("{0}.{1}.{2}", ip, database_name, sql_type);
            //string Unikey2 = string.Format("{0}.{1}.{2}.{3}", ip, database_name, sql_type,"db");
            //memoryCacheHelper.CacheRemove(Unikey);
            //memoryCacheHelper.CacheRemove(Unikey2);

            //DataBaseInfo dbinfo = memoryCacheHelper.GetCacheItem<DataBaseInfo>(Unikey2, delegate ()
            // {

            //     DataBaseInfo dataBaseInfo = null;
            //     dataBaseInfo = new DataBaseInfo();
            //     dataBaseInfo.DB_Name = database_name;
            //     dataBaseInfo.IP = ip;
            //     dataBaseInfo.DB_User_Pwd = database_user_password;
            //     dataBaseInfo.SQL_Type = sql_type;
            //     dataBaseInfo.DB_User_Name = database_user_name;
            //     dataBaseInfo.Port = port;
            //     return dataBaseInfo;
            // }, null, null);

            //DBC_Customer dBC_Customer = null;
            //dBC_Customer = memoryCacheHelper.GetCacheItem<DBC_Customer>(Unikey, delegate ()
            //{


           

            DataBaseInfo dataBaseInfo = null;
            dataBaseInfo = new DataBaseInfo();
            dataBaseInfo.DB_Name = database_name;
            dataBaseInfo.IP = ip;
            dataBaseInfo.DB_User_Pwd = database_user_password;
            dataBaseInfo.SQL_Type = sql_type;
            dataBaseInfo.DB_User_Name = database_user_name;
            dataBaseInfo.Port = port;
            ContainerBuilder cb = new ContainerBuilder();
            IContainer Container;
            cb.RegisterType<DBC_Customer>();//////////////////.SingleInstance();
            Container = cb.Build();
            var scope = Container.BeginLifetimeScope();
            var dBC_Customer = scope.Resolve<DBC_Customer>(
                     new NamedParameter[] { new NamedParameter("ip", ip),
                         new NamedParameter("database_user_name", database_user_name),
                         new NamedParameter("database_user_password", database_user_password),
                         new NamedParameter("DB_TYPE", sql_type),
                         new NamedParameter("port", port),
                         new NamedParameter("database_name", database_name) }
                    );
          
            return dBC_Customer;
        }
        //public DBC_Customer GetConnectCash(string Unikey)
        //{
        //    DBC_Customer dBC_Customer = null;
        //    try
        //    {
        //        dBC_Customer = memoryCacheHelper.GetCacheItem<DBC_Customer>(Unikey, delegate ()
        //        {
        //            return dBC_Customer;
        //        }, null, null);
               
        //    }
        //    catch (Exception ex)
        //    {
               
        //    }
        //   return dBC_Customer;
        //}




    }
}
