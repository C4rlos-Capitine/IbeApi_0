namespace IbeApi.Models
{
    public class Candidate
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
        public String num_ident { get; set; }
        public int idade { get; set; }
        public int codprovi { get; set; }
        public string? rua { get; set; }
        public string? naturalidade { get; set; }
        // public string provincia { get; set; }
        public int dia { get; set; }
        public int mes { get; set; }
        public int ano { get; set; }
        public string? ocupacao { get; set; }
        public string? tipo_doc { get; set; }
        public int dia_emissao { get; set; }
        public int mes_emissao { get; set; }
        public int ano_emissao { get; set; }
        public int dia_validade { get; set; }
        public int mes_validade { get; set; }
        public int ano_validade { get; set; }
        //novos campos
        public int codedital { get; set; }
        public String nivel { get; set; }
        public int codarea { get; set; }
        public string especialidade { get; set; }
        public DateTime data_subm { get; set; }
        public IFormFile bi_file { get; set; }
        public IFormFile  nuit_file { get; set; }
        public IFormFile certificado_file { get; set; }
        public IFormFile foto_passe {  get; set; }
        //campos 16/10/2024
        public float media_obt { get; set; }
        public int nuit {  get; set; }
    }
}
