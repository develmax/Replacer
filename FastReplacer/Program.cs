using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FastReplacer
{
    class Program
    {
        static void Main(string[] args)
        {
            var f = @"..\..\..\..\..\..\customizations.xml";

            var sw = new Stopwatch();

            //Console.ReadKey();
            sw.Start();

            var s = new StreamReader(f);

            var partLen = 100;
            var part2Start = partLen;
            var parts2Len = part2Start + partLen;
            var part3Start = parts2Len;
            var partsLen = part3Start + partLen;

            var all = new char[300];

            var allbuf = new Span<char>(all);

            var buf = allbuf.Slice(0, partLen);
            var buf2 = allbuf.Slice(part2Start, parts2Len);

            var buf3 = allbuf.Slice(part3Start, partLen);

            var i = 0;
            var part = 0;

            var start = 0;
            var end = 0;

            var count = 0;

            var len = s.ReadBlock(allbuf);
            if (len == partsLen)
            {
                len = start = parts2Len;
                count = partsLen;
                
                while (i < len)
                {
                    p(allbuf, i, len);
                    i++;

                    if (i >= len)
                    {
                        if (i != parts2Len) break;

                        buf3.CopyTo(buf);

                        len = s.ReadBlock(buf2);
                        if (len == 0)
                        {
                            end = len = partLen;
                            count = partLen;
                        }
                        else if (len == parts2Len)
                            part++;
                        else
                        {
                            end = partLen + len;
                            count = end;
                        }

                        i = 0;

                    }
                }
            }
            else
            {
                end = len;
                count = len;
            }

            if (len > 0)
            {
                for (; i < len; i++)
                    p(allbuf, i, count);
            }

            sw.Stop();

            Console.WriteLine($"{part}, {start + part*200 + end}, {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void p(Span<char> buf, int i, int len)
        {
            
        }
    }
}
