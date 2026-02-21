using System.Collections;
using System.Reflection;
using System.Text;

namespace DowndetectorMCP.API.Utils
{
    /// <summary>
    /// Converts objects to Toon format string representation.
    /// <see href="https://github.com/toon-format/toon"/>
    /// </summary>
    public static class ToonConverter
    {
        /// <summary>
        /// Converts an object to Toon format string.
        /// </summary>
        /// <typeparam name="T">The type of object to convert</typeparam>
        /// <param name="obj">The object instance to convert</param>
        /// <param name="context">Optional context metadata</param>
        /// <returns>Toon formatted string</returns>
        public static string ToToon<T>(T obj, Dictionary<string, string>? context = null)
        {
            var sb = new StringBuilder();

            // Add context section if provided
            if (context != null && context.Count > 0)
            {
                sb.AppendLine("context:");
                foreach (var kvp in context)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }

            // Convert object properties
            if (obj != null)
            {
                ConvertObject(obj, sb, 0);
            }

            return sb.ToString().TrimEnd();
        }

        private static void ConvertObject(object obj, StringBuilder sb, int indent)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                var propType = prop.PropertyType;
                var indentStr = new string(' ', indent);

                // Handle collections
                if (propType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propType))
                {
                    var collection = (IEnumerable)value;
                    var items = collection.Cast<object>().ToList();

                    if (items.Count == 0) continue;

                    var firstItem = items.First();
                    var itemType = firstItem.GetType();

                    // Simple types (string, primitives)
                    if (IsSimpleType(itemType))
                    {
                        sb.AppendLine($"{indentStr}{prop.Name}[{items.Count}]: {string.Join(",", items)}");
                    }
                    // Complex types (objects with properties)
                    else if (itemType.IsClass)
                    {
                        var itemProps = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var propNames = string.Join(",", itemProps.Select(p => p.Name));

                        sb.AppendLine($"{indentStr}{prop.Name}[{items.Count}]{{{propNames}}}:");

                        foreach (var item in items)
                        {
                            var values = new List<string>();
                            foreach (var itemProp in itemProps)
                            {
                                var itemValue = itemProp.GetValue(item);
                                values.Add(FormatValue(itemValue));
                            }
                            sb.AppendLine($"{indentStr}  {string.Join(",", values)}");
                        }
                    }
                    else
                    {
                        var dict = (IDictionary)value;
                        if (dict.Count == 0) continue;

                        sb.AppendLine($"{indentStr}{prop.Name}:");
                        foreach (DictionaryEntry entry in dict)
                        {
                            sb.AppendLine($"{indentStr}  {entry.Key}: {FormatValue(entry.Value)}");

                        }
                    }
                }
                // Handle dictionaries
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var dict = (IDictionary)value;
                    if (dict.Count == 0) continue;

                    sb.AppendLine($"{indentStr}{prop.Name}:");
                    foreach (DictionaryEntry entry in dict)
                    {
                        sb.AppendLine($"{indentStr}  {entry.Key}: {FormatValue(entry.Value)}");
                    }
                }
                // Simple properties
                else if (IsSimpleType(propType) || propType.IsEnum)
                {
                    sb.AppendLine($"{indentStr}{prop.Name}: {FormatValue(value)}");
                }
                // Nested complex objects
                else if (propType.IsClass)
                {
                    sb.AppendLine($"{indentStr}{prop.Name}:");
                    ConvertObject(value, sb, indent + 2);
                }
            }
        }

        private static string FormatValue(object? value)
        {
            if (value == null) return string.Empty;

            return value switch
            {
                bool b => b.ToString().ToLower(),
                DateTime dt => dt.ToString("dd-MM-yyyyTHH:mm:ss"),
                string s => s,
                _ => value.ToString() ?? string.Empty
            };
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }
    }
}
