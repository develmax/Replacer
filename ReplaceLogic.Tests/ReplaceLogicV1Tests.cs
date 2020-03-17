using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReplaceLogic.V1;

namespace ReplaceLogic.Tests
{
    [TestClass]
    public class ReplaceLogicV1Tests
    {
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ReadStringFromStream(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public string PrepareStreams(string str, Action<StreamReader, StreamWriter> replaceAction)
        {
            using (var stream = GenerateStreamFromString(str))
            using (var data = new StreamReader(stream))
            using (var resultStream = new MemoryStream())
            {
                var target = new StreamWriter(resultStream);

                replaceAction(data, target);

                target.Flush();
                resultStream.Position = 0;

                return ReadStringFromStream(resultStream);
            }
        }

        [TestMethod]
        public void ReplaceNoneSymbol()
        {
            var str = string.Empty;

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_Replace()
        {
            var str = "a";

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual("b", result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_NotReplace()
        {
            var str = "x";

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual("x", result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_EqualReplace()
        {
            var str = "a";

            var tree = new TreeNode
            {
                text = "a",
                repl = "a",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual("a", result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_ReplaceAAA()
        {
            var str = "aaaaaaaaaaa";

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual(new string('b', str.Length), result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_ReplaceAB1()
        {
            var str = "ababababab";

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual(new string('b', str.Length), result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_ReplaceAB2()
        {
            var str = "ababababab";

            var tree = new TreeNode
            {
                text = "b",
                repl = "a",
                len = 1
            };

            var max_len = 1;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual(new string('a', str.Length), result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_ReplaceAA1()
        {
            var str = "aaaaaaaaaa";

            var tree = new TreeNode
            {
                text = "aa",
                repl = "a",
                len = 2
            };

            var max_len = 2;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual(new string('a', str.Length / 2), result);
        }

        [TestMethod]
        public void ReplaceOneSymbol_ReplaceAxB1()
        {
            var str = "abacadfabc";

            var tree = new TreeNode
            {
                text = "a",
                repl = "b",
                len = 1,
                child = new TreeNode
                {
                    text = "b",
                    repl = "a",
                    len = 2,
                    nextSibling = new TreeNode
                    {
                        text = "c",
                        repl = "a",
                        len = 2
                    }
                }
            };

            tree.child.parent = tree;
            tree.child.nextSibling.parent = tree;

            var max_len = 2;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual("aabdfac", result);
        }

        [TestMethod]
        public void ReplaceManySymbol_ReplaceWithNullRoot()
        {
            var str = "ab_cd_efz";

            var tree = new TreeNode
            {
                text = null,
                child = new TreeNode
                {
                    text = "ab_",
                    len = 3,
                    child = new TreeNode
                    {
                        text = "hj",
                        repl = "repl1",
                        len = 5,
                        nextSibling = new TreeNode
                        {
                            text = "cd",
                            repl = "repl2",
                            len = 5
                        }
                    },
                    nextSibling = new TreeNode
                    {
                        text = "_",
                        repl = "repl3",
                        len = 1,
                        child = new TreeNode
                        {
                            text = "e",
                            repl = "repl4",
                            len = 2,
                            nextSibling = new TreeNode
                            {
                                text = "x",
                                repl = "repl5",
                                len = 2
                            }
                        },
                        nextSibling = new TreeNode
                        {
                            text = "f",
                            repl = "repl6",
                            len = 2
                        }
                    }
                }
            };

            tree.child.parent = tree;
            tree.child.child.parent = tree.child;
            tree.child.child.nextSibling.parent = tree.child;
            tree.child.nextSibling.parent = tree;
            tree.child.nextSibling.child.parent = tree.child.nextSibling;
            tree.child.nextSibling.child.nextSibling.parent = tree.child.nextSibling;

            tree.child.nextSibling.nextSibling.parent = tree;

            var max_len = 5;

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len); });

            Assert.AreEqual("repl2repl4repl6z", result);
        }

        [TestMethod]
        public void DictionaryToTreeConverter_Test1()
        {
            var dic = new Dictionary<string, string>
            {
                { "env.name", "test1" },
                { "env.ver", "1.0" },
                { "env.ver.name", "alfa" },
                { "sys.dir.temp", "c:\\temp" },
                { "sys.dir.win", "c:\\windows" },
                { "net.protocol", "http" },
                { "net.product", "sh" }
            };

            var tree = DictionaryToTreeConverter.Convert(dic, out var maxLen);

            var str = "net.protocol:net.product env.name env.ver (env.ver.name)";

            var result = PrepareStreams(str,
                (data, target) => { ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, maxLen); });

            Assert.AreEqual("http:sh test1 1.0 (alfa)", result);
        }
    }
}
