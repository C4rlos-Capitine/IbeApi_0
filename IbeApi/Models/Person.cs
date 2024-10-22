namespace IbeApi.Models
{
    public class Person
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public IFormFile FileA { get; set; }
        public IFormFile FileB { get; set; }
    }
}
