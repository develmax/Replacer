using System;
using System.Collections.Generic;

namespace FastReplacer
{
    partial class Program
    {
        private static Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            {"new_name", "new_caption"},
            {"new_text", "new_info"}
        };

        private class TreeNode
        {
            public string text;
            public string repl;
            public TreeNode child;
            public TreeNode nextSibling;
            public TreeNode parent;
            public int len;
            public int max_len;
            public int min_len;
        }

        private static TreeNode tree = new TreeNode
        {
            text = "new_",
            len = 3,
            max_len = 7,
            min_len = 7,
            child = new TreeNode
            {
                text = "name",
                repl = "new_cation",
                len = 4,
                nextSibling = new TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 4
                }
            }
        };

        static Program()
        {
            tree.child.nextSibling.parent = tree;
        }
    }
}