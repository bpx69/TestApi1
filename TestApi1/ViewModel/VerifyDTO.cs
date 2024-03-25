namespace TestApi1.ViewModel
{
    /// <summary>
    /// DTO containing user name and password for verification
    /// </summary>
    public class VerifyDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public VerifyDTO(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }
}
