using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using Immutability;

// Thanks to https://blogs.msdn.microsoft.com/kevinpilchbisson/2007/11/20/enforcing-immutability-in-code/
namespace NoData.Tests.SubProjects.CodeToolsTests
{
    public class CodeToolsTests
    {
        private Immutability.Test immutability = new Immutability.Test(new[] {
                    typeof(Settings),
                    typeof(Graph.Graph<,,,>),
                });


        [Fact]
        public void EnsureStructsAreImmutableTest()
        {
            immutability.EnsureStructsAreImmutableTest();
        }

        [Fact]
        public void EnsureImmutableTypeFieldsAreMarkedImmutableTest()
        {
            immutability.EnsureImmutableTypeFieldsAreMarkedImmutableTest();
        }
    }
}