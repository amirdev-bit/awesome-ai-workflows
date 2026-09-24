using Oathsunder.Core.Mathematics;
using Oathsunder.Core.Serialization;
using NUnit.Framework;

namespace Oathsunder.Tests.Core
{
    [TestFixture]
    public sealed class JsonReaderTests
    {
        [Test]
        public void ParsesAllValueKinds()
        {
            var root = JsonReader.Parse("{\"s\":\"a\\n\\u0041\",\"n\":-1.5e1,\"t\":true,\"f\":false,\"z\":null,\"a\":[1,2,3],\"o\":{\"k\":1}}");
            Assert.AreEqual("a\nA", root.Get("s").AsString());
            Assert.AreEqual(Fixed.FromInt(-15), root.Get("n").AsFixed());
            Assert.IsTrue(root.Get("t").AsBool());
            Assert.IsFalse(root.Get("f").AsBool());
            Assert.AreEqual(JsonKind.Null, root.Get("z").Kind);
            Assert.AreEqual(3, root.Get("a").Count);
            Assert.AreEqual(1, root.Get("o").Get("k").AsInt());
        }

        [Test]
        public void KeepsDocumentOrderAndPaths()
        {
            var root = JsonReader.Parse("{\"b\":1,\"a\":[{\"x\":2}]}");
            Assert.AreEqual("b", root.Members[0].Key);
            Assert.AreEqual("$.a[0].x", root.Get("a").Items[0].Get("x").Path);
        }

        [TestCase("{", "unexpected end")]
        [TestCase("{\"a\":1,}", "expected a string key")]
        [TestCase("[1 2]", "expected ',' or ']'")]
        [TestCase("{\"a\":1,\"a\":2}", "duplicate key")]
        [TestCase("01", "unexpected trailing content")]
        [TestCase("\"abc", "unterminated string")]
        [TestCase("tru", "expected 'true'")]
        public void RejectsInvalidDocumentsWithLocation(string json, string fragment)
        {
            var exception = Assert.Throws<JsonSyntaxException>(() => JsonReader.Parse(json, "test.json"));
            StringAssert.Contains(fragment, exception.Message);
            StringAssert.StartsWith("test.json(", exception.Message);
        }

        [Test]
        public void ReportsLineAndColumn()
        {
            var exception = Assert.Throws<JsonSyntaxException>(() => JsonReader.Parse("{\n  \"a\": ?\n}"));
            Assert.AreEqual(2, exception.Line);
            Assert.AreEqual(8, exception.Column);
        }

        [Test]
        public void ContentErrorsNameThePath()
        {
            var root = JsonReader.Parse("{\"moves\":[{\"frames\":\"x\"}]}");
            var exception = Assert.Throws<JsonContentException>(() => root.Get("moves").Items[0].Get("frames").AsInt());
            Assert.AreEqual("$.moves[0].frames", exception.JsonPath);
            Assert.Throws<JsonContentException>(() => root.Require("missing"));
        }

        [Test]
        public void ToleratesByteOrderMark()
        {
            Assert.AreEqual(1, JsonReader.Parse("﻿{\"a\":1}").Get("a").AsInt());
        }
    }
}
