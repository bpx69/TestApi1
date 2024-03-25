using Microsoft.AspNetCore.Mvc.Razor;
using TestApi1.Model;

namespace TestApi1.ViewModel
{

    /// <summary>
    /// a Record used in the data transfer to the API Client. No password there.
    /// </summary>
    public class UserDTO
    {
        /// <summary>
        /// Immutable UserId
        /// </summary>
        public string Id { get; init; }
        /// <summary>
        /// Name of the user used to log on to service
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Full name of the user
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// User E-Mail
        /// </summary>
        public string? EMail { get; set; }
        /// <summary>
        /// User's contact mobile phone number
        /// </summary>
        public string? MobilePhoneNumber { get; set; }
        /// <summary>
        /// Selected UI Language of the user
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// Selected input culture as Windows Culture code namy, for example en-US
        /// </summary>
        public string Culture { get; set; }

        public UserDTO(UserDbRecord user)
        {
            this.Id = user.Id.ToString();
            this.UserName = user.UserName;
            this.FullName = user.FullName;
            this.EMail = user.EMail;
            this.MobilePhoneNumber = user.MobilePhoneNumber;
            this.Language = user.Language;
            this.Culture = user.Culture;
        }

        public UserDTO( 
            string id,
            string userName,
            string fullName,
            string eMail,
            string mobilePhoneNumber,
            string language,
            string culture
            )
        {
            this.Id = id;
            this.UserName = userName;
            this.FullName = fullName;
            this.EMail = eMail;
            this.MobilePhoneNumber = mobilePhoneNumber;
            this.Language = language;
            this.Culture = culture;

        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is UserDTO)) return false;
            if (String.Compare((((UserDTO)obj).Id), this?.Id, true) != 0) return false;
            if (this.UserName == null && obj != null || !this.UserName!.Equals(((obj as UserDTO)?.UserName))) return false;
            if (this.FullName == null && obj != null || !this.FullName!.Equals(((obj as UserDTO)?.FullName))) return false;
            if (this.EMail == null && obj != null || !this.EMail!.Equals(((obj as UserDTO)?.EMail))) return false;
            if (this.MobilePhoneNumber == null && obj != null || !this.MobilePhoneNumber!.Equals(((obj as UserDTO)?.MobilePhoneNumber))) return false;
            if (this.Language == null && obj != null || !this.Language!.Equals(((obj as UserDTO)?.Language))) return false;
            if (this.Culture == null &&  obj != null || !this.Culture!.Equals(((obj as UserDTO)?.Culture))) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            unchecked {
                if (Id != null) hash = hash * 23 + Id.GetHashCode();
                if (UserName != null) hash = hash * 17 + UserName.GetHashCode();
                if (FullName != null) hash = hash * 13 + FullName.GetHashCode();
                if (EMail != null) hash = hash * 11 + EMail.GetHashCode();
                if (MobilePhoneNumber != null) hash = hash * 7 + MobilePhoneNumber.GetHashCode();
                if (Language != null) hash = hash * 5 + Language.GetHashCode();
                if (Culture != null) hash = hash * 3 + Culture.GetHashCode();
            }
            return hash;
        }
    }
}
