﻿using System;
using System.IO;

namespace ReplaceLogic.V6
{
    public static class ReplaceLogic
    {
        public static void ReplaceFile(string file, TreeNode tree, int max_len)
        {
            using (var s = new StreamReader(file))
            using (var d = new StreamWriter(file + ".repl"))
            {
                ReplaceFile(s, d, tree, max_len);
                /*d.Flush();
                d.Close();*/
            }
        }

        public static void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree, int max_len)
        {
            var partLen = max_len;
            var part2Len = partLen + partLen;
            var part3Len = part2Len + partLen;
            var partsLen = part2Len + part2Len;

            var partLastIndex = partLen - 1;
            var part2LastIndex = part2Len - 1;
            var part3LastIndex = part3Len - 1;
            var partsLastIndex = partsLen - 1;

            Span<char> buf = new char[partsLen];

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

            TreeNode node = tree;

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

                        if (buf[index] != node.text[0])
                            goto case states.next_node;

                        if (node.text.Length == 1)
                        {
                            goto case states.check_node;
                        }

                        var nodeTextLastIndex = node.text.Length - 1;
                        var lostLen = partsLen - index;

                        if (lastRead)
                        {
                            if (lostLen == node.text.Length)
                            {
                                if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else if (lostLen < node.text.Length)
                            {
                                if (nodeTextLastIndex - lostLen > lastIndex)
                                    goto case states.next_node;

                                if (buf[nodeTextLastIndex - lostLen] != node.text[nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else
                            {
                                if (index + nodeTextLastIndex > lastIndex)
                                    goto case states.next_node;

                                if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                        }
                        else
                        {
                            if (lostLen < node.text.Length)
                            {
                                if (buf[nodeTextLastIndex - lostLen] != node.text[nodeTextLastIndex])
                                    goto case states.next_node;
                            }
                            else
                                if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
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
                        if (node.child != null)
                        {
                            node = node.child;
                            goto case states.move_index;
                        }
                        else if (node.repl != null)
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
                        var tmpLen = node.len;
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

                        d.Write(node.repl);
                        node = tree;

                        if (continueCheck)
                        {
                            continueCheck = false;
                            goto case states.fast_check_node;
                        }

                        goto case states.move_index;
                    }
                case states.next_node:
                    {
                        if (node.nextSibling != null)
                        {
                            node = node.nextSibling;
                            goto case states.fast_check_node;
                        }
                        else if (node.parent != null)
                        {
                            if (node.parent.repl != null)
                            {
                                node = node.parent;

                                if (index == 0)
                                    writeIndex = partsLastIndex;
                                else
                                    writeIndex = index - 1;

                                continueCheck = true;
                                goto case states.write_node;
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
                case states.check_char:
                    {
                        if (buf[index] != node.text[charCheckingIndex])
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
        }
    }
}
