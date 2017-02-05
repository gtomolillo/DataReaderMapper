using System;
using System.Data;

namespace DataReaderMapper.Tests.Helpers
{
	internal static class DataTableHelper
	{
		public static DataTable AddColumn(this DataTable dataTable, string columnName, Type type)
		{
			dataTable.Columns.Add(columnName, type);
			return dataTable;
		}

		public static DataTable AddRow(this DataTable dataTable, params object[] values)
		{
			var row = dataTable.NewRow();

			for (var i = 0; i < values.Length; i++)
			{
				row[i] = values[i] ?? DBNull.Value;
			}

			dataTable.Rows.Add(row);
			return dataTable;
		}
	}
}
