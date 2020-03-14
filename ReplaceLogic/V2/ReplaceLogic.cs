using System;
using System.Diagnostics;
using System.IO;

namespace ReplaceLogic.V2
{
    public static class ReplaceLogic
    {
        public static void ReplaceFile(string file, FastTreeNode[] t, string[] ts, int max_len)
        {
            using (var s = new StreamReader(file))
            using (var d = new StreamWriter(file + ".repl"))
            {
                ReplaceFile(s, d, t, ts, max_len);
                /*d.Flush();
                d.Close();*/
            }
        }

        public static unsafe void ReplaceFile(StreamReader s, StreamWriter d, FastTreeNode[] t, string[] strs, int max_len)
        {
            var n = 0;
            var tsl = stackalloc int[strs.Length];
            var tsi = stackalloc int[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                tsl[i] = strs[i].Length;
                tsi[i] = n;
                n = n + tsl[i];
            }
            Span<char> ts = stackalloc char[n];
            
            for (int i = 0; i < strs.Length; i++)
            {
                strs[i].AsSpan().CopyTo(ts.Slice(tsi[i], tsl[i]));
            }

            var tree = stackalloc FastTreeNode[t.Length];
            for (int i = 0; i < t.Length; i++)
            {
                tree[i] = t[i];
            }

            /*var sw = new Stopwatch();
            sw.Start();*/

            var partLen = max_len;
            var part2Len = partLen + partLen;
            var part3Len = part2Len + partLen;
            var partsLen = part2Len + part2Len;

            var partLastIndex = partLen - 1;
            var part2LastIndex = part2Len - 1;
            var part3LastIndex = part3Len - 1;
            var partsLastIndex = partsLen - 1;

            Span<char> buf = stackalloc char[partsLen];

            //var buf = new Span<char>(array);

            var firstBuf = buf.Slice(0, part2Len);
            //var updateBuf = buf.Slice(partLen, part2Len);
            var lastBuf = buf.Slice(part2Len, part2Len);

            var state = states.start;
            var index = 0;
            var lastUpdatedBlockNumber = 0;
            var blockChecked = false;
            var checkNodeTextLen = 0;
            var charChecking = false;
            var charCheckingIndex = -1;
            var rollbackCharCheckingIndex = -1;
            var writeLastIndex = -1;

            var lastRead = false;
            var lastIndex = -1;

            var writeIndex = -1;
            var continueCheck = false;

            //var readCount = 0l;

            //var j = -1;
            var readLen = 0;
            //var processLen = 0;

            var node = 0;

            switch (state)
            {
                case states.start:
                    {
                        goto case states.read_first;
                    }
                case states.read_first:
                    {
                        readLen = s.ReadBlock(firstBuf);
                        if (readLen == 0)
                            goto case states.end;

                        if (readLen < part2Len)
                        {
                            lastRead = true;
                            lastIndex = readLen-1;
                        }

                        //readCount += readLen;
                        //processLen = readLen;

                        lastUpdatedBlockNumber = 1;

                        goto case states.fast_check_node;
                    }
                case states.move_index:
                    {
                        if (lastRead)
                        {
                            if (index == lastIndex)
                                goto case states.end;
                        }
                        else if (!blockChecked)
                            goto case states.check_block;

                        if (index == partsLastIndex)
                            index = 0;
                        else
                            index++;

                        //if (node == null)
                        //    node = tree;

                        blockChecked = false;

                        if (charChecking)
                        {
                            if (checkNodeTextLen > 1)
                                goto case states.check_char;

                            charChecking = false;
                            goto case states.check_node;
                        }

                        goto case states.fast_check_node;
                    }
                case states.fast_check_node:
                    {
                        /*                    if (readCount >= 10206851 && index == 398)
                                            {

                                            }*/

                        if (buf[index] != ts[tsi[tree[node].text]]/*[0]*/)
                            goto case states.next_node;

                        if (tsl[tree[node].text]/*.Length*/ == 1)
                        {
                            goto case states.check_node;
                        }

                        var nodeTextLastIndex = tsl[tree[node].text]/*.Length*/ - 1;
                        var lostLen = partsLen - index;

                        if (lastRead)
                        {
                            if (lostLen == tsl[tree[node].text]/*.Length*/)
                            {
                                if (buf[index + nodeTextLastIndex] != ts[tsi[tree[node].text]+nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else if (lostLen < tsl[tree[node].text]/*.Length*/)
                            {
                                if (nodeTextLastIndex - lostLen > lastIndex)
                                    goto case states.next_node;

                                if (buf[nodeTextLastIndex - lostLen] != ts[tsi[tree[node].text]+nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else
                            {
                                if (index + nodeTextLastIndex > lastIndex)
                                    goto case states.next_node;

                                if (buf[index + nodeTextLastIndex] != ts[tsi[tree[node].text]+nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                        }
                        else
                        {
                            if (lostLen < tsl[tree[node].text]/*.Length*/)
                            {
                                if (buf[nodeTextLastIndex - lostLen] != ts[tsi[tree[node].text]+nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else
                                if (buf[index + nodeTextLastIndex] != ts[tsi[tree[node].text]+nodeTextLastIndex])
                                goto case states.next_node;
                        }
                        
                        /*
                        if (lostLen < node.text.Length)
                        {
                            if(!buf.Slice(index-1, partsLastIndex-index).SequenceEqual(node.text.AsSpan(1, node.text.Length-lostLen)) ||
                            !buf.Slice(0, lostLen-1).SequenceEqual(node.text.AsSpan(nodeTextLastIndex- lostLen, lostLen-1)))
                                goto case states.next_node;
                        }
                        else
                            if (!buf.Slice(index+1, node.text.Length-2).SequenceEqual(node.text.AsSpan(1, node.text.Length - 2)))
                            goto case states.next_node;*/
                            
                        rollbackCharCheckingIndex = index;
                        charCheckingIndex = 1;
                        checkNodeTextLen = nodeTextLastIndex;
                        charChecking = true;

                        goto case states.move_index;
                    }
                case states.check_node:
                    {
                        if (tree[node].child != -1)
                        {
                            node = tree[node].child;
                            goto case states.move_index;
                        }
                        else if (ts[tree[node].repl] != null)
                        {
                            writeIndex = index;
                            goto case states.write_node;
                        }
                        else
                        {
                            throw new Exception("node.repl is null");
                        }
                    }
                case states.write_node:
                    {
                        var tmpLen = tree[node].len;
                        /*var tmp = node;

                        while (tmp != null)
                        {
                            tmpLen = tmpLen + tmp.text.Length;
                            tmp = tmp.parent;
                        }*/

                        /*var found = readCount >= 10213228;
                        if (found)
                        {
                            found = found;
                        }*/

                        //d.Write(buf.Slice(writeLastIndex + 1, index - tmpLen - writeLastIndex));
                        if (writeLastIndex > writeIndex)
                        {
                            var tmpIndex = tmpLen - 1;
                            var startLen = writeIndex - tmpIndex;

                            if (startLen == 0)
                            {
                                if(partsLastIndex - writeLastIndex > 0)
                                    d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));
                            }
                            else if (startLen > 0)
                            {
                                if (partsLastIndex - writeLastIndex > 0)
                                    d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));

                                d.Write(buf.Slice(0, startLen));
                            }
                            else if(partsLastIndex - writeLastIndex + startLen > 0)
                                d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex + startLen));
                        }
                        else if(writeIndex - writeLastIndex - tmpLen > 0)
                            d.Write(buf.Slice(writeLastIndex + 1, writeIndex - writeLastIndex - tmpLen));

                        writeLastIndex = writeIndex;

                        d.Write(ts[tree[node].repl]);
                        node = 0;

                        if (continueCheck)
                        {
                            continueCheck = false;
                            goto case states.fast_check_node;
                        }

                        goto case states.move_index;
                    }
                case states.next_node:
                    {
                        if (tree[node].nextSibling != -1)
                        {
                            node = tree[node].nextSibling;
                            goto case states.fast_check_node;
                        }
                        else if (tree[node].parent != -1)
                        {
                            if (tree[tree[node].parent].repl != -1)
                            {
                                node = tree[node].parent;

                                if (index == 0)
                                    writeIndex = partsLastIndex;
                                else
                                    writeIndex = index - 1;

                                continueCheck = true;
                                goto case states.write_node;
                            }

                            index = index - tsl[tree[tree[node].parent].text]/*.Length*/;
                            if (index < 0)
                                index = index + partsLen;

                            node = tree[node].parent;
                            goto case states.next_node;
                        }
                        else
                        {
                            node = 0;
                            goto case states.move_index;
                        }
                    }
                case states.check_char:
                    {
                        if (buf[index] != ts[tsi[tree[node].text]+charCheckingIndex])
                        {
                            index = rollbackCharCheckingIndex;
                            charChecking = false;
                            goto case states.next_node;
                        }
                        else
                        {
                            checkNodeTextLen--;
                            charCheckingIndex++;

                            goto case states.move_index;
                        }
                    }
                case states.check_block:
                    {
                        if (index == part3LastIndex)
                        {
                            if (lastUpdatedBlockNumber == 2)
                            {
                                if (writeLastIndex < part2Len)
                                {
                                    if (writeLastIndex == -1)
                                    {
                                        d.Write(firstBuf);
                                    }
                                    else
                                    {
                                        d.Write(buf.Slice(writeLastIndex + 1, part2LastIndex - writeLastIndex));
                                    }

                                    writeLastIndex = part2LastIndex;
                                }

                                readLen = s.ReadBlock(firstBuf);
                                if (readLen < part2Len)
                                {
                                    lastRead = true;
                                    lastIndex = readLen - 1;
                                }

                                //readCount += readLen;

                                lastUpdatedBlockNumber = 1;
                            }
                        }
                        else if (index == partLastIndex)
                        {
                            if (lastUpdatedBlockNumber == 1)
                            {
                                if (writeLastIndex != -1 && writeLastIndex >= part2LastIndex)
                                {
                                    if (writeLastIndex == part2LastIndex)
                                    {
                                        d.Write(lastBuf);
                                    }
                                    else
                                    {
                                        d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));
                                    }

                                    writeLastIndex = -1;
                                }

                                readLen = s.ReadBlock(lastBuf);
                                if (readLen < part2Len)
                                {
                                    lastRead = true;
                                    lastIndex = part2LastIndex + readLen;
                                }

                                //readCount += readLen;

                                lastUpdatedBlockNumber = 2;
                            }
                        }

                        blockChecked = true;
                        goto case states.move_index;
                    }
                case states.end:
                    {
                        if (writeLastIndex > lastIndex)
                        {
                            d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));
                            d.Write(buf.Slice(0, lastIndex + 1));
                        }
                        else if (writeLastIndex < lastIndex)
                            d.Write(buf.Slice(writeLastIndex + 1, lastIndex - writeLastIndex));

                        break;
                    }
            }

            /*sw.Stop();

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms");*/
        }
    }
}
