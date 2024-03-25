namespace TestApi1.Authentication
{
    /// <summary>
    /// Adds the Headers Key Attribute to Swagger API Description
    /// </summary>
    public class SwaggerHeaderAttribute : Attribute
    {
        public string HeaderName { get; }
        public string Description { get; }
        public bool IsRequired { get; }

        public SwaggerHeaderAttribute(string headerName, string description = "",  bool isRequired = false)
        {
            HeaderName = headerName;
            Description = description;
            IsRequired = isRequired;
        }
    }
}