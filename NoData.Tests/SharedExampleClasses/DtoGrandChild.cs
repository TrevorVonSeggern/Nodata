using System.Collections.Generic;

namespace NoData.Tests.SharedExampleClasses
{
	public class DtoGrandChild
	{
			public int id { get; set; }
			public string Name { get; set; }
			public string region_code { get; set; }

			public IEnumerable<int> GetAllIds()
			{
				yield return id;
			}
	}
}