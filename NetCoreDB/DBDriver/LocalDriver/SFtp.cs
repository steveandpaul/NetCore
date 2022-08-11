using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreDBModule.NetCoreDB.DBDriver.LocalDriver
{
   public class SFtp
    {
		private SftpClient sftp;
		/// <summary>
		/// 连接状态
		/// </summary>
		public bool Connected
		{
			get
			{
				return this.sftp.IsConnected;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="host">sftp的IP</param>
		/// <param name="port">sftp的端口</param>
		/// <param name="username">sftp的帐户</param>
		/// <param name="password">sftp的密码</param>
		public SFtp(string host, int port, string username, string password)
		{
			this.sftp = new SftpClient(host, port, username, password);
		}
		/// <summary>
		/// 连接sftp服务器
		/// </summary>
		/// <returns>连接状态</returns>
		public bool Connect()
		{
			bool result;
			try
			{
				bool flag = !this.Connected;
				if (flag)
				{
					this.sftp.Connect();
				}
				result = true;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("连接SFTP失败，原因：{0}", ex.Message));
			}
			return result;
		}
		/// <summary>
		/// 断开连接
		/// </summary>
		public void Disconnect()
		{
			try
			{
				bool flag = this.sftp != null && this.Connected;
				if (flag)
				{
					this.sftp.Disconnect();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("断开SFTP失败，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 上传文件
		/// </summary>
		/// <param name="localPath">本地文件路径</param>
		/// <param name="remotePath">服务器端文件路径</param>
		public void Put(string localPath, string remotePath)
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(localPath))
				{
					this.Connect();
					this.sftp.UploadFile(fileStream, remotePath, null);
					this.Disconnect();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 上传文字字节数据
		/// </summary>
		/// <param name="fileByteArr">文件内容字节</param>
		/// <param name="remotePath">上传到服务器的路径</param>
		public void Put(byte[] fileByteArr, string remotePath)
		{
			try
			{
				Stream input = new MemoryStream(fileByteArr);
				this.Connect();
				this.sftp.UploadFile(input, remotePath, null);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 将sftp服务器的文件下载本地
		/// </summary>
		/// <param name="remotePath">服务器上的路径</param>
		/// <param name="localPath">本地的路径</param>
		public void Get(string remotePath, string localPath)
		{
			try
			{
				this.Connect();
				byte[] bytes = this.sftp.ReadAllBytes(remotePath);
				this.Disconnect();
				File.WriteAllBytes(localPath, bytes);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件获取失败，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		///  删除ftp服务器上的文件
		/// </summary>
		/// <param name="remoteFile">服务器上的路径</param>
		public void Delete(string remoteFile)
		{
			try
			{
				this.Connect();
				this.sftp.Delete(remoteFile);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 获取ftp服务器上指定路径上的指定前缀的文件名列表
		/// </summary>
		/// <param name="remotePath">服务器上的路径</param>
		/// <param name="fileSuffix">文件名前缀</param>
		/// <returns></returns>
		public List<string> GetFileList(string remotePath, string fileSuffix)
		{
			List<string> result;
			try
			{
				this.Connect();
				IEnumerable<SftpFile> enumerable = this.sftp.ListDirectory(remotePath, null);
				this.Disconnect();
				result = new List<string>();
				foreach (SftpFile current in enumerable)
				{
					string name = current.Name;
					bool flag = name.Length > fileSuffix.Length + 1 && fileSuffix == name.Substring(name.Length - fileSuffix.Length);
					if (flag)
					{
						result.Add(name);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
			}
			return result;
		}
		/// <summary>
		/// ftp服务器端文件移动
		/// </summary>
		/// <param name="oldRemotePath">原来服务器上路径</param>
		/// <param name="newRemotePath">移动后服务器上新路径</param>
		public void Move(string oldRemotePath, string newRemotePath)
		{
			try
			{
				this.Connect();
				this.sftp.RenameFile(oldRemotePath, newRemotePath);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件移动失败，原因：{0}", ex.Message));
			}
		}
	}
}

