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
        var fnHeaderNodes = doc.DocumentNode.SelectNodes("//dt");

        Module module = new() { Header = header };
        foreach (var headerNode in fnHeaderNodes)
        {
            var nameNode = headerNode.ChildNodes.First(n => n.OriginalName == "a");

            string rawFieldName = nameNode.NextSiblingWhere(n => n.Name == "strong")!.InnerText;
            bool isFn = rawFieldName.Contains('('); // a name containing a bracket ought to be a function right?
            
            string fieldName = nameNode.Attributes["name"].Value;
            var detailsNode = headerNode.NextSiblingWhere(n => n.Name == "dd")!;
            string description = detailsNode.FirstChild.InnerText;

            if (isFn)
            {
                Function fn = new();
                
                fn.Static = !rawFieldName.Contains(':'); // a : indicates a non-static function like script:disable
                if (fn.Static)
                    fn.Name = fieldName;
                else
                    fn.Name = fieldName.Remove(0, fieldName.IndexOf(':', StringComparison.Ordinal)); // filter the modulename: prefix of the function name out

                // Get parameters, if any
                var parameterHeaderNode = detailsNode.ChildNodes.FirstOrDefault(n => n.InnerText == "Parameters:");
                if (parameterHeaderNode != null)
                {
                    
                }
                
                // get return value(s)
                var returnsHeaderNode = detailsNode.ChildNodes.FirstOrDefault(n => n.InnerText == "Returns:");
                if (returnsHeaderNode != null)
                {
                    
                }
                
                module.Functions.Add(fn);
            }
            else // the object is not a function
            {
                
            }
        }
        return module;
    }
}