using ReplaceLogic;

namespace FastReplacer
{
    partial class Program
    {
        /*private static Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            {"new_name", "new_caption"},
            {"new_text", "new_info"}
        };*/

        private static ReplaceLogic.V1.TreeNode treeV1 = new ReplaceLogic.V1.TreeNode
        {
            text = "new_",
            len = 4,
            child = new ReplaceLogic.V1.TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new ReplaceLogic.V1.TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8
                }
            }
        };

        static void ReplaceLogicV1()
        {
            treeV1.child.parent = treeV1;
            treeV1.child.nextSibling.parent = treeV1;
        }

        private static string[] treeV2Str = 
        {
            "new_",
            "name",
            "new_caption",
            "text",
            "new_info"
        };

        private static ReplaceLogic.V2.FastTreeNode[] treeV2 = 
        {
            new ReplaceLogic.V2.FastTreeNode
            {
                text = 0,
                repl = -1,
                len = 4,
                child = 1,
                parent = -1,
                nextSibling = -1
            },
            new ReplaceLogic.V2.FastTreeNode
            {
                text = 1,
                repl = 2,
                len = 8,
                child = -1,
                nextSibling = 2,
                parent = 0
            },
            new ReplaceLogic.V2.FastTreeNode
            {
                text = 3,
                repl = 4,
                len = 8,
                child = -1,
                parent = 0,
                nextSibling = -1
            }
        };

        static void ReplaceLogicV2()
        {

        }

        private static ReplaceLogic.V3.TreeNode treeV3 = new ReplaceLogic.V3.TreeNode
        {
            text = "new_",
            len = 4,
            child = new ReplaceLogic.V3.TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new ReplaceLogic.V3.TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8
                }
            }
        };

        static void ReplaceLogicV3()
        {
            treeV3.child.parent = treeV3;
            treeV3.child.nextSibling.parent = treeV3;
        }

        private static ReplaceLogic.V4.TreeNode treeV4 = new ReplaceLogic.V4.TreeNode
        {
            text = "new_",
            len = 4,
            child = new ReplaceLogic.V4.TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new ReplaceLogic.V4.TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8
                }
            }
        };

        static void ReplaceLogicV4()
        {
            treeV4.child.parent = treeV4;
            treeV4.child.nextSibling.parent = treeV4;
        }

        private static ReplaceLogic.V5.TreeNode treeV5 = new ReplaceLogic.V5.TreeNode
        {
            text = "new_",
            len = 4,
            child = new ReplaceLogic.V5.TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new ReplaceLogic.V5.TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8
                }
            }
        };

        static void ReplaceLogicV5()
        {
            treeV5.child.parent = treeV5;
            treeV5.child.nextSibling.parent = treeV5;
        }

        private static ReplaceLogic.V6.TreeNode treeV6 = new ReplaceLogic.V6.TreeNode
        {
            text = "new_",
            len = 4,
            child = new ReplaceLogic.V6.TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new ReplaceLogic.V6.TreeNode
                {
                    text = "text",
                    repl = "new_info",
                    len = 8
                }
            }
        };

        static void ReplaceLogicV6()
        {
            treeV6.child.parent = treeV6;
            treeV6.child.nextSibling.parent = treeV6;
        }

        private static string[] treeV7Str =
        {
            "new_",
            "name",
            "new_caption",
            "text",
            "new_info"
        };

        private static ReplaceLogic.V7.FastTreeNode[] treeV7 =
        {
            new ReplaceLogic.V7.FastTreeNode
            {
                text = 0,
                repl = -1,
                len = 4,
                child = 1,
                parent = -1,
                nextSibling = -1
            },
            new ReplaceLogic.V7.FastTreeNode
            {
                text = 1,
                repl = 2,
                len = 8,
                child = -1,
                nextSibling = 2,
                parent = 0
            },
            new ReplaceLogic.V7.FastTreeNode
            {
                text = 3,
                repl = 4,
                len = 8,
                child = -1,
                parent = 0,
                nextSibling = -1
            }
        };

        static void ReplaceLogicV7()
        {

        }

        static Program()
        {
            ReplaceLogicV1();
            ReplaceLogicV2();
            ReplaceLogicV3();
            ReplaceLogicV4();
            ReplaceLogicV5();
            ReplaceLogicV6();
            ReplaceLogicV7();
        }
    }
}