using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using DataReaderMapper.Helpers;

namespace DataReaderMapper
{
	public class DataReaderMapper : IObjectMapper
	{
		private static readonly MethodInfo MapMethod = typeof(DataReaderMapper).GetMethod(nameof(Map),
			BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo MapCollectionMethod = typeof(DataReaderMapper).GetMethod(nameof(MapCollection),
			BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo MapObjectMethod = typeof(DataReaderMapper).GetMethod(nameof(MapObject),
			BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo MapGenericListMethod = typeof(DataReaderMapper).GetMethod(nameof(MapGenericList),
			BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo MapGenericCollectionMethod = typeof(DataReaderMapper).GetMethod(nameof(MapGenericCollection),
			BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly Type DataRecordType = typeof(IDataReader);

		private static readonly Dictionary<Type, Type> ListMappings = new Dictionary<Type, Type>
		{
			{ typeof(IEnumerable<>), typeof(List<>) },
			{ typeof(IList<>), typeof(List<>) },
			{ typeof(IReadOnlyList<>), typeof(ReadOnlyCollection<>) },
			{ typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>) },
			{ typeof(ICollection<>), typeof(Collection<>) }
		};

		public bool IsMatch(TypePair context)
		{
			return DataRecordType.IsAssignableFrom(context.SourceType);
		}

		public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
			PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
		{
			return Expression.Call(Expression.Constant(this), MapMethod.MakeGenericMethod(destExpression.Type), sourceExpression, destExpression,
				contextExpression);
		}

		[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
		protected virtual TDestination Map<TDestination>(IDataReader source, TDestination destination,
			ResolutionContext context)
		{
			var lookup = TypeLookup.Lookup(typeof(TDestination));

			if (lookup.IsTuple)
			{
				return MapTuple(source, destination, context);
			}
			if (lookup.IsCollection)
			{
				return MapCollection(source, destination, context);
			}
			if (source.Read())
			{
				return MapObject(source, destination, context);
			}
			if (destination == null)
			{
				destination = (TDestination)context.Mapper.ServiceCtor.Invoke(typeof(TDestination));
			}

			return destination;
		}

		protected virtual TDestination MapTuple<TDestination>(IDataReader source, TDestination destination,
			ResolutionContext context)
		{
			var lookup = TypeLookup.Lookup(typeof(TDestination));
			var innerLookups = lookup.GenericArguments;

			var arguments = new object[innerLookups.Length];
			
			var index = 0;
			do
			{
				var innerLookup = innerLookups[index];
				if (innerLookup.IsCollection)
				{
					var method = MapCollectionMethod.MakeGenericMethod(innerLookup.Type);
					arguments[index] = method.Invoke(this, new object[] { source, null, context });
				}
				else if (source.Read())
				{
					var method = MapObjectMethod.MakeGenericMethod(innerLookup.Type);
					arguments[index] = method.Invoke(this, new object[] { source, null, context, null });
				}

				index++;
			} while (source.NextResult());

			return (TDestination)Activator.CreateInstance(lookup.Type, arguments);
		}

		[SuppressMessage("ReSharper", "ConvertIfStatementToNullCoalescingExpression")]
		protected virtual TDestination MapCollection<TDestination>(IDataReader source, TDestination destination,
			ResolutionContext context)
		{
			var type = typeof(TDestination);
			if (type.IsArray)
			{
				var method = MapGenericListMethod.MakeGenericMethod(type.GetElementType());
				var list = (ICollection)method.Invoke(this, new object[] { source, null, context });

				var array = Array.CreateInstance(type.GetElementType(), list.Count);
				list.CopyTo(array, 0);

				return (TDestination)(object)array;
			}

			var lookup = TypeLookup.Lookup(type);
			if (lookup.IsDeclaredAsCollection)
			{
				var concreteType = lookup.ConcreteType ?? (
					lookup.ConcreteType = type.IsInterface
						? ListMappings[lookup.GenericType].MakeGenericType(lookup.GenericArguments[0].Type)
						: type
				);

				if (lookup.IsReadOnly)
				{
					var method = MapGenericListMethod.MakeGenericMethod(lookup.GenericArguments[0].Type);
					var list = (ICollection)method.Invoke(this, new object[] { source, null, context });
					return (TDestination)concreteType.GetConstructors()[0].Invoke(new object[] { list });
				}
				else
				{
					var method = MapGenericCollectionMethod.MakeGenericMethod(concreteType, type.GenericTypeArguments[0]);
					return (TDestination)method.Invoke(this, new object[] { source, null, context });
				}
			}

			Type elementType;
			if (lookup.FindElementType(out elementType))
			{
				var method = MapGenericCollectionMethod.MakeGenericMethod(type, elementType);
				return (TDestination)method.Invoke(this, new object[] { source, null, context });
			}

			if (destination == null)
			{
				destination = (TDestination)context.Mapper.ServiceCtor.Invoke(typeof(TDestination));
			}
			return destination;
		}

		protected virtual List<TElement> MapGenericList<TElement>(IDataReader source, List<TElement> destination,
			ResolutionContext context)
		{
			return MapGenericCollection<List<TElement>, TElement>(source, null, context);
		}

		protected virtual T MapGenericCollection<T, TElement>(IDataReader source, T destination,
			ResolutionContext context) where T: ICollection<TElement>, new()
		{
			if (destination == null)
			{
				destination = new T();
			}
			
			var fields = new HashSet<string>(Enumerable.Range(0, source.FieldCount).Select(source.GetName));
			while (source.Read())
			{
				destination.Add(MapObject(source, default(TElement), context, fields));
			}

			return destination;
		}

		protected virtual TDestination MapObject<TDestination>(IDataReader source, TDestination destination,
			ResolutionContext context, HashSet<string> fields = null)
		{
			if (destination == null)
			{
				destination = (TDestination)context.Mapper.ServiceCtor.Invoke(typeof(TDestination));
			}
			var typeDetails = context.ConfigurationProvider.Configuration.CreateTypeDetails(typeof(TDestination));
			var lookup = TypeLookup.Lookup(typeof(TDestination));
			fields = fields ?? new HashSet<string>(Enumerable.Range(0, source.FieldCount).Select(source.GetName));

			foreach (var member in typeDetails.PublicWriteAccessors)
			{
				var name = lookup.GetMemberName(member);
				if (!fields.Contains(name))
				{
					continue;
				}

				object sourceMemberValue;
				try
				{
					sourceMemberValue = source[name];
				}
				catch (Exception)
				{
					continue;
				}
				var destinationMemberValue = context.MapMember(member, sourceMemberValue != DBNull.Value ? sourceMemberValue : null, destination);
				member.SetMemberValue(destination, destinationMemberValue);
			}

			return destination;
		}
	}
}