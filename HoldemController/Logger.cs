using System.IO;

namespace HoldemController
{

    public class Logger
    {
        private static readonly TextWriter LogTextWriter;

        static Logger()
        {
            if (LogTextWriter == null)
            {
                LogTextWriter = new StreamWriter("gamelog.txt", false);
            }
        }

        public static void Log(string sMessage, params object[] arg)
        {
//            System.Console.WriteLine(sMessage, arg); 
            LogTextWriter.WriteLine(sMessage, arg);
        }

        public static void Close()
        {
            LogTextWriter.Close();
        }
    }
}
