namespace DowndetectorMCP.API.Exceptions
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(string technicalServiceName) : base($"FAIL: The service '{technicalServiceName}' was not found. Make sure you have the correct technical service name. Have you called the tool search_service_name?") { }
    }
}
