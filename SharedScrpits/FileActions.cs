using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedScrpits
{
    public class FileActions_Share
    {

        public void ClearOldTempFile()
        {
            string path = GetUploadFolder();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else
            {
                (from f in new DirectoryInfo(GetUploadFolder()).GetFiles()
                 where f.CreationTime < DateTime.Now.Subtract(TimeSpan.FromDays(3))
                 select f
                ).ToList()
                    .ForEach(f => f.Delete());
            }
        }


        public string GetUploadFolder()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").Replace("bin", "UploadedFiles") + "\\";
        }


        public string GetImageFolder()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").Replace("bin", "Images") + "\\";
        }


        public string GetTemplateFolder()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").Replace("bin", "Templates") + "\\";
        }


        public void deleteWhenUnlock(FileInfo pmFile)
        {
            if (!pmFile.Exists)
                return;

            FileStream stream = null;
            int time_threshold = 20;

            while (true)
            {
                try
                {
                    stream = pmFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException)
                {
                    Thread.Sleep(3000);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        pmFile.Delete();
                    }
                }

                time_threshold--;
                pmFile.Refresh();
                if (!pmFile.Exists || time_threshold <= 0)
                    break;
            }
        }

    }
}
