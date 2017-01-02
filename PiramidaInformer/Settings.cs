using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PiramidaInformer
{
    class Settings
    {
        private string server;
        private string database;
        private string userName;
        private string password;
        private string sMTPServer;
        private string sMTPUserName;
        private string sMTPPassword;
        private int sMTPPort;
        private string addressTo;

        #region Properties

        public string Server
        {
            get
            {
                return server;
            }
        }

        public string Database
        {
            get
            {
                return database;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
        }

        public string SMTPServer
        {
            get
            {
                return sMTPServer;
            }
        }

        public string SMTPUserName
        {
            get
            {
                return sMTPUserName;
            }
        }

        public string SMTPPassword
        {
            get
            {
                return sMTPPassword;
            }
        }

        public int SMTPPort
        {
            get
            {
                return sMTPPort;
            }
        }

        public string AddressTo
        {
            get
            {
                return addressTo;
            }
        }
        #endregion

        public Settings(string fileName)
        {
            string[] lines = File.ReadAllLines("Settings.ini");
            Dictionary<string, string> s = lines.ToDictionary<string, string, string>((string inp) => inp.Split('=')[0].Trim(),
                                                                              (string el) => el.Split('=')[1].Trim());
            try
            {
                server = s["Server"];
                database = s["Database"];
                userName = s["UserName"];
                password = s["Password"];
                sMTPServer = s["SMTPServer"];
                sMTPPort = int.Parse(s["SMTPPort"]);
                sMTPUserName = s["SMTPUserName"];
                sMTPPassword = s["SMTPPassword"];
                addressTo = s["AddressTo"];
            }
            catch (Exception ex)
            {
                throw new Exception("Не могу прочитать настройки.", ex);
            }
        }

        public void ReadSettings(string fileName)
        {

        }
    }
}
