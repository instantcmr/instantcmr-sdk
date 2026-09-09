using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Icmr.Integration
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NotNullAttribute : ValidationAttribute
    {
        public NotNullAttribute()
            : base("The {0} field must not be null.")
        {
        }

        public override bool IsValid(object value) => value != null;
    }

    public sealed class JsonValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public JsonValidationException(string fpat, IReadOnlyList<string> errors)
            : base($"invalid JSON data in '{fpat}':{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(error => $"  - {error}"))}")
        {
            Errors = errors;
        }
    }

    public static class JsonValidation
    {
        public static void Validate(object value, string fpat)
        {
            var errors = new List<string>();
            Validate(value, "", errors, new HashSet<object>(ReferenceEqualityComparer.Instance));
            if (errors.Count != 0)
                throw new JsonValidationException(fpat, errors);
        }

        static void Validate(object value, string path, List<string> errors, HashSet<object> visited)
        {
            if (value == null || IsTerminal(value.GetType()))
                return;

            if (!value.GetType().IsValueType && !visited.Add(value))
                return;

            if (value is IEnumerable enumerable)
            {
                var index = 0;
                foreach (var item in enumerable)
                    Validate(item, $"{path}[{index++}]", errors, visited);
                return;
            }

            foreach (var member in GetDataMembers(value.GetType()))
            {
                var memberValue = GetValue(member, value);
                var memberPath = string.IsNullOrEmpty(path) ? member.Name : $"{path}.{member.Name}";
                var context = new ValidationContext(value)
                {
                    DisplayName = memberPath,
                    MemberName = member.Name,
                };

                foreach (var attribute in member.GetCustomAttributes<ValidationAttribute>())
                {
                    var result = attribute.GetValidationResult(memberValue, context);
                    if (result != ValidationResult.Success)
                        errors.Add($"{memberPath}: {result.ErrorMessage}");
                }

                Validate(memberValue, memberPath, errors, visited);
            }
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type) =>
            type.GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.CanRead && property.GetIndexParameters().Length == 0));

        static object GetValue(MemberInfo member, object owner) => member switch
        {
            FieldInfo field => field.GetValue(owner),
            PropertyInfo property => property.GetValue(owner),
            _ => null,
        };

        static bool IsTerminal(Type type) =>
            type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(Uri);
    }
}
