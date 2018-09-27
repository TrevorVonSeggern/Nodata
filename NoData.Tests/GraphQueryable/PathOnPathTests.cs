using Xunit;
using System.Collections.Generic;
using NoData.Utility;

namespace NoData.Tests.GraphQueryable
{
    using System.Linq;
    using FluentAssertions;
    using SharedTypes;

    public class PathOnPathTests
    {
        [Fact]
        public void PathOnPathTests_FilteringPaths()
        {
            //Given
            var v1 = new Vertex("1");
            var v2 = new Vertex("2");
            var v3 = new Vertex("3");
            var e1 = new Edge(v1, v2, "e1-2");
            var e2 = new Edge(v2, v3, "e2-3");
            var p1 = new Path(new[] { e1 });
            var p2_too_long = new Path(new[] { e1, e2 });
            var paths_just_right = new[] { p1 };
            var paths = new[] { p1, p2_too_long };

            //When
            var paths_filtered = paths.Where(p => paths_just_right.Any(pj => pj.Equals(p))).ToList();

            //Then
            paths_filtered.Should().NotBeNullOrEmpty();
            paths_filtered.SequenceEqual(paths_just_right);
        }

        [Fact]
        public void PathOnPathTests_FilterPaths_After_TreeFlatten()
        {
            //Given
            var v1 = new Vertex("1");
            var v2 = new Vertex("2");
            var v3 = new Vertex("3");
            var e1 = new Edge(v1, v2, "e1-2");
            var e2 = new Edge(v2, v3, "e2-3");
            var p1 = new Path(new[] { e1 });
            var p2_too_long = new Path(new[] { e1, e2 });
            var paths_just_right = new[] { p1 };
            var paths = new[] { p1, p2_too_long };
            var t1 = new Tree(v1, paths_just_right);
            var paths_from_tree = t1.EnumerateAllPaths<Path>(edges => new Path(edges)).ToList();

            //When
            var paths_filtered = paths.Where(p => paths_from_tree.Any(pj => pj.Equals(p))).ToList();

            //Then
            paths_filtered.Should().NotBeNullOrEmpty();
            paths_filtered.SequenceEqual(paths_just_right);
        }

        [Fact]
        public void PathOnPathTests_TreeFlatten_WithLongPaths_FlatteningIncludesShortPaths()
        {
            //Given
            var v1 = new Vertex("1");
            var v2 = new Vertex("2");
            var v3 = new Vertex("3");
            var v4 = new Vertex("4");
            var e1 = new Edge(v1, v2, "e1-2");
            var e2 = new Edge(v2, v3, "e2-3");
            var e3 = new Edge(v3, v4, "e3-4");
            var p1 = new Path(new[] { e1 });
            var p2 = new Path(new[] { e1, e2 }); // included in the tree.
            var p3 = new Path(new[] { e1, e2, e3 });
            var all_paths = new[] { p1, p2, p3 };
            var t1 = new Tree(v1, new[] { p2 });
            var paths_from_tree = t1.EnumerateAllPaths<Path>(edges => new Path(edges)).ToList();

            //When
            var paths_filtered = all_paths.Where(p => paths_from_tree.Any(pj => pj.Equals(p))).ToList();

            //Then
            var paths_should_be_included = new[] { p1, p2 };
            var paths_should_not_be_included = new[] { p3 };
            paths_filtered.Should().NotBeNullOrEmpty();
            paths_filtered.SequenceEqual(paths_should_be_included);
            paths_filtered.Should().NotContain(p3);
        }
    }
}