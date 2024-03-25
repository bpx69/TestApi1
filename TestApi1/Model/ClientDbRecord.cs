using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TestApi1.Model
{
    [Table("Clients")]
    public record ClientDbRecord
    {
        [Key]
        public required Guid Id { get; init; }

        [Required]
        [MaxLength(32)]
        public required string ApiKey { get; init; }

        [Required]
        [MaxLength(32)]
        public required string ClientName { get; init; }

        [SetsRequiredMembers]
        public ClientDbRecord(Guid id, string apiKey, string clientName)
        {
            Id = id;
            ApiKey = apiKey;
            ClientName = clientName;
        }
    }
}
