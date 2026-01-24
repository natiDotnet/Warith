using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using static Warith.Services.ApiCallService;

namespace Warith.Services;

public interface IApiCallService
{
    Task<List<HeirShare>> CalculateInheritanceAsync(Dictionary<string, string> formData);
}

public class ApiCallService : IApiCallService
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;
    private const string BaseUrl = "https://almwareeth.com/review-inheritors";

    public ApiCallService()
    {
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler);

        // Default headers to mimic a browser
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Pragma", "no-cache");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-gpc", "1");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36"
        );
    }

    public async Task<List<HeirShare>> CalculateInheritanceAsync(Dictionary<string, string> formData)
    {
        // -------- FIRST REQUEST --------
        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
        {
            Content = new FormUrlEncodedContent(formData)
        };
        request.Headers.Referrer = new Uri("https://almwareeth.com/islamic-inheritance-calculator");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Extract the response text
        var responseText = doc
            .GetElementbyId("responsehaml")
            ?.InnerText
            ?.Trim();

        // Extract the xajax onclick
        var onClick = doc
            .DocumentNode
            .SelectSingleNode("//span[@class='btnhesab']//a")
            ?.GetAttributeValue("onclick", "");

        if (string.IsNullOrWhiteSpace(onClick))
            throw new InvalidOperationException("onclick not found");

        // -------- SECOND REQUEST (XAJAX) --------
        var xajaxResponse = await CallXajaxAsync(onClick);

        var json = ParseXajaxTable(xajaxResponse);

        // You can choose what to return; here we return the XAJAX response
        return ParseXajaxTable(xajaxResponse);
    }

    private async Task<string> CallXajaxAsync(string onClickJs)
    {
        // Parse xajax function and args
        var match = Regex.Match(onClickJs, @"xajax_(\w+)\(([^)]*)\)", RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new InvalidOperationException("xajax call not found");

        var method = match.Groups[1].Value;
        var args = match.Groups[2].Value
            .Split(',')
            .Select(a => a.Trim())
            .Where(a => a.Length > 0);

        // Build form data (including xajaxr timestamp)
        var form = new List<KeyValuePair<string, string>>
        {
            new("xajax", method),
            new("xajaxr", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
        };

        foreach (var arg in args)
            form.Add(new KeyValuePair<string, string>("xajaxargs[]", arg));

        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
        {
            Content = new FormUrlEncodedContent(form)
        };
        request.Headers.Referrer = new Uri(BaseUrl);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }


    public static List<HeirShare> ParseXajaxTable(string xajaxResponse)
    {
        var docXml = XDocument.Parse(xajaxResponse);

        // Extract CDATA content inside <cmd> where t="edadat_tawleed"
        var cdataHtml = docXml
            .Descendants("cmd")
            .FirstOrDefault(x => (string)x.Attribute("t") == "edadat_tawleed")
            ?.Value;

        if (string.IsNullOrWhiteSpace(cdataHtml))
            return new List<HeirShare>();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(cdataHtml);

        // Select the main table
        var tableNode = htmlDoc.DocumentNode.SelectSingleNode("//table");
        if (tableNode == null)
            return new List<HeirShare>();

        var heirs = new List<HeirShare>();

        // Skip the header row
        var rows = tableNode.SelectNodes(".//tr").Skip(1);

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells == null || cells.Count < 4)
                continue;

            heirs.Add(new HeirShare
            {
                Percent = cells[0].InnerText.Trim('%'),
                Share = HtmlEntity.DeEntitize(cells[1].InnerText.Trim()).Replace("ــــــــ", "").Replace("\n", "").Trim(),
                Count = int.TryParse(cells[2].InnerText.Trim(), out int count) ? count : 0,
                Heir = cells[3].InnerText.Trim()
            });
        }

        return heirs;
    }
    public class HeirShare
    {
        public string Percent { get; set; }      // %25, %50, etc.
        public string Share { get; set; }        // "1/4", "2/4" etc
        public int Count { get; set; }           // number of heirs
        public string Heir { get; set; }         // "Wife", "Mother", "Father"
    }

}
