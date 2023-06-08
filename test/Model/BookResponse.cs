using System.Text.Json.Serialization;

namespace test.Model
{

    public class BookResponse
    {
        public BookResponse(Book book, Author? author)
        {
            this.Id = book.Id;
            this.Title = book.Title;
            if (author != null)
            {
                this.AuthorName = author.Name;
                this.AuthorDesc = author.Description;
            }
            this.Type = book.Type;
        }

        [JsonPropertyName("bookId")]
        public int Id { get; set; }
        public string? Title { get; set; }

        public string? AuthorName { get; set; }

        public string? AuthorDesc { get; set; }

        public string? Type { get; set; }
    }
}
