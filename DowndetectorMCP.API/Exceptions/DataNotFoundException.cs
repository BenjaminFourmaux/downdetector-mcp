namespace DowndetectorMCP.API.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException(string technicalServiceName) : base($"WARM: No data found for service '{technicalServiceName}'") { }
    }
}
