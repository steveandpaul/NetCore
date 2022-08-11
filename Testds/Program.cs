using NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Testds
{
    class Program
    {
        static void Main(string[] args)
        {
            DBC dBC = new DBC();
            Console.WriteLine("请试用");
            String STR = Console.ReadLine();
            DbTransaction transaction = dBC.GetDbTransaction();
            while (true)
            {
                try
                {
                   
                    Hashtable hashtable = null;
                    hashtable = new Hashtable();
                    hashtable["c_day"] = DateTime.Now;
                    hashtable["c_json"] = Guid.NewGuid().ToString();
                    hashtable["VIDEO_STATE"] = 3;
                    string s = " insert into computing(c_day,c_json)values(@c_day,@c_json)";
                    int r = dBC.Execute(s, hashtable, transaction);
                    string s1 = @" update video_detail_copy1 set VIDEO_STATE=@VIDEO_STATE  where guid in ('0e955708-b766-4fbc-bfc6-38507637e55d','0f19139a-52d2-40f8-8c8d-4af8b366e7d3','1c7ad19b-c73c-45fb-be54-d66ca8160cef','21049c83-1556-4377-a0cc-4288d02bda53','2290ae28-838c-4e95-86e5-560d6b8ab822') ";
                    int r1 = dBC.Execute(s1, hashtable, transaction);
                    if (r > 0 && r1 > 0)
                    {
                        dBC.CommitTransaction();
                    }
                    else
                    {
                        dBC.RollbackTransaction();
                    }


                    string sql = @" select  
                                VIDEO_STATE,GUID
                                from video_detail_copy1  where   VIDEO_STATE=3   ";
                    var List = dBC.SelectList(sql, null);
                    foreach (var obj in List)
                    {
                        Console.WriteLine(obj["VIDEO_STATE"].ToString() + "    " + obj["GUID"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                
                }

                Console.WriteLine("请试用");
                String aaa = Console.ReadLine();
              
            }


            Console.ReadLine();


        }
    }
}
