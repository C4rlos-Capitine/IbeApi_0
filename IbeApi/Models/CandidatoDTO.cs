namespace IbeApi.Models
{
    public class CandidatoDTO
    {
        public int codcandi { get; set; }
        public string nome { get; set; }
        public string apelido { get; set; }
        public string nomecomp { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }  // Changed to long
        public string telemovel { get; set; } // Changed to long
        public string genero { get; set; }
        public string password { get; set; }
        public bool FindTrue { get; set; }
        public long num_ident { get; set; }
        public int idade { get; set; }
        public int codprovi { get; set; }
        public int cod_edital { get; set; }
        public string estado { get; set; }
        // public string provincia { get; set; }
        public DateTime datadena { get; set; }
        public int dia { get; set; }
        public int mes { get; set; }
        public int ano { get; set; }
        public string? ocupacao { get; set; }
        public string? rua { get; set; }
        public string? naturalidade { get; set; }
    }
}
