using System;
using System.Collections.Generic;
using System.Text;

namespace unit_test.Utils
{
    class Log
    {
        public static void Info(string message)
        {
            System.Console.WriteLine(string.Format("[INFO] {0}", message));
        }

        public static void Error(string message)
        {
            System.Console.WriteLine(string.Format("[ERROR] {0}", message));
        }

        public static void Warning(string message)
        {
            System.Console.WriteLine(string.Format("[WARNING] {0}", message));
        }
    }
}
