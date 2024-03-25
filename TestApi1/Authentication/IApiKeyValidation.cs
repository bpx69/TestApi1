namespace TestApi1.Authentication
{
    public interface IApiKeyValidation
    {
        bool IsValidApiKey(string userApiKey, out Guid? clientGuid, out string? clientName);
    }
}
