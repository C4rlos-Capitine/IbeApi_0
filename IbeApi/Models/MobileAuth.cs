namespace IbeApi.Models
{
    public class MobileAuth
    {
        public int Id { get; set; } 
        public string email { get; set; }
        public DateTime datageracao { get; set; }
        public int codigo { get; set; }
        public int autenticou { get; set; }
    }
}
