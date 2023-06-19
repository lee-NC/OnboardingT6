using System.Text.Json.Serialization;

namespace test.Model;

public class BookResponse
{
    public BookResponse(Book book, Author? author)
    {
        Id = book.Id;
        Title = book.Title;
        if (author != null)
        {
            AuthorName = author.Name;
            AuthorDesc = author.Description;
        }

        Type = book.Type;
    }

    [JsonPropertyName("bookId")] public int Id { get; set; }

    public string? Title { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorDesc { get; set; }

    public string? Type { get; set; }
}