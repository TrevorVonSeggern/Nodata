//using NUnit.Framework;
//using NoData.Internal.TreeParser.Tokenizer;

//namespace BinaryExpressionParserTests
//{
//    [TestFixture]
//    public class BinaryExpressionParserTests
//    {
//        class Dto
//        {
//            public string id { get; set; }
//            public string Name { get; set; }
//            public string region_code { get; set; }
//        }

//        [TestCase("gt")]
//        [TestCase("ge")]
//        [TestCase("lt")]
//        [TestCase("le")]
//        [TestCase("eq")]
//        [TestCase("ne")]
//        public void Test_id_comparedTo_1(string comparison)
//        {
//            // id gt 1
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree($"id {comparison} 1");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Token.Value, comparison);
//            Assert.AreEqual(ft.Root.Children[0].Token.Type, TokenTypes.classProperties.ToString());
//            Assert.AreEqual(ft.Root.Children[1].Token.Type, TokenTypes.number.ToString());
//        }

//        [TestCase("eq")]
//        [TestCase("ne")]
//        public void Test_Name_EqualsOrNot_String(string comparison)
//        {
//            // id eq 'George'
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree($"Name {comparison} 'George'");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Token.Value, comparison);
//            Assert.AreEqual(ft.Root.Children[0].Token.Type, TokenTypes.classProperties.ToString());
//            Assert.AreEqual(ft.Root.Children[1].Token.Type, TokenTypes.quotedString.ToString());
//        }

//        [Test]
//        public void Test_MultipleComparisons_1()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("id gt 1 and Name eq 'George'");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(LogicalOperatorNode));
//            Assert.AreEqual(ft.Root.Children[0].GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Children[1].GetType(), typeof(ComparitorNode));
//        }

//        [Test]
//        public void Test_MultipleComparisons_2()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("region_code ne 'george' and Name eq 'ES'");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(LogicalOperatorNode));
//            Assert.AreEqual(ft.Root.Children[0].GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Children[1].GetType(), typeof(ComparitorNode));
//        }

//        [Test]
//        public void Test_MultipleComparisons_3()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree($"region_code eq 'george' or Name eq 'ES'");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(LogicalOperatorNode));
//            Assert.AreEqual(ft.Root.Children[0].GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Children[1].GetType(), typeof(ComparitorNode));
//        }

//        [Test]
//        public void Test_MultipleComparisons_4()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("id le 1 or id ge 1");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(LogicalOperatorNode));
//            Assert.AreEqual(ft.Root.Children[0].GetType(), typeof(ComparitorNode));
//            Assert.AreEqual(ft.Root.Children[1].GetType(), typeof(ComparitorNode));
//        }

//        [Test]
//        public void Test_MultipleComparisons_5()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("id eq 1 or id eq 1 and id eq 1 or id eq 1 and id eq 1 or id eq 1");
//            Assert.NotNull(ft?.Root);
//            Assert.AreEqual(ft.Root.GetType(), typeof(LogicalOperatorNode));
//        }

//        [Test]
//        public void Test_MultipleComparisons_fail_1()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("id le 1 and or id ge 1");
//            Assert.Null(ft.Root);
//        }

//        [Test]
//        public void Test_MultipleComparisons_fail_2()
//        {
//            var ft = new FilterTree<Dto>();
//            ft.ParseTree("id Name le 1 and id ge 1");
//            Assert.Null(ft.Root);
//        }

//        [Test]
//        public void TestMethod1()
//        {
//            // endswith(Name,'eorge')
//            // startswith(Name,'george')
//            // substringof(Name,'eorg') // right is contained within the left paramter
//            // length(Name) gt 1
//            // indexof(Name, 'ame') eq 1
//            // replace(Name, 'Name', 'ReplacedName') eq 'ReplacedName'
//            // substring(Name, 'Name', 'ReplacedName') eq 'ReplacedName'
//        }
//    }
//}
