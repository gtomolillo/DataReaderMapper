using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoMapper;

namespace DataReaderMapper.Helpers
{
	internal static class ReflectionHelper
	{
		public static object MapMember(this ResolutionContext context, MemberInfo member, object value, object destination)
		{
			var memberType = member.GetMemberType();
			var destValue = member.GetMemberValue(destination);
			return context.Mapper.Map(value, destValue, value?.GetType() ?? memberType, memberType, context);
		}

		[SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
		[SuppressMessage("ReSharper", "UseNullPropagation")]
		public static Type GetMemberType(this MemberInfo memberInfo)
		{
			if (memberInfo is MethodInfo)
				return ((MethodInfo)memberInfo).ReturnType;
			if (memberInfo is PropertyInfo)
				return ((PropertyInfo)memberInfo).PropertyType;
			if (memberInfo is FieldInfo)
				return ((FieldInfo)memberInfo).FieldType;
			return null;
		}

		public static object GetMemberValue(this MemberInfo propertyOrField, object target)
		{
			var property = propertyOrField as PropertyInfo;
			if (property != null)
			{
				return property.GetValue(target, null);
			}
			var field = propertyOrField as FieldInfo;
			if (field != null)
			{
				return field.GetValue(target);
			}

			throw new ArgumentOutOfRangeException(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);
		}

		public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
		{
			var property = propertyOrField as PropertyInfo;
			if (property != null)
			{
				property.SetValue(target, value, null);
				return;
			}
			var field = propertyOrField as FieldInfo;
			if (field != null)
			{
				field.SetValue(target, value);
				return;
			}

			throw new ArgumentOutOfRangeException(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);
		}
	}
}
