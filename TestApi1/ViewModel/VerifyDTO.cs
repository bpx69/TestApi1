namespace TestApi1.ViewModel
{
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
