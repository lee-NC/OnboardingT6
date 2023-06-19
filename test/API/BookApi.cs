using test.Filter;
using test.Model;

namespace test.API;

//map tuong tu voi viec khai bao controller don le
//dinh danh cac controller va controller impl
public static class BookApi
{
    public static RouteGroupBuilder MapBookApi(this RouteGroupBuilder groups)
    {
        groups.MapPost("tmp", AddBook)
            .RequireAuthorization("admin")
            .AddEndpointFilter<TodoIsValidFilter>();
        groups.MapGet("tmp", GetBook).AddEndpointFilter<TodoIsValidFilter>();
        return groups;
    }

    private static Task<IResult> AddBook(Book b)
    {
        return Task.FromResult<IResult>(TypedResults.Ok(b));
    }

    private static Task<IResult> GetBook(int id)
    {
        var book = new Book { Id = id, Title = "name " + id, AuthorId = id + 3, Type = "Horror" };
        return Task.FromResult<IResult>(TypedResults.Ok(book));
    }
}