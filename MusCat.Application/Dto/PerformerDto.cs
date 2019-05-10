namespace MusCat.Application.Dto
{
    public class PerformerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public CountryDto Country { get; set; }
    }
}
