namespace DowndetectorMCP.API.Utils
{
    internal class ApiUrl
    {
        public const string DefaultUrl = "https://downdetector.com";
        public const string Url = "https://downdetector.com/api";

        private static readonly Dictionary<string, string> CountryUrlMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // North America
            { "CA", "https://downdetector.ca" }, // Canada
            { "CR", "https://cr.downdetector.com" }, // Costa Rica
            { "GT", "https://downdetector.com.gt" }, // Guatemala
            { "MX", "https://downdetector.mx" }, // Mexico
            { "PA", "https://pa.downdetector.com" }, // Panama
            { "PR", "https://downdetector.pr" }, // Puerto Rico
            { "DO", "https://do.downdetector.com" }, // Dominican Republic
            { "US", "https://downdetector.com" }, // United States

            // South America
            { "AR", "https://downdetector.com.ar" }, // Argentina
            { "BR", "https://downdetector.com.br" }, // Brazil
            { "CL", "https://downdetector.cl" }, // Chile
            { "CO", "https://downdetector.com.co" }, // Colombia
            { "EC", "https://downdetector.ec" }, // Ecuador
            { "PE", "https://downdetector.pe" }, // Peru

            // Europe
            { "BE", "https://allestoringen.be" }, // Belgium
            { "BG", "https://bg.downdetector.com" }, // Bulgaria
            { "DK", "https://downdetector.dk" }, // Denmark
            { "DE", "https://allestörungen.de" }, // Germany
            { "ES", "https://downdetector.es" }, // Spain
            { "FR", "https://downdetector.fr" }, // France
            { "HR", "https://downdetector.hr" }, // Croatia
            { "IE", "https://downdetector.ie" }, // Ireland
            { "IT", "https://downdetector.it" }, // Italy
            { "HU", "https://downdetector.hu" }, // Hungary
            { "NL", "https://allestoringen.nl" }, // Netherlands
            { "NO", "https://downdetector.no" }, // Norway
            { "PL", "https://downdetector.pl" }, // Poland
            { "PT", "https://downdetector.pt" }, // Portugal
            { "RO", "https://downdetector.ro" }, // Romania
            { "CH", "https://allestörungen.ch" }, // Switzerland
            { "RS", "https://downdetector.rs" }, // Serbia
            { "SK", "https://downdetector.sk" }, // Slovakia
            { "FI", "https://downdetector.fi" }, // Finland
            { "SE", "https://downdetector.se" }, // Sweden
            { "UK", "https://downdetector.co.uk" }, // United Kingdom
            { "UA", "https://downdetector.com.ua" }, // Ukraine
            { "AT", "https://allestörungen.at" }, // Austria
            { "CZ", "https://downdetector.cz" }, // Czech Republic
            { "GR", "https://downdetector.gr" }, // Greece

            // Asia-Middle East
            { "AZ", "https://az.downdetector.com" }, // Azerbaijan
            { "BH", "https://downdetector.bh" }, // Bahrain
            { "BD", "https://bd.downdetector.com" }, // Bangladesh
            { "HK", "https://downdetector.hk" }, // Hong Kong
            { "IN", "https://downdetector.in" }, // India
            { "ID", "https://downdetector.id" }, // Indonesia
            { "MY", "https://downdetector.my" }, // Malaysia
            { "PK", "https://downdetector.pk" }, // Pakistan
            { "PH", "https://downdetector.ph" }, // Philippines
            { "SG", "https://downdetector.sg" }, // Singapore
            { "TR", "https://downdetector.tr" }, // Turkey
            { "AE", "https://downdetector.ae" }, // United Arab Emirates
            { "VN", "https://vn.downdetector.com" }, // Vietnam
            { "IL", "https://downdetector.co.il" }, // Israel
            { "JO", "https://jo.downdetector.com" }, // Jordan
            { "SA", "https://sa.downdetector.com" }, // Saudi Arabia
            { "TH", "https://th.downdetector.com" }, // Thailand
            { "TW", "https://downdetector.tw" }, // Taiwan
            { "JP", "https://downdetector.jp" }, // Japan

            // Africa
            { "DZ", "https://dz.downdetector.com" }, // Algeria
            { "KE", "https://downdetector.co.ke" }, // Kenya
            { "MA", "https://downdetector.ma" }, // Morocco
            { "NG", "https://downdetector.com.ng" }, // Nigeria
            { "ZA", "https://downdetector.co.za" }, // South Africa
            { "TN", "https://tn.downdetector.com" }, // Tunisia
            { "EG", "https://eg.downdetector.com" }, // Egypt

            // Oceania
            { "AU", "https://downdetector.com.au" }, // Australia
            { "NZ", "https://downdetector.co.nz" }, // New Zealand

            // Others
            { "RU", "https://downdetector.ru" }, // Russia
        };

        /// <summary>
        /// Récupère l'URL Downdetector correspondant au code pays alpha2.
        /// </summary>
        /// <param name="countryCode">Code pays alpha2 (ex: "FR", "US", "GB")</param>
        /// <returns>L'URL Downdetector pour le pays ou l'URL par défaut si non trouvé</returns>
        public static string GetUrlByCountryCode(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return DefaultUrl;
            }

            return CountryUrlMap.TryGetValue(countryCode, out var url) ? url : DefaultUrl;
        }

        /// <summary>
        /// Vérifie si un code pays est supporté.
        /// </summary>
        /// <param name="countryCode">Code pays alpha2</param>
        /// <returns>True si le code pays est supporté, False sinon</returns>
        public static bool IsCountrySupported(string countryCode)
        {
            return !string.IsNullOrWhiteSpace(countryCode) && CountryUrlMap.ContainsKey(countryCode);
        }
    }
}