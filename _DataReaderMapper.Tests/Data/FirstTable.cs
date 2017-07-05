using System;

namespace DataReaderMapper.Tests.Data
{
	public class FirstTable
	{
		public int Id { get; set; }
		public string StringColumn { get; set; }
		public decimal DecimalColumn { get; set; }
		public DateTime DateTimeColumn { get; set; }
		public int? NullableIntColumn { get; set; }
		public decimal? NullableDecimalColumn { get; set; }
		public DateTime? NullableDateTimeColumn { get; set; }
	}
}
