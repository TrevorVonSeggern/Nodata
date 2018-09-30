using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CodeTools;
using Xunit;

// Thanks to https://blogs.msdn.microsoft.com/kevinpilchbisson/2007/11/20/enforcing-immutability-in-code/

namespace NoData.Tests.SubProjects.CodeToolsTests
{
    public class CodeToolsTests
    {
        private IEnumerable<Assembly> AssembliesToTest
        {
            get
            {
                return new[] {
                    System.Reflection.Assembly.GetExecutingAssembly(),
                    Assembly.GetAssembly(typeof(Settings)),
                    Assembly.GetAssembly(typeof(Graph.Graph<,,,>)),
                };
            }
        }

        // It's particularly important that 'struct' types are immutable.
        // for a short discussion, see http://blogs.msdn.com/jaybaz_ms/archive/2004/06/10/153023.aspx
        [Fact]
        public void EnsureStructsAreImmutableTest()
        {
            var mutableStructs = from type in AssembliesToTest.GetTypes()
                                 where IsMutableStruct(type)
                                 select type;

            if (mutableStructs.Any())
            {
                Assert.False(true, $"'{mutableStructs.First().FullName}' is a value type, but was not marked with the [Immutable] attribute");
            }
        }

        // ensure that any type marked [Immutable] has fields that are all immutable
        [Fact]
        public void EnsureImmutableTypeFieldsAreMarkedImmutableTest()
        {
            var whiteList = new Type[] {
                typeof(IEnumerable<>),
                typeof(IList<>),
                typeof(IReadOnlyCollection<>),
                typeof(ICollection<>),
                typeof(IDictionary<,>),
            };
            try
            {
                ImmutableAttribute.VerifyTypesAreImmutable(AssembliesToTest, whiteList);
            }
            catch (ImmutableAttribute.ImmutableFailureException ex)
            {
                Console.Write(FormatExceptionForAssert(ex));
                Assert.False(true, $"'{ex.Type.Name}' failed the immutability test. " + Environment.NewLine + ex.Message);
            }
        }

        internal static bool IsMutableStruct(Type type)
        {
            if (!type.IsValueType) return false;
            if (type.IsEnum) return false;
            if (type.IsSpecialName) return false;
            if (type.Name.StartsWith("__")) return false;
            if (type.IsDefined(typeof(CompilerGeneratedAttribute), false)) return false;
            if (ReflectionHelper.TypeHasAttribute<ImmutableAttribute>(type)) return false;
            if (!type.IsInterface) return false;
            return true;
        }

        static string FormatExceptionForAssert(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            string indent = "";

            for (; ex != null; ex = ex.InnerException)
            {
                sb.Append(indent);
                sb.AppendLine(ex.Message);

                indent = indent + "    ";
            }

            return sb.ToString();
        }

    }
}