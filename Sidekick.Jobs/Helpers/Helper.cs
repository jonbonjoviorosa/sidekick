using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Jobs.Helpers
{
    public static class Helper
    {
        public static void WriteToFile(string title, string text)
        {
            string path = "C:\\ProcessorServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"{title} {text} {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}");
                writer.Close();
            }
        }
    }
}
