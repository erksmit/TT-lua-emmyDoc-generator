using HtmlAgilityPack;

namespace docGenerator;

public class HtmlParser
{
    const string baseUrl = "https://doc.theotown.com";
    private HttpClient client = new() { BaseAddress = new Uri(baseUrl) };
    
    public async Task<List<ModuleHeader>> GetModuleHeaders()
    {
        var pageStr = await (await client.GetAsync("index.html")).Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(pageStr);

        var stuff = doc.DocumentNode.SelectSingleNode("//table").SelectNodes("//tr");
        
        var tableNode = doc.DocumentNode.Descendants(0).First(n => n.HasClass("module_list"));
        var rows = tableNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element);

        List<ModuleHeader> headers = [];
        foreach (var row in rows)
        {
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

    public async Task<Module> GetModule(ModuleHeader header)
    {
        var pageStr = await (await client.GetAsync(header.Path)).Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(pageStr);
        var allNodes = doc.DocumentNode.Descendants(0).ToArray();
        return new Module();
    }
}