namespace IbeApi.Models
{
    public class MailData
    {
        public string EmailToId { get; set; }
        public string EmailToName { get; set; }
        public string EmailSubject { get; set; }
        public string password { get; set; }
        public int auth {  get; set; }
        private int codigo { get; set; }
        public void setCodigo(int codigo){this.codigo = codigo; }
        public int getCodigo(){return this.codigo;}
        //public string EmailBody { get; set; }
        //public string HtmlBody { get; set; }
    }
}
