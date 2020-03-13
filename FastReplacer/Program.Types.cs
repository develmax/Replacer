using System;
using System.Collections.Generic;

namespace FastReplacer
{
    partial class Program
    {
        /*private static Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            {"new_name", "new_caption"},
            {"new_text", "new_info"}
        };*/

        

        private static TreeNode tree = new TreeNode
        {
            text = "new_",
            len = 4,
            max_len = 8,
            //max_len = 7,
            //min_len = 7,
            child = new TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                //max_len = 8,
                nextSibling = new TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8//,
                    //max_len = 8
    }
            }
        };

        static Program()
        {
            tree.child.parent = tree;
            tree.child.nextSibling.parent = tree;
        }
    }
}