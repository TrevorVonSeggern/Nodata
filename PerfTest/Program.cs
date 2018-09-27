using System;
using System.Diagnostics;
using NoData.Tests.ExpandTests;

namespace PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program();
            program.LogProcessId();
            for (int i = 0; i < 100; ++i)
            {
                program.TestThings();
                Console.WriteLine(i);
            }
        }
        void LogProcessId()
        {
            Console.WriteLine("Hello World! - " + Process.GetCurrentProcess().Id);
        }


        void TestThings()
        {
            Test(null, 1, 2, 3, 4, 5, 6); // returns everything.
            Test("", 1, 2, 3, 4, 5, 6); // returns everything.
            Test("partner", 1, 2, 3, 4, 5, 6, 1, 2); // one expand
            Test("children", 1, 2, 3, 4, 5, 6, 10, 30, 40, 50, 60);
            Test("favorite", 1, 2, 3, 4, 5, 6, 10, 40);
            Test("favorite/favorite", 1, 2, 3, 4, 5, 6, 10, 40, 300);
            Test("partner,children", 1, 2, 3, 4, 5, 6, 1, 2, 10, 30, 40, 50, 60); // multiple expands.
            Test("children/partner", 1, 2, 3, 4, 5, 6, 10, 30, 40, 50, 60, 10, 60);
            Test("partner,children/partner", 1, 2, 3, 4, 5, 6, 2, 1, 10, 30, 40, 50, 60, 60, 10/*, 100, 200, 300, 400, 500, 600*/);
            Test("partner/children,partner/favorite", 1, 2, 3, 4, 5, 6, /*root*/ 1, 2, /*partner*/ 10, /*partner/children*/ 10 /*partner/favorite*/ );
            Test("partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2); // one expand
            Test("partner/partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2); // one expand
            Test("partner/partner/partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2, 1, 2); // one expand
            Test("partner($expand=partner)", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2);
            Test("partner($expand=partner($expand=partner))", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2);
            Test("partner($expand=partner($expand=partner($expand=partner)))", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2, 1, 2);
            Test("partner($expand=partner($expand=partner($expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2);
            Test("partner($expand=partner;$filter=id eq 1)", 2, 1, 2);
            Test("partner($expand=partner($expand=partner($expand=partner;$filter=id eq 1;$select=id)))", 2, 1, 2, 1, 2);
            Test("partner($expand=partner;$filter=id eq 1;$select=id)", 2, 1, 2);
            Test("partner($expand=partner($expand=partner($select=id;$expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2);
            Test("partner($select=id;$expand=partner;$filter=id eq 1)", 2, 1, 2);
            Test("partner($select=id,Name;$expand=partner($select=id,region_code;$expand=partner($select=id;$select=id;$expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2);
        }

        void Test(string expand, params int[] expectedIds)
        {
            var tst = new ExpandTest();
            tst.Expand_Expression(expand, expectedIds);
        }

    }
}
