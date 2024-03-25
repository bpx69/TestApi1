namespace TestApi1.ViewModel
{
    /// <summary>
    /// DTO to transfer User Data from Client to API. Contains plain text password (over HTTPS)
    /// </summary>
    public class UserWithPasswordDTO : UserDTO
    {
        public string Password { get; set; }

        public UserWithPasswordDTO(
            string id,
            string userName,
            string fullName,
            string eMail,
            string mobilePhoneNumber,
            string language,
            string culture,
            string password
            ) : base(id, userName, fullName, eMail, mobilePhoneNumber, language, culture)
        {
            this.Password = password;
        }
    }
}
