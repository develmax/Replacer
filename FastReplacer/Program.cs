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
            var f = @"..\..\..\..\..\..\customizations.xml";

            var sw = new Stopwatch();

            //Console.ReadKey();
            sw.Start();

            ReplaceFile(f);

            sw.Stop();

            Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

            //Console.WriteLine($"{part}, {start + part*200 + end}, {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();
        }

        private static void ReplaceFile(string file)
        {
            using (var s = new StreamReader(file))
            using (var d = new StreamWriter(file + ".repl"))
            {
                ReplaceFile(s, d, tree);
                d.Flush();
                d.Close();
            }
        }

        private enum states : sbyte
        {
            start = -1,
            first_read = 10,
            block_check = 20,
            move_index = 30,
            fast_node_check = 40,
            node_check = 45,
            next_node = 50,
            char_check = 70,
            end = 100
        }

        private static unsafe void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree)
        {
            var partLen = 100;
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
            var index = -1;
            var firstBlockIsEmpty = true;
            var lastBlockIsEmpty = true;
            var blockChecked = false;
            var checkNodeTextLen = 0;
            var charChecking = false;
            var charCheckingIndex = -1;
            var writeLastIndex = -1;

            //var j = -1;
            var readLen = 0;
            //var processLen = 0;

            TreeNode node = tree;

            switch (state)
            {
                case states.start:
                {
                    goto case states.first_read;
                }
                case states.first_read:
                {
                    readLen = s.ReadBlock(firstBuf);
                    if (readLen == 0)
                        goto case states.end;

                    //processLen = readLen;

                    firstBlockIsEmpty = false;

                    goto case states.move_index;
                }
                case states.move_index:
                {
                    if(!blockChecked)
                        goto case states.block_check;

                    if (index == partsLastIndex)
                        index = 0;
                    else
                        index++;

                    if (node == null)
                        node = tree;

                    blockChecked = false;

                    if(charChecking)
                        goto case states.char_check;
                    else
                        goto case states.fast_node_check;
                }
                case states.fast_node_check:
                {
                    if (buf[index] != node.text[0])
                        goto case states.next_node;

                    if (node.text.Length == 1)
                    {
                        charChecking = true;
                        checkNodeTextLen = 1;
                        charCheckingIndex = 1;
                        goto case states.node_check;
                    }

                    if (node.text.Length == 2)
                        goto case states.char_check;

                    if (index + node.text.Length > partsLen)
                    {
                        if (buf[index + node.text.Length - 1 - partsLen] != node.text[node.text.Length - 1])
                            goto case states.next_node;
                    }
                    else if (buf[index + node.text.Length - 1] != node.text[node.text.Length - 1])
                        goto case states.next_node;

                    checkNodeTextLen = node.text.Length - 1;
                    charChecking = true;
                    charCheckingIndex = 1;
                    goto case states.move_index;
                }
                case states.node_check:
                {
                    if (node.child != null)
                    {
                        node = node.child;
                        goto case states.move_index;
                    }
                    else if (node.repl != null)
                    {
                        // write
                        var tmp = node;
                        var tmpLen = 0;
                        while (tmp != null)
                        {
                            tmpLen = tmpLen + tmp.text.Length;
                            tmp = tmp.parent;
                        }

                        if (writeLastIndex >= index)
                        {
                            if (index - tmpLen > 0)
                                d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));
                            else
                                d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex + (index - tmpLen)));

                            if (index - tmpLen > 0)
                                d.Write(buf.Slice(0,  index+1 - tmpLen));
                        }
                        else
                            d.Write(buf.Slice(writeLastIndex + 1, index - tmpLen - writeLastIndex));

                        writeLastIndex = index;

                        d.Write(node.repl);
                        node = tree;
                        goto case states.move_index;
                    }
                    else
                    {
                        throw new Exception("node.repl is null");
                    }
                }
                case states.next_node:
                {
                    if (node.nextSibling != null)
                    {
                        node = node.nextSibling;
                        goto case states.fast_node_check;
                    }
                    else if(node.parent != null)
                    {
                        if (node.parent.repl != null)
                        {
                            // write
                            var tmp = node;
                            var tmpLen = 0;
                            while (tmp != null)
                            {
                                tmpLen = tmpLen + tmp.text.Length;
                                tmp = tmp.parent;
                            }

                            d.Write(buf.Slice(writeLastIndex + 1, index - tmpLen - writeLastIndex));

                            writeLastIndex = index;

                            d.Write(node.repl);
                            node = tree;
                            goto case states.fast_node_check;
                        }

                        index = index - node.parent.text.Length;
                        if (index < 0)
                            index = index + partsLen;

                        node = node.parent;
                        goto case states.next_node;
                    }
                    else
                    {
                        node = tree;
                        goto case states.move_index;
                    }
                }
                case states.char_check:
                {
                    if (buf[index] != node.text[charCheckingIndex])
                    {
                        index = index - charCheckingIndex;
                        if (index < 0)
                            index = index + partsLen;

                        charChecking = false;
                        goto case states.next_node;
                    }
                    else
                    {
                        if (checkNodeTextLen == 1)
                        {
                            charChecking = false;
                            goto case states.node_check;
                        }
                        else
                        {
                            checkNodeTextLen--;
                            charCheckingIndex++;

                            goto case states.move_index;
                        }
                    }
                }
                case states.block_check:
                {
                    if (index == partsLastIndex)
                    {
                        if (firstBlockIsEmpty)
                        {
                            if (writeLastIndex < part2Len)
                            {
                                if (writeLastIndex == -1)
                                {
                                    d.Write(firstBuf);
                                }
                                else
                                {
                                    d.Write(buf.Slice(writeLastIndex, part2LastIndex - writeLastIndex));
                                }

                                writeLastIndex = part2LastIndex;
                            }

                            readLen = s.ReadBlock(firstBuf);
                            if (readLen == 0)
                                goto case states.end;

                            firstBlockIsEmpty = false;
                        }
                    }
                    else if (index == part3LastIndex)
                    {
                        if(!firstBlockIsEmpty)
                            firstBlockIsEmpty = true;
                    }
                    else if (index == part2LastIndex)
                    {
                        if (lastBlockIsEmpty)
                        {
                            if (writeLastIndex != -1 && writeLastIndex >= part2LastIndex)
                            {
                                if (writeLastIndex == part2LastIndex)
                                {
                                    d.Write(lastBuf);
                                }
                                else
                                {
                                    d.Write(buf.Slice(writeLastIndex, partsLastIndex - writeLastIndex));
                                }

                                writeLastIndex = -1;
                            }

                            readLen = s.ReadBlock(lastBuf);
                            if (readLen == 0)
                                goto case states.end;

                            lastBlockIsEmpty = false;
                        }
                    }
                    else if (index == partLastIndex)
                    {
                        if (!lastBlockIsEmpty)
                            lastBlockIsEmpty = true;
                        }

                    blockChecked = true;
                    goto case states.move_index;
                }
                case states.end:
                    break;
            }
        }

        /*
         *
         private enum states : sbyte
        {
            start = -1,
            pre_check = 0,
            next_pre_check = 1,
            next_process = 2,
            process = 3,
            nextCheck = 4,
            move = 5,
            end = 6
        }
         private static unsafe void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree)
         
        {
            var partLen = 100;
            var part2Len = partLen + partLen;
            var partsLen = part2Len + part2Len;

            Span<char> buf = stackalloc char[partsLen];

            //var buf = new Span<char>(array);

            var buf1 = buf.Slice(0, partLen);
            var buf2 = buf.Slice(partLen, part2Len);
            var buf3 = buf.Slice(part2Len, partLen);

            var state = states.start;

            var i = 0;
            var j = 0;
            var readLen = 0;
            var processLen = 0;
            var checkNodeTextLen = 0;
            var node = tree;

            while (state != states.end)
            {
                switch (state)
                {
                    case states.start:
                    {
                        readLen = s.ReadBlock(buf);
                        if (readLen == buf.Length)
                        {
                            processLen = buf1.Length;
                            state = states.next_pre_check;
                        }

                        break;
                    }
                    case states.pre_check:
                    {
                        var check = precheck_found(node, buf, i);
                        if (check == -1) // not found
                        {
                            /*if (node == tree)
                                state = states.next_pre_check;
                            else#1#
                                state = states.nextCheck;
                        }
                        else if (check == 0) // found
                        {
                            if (node.child == null)
                            {
                                i = i + node.text.Length;
                                write_repl(d, ref node);
                                
                                node = tree;
                                state = states.next_pre_check;
                            }
                            else
                            {
                                node = node.child;
                                state = states.next_pre_check;
                            }
                        }
                        else // continue check
                        {
                            state = states.next_process;
                            checkNodeTextLen = node.text.Length - 1;
                            j = 0;
                        }

                        break;
                    }
                    case states.next_pre_check:
                    {
                        if (i >= processLen)
                        {
                            /*if (readLen < processLen)
                            {
                                state = states.end;
                            }
                            else
                                state = states.move;#1#
                            goto case states.move;
                        }
                        else
                            state = states.pre_check;

                        i++;
                        break;
                    }
                    case states.next_process:
                    {
                        if (i >= processLen)
                        {
                            /*if (readLen < processLen)
                            {
                                //state = states.end;
                                goto case states.move;
                            }
                            else
                                state = states.move;#1#
                            goto case states.move;
                        }
                        else
                            state = states.process;

                        i++;
                        j++;
                        checkNodeTextLen--;
                        break;
                    }
                    case states.process:
                    {
                        if (buf[i] != node.text[j])
                        {
                            i = i - j; // revert i
                            //j = 0;

                            state = states.nextCheck;
                        }
                        else
                        {
                            if (checkNodeTextLen == 1)
                            {
                                if (node.child == null)
                                {
                                    write_repl(d, ref node);

                                    node = tree;
                                }
                                else
                                {
                                    node = node.child;
                                }

                                i++;
                                /*j = 0;
                                checkNodeTextLen = 0;#1#
                                state = states.next_pre_check;
                            }
                            else
                                state = states.next_process;
                        }

                        break;
                    }
                    case states.nextCheck:
                    {
                        if (node.nextSibling == null) // not sibling
                        {
                            if (node.parent == null)
                                i++;
                            else
                            while (node.parent != null)
                            {
                                node = node.parent;

                                if (node.repl != null) // not replacement
                                {
                                    write_repl(d, ref node);
                                    node = tree;
                                    break;
                                }
                                else if (node.nextSibling != null) // not sibling
                                {
                                    node = node.nextSibling;
                                    goto case states.pre_check;
                                }
                                else
                                {
                                    i = i - node.text.Length;
                                    if (i < 0)
                                    {

                                    }
                                }
                            }

                            state = states.next_pre_check;
                        }
                        else
                        {
                            node = node.nextSibling;
                            state = states.next_pre_check;
                        }

                        break;
                    }
                    case states.move:
                    {
                        //if (i != part2Len) break;
    
                        buf3.CopyTo(buf1);

                        var prevReadLen = readLen;
                        readLen = s.ReadBlock(buf2);
                        if (readLen == 0)
                        {
                            //if(processLen == prevReadLen)
                            state = states.end;

                            break;
                            //count = part;
                        }
                        else if (readLen == part2Len)
                            ; //part++;
                        else
                        {
                            //count = part + len;
                        }
    
                        i = i - processLen;

                        if (readLen == buf.Length)
                        {
                            processLen = buf1.Length;
                        }
                        else
                            processLen = readLen;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }*/

            /*[

            MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void write_pre(StreamWriter d, ref char[] all, ref int i)
        {
            d.Write(all, 0, i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void write_repl(StreamWriter d, ref TreeNode t)
        {
            d.Write(t.repl);
        }

        //private static int inc = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int precheck_found(TreeNode t, Span<char> buf, int i)
        {
            if (buf[i] != t.text[0]) return -1;

            if (t.text.Length == 1) return 0;

            if (buf[i + t.text.Length - 1] != t.text[t.text.Length - 1])
                return -1;

            return t.text.Length == 2 ? 0 : 1;
        }*/

        //ReplaceFile(s, d, tree, partLen, parts2Len, ref all, buf, buf1, buf2, buf3);
        /*private static void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree,
            int part, int part2, char[] all, Span<char> buf, Span<char> buf1, Span<char> buf2, Span<char> buf3)
        {
            var i = 0;
            var j = 0;
            var tlen = 0;
            var count = 0;
            var t = tree;

            var len = s.ReadBlock(buf);
            if (len == buf.Length)
            {
                len = buf1.Length;
                count = part2;

                ReplaceFile(s, d, tree, ref t, part, part2, ref all, buf, buf1, buf2, buf3, ref i, ref j, ref count, ref len, ref tlen);
            }
            else
            {
                //end = len;
                count = len;
            }

            if (len > 0)
            {
                for (; i < len; i++)
                    ;// ReplaceBuf(allbuf, i, count);
            }
        }

        private static int precheck_found(TreeNode t, Span<char> buf, int i)
        {
            if (buf[i] == t.text[i] && buf[i + t.text.Length - 1] == t.text[t.text.Length - 1])
            {
                if (t.text.Length <= 2)
                {
                    return 0;
                }
                else
                    return 1;
            }

            return -1;
        }

        private static void write_pre(StreamWriter d, ref char[] all, ref int i)
        {
            d.Write(all, 0, i);
        }

        private static void write_repl(StreamWriter d, ref TreeNode t)
        {
            d.Write(t.repl);
        }

        private enum states
        {
            start,
            find,
            move,
            end
        }



        private static void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree, ref TreeNode t,
            int part, int part2, ref char[] all, Span<char> buf, Span<char> buf1, Span<char> buf2, Span<char> buf3,
            ref int i, ref int j, ref int count, ref int len, ref int tlen)
        {
            var state = states.find;

            while (i < len)
            {
                switch (state)
                {
                    case states.start:
                        break;
                    case states.find:
                    {
                        if (buf[i] != t.text[j])
                        {
                            i = t.text.Length - tlen;
                            t = t.next;
                            tlen = t.text.Length;
                            if (precheck_found(t, buf, i) == -1)
                            {
                                
                            }
                        }

                        i++;
                        tlen--;
                        if (tlen == 0)
                        {
                            if (t.node == null)
                            {
                                write_pre(d, ref all, ref i);
                                write_repl(d, ref t);
                                t = tree;
                                j = 0;
                            }
                            else
                            {
                                t = t.node[0];
                                if (precheck_found(t, buf, i))
                                {
                                    
                                }

                            }
                        }

                        if (i >= len)
                        {
                            state = states.move;
                        }

                        break;
                    }
                    case states.move:
                    {
                        if (i != part2) break;

                        buf3.CopyTo(buf1);

                        len = s.ReadBlock(buf2);
                        if (len == 0)
                        {
                            count = part;
                        }
                        else if (len == part2)
                            part++;
                        else
                        {
                            count = part + len;
                        }

                        i = 0;
                            break;
                    }
                    case states.end:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }*/

        /*private static void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree, ref TreeNode t,
            int part, int part2, ref char[] all, Span<char> buf, Span<char> buf1, Span<char> buf2, Span<char> buf3,
            ref int i, ref int j, ref int count, ref int len, ref int tlen)
        {
            while (i < len)
            {
                //ReplaceBuf(allbuf, i, len);
                if (buf[i] == t.text[j] /*&& buf[i + t.text.Length] == t.text[t.text.Length - 1]#1#)
                {
                }

                i++;
                tlen--;
                if (tlen == 0)
                {
                    if (t.node == null)
                    {
                        write_pre(d, ref all, ref i);
                        write_repl(d, ref t);
                        t = tree;
                        j = 0;
                    }
                    else
                    {
                        t = t.node[0];
                        if (precheck_found(t, buf, i))
                        {

                        }

                    }
                }

                if (i >= len)
                {
                    if (i != part2) break;

                    buf3.CopyTo(buf1);

                    len = s.ReadBlock(buf2);
                    if (len == 0)
                    {
                        count = part;
                    }
                    else if (len == part2)
                        part++;
                    else
                    {
                        count = part + len;
                    }

                    i = 0;
                }
            }
        }*/

        /*private static void ReplaceFile(string file)
        {
            using (var s = new StreamReader(file))
            using (var r = new StreamWriter(file + ".repl"))
            {
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
                //var end = 0;

                var t = tree;
                var j = 0;

                var count = 0;

                var len = s.ReadBlock(allbuf);
                if (len == partsLen)
                {
                    len = start = parts2Len;
                    count = partsLen;

                    while (i < len)
                    {
                        ReplaceBuf(allbuf, i, len);
                        /*if (len >= t.min_len)
                        {
                            if (allbuf[i] == t.text[i] &&
                                allbuf[i + t.text.Length] == t.text[t.text.Length - 1])
                            {

                            }
                        }#1#

                        i++;

                        if (i >= len)
                        {
                            if (i != parts2Len) break;

                            buf3.CopyTo(buf);

                            len = s.ReadBlock(buf2);
                            if (len == 0)
                            {
                                //end = len = partLen;
                                count = partLen;
                            }
                            else if (len == parts2Len)
                                part++;
                            else
                            {
                                //end = partLen + len;
                                count = partLen + len;
                            }

                            i = 0;

                        }
                    }
                }
                else
                {
                    //end = len;
                    count = len;
                }

                if (len > 0)
                {
                    for (; i < len; i++)
                        ReplaceBuf(allbuf, i, count);
                }
            }
        }*/

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReplaceBuf(Span<char> buf, int i, int len)
        {
            var t = tree;

            if (len >= t.min_len)
            {
                if (buf[i] == t.text[0] &&
                    buf[i + t.text.Length] == t.text[tree.text.Length - 1])
                {
                    var found = true;

                    if (t.text.Length > 2)
                    for (int j = 1; j < t.text.Length - 2; j++)
                    {
                        if (buf[i + j] != t.text[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        // write
                    }
                }
            }
        }*/
    }
}
