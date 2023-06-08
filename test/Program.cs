
using test.API;
using test.Filter;
using test.Handler;
using test.Model;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddAuthenticationScheme<AuthenHandler>(config);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

#region NonAuthen

var books = new List<Book>
{
    new Book { Id = 1, Title = "name 1", AuthorId = 1, Type = "Horror" },
    new Book { Id = 2, Title = "name 2", AuthorId = 2, Type = "Romance" },
    new Book { Id = 3, Title = "name 3", AuthorId = 3, Type = "Detective" },
};

var authors = new List<Author>
{
    new Author { Id = 1, Name = "name 1", Description = " desc 1" },
    new Author { Id = 2, Name = "name 2", Description = " desc 2" },
    new Author { Id = 3, Name = "name 3", Description = " desc 3" }
};


app.MapGet("/book", () =>
{
    var response = new List<BookResponse>();
    for (var i = 0; i < books.Count(); i++)
    {
        var book = books.ElementAt(i);
        var author = authors.Find(match: a => a.Id == book.AuthorId);
        response.Add(new BookResponse(book, author));
    }

    return response;
});

app.MapGet("/book/{id}", (int id) =>
{
    var book = books.Find(b => b.Id == id);
    if (book is null)
    {
        return Results.NotFound("Not found");
    }

    return Results.Ok(book);
});


app.MapPost("/book", (Book b) =>
    {
        books.Add(b);
        return books;
    })
    //validation
    // change value request
    .AddEndpointFilter(async (invocationContext, next) =>
    {
        var book = invocationContext.GetArgument<Book>(0);

        var id = books.Max(d => d.Id) + 1;
        if (id == book.Id)
        {
            book.Title = "check last";

            books.Add(book);
            return books;
        }

        return await next(invocationContext);
    })
//logging
    .AddEndpointFilter(async (efiContext, next) =>
    {
        app.Logger.LogInformation("Before first filter");
        var result = await next(efiContext);
        app.Logger.LogInformation("After first filter");
        return result;
    })
    .AddEndpointFilter<TodoIsValidFilter>()
    ;

app.MapPut("/book/{id}", (UpdateField updateField, int id) =>
{
    var book = books.Find(b => b.Id == id);
    if (book is null)
    {
        return TypedResults.Problem(
            detail: "Khong tim thay ban ghi",
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found");
    }

    book.Title = updateField.Title;
    book.AuthorId = updateField.AuthorId;

    return Results.Ok(books);
});

app.MapDelete("/book/{id}", (int id) =>
{
    var book = books.Find(b => b.Id == id);
    if (book is null)
    {
        return Results.NotFound("Not found");
    }

    books.Remove(book);

    return Results.Ok(books);
});

app.MapGroup("/map")
    .MapBookApi()
    .WithTags("book")
    .WithOpenApi()
    .WithMetadata();


#endregion

app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();

// namespace test;
//
// public class Program
// {
//     public static void Main(string[] args)
//     {
//         CreateHostBuilder(args).Build().Run();
//     }
//
//     private static IHostBuilder CreateHostBuilder(string[] args) =>
//         Host.CreateDefaultBuilder(args)
//             .ConfigureWebHostDefaults(webBuilder =>
//             {
//                 webBuilder.UseStartup<Config>();
//             });
// }