using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplaceLogic.V1
{
    public static class DictionaryToTreeConverter
    {
        public static TreeNode Convert(Dictionary<string, string> dic, out int maxLen)
        {
            TreeNode root = null;

            var items = new Dictionary<string, TreeNode>();

            maxLen = 0;

            foreach (var item in dic.OrderByDescending(i => i.Key.Length))
            {
                if (string.IsNullOrEmpty(item.Key)) continue;

                if (items.TryGetValue(item.Key, out var foundNode))
                {
                    foundNode.repl = item.Value;
                    foundNode.len = item.Key.Length;

                    continue;
                }

                var node = new TreeNode
                {
                    repl = item.Value,
                    text = item.Key,
                    len = item.Key.Length
                };

                if (maxLen == 0) maxLen = node.len;

                //var addInParent = false;
                var founded = false;

                var foundedCount = 1;
                var foundedLength = item.Key.Length;

                for (var j = item.Key.Length - 1; j >= 1; j--)
                {
                    var str = item.Key.Substring(0, j);

                    var count = dic.Count(l => l.Key.StartsWith(str));

                    if (foundedCount != count)
                    {
                        node.text = item.Key.Substring(j, foundedLength - j);

                        if (!items.TryGetValue(str, out var foundParent))
                        {
                            foundParent = new TreeNode
                            {
                                text = item.Key.Substring(0, j),
                                child = node
                            };

                            node.parent = foundParent;
                            node = foundParent;

                            items.Add(str, node);

                            //addInParent = true;
                        }
                        else
                        {
                            if (foundParent.child == null)
                            {
                                foundParent.child = node;
                            }
                            else
                            {
                                var child = foundParent.child;

                                while (child.nextSibling != null)
                                    child = child.nextSibling;
                                child.nextSibling = node;
                            }

                            node.parent = foundParent;
                            node = foundParent;

                            //addInParent = true;
                            founded = true;
                            break;
                        }

                        foundedLength = j;
                        foundedCount = count;
                    }
                }

                /*if (!addInParent)
                {
                    if (root == null) root = node;
                    else
                    {
                        if (root.repl == null && root.text == null)
                        {
                            if (root.child == null)
                                root.child = node;
                            else
                            {
                                var child = root.child;

                                while (child.nextSibling != null)
                                    child = child.nextSibling;
                                child.nextSibling = node;
                            }
                        }
                        else
                        {
                            root = new TreeNode { child = root };

                            var child = root.child;

                            while (child.nextSibling != null)
                                child = child.nextSibling;
                            child.nextSibling = node;
                        }

                        node.parent = root;
                    }
                }
                else */if (root == null)
                    root = node;
                else if (!founded)
                {
                    if (root.repl == null && root.text == null)
                    {
                        if (root.child == null)
                            root.child = node;
                        else
                        {
                            var child = root.child;

                            while (child.nextSibling != null)
                                child = child.nextSibling;
                            child.nextSibling = node;
                        }
                    }
                    else
                    {
                        root = new TreeNode { child = root };
                        root.child.parent = root;

                        var child = root.child;

                        while (child.nextSibling != null)
                            child = child.nextSibling;
                        child.nextSibling = node;
                    }

                    node.parent = root;
                }
            }

            return root;
        }
    }
}