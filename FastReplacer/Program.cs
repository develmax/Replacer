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

            //Console.ReadKey();

            var repeat = 100;
            var index = 0;

            Console.WriteLine($"repeats: {repeat}");

            var v1 = 0d;
            var v2 = 0d;
            /*var v3 = 0d;
            var v4 = 0d;*/
            //var v5 = 0d;

            var v6 = 0d;
            var v7 = 0d;

            while (index < repeat)
            {
                index++;

                Console.WriteLine($"repeat: {index}");

                sw.Start();
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(f, treeV1, 8);
                sw.Stop();

                v1 += sw.ElapsedMilliseconds;

                Console.WriteLine($"ReplaceLogicV1(one method, array in stack): {sw.ElapsedMilliseconds}ms");

                sw.Reset();

                /*sw.Start();
                ReplaceLogic.V2.ReplaceLogic.ReplaceFile(f, treeV2, treeV2Str, 8);
                sw.Stop();

                v2 += sw.ElapsedMilliseconds;

                Console.WriteLine($"ReplaceLogicV2(one method, array in stack, tree in stack): {sw.ElapsedMilliseconds}ms");

                sw.Reset();*/

                /*sw.Start();
                new ReplaceLogic.V3.ReplaceLogic().ReplaceFile(f, treeV3, 8);
                sw.Stop();

                v3 += sw.ElapsedMilliseconds;

                sw.Reset();

                sw.Start();
                new ReplaceLogic.V4.ReplaceLogic().ReplaceFile(f, treeV4, 8);
                sw.Stop();

                v4 += sw.ElapsedMilliseconds;

                sw.Reset();*/

                /*sw.Start();
                new ReplaceLogic.V5.ReplaceLogic().ReplaceFile(f, treeV5, 8);
                sw.Stop();

                v5 += sw.ElapsedMilliseconds;

                sw.Reset();*/

                /*sw.Start();
                ReplaceLogic.V6.ReplaceLogic.ReplaceFile(f, treeV6, 8);
                sw.Stop();

                v6 += sw.ElapsedMilliseconds;

                Console.WriteLine($"ReplaceLogicV6(one method, array in memory): {sw.ElapsedMilliseconds}ms");

                sw.Start();
                ReplaceLogic.V7.ReplaceLogic.ReplaceFile(f, treeV7, treeV7Str, 8);
                sw.Stop();

                v7 += sw.ElapsedMilliseconds;

                Console.WriteLine($"ReplaceLogicV7(one method, array in memory, tree in memory): {sw.ElapsedMilliseconds}ms");

                sw.Reset();*/
            }

            Console.WriteLine($"--");
            Console.WriteLine($"ReplaceLogicV1(one method, array in stack): {v1 / repeat}ms");
            //Console.WriteLine($"ReplaceLogicV2(one method, array in stack, tree in stack): {v2 / repeat}ms");
            /*Console.WriteLine(
                $"ReplaceLogicV3(new object, many methods, array in stack, return state): {v3 / repeat}ms");
            Console.WriteLine(
                $"ReplaceLogicV4(new object, many methods, array in memory, return state): {v4 / repeat}ms");*/
            /*Console.WriteLine(
                $"ReplaceLogicV5(new object, many methods, array in memory, return delegate): {v5 / repeat}ms");*/
            /*Console.WriteLine($"ReplaceLogicV6(one method, array in memory): {v6 / repeat}ms");
            Console.WriteLine($"ReplaceLogicV7(one method, array in memory, tree in memory): {v7 / repeat}ms");*/

            Console.ReadKey();
        }
    }
}
