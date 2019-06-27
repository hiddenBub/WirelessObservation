using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WirelessObservation.Vendor
{
    public class FTPHelper
    {
        private static string FTPCONSTR = @"ftp://192.168.1.234:6000/";//FTP的服务器地址，格式为ftp://192.168.1.234:8021/。ip地址和端口换成自己的，这些建议写在配置文件中，方便修改
        private static string FTPUSERNAME = "ftpuser";//FTP服务器的用户名
        private static string FTPPASSWORD = "1111";//FTP服务器的密码
        public enum Type
        {
            folder = 0,
            file = 1,
        }

        #region 本地文件上传到FTP服务器
        /// <summary>
        /// 上传文件到远程ftp
        /// </summary>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <returns></returns>
        public static bool UploadFile(Entity.FTPEntity ftpe,string path, string name)
        {
            string erroinfo = "";
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            path = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + "/data/uploadFile/photo/" + name;//这个路径是我要传到ftp目录下的这个目录下
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 上传文件到远程ftp
        /// </summary>
        /// <param name="ftpPath">ftp上的文件路径</param>
        /// <param name="path">本地的文件目录</param>
        /// <param name="id">文件名</param>
        /// <returns></returns>
        public static bool UploadFile(Entity.FTPEntity ftpe, string ftpPath, string path, string id)
        {
            string erroinfo = "";
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            bool b = MakeDir(ftpe,ftpPath);
            if (b == false)
            {
                return false;
            }
            path = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + ftpPath + id;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <param name="pb">进度条</param>
        /// <returns></returns>
        public static bool UploadFile(Entity.FTPEntity ftpe, string path, string name, ProgressBar pb)
        {
            string erroinfo = "";
            float percent = 0;
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            path = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + "/data/uploadFile/photo/" + name;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            int allbye = (int)f.Length;
            if (pb != null)
            {
                pb.Maximum = (int)allbye;

            }
            int startbye = 0;
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    startbye = contentLen + startbye;
                    if (pb != null)
                    {
                        pb.Value = (int)startbye;
                    }
                    contentLen = fs.Read(buff, 0, buffLength);
                    percent = (float)startbye / (float)allbye * 100;
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 文件上传到ftp
        /// </summary>
        /// <param name="ftpPath">ftp的文件路径</param>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <param name="pb">进度条</param>
        /// <returns></returns>
        public static bool UploadFile(Entity.FTPEntity ftpe, string ftpPath, string path, string name, ProgressBar pb)
        {
            //path = "ftp://" + UserUtil.serverip + path;
            string erroinfo = "";
            float percent = 0;
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            bool b = MakeDir(ftpe, ftpPath);
            if (b == false)
            {
                return false;
            }
            path = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + ftpPath + name;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            int allbye = (int)f.Length;
            //if (pb != null)
            //{
            //    pb.Maximum = (int)allbye;

            //}
            int startbye = 0;
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    startbye = contentLen + startbye;
                    percent = (float)startbye / (float)allbye * 100;
                    if (percent <= 100)
                    {
                        int i = (int)percent;
                        if (pb != null)
                        {
                            pb.BeginInvoke(new updateui(upui), new object[] { allbye, i, pb });
                        }
                    }

                    contentLen = fs.Read(buff, 0, buffLength);

                    //  Console.WriteLine(percent);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        private delegate void updateui(long rowCount, int i, ProgressBar PB);
        public static void upui(long rowCount, int i, ProgressBar PB)
        {
            try
            {
                PB.Value = i;
            }
            catch { }
        }

        #endregion

        #region 从ftp服务器下载文件

        ////上面的代码实现了从ftp服务器下载文件的功能
        //public static Stream Download(Entity.FTPEntity ftpe, string ftpfilepath)
        //{
        //    Stream ftpStream = null;
        //    FtpWebResponse response = null;
        //    try
        //    {
        //        ftpfilepath = ftpfilepath.Replace("\\", "/");
        //        string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + ftpfilepath;
        //        FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
        //        reqFtp.UseBinary = true;
        //        reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
        //        response = (FtpWebResponse)reqFtp.GetResponse();
        //        ftpStream = response.GetResponseStream();
        //    }
        //    catch (Exception ee)
        //    {
        //        if (response != null)
        //        {
        //            response.Close();
        //        }
        //        MessageBox.Show("文件读取出错，请确认FTP服务器服务开启并存在该文件");
        //    }
        //    return ftpStream;
        //}

        /// <summary>
        /// 从ftp服务器下载文件的功能
        /// </summary>
        /// <param name="ftpfilepath">ftp下载的地址</param>
        /// <param name="filePath">存放到本地的路径</param>
        /// <param name="fileName">保存的文件名称</param>
        /// <returns></returns>
        public static bool Download(Entity.FTPEntity ftpe, string ftpfilepath, string filePath, string fileName)
        {
            try
            {
                filePath = filePath.Replace("我的电脑\\", "");
                if (filePath.Last() != '/' || filePath.Last() != '\\') filePath = filePath + "\\";
                string onlyFileName = Path.GetFileName(fileName);
                string newFileName = filePath + onlyFileName;
                if (File.Exists(newFileName))
                {
                    //errorinfo = string.Format("本地文件{0}已存在,无法下载", newFileName);                   
                    File.Delete(newFileName);
                    //return false;
                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + ftpfilepath;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                FileStream outputStream = new FileStream(newFileName, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }
        }
        //
        /// <summary>
        /// 从ftp服务器下载文件的功能----带进度条
        /// </summary>
        /// <param name="ftpfilepath">ftp下载的地址</param>
        /// <param name="filePath">保存本地的地址</param>
        /// <param name="fileName">保存的名字</param>
        /// <param name="pb">进度条引用</param>
        /// <returns></returns>
        public static bool Download(Entity.FTPEntity ftpe, string ftpfilepath, string filePath, string fileName, ProgressBar pb)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            FileStream outputStream = null;
            try
            {
                filePath = filePath.Replace("我的电脑\\", "");
                String onlyFileName = Path.GetFileName(fileName);
                string newFileName = filePath + onlyFileName;
                if (File.Exists(newFileName))
                {
                    try
                    {
                        File.Delete(newFileName);
                    }
                    catch { }

                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + ftpfilepath;
                reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                response = (FtpWebResponse)reqFtp.GetResponse();
                ftpStream = response.GetResponseStream();
                long cl = GetFileSize(ftpe, url);
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                outputStream = new FileStream(newFileName, FileMode.Create);

                float percent = 0;
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    percent = (float)outputStream.Length / (float)cl * 100;
                    if (percent <= 100)
                    {
                        if (pb != null)
                        {
                            pb.Invoke(new updateui(upui), new object[] { cl, (int)percent, pb });
                        }
                    }
                    // pb.Invoke(new updateui(upui), new object[] { cl, outputStream.Length, pb });

                }

                //MessageBoxEx.Show("Download0");
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                //MessageBoxEx.Show("Download00");
                return false;
            }
            finally
            {
                //MessageBoxEx.Show("Download2");
                if (reqFtp != null)
                {
                    reqFtp.Abort();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (ftpStream != null)
                {
                    ftpStream.Close();
                }
                if (outputStream != null)
                {
                    outputStream.Close();
                }
            }
        }
        #endregion

        #region 获得文件的大小
        /// <summary>
        /// 获得文件大小
        /// </summary>
        /// <param name="url">FTP文件的完全路径</param>
        /// <returns></returns>
        public static long GetFileSize(Entity.FTPEntity ftpe, string remoteFile)
        {

            long fileSize = 0;
            FtpWebRequest reqFtp = null;
            try
            {
                string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + remoteFile;
                reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                reqFtp.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                fileSize = response.ContentLength;

                response.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return 0;
            }
            return fileSize;
        }
        #endregion

        #region 在ftp服务器上创建文件目录

        /// <summary>
        ///在ftp服务器上创建文件目录
        /// </summary>
        /// <param name="dirName">文件目录</param>
        /// <returns></returns>
        public static bool MakeDir(Entity.FTPEntity ftpe, string dirName)
        {
            try
            {
                bool b = RemoteFtpDirExists(ftpe, dirName);
                if (b)
                {
                    return true;
                }
                string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + dirName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                // reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }

        }
        /// <summary>
        /// 判断ftp上的文件目录是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool RemoteFtpDirExists(Entity.FTPEntity ftpe, string path)
        {

            path = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + path;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
            reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse resFtp = null;
            try
            {
                resFtp = (FtpWebResponse)reqFtp.GetResponse();
                FtpStatusCode code = resFtp.StatusCode;//OpeningData
                resFtp.Close();
                return true;
            }
            catch
            {
                if (resFtp != null)
                {
                    resFtp.Close();
                }
                return false;
            }

        }
        #endregion

        #region 从ftp服务器删除文件的功能
        /// <summary>
        /// 从ftp服务器删除文件的功能
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool DeleteFile(Entity.FTPEntity ftpe, string fileName)
        {
            try
            {
                string url = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + fileName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFtp.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取远程服务器文件夹中文件列表
        /// </summary>
        /// <param name="ftpe">ftp实体</param>
        /// <param name="remoteDir">远程文件夹</param>
        /// <param name="pattern">匹配字符串（通配符、正则表达式）</param>
        /// <returns></returns>
        public static List<string> ListFiles(Entity.FTPEntity ftpe, string remoteDir,Type type = Type.file , string pattern= "*")
        {
            List<string> files = new List<string>();
            if (remoteDir.First() != '/')
            {
                remoteDir = "/" + remoteDir;
            }
            try
            {
                string uri = ftpe.Protocol + "://" + ftpe.Ip + ":" + ftpe.Port + remoteDir;   //目标路径 path为服务器地址
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(ftpe.Username, ftpe.Passwd);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());//中文文件名

                string line = reader.ReadLine();
                while (line != null)
                {
                    //"-rw-r--r--    1 1000     1000        36550 Apr 29 02:24 weatherdata-GFP1041J001-20190423AM.txt"
                    string[] fileDetials = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    // privilage type(file:1;dir:2) belong group size month day time filename;
                    // 
                    if (fileDetials[0].First() == '-' && fileDetials[1] == "1" && type == Type.file)
                    {
                        if (pattern != "*")
                        {
                            if (pattern.First() == '^')
                            {
                                Regex Pattern = new Regex(pattern);
                                if (Pattern.Match(fileDetials[8]).Success)
                                {
                                    files.Add(fileDetials[8]);
                                }
                            }
                            else
                            {
                                if (fileDetials[8] == pattern)
                                {
                                    files.Add(fileDetials[8]);
                                }
                            }
                        }
                        else
                        {
                            files.Add(fileDetials[8]);
                        }
                        
                    }
                    //else
                    //{

                    //}
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取目录出错：" + ex.Message);
                
            }
            
            return files;

        }
        #endregion
    }
}
