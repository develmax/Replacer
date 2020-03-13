using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FastReplacer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var f = @"..\..\..\..\..\..\test.xml";
            //var f = @"..\..\..\..\..\..\test.txt";

            var sw = new Stopwatch();

            //Console.ReadKey();
            sw.Start();

            ReplaceFile(f);

            sw.Stop();

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

            //Console.WriteLine($"{part}, {start + part*200 + end}, {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();
        }

        
    }
}
