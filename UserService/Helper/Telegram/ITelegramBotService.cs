using System.Data;
using System.Net;
using System.Net.Sockets;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace UserService.Helper.Telegram;

public interface ITelegramBotService
{
    Task SendMessage(string message);

    Task SendPhoto(string path);

    Task SendReport(string destFileName);
}

public class TelegramBotService : ITelegramBotService
{
    private static TelegramBotOptions _config;
    private static TelegramBotClient _client;
    private readonly string _ip = "0.0.0.0";
    private static string _filePath = "../Users/lengo/Pictures/Saved Pictures/ngon-ngu-meo.jpg";
    private static string _destinationPath = "../BUCA/demo/TelegramBot/TelegramBot/demo.json";
    private static string _fileName = "demo.xlsx";

    private static List<Book> lstBooks = new List<Book>()
    {
        new Book { Id = 1, Name = "name 1", Author = "author 1" },
        new Book { Id = 2, Name = "name 2", Author = "author 2" },
        new Book { Id = 3, Name = "name 3", Author = "author 3" },
    };

    public TelegramBotService(IOptions<TelegramBotOptions> options)
    {
        if (options!.Value == null) throw new ArgumentNullException(nameof(options.Value));
        _config = options.Value;
        if (string.IsNullOrEmpty(_config.ApiToken))
        {
            throw new ArgumentNullException(nameof(options.Value));
        }

        _client = new TelegramBotClient(_config.ApiToken!);
        try
        {
            _ip = getIP();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            SendMessage("Server Error");
        }
    }

    private static async Task ErrorHandler(ITelegramBotClient botClient, Exception e,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(e.Message);
    }

    public async Task SendMessage(string message)
    {
        message = $"[{_ip}] {message}";
        await _client.SendTextMessageAsync(
            chatId: _config.ChatId,
            text: message);
    }

    public async Task SendPhoto(string path)
    {
        try
        {
            SaveToFile();
            await using Stream stream = System.IO.File.OpenRead(_fileName);
            await _client.SendPhotoAsync(_config.ChatId,
                InputFile.FromStream(stream: stream));
        }
        catch (Exception e)
        {
            await _client.SendTextMessageAsync(_config.ChatId,
                e.Message);
        }
    }

    public async Task SendReport(string destFileName)
    {
        try
        {
            SaveToFile();
            await using Stream stream = System.IO.File.OpenRead(_fileName);
            await _client.SendDocumentAsync(_config.ChatId,
                InputFile.FromStream(stream: stream, fileName: destFileName));
        }
        finally
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    File.Delete(_fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    private static void SaveToFile()
    {
        try
        {
            DataTable dt = _dataTable();
            using var wbook = new XLWorkbook();

            var ws = wbook.Worksheets.Add(dt, "Report");
            wbook.SaveAs(_fileName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static DataTable _dataTable()
    {
        DataTable dataTable = new DataTable("Books");

        // Adding columns to the DataTable object
        dataTable.Columns.Add("Product ID", typeof(int));
        dataTable.Columns.Add("Product Name", typeof(string));
        dataTable.Columns.Add("Author", typeof(string));

        foreach (Book book in lstBooks)
        {
            DataRow dr = dataTable.NewRow();
            dr[0] = book.Id;
            dr[1] = book.Name;
            dr[2] = book.Author;
            dataTable.Rows.Add(dr);
        }

        return dataTable;
    }

    private string getIP()
    {
        var myhost = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ipaddr in myhost.AddressList)
        {
            if (ipaddr.AddressFamily == AddressFamily.InterNetwork)
            {
                return ipaddr.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address was found");
    }
}

public class Book
{
    public string Name { get; set; }
    public int Id { get; set; }
    public string? Author { get; set; }
}