using System;
using System.IO;

namespace HandySafeConverter
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string fileName = args[0];
                if (File.Exists(fileName))
                {
                    Converter converter = new Converter();
                    string csv = converter.Convert(fileName);
                    File.WriteAllText(fileName + ".csv", csv);
                }
             }

        }
    }
}
