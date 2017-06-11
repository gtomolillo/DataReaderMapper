using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using AutoMapper;
using DataReaderMapper.Tests.Data;
using DataReaderMapper.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataReaderMapper.Tests
{
	[TestClass]
	public class DataReaderMapperTests
	{
		private readonly IMapper _mapper;

		public DataReaderMapperTests()
		{
			_mapper = new Mapper(new MapperConfiguration(config =>
			{
				config.Mappers.Insert(0, new DataReaderObjectMapper());
			}));
		}

		#region Object Mapping

		[TestMethod]
		public void ObjectMappingTest()
		{
			var table = new DataTable("FirstTable")
				.AddColumn("Id", typeof(int))
				.AddColumn("StringColumn", typeof(string))
				.AddColumn("DecimalColumn", typeof(decimal))
				.AddColumn("DateTimeColumn", typeof(DateTime))
				.AddColumn("NullableIntColumn", typeof(int))
				.AddColumn("NullableDecimalColumn", typeof(decimal))
				.AddColumn("NullableDateTimeColumn", typeof(DateTime));

			table.AddRow(
				1
				, "First Line"
				, 666.45
				, new DateTime(2017, 1, 1)
				, 999
				, null
				, DBNull.Value
			);

			table.AddRow(
				2
				, "Second Line"
				, decimal.MaxValue
				, DateTime.MaxValue
				, int.MaxValue
				, null
				, DBNull.Value
			);

			var dataReader = table.CreateDataReader();

			var obj = _mapper.Map<FirstTable>(dataReader);

			Assert.AreEqual(1, obj.Id);
			Assert.AreEqual("First Line", obj.StringColumn);
			Assert.AreEqual(666.45m, obj.DecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.DateTimeColumn);
			Assert.AreEqual(999, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);
		}

		#endregion

		#region List Mapping

		[TestMethod]
		public void ListMappingTest_WithIEnumerable()
		{
			ListMappingWithGeneric<IEnumerable<FirstTable>>();
		}

		[TestMethod]
		public void ListMappingTest_WithICollection()
		{
			ListMappingWithGeneric<ICollection<FirstTable>>();
		}

		[TestMethod]
		public void ListMappingTest_WithIList()
		{
			ListMappingWithGeneric<IList<FirstTable>>();
		}

		[TestMethod]
		public void ListMappingTest_WithCollection()
		{
			ListMappingWithGeneric<Collection<FirstTable>>();
		}

		[TestMethod]
		public void ListMappingTest_WithList()
		{
			ListMappingWithGeneric<List<FirstTable>>();
		}

		[TestMethod]
		public void ListMappingTest_WithArray()
		{
			ListMappingWithGeneric<FirstTable[]>();
		}

		private void ListMappingWithGeneric<T>() where T: IEnumerable<FirstTable>
		{
			var table = new DataTable("FirstTable")
				.AddColumn("Id", typeof(int))
				.AddColumn("StringColumn", typeof(string))
				.AddColumn("DecimalColumn", typeof(decimal))
				.AddColumn("DateTimeColumn", typeof(DateTime))
				.AddColumn("NullableIntColumn", typeof(int))
				.AddColumn("NullableDecimalColumn", typeof(decimal))
				.AddColumn("NullableDateTimeColumn", typeof(DateTime));

			table.AddRow(
				1
				, "First Line"
				, 666.45
				, new DateTime(2017, 1, 1)
				, 999
				, null
				, DBNull.Value
			);
			table.AddRow(
				2
				, "Second Line"
				, decimal.MaxValue
				, DateTime.MaxValue
				, int.MaxValue
				, null
				, DBNull.Value
			);
			table.AddRow(
				3
				, "Third Line"
				, 0.05
				, DateTime.MinValue
				, null
				, -450.50
				, new DateTime(2017, 1, 1)
			);

			var dataReader = table.CreateDataReader();

			var list = _mapper.Map<T>(dataReader).ToArray();
			Assert.AreEqual(3, list.Length);

			var obj = list[0];
			Assert.AreEqual(1, obj.Id);
			Assert.AreEqual("First Line", obj.StringColumn);
			Assert.AreEqual(666.45m, obj.DecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.DateTimeColumn);
			Assert.AreEqual(999, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);

			obj = list[1];
			Assert.AreEqual(2, obj.Id);
			Assert.AreEqual("Second Line", obj.StringColumn);
			Assert.AreEqual(decimal.MaxValue, obj.DecimalColumn);
			Assert.AreEqual(DateTime.MaxValue, obj.DateTimeColumn);
			Assert.AreEqual(int.MaxValue, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);

			obj = list[2];
			Assert.AreEqual(3, obj.Id);
			Assert.AreEqual("Third Line", obj.StringColumn);
			Assert.AreEqual(0.05m, obj.DecimalColumn);
			Assert.AreEqual(DateTime.MinValue, obj.DateTimeColumn);
			Assert.AreEqual(null, obj.NullableIntColumn);
			Assert.AreEqual(-450.50m, obj.NullableDecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.NullableDateTimeColumn);
		}

		#endregion

		#region Tuple Mapping

		[TestMethod]
		public void TupleMappingTest()
		{
			var dataSet = new DataSet();
			var table = new DataTable("FirstTable")
				.AddColumn("Id", typeof(int))
				.AddColumn("StringColumn", typeof(string))
				.AddColumn("DecimalColumn", typeof(decimal))
				.AddColumn("DateTimeColumn", typeof(DateTime))
				.AddColumn("NullableIntColumn", typeof(int))
				.AddColumn("NullableDecimalColumn", typeof(decimal))
				.AddColumn("NullableDateTimeColumn", typeof(DateTime));

			table.AddRow(
				1
				, "First Line"
				, 666.45
				, new DateTime(2017, 1, 1)
				, 999
				, null
				, DBNull.Value
			);
			table.AddRow(
				2
				, "Second Line"
				, decimal.MaxValue
				, DateTime.MaxValue
				, int.MaxValue
				, null
				, DBNull.Value
			);
			table.AddRow(
				3
				, "Third Line"
				, 0.05
				, DateTime.MinValue
				, null
				, -450.50
				, new DateTime(2017, 1, 1)
			);

			dataSet.Tables.Add(table);

			table = new DataTable("SecondTable")
				.AddColumn("Id", typeof(int))
				.AddColumn("StringColumn", typeof(string))
				.AddColumn("DecimalColumn", typeof(decimal))
				.AddColumn("DateTimeColumn", typeof(DateTime))
				.AddColumn("NullableIntColumn", typeof(int))
				.AddColumn("NullableDecimalColumn", typeof(decimal))
				.AddColumn("NullableDateTimeColumn", typeof(DateTime));

			table.AddRow(
				1
				, "First Line"
				, 666.45
				, new DateTime(2017, 1, 1)
				, 999
				, null
				, DBNull.Value
			);

			dataSet.Tables.Add(table);

			var dataReader = dataSet.CreateDataReader();

			var tuple = _mapper.Map<Tuple<IList<FirstTable>, FirstTable>>(dataReader);
			var list = tuple.Item1;
			Assert.AreEqual(3, list.Count);

			var obj = list[0];
			Assert.AreEqual(1, obj.Id);
			Assert.AreEqual("First Line", obj.StringColumn);
			Assert.AreEqual(666.45m, obj.DecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.DateTimeColumn);
			Assert.AreEqual(999, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);

			obj = list[1];
			Assert.AreEqual(2, obj.Id);
			Assert.AreEqual("Second Line", obj.StringColumn);
			Assert.AreEqual(decimal.MaxValue, obj.DecimalColumn);
			Assert.AreEqual(DateTime.MaxValue, obj.DateTimeColumn);
			Assert.AreEqual(int.MaxValue, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);

			obj = list[2];
			Assert.AreEqual(3, obj.Id);
			Assert.AreEqual("Third Line", obj.StringColumn);
			Assert.AreEqual(0.05m, obj.DecimalColumn);
			Assert.AreEqual(DateTime.MinValue, obj.DateTimeColumn);
			Assert.AreEqual(null, obj.NullableIntColumn);
			Assert.AreEqual(-450.50m, obj.NullableDecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.NullableDateTimeColumn);

			obj = tuple.Item2;
			Assert.AreEqual(1, obj.Id);
			Assert.AreEqual("First Line", obj.StringColumn);
			Assert.AreEqual(666.45m, obj.DecimalColumn);
			Assert.AreEqual(new DateTime(2017, 1, 1), obj.DateTimeColumn);
			Assert.AreEqual(999, obj.NullableIntColumn);
			Assert.AreEqual(null, obj.NullableDecimalColumn);
			Assert.AreEqual(null, obj.NullableDateTimeColumn);
		}

		#endregion
	}
}
