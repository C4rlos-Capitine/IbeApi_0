using System.Numerics;

namespace IbeApi.Models
{
    public class Candidato
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
        public long num_ident {  get; set; } 
        public int idade { get; set; }  
        public int codprovi { get; set; }
       // public string provincia { get; set; }
        public DateTime datadena { get; set; }
        public int dia {  get; set; }
        public int mes { get; set; }
        public int ano { get; set; }
        /*
        public int dia_emissao { get; set; }
        public int mes_emissao { get; set; }
        public int ano_emissao { get; set; } 
        */   
    }


}
