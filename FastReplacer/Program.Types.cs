using ReplaceLogic;
using ReplaceLogic.V1;
using ReplaceLogic.V2;

namespace FastReplacer
{
    partial class Program
    {
        /*private static Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            {"new_name", "new_caption"},
            {"new_text", "new_info"}
        };*/

        private static TreeNode treeV1 = new TreeNode
        {
            text = "new_",
            len = 4,
            child = new TreeNode
            {
                text = "name",
                repl = "new_caption",
                len = 8,
                nextSibling = new TreeNode
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

        private static FastTreeNode[] treeV2 = 
        {
            new FastTreeNode
            {
                text = 0,
                repl = -1,
                len = 4,
                child = 1,
                parent = -1,
                nextSibling = -1
            },
            new FastTreeNode
            {
                text = 1,
                repl = 2,
                len = 8,
                child = -1,
                nextSibling = 2,
                parent = 0
            },
            new FastTreeNode
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

        static Program()
        {
            ReplaceLogicV1();
            ReplaceLogicV2();
            ReplaceLogicV3();
            ReplaceLogicV4();
            ReplaceLogicV5();
        }
    }
}