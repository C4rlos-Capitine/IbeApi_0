namespace IbeApi.Models
{
    public class Candidato
    {
        public int codcandi { get; set; }
        public string nome { get; set; }
        public string apelido { get; set; }
        public string nomecomp { get; set; }
        public string email { get; set; }
        public long telefone { get; set; }  // Changed to long
        public long telemovel { get; set; } // Changed to long
        public string genero { get; set; }
        public string password { get; set; }
    }
}
