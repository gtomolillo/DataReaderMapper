using AutoMapper.Mappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataReaderMapper.Tests
{
	[TestClass]
	public class Initializer
	{
		[AssemblyInitialize]
		public static void Init(TestContext context)
		{
			MapperRegistry.Mappers.Insert(0, new DataReaderMapper());
		}
	}
}
