using System;
using System.IO;

namespace HandySafeConverter
{
    class MainClass
    {
        public static void Main(string[] args)
        {


            string fileName = "//Users//macwhite//Desktop//all-safe.xml";
            var file = File.Open(fileName, FileMode.Open);
            file.Close();
            Converter converter = new Converter();
            string csv = converter.Convert(fileName);
            File.WriteAllText(fileName + ".csv", csv);
        }
    }
}
