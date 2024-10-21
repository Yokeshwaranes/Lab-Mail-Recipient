using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LabNotificationRecipient
{

    public class Logger
    {
        private static volatile Logger instance;
        private static object syncRoot = new Object();

        private List<string> WriteList = new List<string>();

        private Logger() { }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logger();
                    }
                }

                return instance;
            }
        }

        public void WriteDebug(string Source, string strMessage)
        {
            WriteToLog(Source, strMessage, ".dbg", 1);
        }

        public static void WriteError(string Source, string strMessage)
        {
            WriteToLog(Source, strMessage, ".txt", 1);
        }

        public void WriteDebug(string Source, string strMessage, short Level)
        {
            WriteToLog(Source, strMessage, ".dbg", Level);
        }

        public void WriteError(string Source, string strMessage, short Level)
        {
            WriteToLog(Source, strMessage, ".elg", Level);
        }

        public static void WriteToLog(string Source, string strMessage, string ext, short Level)
        {
            try
            {
                StringBuilder FormattedMessage = new StringBuilder();

                FormattedMessage.AppendLine(DateTime.Now.ToString("HH:mm:ss"));
                FormattedMessage.Append(string.Format("\t"));
                FormattedMessage.Append(Source.PadRight(02));
                FormattedMessage.Append(string.Format("\t"));
                FormattedMessage.AppendLine(strMessage.Replace((char)13, (char)0).Replace((char)10, (char)32));

                FormattedMessage.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                string strPath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                //string filepath = System.IO.Path.GetTempPath() + "\\FHIRErrorLog_" + Settings.Default.JsonServiceTypeID.ToString();
                string filepath = strPath + "\\PrivilegeEmailErrorLog";

                if (!Directory.Exists(filepath))
                    Directory.CreateDirectory(filepath);

                string filename = filepath + "\\" + "PrivilegeEmail_" + DateTime.Now.ToString("ddMMyyyy") + ext;

                StreamWriter f = new StreamWriter(filename, true);
                f.WriteLine(FormattedMessage.ToString());
                f.Close();

                //}
            }
            catch (Exception ex)
            {
            }
        }
    }
}
