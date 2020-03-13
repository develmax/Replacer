using System;
using System.Diagnostics;

namespace FastReplacer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var f = @"..\..\..\..\..\..\test.xml";
            //var f = @"..\..\..\..\..\..\test.txt";

            var sw = new Stopwatch();
            var t = tree;
            //Console.ReadKey();
            sw.Start();

            ReplaceLogic.V1.ReplaceLogic.ReplaceFile(f, t, 8);

            sw.Stop();

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

            //Console.WriteLine($"{part}, {start + part*200 + end}, {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();
        }
    }
}
