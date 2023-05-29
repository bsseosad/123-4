using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bar
{
   static internal class Log
    {
       static  public void writeLog(string strContent)
        {
            DirectoryInfo di = new DirectoryInfo(".\\log");
            if (di.Exists == false)
                di.Create();

            string logFileName = "";

            logFileName = DateTime.Now.ToString("yyyyMMdd") + ".log";

            string logline = null;

            logline = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "," + strContent + "\r\n";

            FileStream writeData = null;

            try
            {
                writeData = new FileStream(".\\log\\" + logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                writeData.Write(Encoding.UTF8.GetBytes(logline), 0, Encoding.UTF8.GetByteCount(logline));
            }
            catch (Exception ex)
            {
                writeLog(ex.ToString());
                return;
            }
            finally
            {
                writeData.Close();
            }
        }
    }
}
