

using docGenerator;
using HtmlAgilityPack;

const string baseUrl = "https://doc.theotown.com";

HttpClient client = new();
client.BaseAddress = new Uri(baseUrl);
var headers = await GetModuleHeaders(client);

async Task<List<ModuleHeader>> GetModuleHeaders(HttpClient client)
{
    var pageStr = await (await client.GetAsync("index.html")).Content.ReadAsStringAsync();
    var doc = new HtmlDocument();
    doc.LoadHtml(pageStr);
    var tableNode = doc.DocumentNode.Descendants(0).First(n => n.HasClass("module_list"));
    var rows = tableNode.ChildNodes;

    List<ModuleHeader> headers = [];
    foreach (var row in rows)
    {
        if (row.NodeType != HtmlNodeType.Element)
            continue;
        
        var a = row.ChildNodes.First(n => n.HasClass("name")).FirstChild;
        string uri = a.Attributes["href"].Value;
        string name = a.InnerText;
        
        var descNode = row.ChildNodes.First(n => n.HasClass("summary"));
        string description = descNode.InnerText;

        ModuleHeader header = new() { Name = name, Description = description, Path = uri };
        headers.Add(header);
        Console.WriteLine(header);
    }
    return headers;
}

