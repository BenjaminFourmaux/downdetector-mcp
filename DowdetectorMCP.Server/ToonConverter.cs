namespace DowdetectorMCP.Server
{
    /// <summary>
    /// https://github.com/toon-format/toon
    /// </summary>
    public class ToonConverter
    {
        public static string ConvertToString<T>(T obj)
        {
            return obj?.ToString() ?? string.Empty;
        }
    }
}
