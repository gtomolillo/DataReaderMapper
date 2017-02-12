# DataReaderMapper

Use AutoMapper to Map an IDataReader to an object

## Configuration

```csharp
MapperRegistry.Mappers.Insert(0, new DataReaderMapper());
```

## Usage

```csharp
//Read a single object
using (var connection = new SqlConnection("MyConnectionString"))
{
	connection.Open();
	using (var command = new SqlCommand("MyQuery", connection))
	{
	  	using (var reader = command.ExecuteReader())
	  	{
  	  		var myObject = mapper.Map<MyObject>(reader);
	  	}
	}
}

//Read a multi result set reader
//Use Tuple<T, T1, Tn> to read a multi result set DataReader
using (var connection = new SqlConnection("MyConnectionString"))
{
	connection.Open();
	using (var command = new SqlCommand("MyQuery", connection))
	{
	  	using (var reader = command.ExecuteReader())
	  	{
			var result = mapper.Map<Tuple<MyObject1, MyObject2>>(reader);
      			var myObject1 = result.Item1;
      			var myObject2 = result.Item2;
	  	}
	}
}

//Read a single result set list
//Supported collection types:
//IEnumerable<T>, ICollection<T>, IList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
//List<T>, Collection<T>, ReadOnlyCollection<T> and T[]
using (var connection = new SqlConnection("MyConnectionString"))
{
	connection.Open();
	using (var command = new SqlCommand("MyQuery", connection))
	{
	  	using (var reader = command.ExecuteReader())
	  	{
	  		var myList = mapper.Map<IEnumerable<MyObject1>>(reader);
	  	}
	}
}
```