using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    public class FTPEntity
    {
        private string protocol;
        private string ip;
        private int port;
        private string username;
        private string passwd;

        public string Protocol { get => protocol; set => protocol = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string Username { get => username; set => username = value; }
        public string Passwd { get => passwd; set => passwd = value; }
        
        public static string GetProtocol(int port)
        {
            string protocol = string.Empty;
            switch( port) {
                case 22:
                    protocol = "sftp";
                    break;
                default:
                    protocol = "ftp";
                    break;
            }
            return protocol;
        }

        public FTPEntity(string ip,string username, string psd, int port=22, string prot="")
        {
            Protocol = prot == "" ? GetProtocol(port) : prot;
            Ip = ip;
            Username = username;
            Passwd = psd;
            Port = port;
        }
            
    }
}
