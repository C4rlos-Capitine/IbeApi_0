namespace IbeApi.Models
{
    public class CandidatoDTOPDFcs
    {
        public int codcandi { get; set; }
        public string nome { get; set; }
        public string apelido { get; set; }
        public string nomecomp { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }  // Changed to long
        public string telemovel { get; set; } // Changed to long
        public string genero { get; set; }
        //public string password { get; set; }
        public bool FindTrue { get; set; }
        public String identificacao { get; set; }
        public int idade { get; set; }
        public int codprovi { get; set; }
        //public int codedita { get; set; }
        public string estado { get; set; }
        public string provincia { get; set; }
        //public string cod_prov {  get; set; }
        public DateTime datadena { get; set; }
        public DateTime data_emissao { get; set; }
        public DateTime data_validade { get; set; }
        public DateTime data_subm { get; set; }
        public string? ocupacao { get; set; }
        public string? rua { get; set; }
        public string? naturalidade { get; set; }
        public string? edital { get; set; }
        public string? area { get; set; }
        public string? nivel { get; set; }
        public int? pontuacao { get; set; }
        public String? tipo_bolsa { get; set; }
        // public int codarea { get; set; }
        public string especialidade { get; set; }
        //17/10/2024
        public String bairro { get; set; }
        public String distrito { get; set; }
        public double media { get; set; }
        public double nuit {  get; set; }
    }
}
