namespace DowndetectorMCP.API.Exceptions
{
    public class RateLimitException : Exception
    {
        public RateLimitException() : base($"WARN: Page unavailable due to the rate limit. Please try later") { }
    }
}