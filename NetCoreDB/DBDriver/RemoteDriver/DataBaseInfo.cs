using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver
{
   public class DataBaseInfo
    {
          public string IP { get; set; }

          public string DB_User_Name { get; set; }

          public string DB_User_Pwd { get; set; }

         public string Port { get; set; }

         public string DB_Name { get; set; }

        public string SQL_Type { get; set; }
    }
}
