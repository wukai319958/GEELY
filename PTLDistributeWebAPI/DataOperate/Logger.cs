using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.DataOperate
{
    /// <summary>
    /// 文本日志。
    /// </summary>
    public static class Logger
    {
        private static object asyncRoot = new object();
        /// <summary>
        /// 记录日志到文本文件。
        /// </summary>
        /// <param name="fileNameWithoutExtension">文件名。</param>
        /// <param name="message">日志信息。</param>
        public static void Log(string fileNameWithoutExtension, string message)
        {
            try
            {
                lock (asyncRoot)
                {
                    string folderByDate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", DateTime.Today.ToString("yyyy-MM-dd"));
                    string filePath = Path.Combine(folderByDate, fileNameWithoutExtension + ".txt");
                    if (!Directory.Exists(folderByDate))
                        Directory.CreateDirectory(folderByDate);

                    File.AppendAllText(filePath, message);
                }
            }
            catch { }
        }
    }
}