namespace Updater.Helper
{
    /// <summary>
    /// Class to define base url to this site
    /// </summary>
    public static class BaseURL
    {
        /// <summary>
        /// Base url loaded in program.cs
        /// </summary>
        public static string URL { get; set; } = "";
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string? BaseUrl(this HttpRequest req)
        {
            if (req == null) return null;
            var uriBuilder = new UriBuilder(req.Scheme, req.Host.Host, req.Host.Port ?? -1);
            if (uriBuilder.Uri.IsDefaultPort)
            {
                uriBuilder.Port = -1;
            }

            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}
