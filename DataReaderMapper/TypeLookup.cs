using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using DataReaderMapper.Helpers;

namespace DataReaderMapper
{
	internal class TypeLookup
	{
		private static readonly ConcurrentDictionary<Type, TypeLookup> Cache = new ConcurrentDictionary<Type, TypeLookup>();

		private readonly ConcurrentDictionary<string, string> _members = new ConcurrentDictionary<string, string>();
		private bool? _isCollection, _isDeclaredAsCollection, _isReadOnly, _isTuple;
		private bool _elementTypeSearched;
		private TypeLookup[] _genericArguments;
		private Type _genericType, _elementType;

		private TypeLookup(Type type)
		{
			Type = type;
		}

		public Type Type { get; }

		public bool IsCollection
			=> _isCollection ?? (_isCollection = Type.IsCollection()).Value;

		public bool IsDeclaredAsCollection
			=> _isDeclaredAsCollection ?? (_isDeclaredAsCollection = Type.IsCollection(false)).Value;

		public bool IsReadOnly
			=> _isReadOnly ?? (_isReadOnly = Type.IsReadOnly()).Value;

		public bool IsTuple
			=> _isTuple ?? (_isTuple = Type.IsTuple()).Value;

		public TypeLookup[] GenericArguments
			=> _genericArguments ?? (_genericArguments = !Type.IsGenericType
				? new TypeLookup[0]
				: Type.GetGenericArguments().Select(Lookup).ToArray());

		public Type GenericType
			=> _genericType ?? (_genericType = Type.IsGenericType ? Type.GetGenericTypeDefinition() : Type);

		public Type ConcreteType { get; set; }

		public bool FindElementType(out Type elementType)
		{
			if (!_elementTypeSearched)
			{
				Type.FindCollectionType(out _elementType);
				_elementTypeSearched = true;
			}

			elementType = _elementType;
			return _elementType != null;
		}

		public string GetMemberName(MemberInfo member)
		{
			return _members.GetOrAdd(member.Name, m =>
			{
				var attribute = member.GetCustomAttribute<ColumnNameAttribute>();
				return attribute?.Name ?? m;
			});
		}

		public static TypeLookup Lookup(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), "Type must be not null");
			}

			return Cache.GetOrAdd(type, t => new TypeLookup(t));
		}
	}
}
