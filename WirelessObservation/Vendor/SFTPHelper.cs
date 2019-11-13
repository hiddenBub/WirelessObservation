using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tamir.SharpSsh.jsch;

//using DiffieHellman;

namespace WirelessObservation.Vendor
{
    public class SFTPHelper
    {
        private Session m_session;
        private Channel m_channel;
        private ChannelSftp m_sftp;

        //host:sftp地址   user：用户名   pwd：密码        
        public SFTPHelper(string host, string user, string pwd)
        {
            string[] arr = host.Split(':');
            string ip = arr[0];
            int port = 22;
            if (arr.Length > 1) port = Int32.Parse(arr[1]);

            JSch jsch = new JSch();
            m_session = jsch.getSession(user, ip, port);
            MyUserInfo ui = new MyUserInfo();
            ui.setPassword(pwd);
            m_session.setUserInfo(ui);

        }

        //SFTP连接状态        
        public bool Connected { get { return m_session.isConnected(); } }

        //连接SFTP        
        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    m_session.connect();
                    m_channel = m_session.openChannel("sftp");
                    m_channel.connect();
                    m_sftp = (ChannelSftp)m_channel;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //断开SFTP        
        public void Disconnect()
        {
            if (Connected)
            {
                m_channel.disconnect();
                m_session.disconnect();
            }
        }

        //SFTP存放文件        
        public bool Put(string localPath, string remotePath)
        {
            try
            {
                Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(localPath);
                Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(remotePath);
                m_sftp.put(src, dst);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //SFTP获取文件        
        public bool Get(string remotePath, string localPath)
        {
            try
            {
                Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(remotePath);
                Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(localPath);
                m_sftp.get(src, dst);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //删除SFTP文件
        public bool Delete(string remoteFile)
        {
            try
            {
                m_sftp.rm(remoteFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //获取SFTP文件列表        
        public List<string> GetFileList(string remotePath, string fileType)
        {
            try
            {
                Tamir.SharpSsh.java.util.Vector vvv = m_sftp.ls(remotePath);
                List<string> objList = new List<string>();
                Regex Partten;
                if (fileType.First() == '~')
                {
                    
                }

                foreach (Tamir.SharpSsh.jsch.ChannelSftp.LsEntry qqq in vvv)
                {
                    string sss = qqq.getFilename();
                    string ext = System.IO.Path.GetExtension(sss).Trim('.');
                    if (fileType.First() == '~')
                    {
                        string regee = fileType.TrimStart('~');
                        Partten = new Regex(regee);
                        if (Partten.Match(sss).Success)
                        {
                            objList.Add(sss);
                        }
                    }
                    else if (ext.ToUpper() == fileType.ToUpper())
                    {
                        objList.Add(sss);
                    }
                }
                objList.Sort((a, b) => a.CompareTo(b));
                return objList;
            }
            catch
            {
                return null;
            }
        }

        public long GetFileSize(string remotePath)
        {
            long size = 0;
            try
            {
                //Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(remotePath + " -l");
                Tamir.SharpSsh.java.util.Vector vvv = m_sftp.ls(remotePath);


                List<string> part = splitList(vvv[0]);
                    if (IsFile(remotePath))
                    {
                        size = Convert.ToInt64(part[4]);
                    }
                    //if (ext.ToUpper() == fileType.ToUpper())
                    //{
                    //    objList.Add(sss);
                    //}
                
            }
            catch
            {
                return size;
            }
            return size;
        }

        private List<string> splitList(Object ls)
        {
            return ls.ToString().TrimStart('{').TrimEnd('}').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public bool IsFile(string remotePath)
        {
            bool isfile = false;
            Tamir.SharpSsh.java.util.Vector list = m_sftp.ls(remotePath);
            if (list.Count == 1 && splitList(list[0])[0].First() == '-')
            {
                isfile = true;
            }
            return isfile;
        }


        //登录验证信息        
        public class MyUserInfo : UserInfo
        {
            String passwd;
            public String getPassword() { return passwd; }
            public void setPassword(String passwd) { this.passwd = passwd; }

            public String getPassphrase() { return null; }
            public bool promptPassphrase(String message) { return true; }

            public bool promptPassword(String message) { return true; }
            public bool promptYesNo(String message) { return true; }
            public void showMessage(String message) { }
        }
    }
}
