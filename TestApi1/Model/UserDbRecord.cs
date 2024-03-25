using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TestApi1.Model
{
    /// <summary>
    /// In memory representation of a User Database table row
    /// </summary>
    [Table("Users")]
    [Index(nameof(UserName), nameof(ClientId), IsUnique = true)]
    public record UserDbRecord
    {
        [SetsRequiredMembers]
        public UserDbRecord(Guid id, string userName, string? fullName, string? eMail, string? mobilePhoneNumber, string language, string culture, string passwordHash, Guid clientId)
        {
            Id = id;
            UserName = userName;
            FullName = fullName;
            EMail = eMail;
            MobilePhoneNumber = mobilePhoneNumber;
            Language = language;
            Culture = culture;
            PasswordHash = passwordHash;
            ClientId = clientId;
        }

        /// <summary>
        /// Immutable UserId
        /// </summary>
        [Key]
        public required Guid Id { get; init; }

        /// <summary>
        /// Name of the user used to log on to service
        /// </summary>
        [Required]
        [MaxLength(32)]
        public required string UserName { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        [MaxLength(100)]
        public string? FullName { get; set; }

        /// <summary>
        /// User E-Mail
        /// </summary>
        [MaxLength(50)]
        public string? EMail { get; set; }

        /// <summary>
        /// User's contact mobile phone number
        /// </summary>
        [MaxLength(50)]
        public string? MobilePhoneNumber { get; set; }

        /// <summary>
        /// Selected UI Language of the user
        /// </summary>
        [MaxLength(32)]
        public required string Language { get; set; }

        /// <summary>
        /// Selected input culture as Windows Culture code namy, for example en-US
        /// </summary>
        [MaxLength(10)]
        public required string Culture { get; set; }

        /// <summary>
        /// Salted Password Hash of the user
        /// </summary>
        [MaxLength(255)]
        public required string PasswordHash { get; set; }

        public required Guid ClientId { get; set; } // Required foreign key property
        public ClientDbRecord? Client { get; set; }

    }
}
