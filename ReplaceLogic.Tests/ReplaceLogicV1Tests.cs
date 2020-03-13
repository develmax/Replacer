using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

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

            var result = PrepareStreams(str, (data, target) =>
            {
                ReplaceLogic.V1.ReplaceLogic.ReplaceFile(data, target, tree, max_len);
            });

            Assert.AreEqual(new string('a', str.Length/2), result);
        }
    }
}
