using TestApi1.Model;

namespace TestApi1.Authentication
{
    /// <summary>
    /// Transient Service performing API Key Validation
    /// </summary>
    public class ApiKeyValidation : IApiKeyValidation
    {
        private readonly UserAPIDbContext _context;
        private readonly Dictionary<string, ClientDbRecord> _cache;
        public ApiKeyValidation(UserAPIDbContext context)
        {
            _context = context;
            _cache = new Dictionary<string, ClientDbRecord>();
        }

        /// <summary>
        /// Checks if API Key is valid
        /// </summary>
        /// <param name="userApiKey"></param>
        /// <param name="clientGuid"></param>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public bool IsValidApiKey(string userApiKey, out Guid? clientGuid, out string? clientName)
        {

            clientName = null;
            clientGuid = null;

            if (string.IsNullOrWhiteSpace(userApiKey))
                return false;

            ClientDbRecord? cachedValue;
            if (!_cache.TryGetValue(userApiKey, out cachedValue))
            {
                var foundRecord = _context.Clients.FirstOrDefault((rec) => rec.ApiKey == userApiKey);
                if (foundRecord != null)
                {
                    _cache.Add(foundRecord.ApiKey, foundRecord);
                    clientGuid = foundRecord.Id;
                    clientName = foundRecord.ClientName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                clientGuid = cachedValue.Id;
                clientName = cachedValue.ClientName;
                return true;
            };
        }
    }
}
