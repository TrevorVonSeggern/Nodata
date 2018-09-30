using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleEFCoreApi.Controllers
{
    public static class EnumerableStreamer
    {
        public static IEnumerable<int> SlowEnumerable()
        {
            foreach (var number in Enumerable.Range(0, 5))
            {
                Console.WriteLine("+");
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                yield return number;
            }
        }
    }
}