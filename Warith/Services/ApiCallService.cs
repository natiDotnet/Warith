using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using static Warith.Services.ApiCallService;

using Warith.Models;

namespace Warith.Services;

public interface IApiCallService
{
    Task<WarethResponse> CalculateInheritanceAsync(Dictionary<string, string> formData);
    Task<WarethResponse> CalculateInheritanceAsync(InheritanceForm inheritance);
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

    public async Task<WarethResponse> CalculateInheritanceAsync(Dictionary<string, string> formData)
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

        return new WarethResponse(responseText ?? string.Empty, ParseXajaxTable(xajaxResponse));
    }

    public Task<WarethResponse> CalculateInheritanceAsync(InheritanceForm inheritance)
    {
        var formData = MapToDictionary(inheritance);
        return CalculateInheritanceAsync(formData);
    }

    private Dictionary<string, string> MapToDictionary(InheritanceForm inheritance)
    {
        var dict = new Dictionary<string, string>
        {
            { "gender", inheritance.Gender ?? "zakar" },
            { "dyoon", inheritance.Debt?.ToString() ?? "" },
            { "floos", inheritance.TotalAmount?.ToString() ?? "" },
            { "eradyya1", inheritance.Will1 ?? "" },
            { "eradyya2", inheritance.Will2 ?? "" },
            { "eradyya3", inheritance.Will3 ?? "" },
            { "agazo", inheritance.AcceptWillMoreThanOneThird == true ? "1" : "0" },
            { "zawgat", inheritance.NumberOfWives?.ToString() ?? "0" },
            { "u41", inheritance.IsHusbandAlive == true ? "yes" : "no" }, // Mapping u41 as Husband based on model comments
            { "om1", inheritance.IsMotherAlive == true ? "yes" : "no" },
            { "ab1", inheritance.IsFatherAlive == true ? "yes" : "no" },
            { "omom1", inheritance.IsGrandMotherMaternalAlive == true ? "yes" : "no" },
            { "abab1", inheritance.IsGrandFatherPaternalAlive == true ? "yes" : "no" },
            { "ababab1", inheritance.IsGreatGrandFatherPaternalAlive == true ? "yes" : "no" },
            { "omomom1", inheritance.IsGreatGrandMotherMaternalAlive == true ? "yes" : "no" },
            { "omab1", inheritance.IsGrandMotherPaternalAlive == true ? "yes" : "no" },
            { "omabab1", inheritance.IsMotherOfFatherOfFatherAlive == true ? "yes" : "no" },
            { "omomab1", inheritance.IsMotherOfMotherOfFatherAlive == true ? "yes" : "no" },
            { "s1", MapIntToYesNo(inheritance.Sons) },
            { "s2", MapIntToYesNo(inheritance.Daughters) },
            { "s3", MapIntToYesNo(inheritance.SonsOfSon) },
            { "s4", MapIntToYesNo(inheritance.DaughtersOfSon) },
            { "s5", MapIntToYesNo(inheritance.SonsOfSonOfSon) },
            { "s6", MapIntToYesNo(inheritance.DaughtersOfSonOfSon) },
            { "s7", MapIntToYesNo(inheritance.FullBrothers) },
            { "s8", MapIntToYesNo(inheritance.FullSisters) },
            { "s9", MapIntToYesNo(inheritance.PaternalBrothers) },
            { "s10", MapIntToYesNo(inheritance.PaternalSisters) },
            { "s11", MapIntToYesNo(inheritance.UterineBrothers) },
            { "s12", MapIntToYesNo(inheritance.UterineSisters) },
            { "s13", MapIntToYesNo(inheritance.SonsOfFullBrother) },
            { "s14", MapIntToYesNo(inheritance.SonsOfPaternalBrother) },
            { "s15", MapIntToYesNo(inheritance.SonsOfSonOfFullBrother) },
            { "s16", MapIntToYesNo(inheritance.SonsOfSonOfPaternalBrother) },
            { "s17", MapIntToYesNo(inheritance.FullBrothersOfFather) },
            { "s18", MapIntToYesNo(inheritance.PaternalBrothersOfFather) },
            { "s19", MapIntToYesNo(inheritance.SonsOfFullBrotherOfFather) },
            { "s20", MapIntToYesNo(inheritance.SonsOfPaternalBrotherOfFather) },
            { "s21", MapIntToYesNo(inheritance.SonsOfSonOfFullBrotherOfFather) },
            { "s22", MapIntToYesNo(inheritance.SonsOfSonOfPaternalBrotherOfFather) },
            { "s23", MapIntToYesNo(inheritance.FullBrotherOfGrandFather) },
            { "s24", MapIntToYesNo(inheritance.PaternalBrotherOfGrandFather) },
            { "s25", MapIntToYesNo(inheritance.SonsOfFullBrotherOfGrandFather) },
            { "s26", MapIntToYesNo(inheritance.SonsOfPaternalBrotherOfGrandFather) },
            { "grouphalat[monaskha]", inheritance.PredeceasedRelative == true ? "yes" : "no" },
            { "grouphalat[wasyawagba]", inheritance.OffspringOfPredeceased == true ? "yes" : "no" },
            { "grouphalat[haml]", inheritance.Fetus == true ? "yes" : "no" },
            { "grouphalat[mafkood]", inheritance.MissingPerson == true ? "yes" : "no" },
            { "mazaheb", inheritance.SchoolOrLaw ?? "gm" },
            { "insert2", "Calculate Inheritance" }
        };

        return dict;
    }

    private string MapIntToYesNo(int? value)
    {
        if (value == null || value <= 0) return "no";
        return value.ToString();
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
                Share = HtmlEntity.DeEntitize(cells[1].InnerText.Trim()).Replace("ــــــــ", "/").Replace("\n", "").Trim(),
                Count = int.TryParse(cells[2].InnerText.Trim(), out int count) ? count : 0,
                Heir = cells[3].InnerText.Trim()
            });
        }

        return heirs;
    }
}
