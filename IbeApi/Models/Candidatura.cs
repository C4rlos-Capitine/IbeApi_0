namespace IbeApi.Models
{
    public class Candidatura
    {
        public int codcandi {  get; set; }
        public int cod_edital { get; set; } 
        public int codecurso { get; set; }  
        public String estado { get; set; }
        public String resultado { get; set; }
        public String curso { get; set; }
        public String edital { get; set; }
        public DateTime data_subm { get; set; }
    }
}
