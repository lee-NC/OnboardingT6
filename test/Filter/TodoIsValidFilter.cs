using test.Model;

namespace test.Filter;

public class TodoIsValidFilter : IEndpointFilter
{
    private readonly ILogger _logger;

    public TodoIsValidFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TodoIsValidFilter>();
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext efiContext,
        EndpointFilterDelegate next)
    {
        var book = efiContext.GetArgument<Book>(0);

        var name = book.Title;

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning(name + " is null");
            return TypedResults.Problem(
                    "Field not null",
                    title: "Bad Request",
                    statusCode: StatusCodes.Status400BadRequest
                )
                ;
        }

        return await next(efiContext);
    }
}