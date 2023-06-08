namespace test.Model
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public int? AuthorId { get; set; }

        public string? Type { get; set; }
    }
}
