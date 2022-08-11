using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver
{
    public class FileTransfer
    {
        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="path">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        public  bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use " + path + " " + passWord + " /user:" + userName;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">远程服务器路径（共享文件夹路径）</param>
        /// <param name="dst">本地文件夹路径</param>
        /// <param name="fileName"></param>
        public void TransportRemoteToServer(string dst, string src, string fileName)
        {
            if (!Directory.Exists(src))
            {
                Directory.CreateDirectory(src);
            }
            src = src + fileName;
            FileStream inFileStream = new FileStream(dst, FileMode.OpenOrCreate);    //从远程服务器下载到本地的文件 

            FileStream outFileStream = new FileStream(src, FileMode.Open);    //远程服务器文件  此处假定远程服务器共享文件夹下确实包含本文件，否则程序报错  

            byte[] buf = new byte[outFileStream.Length];

            int byteCount;

            while ((byteCount = outFileStream.Read(buf, 0, buf.Length)) > 0)
            {

                inFileStream.Write(buf, 0, byteCount);

            }

            inFileStream.Flush();

            inFileStream.Close();

            outFileStream.Flush();

            outFileStream.Close();

        }

        /// <summary>
        /// 从远程服务器下载文件到本地
        /// </summary>
        /// <param name="src">下载到本地后的文件路径，包含文件的扩展名</param>
        /// <param name="dst">远程服务器路径（共享文件夹路径）</param>
        /// <param name="fileName">远程服务器（共享文件夹）中的文件名称，包含扩展名</param>
        public  void TransportRemoteToLocal(string src, string dst, string fileName)   //src：下载到本地后的文件路径  dst：远程服务器路径 fileName:远程服务器dst路径下的文件名
        {
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }
            dst = dst + fileName;
            FileStream inFileStream = new FileStream(dst, FileMode.Open);    //远程服务器文件  此处假定远程服务器共享文件夹下确实包含本文件，否则程序报错

            FileStream outFileStream = new FileStream(src, FileMode.OpenOrCreate);   //从远程服务器下载到本地的文件

            byte[] buf = new byte[inFileStream.Length];

            int byteCount;

            while ((byteCount = inFileStream.Read(buf, 0, buf.Length)) > 0)
            {

                outFileStream.Write(buf, 0, byteCount);

            }

            inFileStream.Flush();

            inFileStream.Close();

            outFileStream.Flush();

            outFileStream.Close();

        }

    }


}
