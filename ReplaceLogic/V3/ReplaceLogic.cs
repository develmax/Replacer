using System;
using System.IO;

namespace ReplaceLogic.V3
{
    public class ReplaceLogic
    {
        private StreamReader s;
        private StreamWriter d;
        private TreeNode tree;
        private int max_len;

        public void ReplaceFile(string file, TreeNode tree, int max_len)
        {
            using (var s = new StreamReader(file))
            using (var d = new StreamWriter(file + ".repl"))
            {
                ReplaceFile(s, d, tree, max_len);
                /*d.Flush();
                d.Close();*/
            }
        }

        private int partLen;
        private int part2Len;
        private int part3Len;
        private int partsLen;

        private int partLastIndex;
        private int part2LastIndex;
        private int part3LastIndex;
        private int partsLastIndex;

        private int index = 0;
        private int lastUpdatedBlockNumber = 0;
        private bool blockChecked = false;
        private int checkNodeTextLen = 0;
        private bool charChecking = false;
        private int charCheckingIndex = -1;
        private int rollbackCharCheckingIndex = -1;
        private int writeLastIndex = -1;

        private bool lastRead = false;
        private int lastIndex = -1;

        private int writeIndex = -1;
        private bool continueCheck = false;

        private int readLen = 0;

        private TreeNode node;

        private states state;

        private unsafe void ReplaceFile(StreamReader s, StreamWriter d, TreeNode tree, int max_len)
        {
            this.s = s;
            this.d = d;
            this.tree = tree;
            this.max_len = max_len;

            this.partLen = max_len;
            this.part2Len = partLen + partLen;
            this.part3Len = part2Len + partLen;
            this.partsLen = part2Len + part2Len;

            this.partLastIndex = partLen - 1;
            this.part2LastIndex = part2Len - 1;
            this.part3LastIndex = part3Len - 1;
            this.partsLastIndex = partsLen - 1;

            Span<char> buf = stackalloc char[partsLen];

            var firstBuf = buf.Slice(0, part2Len);
            var lastBuf = buf.Slice(part2Len, part2Len);

            this.node = tree;

            this.state = states.start;

            while(state != states.exit)
            switch (state)
            {
                case states.start:
                    state = start();
                    break;
                case states.read_first:
                    state = read_first(ref firstBuf);
                    break;
                case states.check_block:
                    state = check_block(ref buf, ref firstBuf, ref lastBuf);
                    break;
                case states.move_index:
                    state = move_index();
                    break;
                case states.fast_check_node:
                    state = fast_check_node(ref buf);
                    break;
                case states.check_node:
                    state = check_node();
                    break;
                case states.next_node:
                    state = next_node();
                    break;
                case states.check_char:
                    state = check_char(ref buf);
                    break;
                case states.write_node:
                    state = write_node(ref buf);
                    break;
                case states.end:
                    state = end(ref buf);
                    break;
            }
        }

        private states start()
        {
            return states.read_first;
        }

        private states read_first(ref Span<char> firstBuf)
        {
            readLen = s.ReadBlock(firstBuf);
            if (readLen == 0)
                return states.end;

            if (readLen < part2Len)
            {
                lastRead = true;
                lastIndex = readLen-1;
            }

            //readCount += readLen;
            //processLen = readLen;

            lastUpdatedBlockNumber = 1;

            return states.fast_check_node;
        }

        private states move_index()
        {
            if (lastRead)
            {
                if (index == lastIndex)
                    return states.end;
            }
            else if (!blockChecked)
                return states.check_block;

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
                    return states.check_char;

                charChecking = false;
                return states.check_node;
            }

            return states.fast_check_node;
        }

        private states fast_check_node(ref Span<char> buf)
        {
            if (buf[index] != node.text[0])
                return states.next_node;

            if (node.text.Length == 1)
            {
                return states.check_node;
            }

            var nodeTextLastIndex = node.text.Length - 1;
            var lostLen = partsLen - index;

            if (lastRead)
            {
                if (lostLen == node.text.Length)
                {
                    if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
                        return states.next_node;
                }
                else if (lostLen < node.text.Length)
                {
                    if (nodeTextLastIndex - lostLen > lastIndex)
                        return states.next_node;

                    if (buf[nodeTextLastIndex - lostLen] != node.text[nodeTextLastIndex])
                        return states.next_node;
                }
                else
                {
                    if (index + nodeTextLastIndex > lastIndex)
                        return states.next_node;

                    if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
                        return states.next_node;
                }
            }
            else
            {
                if (lostLen < node.text.Length)
                {
                    if (buf[nodeTextLastIndex - lostLen] != node.text[nodeTextLastIndex])
                        return states.next_node;
                }
                else
                    if (buf[index + nodeTextLastIndex] != node.text[nodeTextLastIndex])
                        return states.next_node;
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

            return states.move_index;
        }

        private states check_node()
        {
            if (node.child != null)
            {
                node = node.child;
                return states.move_index;
            }
            else if (node.repl != null)
            {
                writeIndex = index;
                return states.write_node;
            }
            else
            {
                throw new Exception("node.repl is null");
            }
        }

        private states write_node(ref Span<char> buf)
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
                return states.fast_check_node;
            }

            return states.move_index;
        }

        private states next_node()
        {
            if (node.nextSibling != null)
            {
                node = node.nextSibling;
                return states.fast_check_node;
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
                    return states.write_node;
                }

                index = index - node.parent.text.Length;
                if (index < 0)
                    index = index + partsLen;

                node = node.parent;
                return states.next_node;
            }
            else
            {
                node = tree;
                return states.move_index;
            }
        }

        private states check_char(ref Span<char> buf)
        {
            if (buf[index] != node.text[charCheckingIndex])
            {
                index = rollbackCharCheckingIndex;
                charChecking = false;
                return states.next_node;
            }
            else
            {
                checkNodeTextLen--;
                charCheckingIndex++;

                return states.move_index;
            }
        }


        private states check_block(ref Span<char> buf, ref Span<char> firstBuf, ref Span<char> lastBuf)
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
            return states.move_index;
        }

        private states end(ref Span<char> buf)
        {
            if (writeLastIndex > lastIndex)
            {
                d.Write(buf.Slice(writeLastIndex + 1, partsLastIndex - writeLastIndex));
                d.Write(buf.Slice(0, lastIndex + 1));
            }
            else if (writeLastIndex < lastIndex)
                d.Write(buf.Slice(writeLastIndex + 1, lastIndex - writeLastIndex));

            return states.exit;
        }
    }
}
