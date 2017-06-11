using System;

namespace DataReaderMapper
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ColumnNameAttribute : Attribute
	{
		public string Name { get; }

		public ColumnNameAttribute(string name)
		{
			Name = name;
		}
	}
}
