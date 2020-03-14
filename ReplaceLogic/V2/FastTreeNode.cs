using System;
using System.Collections.Generic;
using System.Text;

namespace ReplaceLogic.V2
{
    public struct FastTreeNode
    {
        public int text;
        public int repl;
        public int child;
        public int nextSibling;
        public int parent;
        public int len;
    }
}
