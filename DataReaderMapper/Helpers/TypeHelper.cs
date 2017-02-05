using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataReaderMapper.Helpers
{
	internal static class TypeHelper
	{
		private static readonly Type[] EnumerableTypes = {
			typeof(IEnumerable<>), typeof(IList<>), typeof(IReadOnlyList<>), typeof(ICollection<>),
			typeof(IReadOnlyCollection<>), typeof(List<>), typeof(Collection<>), typeof(ReadOnlyCollection<>)
		};
		private static readonly Type[] CollectionTypes = {
			typeof(IList<>), typeof(ICollection<>), typeof(List<>), typeof(Collection<>)
		};
		private static readonly Type[] ReadOnlyTypes = {
			typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>)
		};
		private static readonly Type[] TupleTypes = {
			typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>),
			typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>)
		};

		private static readonly Type StringType = typeof(string);

		public static bool IsTuple(this Type type, bool checkBaseTypes = true)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			do
			{
				if (type.IsGenericType && TupleTypes.Contains(type.GetGenericTypeDefinition()))
				{
					return true;
				}

				type = type.BaseType;
			} while (checkBaseTypes && type != null);

			return false;
		}

		public static bool IsCollection(this Type type, bool checkBaseTypes = true)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (type == StringType)
			{
				return false;
			}
			if (type.IsArray)
			{
				return true;
			}

			Type elementType;
			return type.FindEnumerableType(out elementType, checkBaseTypes);
		}

		public static bool IsReadOnly(this Type type, bool checkBaseTypes = false, bool checkInterfaces = false)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (type == StringType)
			{
				return false;
			}

			do
			{
				if (type.IsGenericType && ReadOnlyTypes.Contains(type.GetGenericTypeDefinition()))
				{
					return true;
				}

				if (checkInterfaces)
				{
					foreach (var interfaceType in type.GetInterfaces())
					{
						if (interfaceType.IsGenericType && ReadOnlyTypes.Contains(interfaceType.GetGenericTypeDefinition()))
						{
							return true;
						}
					}
				}

				type = type.BaseType;
			} while (type != null && checkBaseTypes);

			return false;
		}

		public static bool FindCollectionType(this Type type, out Type elementType, bool checkBaseTypes = true)
		{
			return type.FindType(CollectionTypes, out elementType, checkBaseTypes);
		}

		public static bool FindEnumerableType(this Type type, out Type elementType, bool checkBaseTypes = true)
		{
			return type.FindType(EnumerableTypes, out elementType, checkBaseTypes);
		}

		private static bool FindType(this Type type, Type[] matchTypes, out Type elementType, bool checkBaseTypes = true)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			elementType = null;
			if (type == StringType)
			{
				return false;
			}

			do
			{
				if (type.IsGenericType && matchTypes.Contains(type.GetGenericTypeDefinition()))
				{
					elementType = type.GenericTypeArguments[0];
					return true;
				}

				foreach (var interfaceType in type.GetInterfaces())
				{
					if (interfaceType.IsGenericType && matchTypes.Contains(interfaceType.GetGenericTypeDefinition()))
					{
						elementType = interfaceType.GenericTypeArguments[0];
						return true;
					}
				}

				type = type.BaseType;
			} while (type != null && checkBaseTypes);

			return false;
		}
	}
}
