using HtmlAgilityPack;

namespace docGenerator;

public class HtmlParser
{
    private const string BASE_URL = "https://doc.theotown.com";
    private readonly HttpClient client = new() { BaseAddress = new Uri(BASE_URL), DefaultRequestHeaders = {{"User-Agent", "erksmit's amazing documentation generator"}}};
    
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
            string description = descNode.InnerText.TrimAll();

            ModuleHeader header = new() { Name = name, Description = description, Path = uri };
            headers.Add(header);
        }
        return headers;
    }

    public async Task<Module> GetModule(ModuleHeader header)
    {
        Console.WriteLine("Fetching module " + header.Name);
        var pageStr = await (await client.GetAsync(header.Path)).Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(pageStr);
        var fnHeaderNodes = doc.DocumentNode.SelectNodes("//dt");

        bool isBuildingModule = header.Name == "Building";
        
        Module module = new() { Header = header };
        foreach (var headerNode in fnHeaderNodes)
        {
            var nameNode = headerNode.ChildNodes.First(n => n.OriginalName == "a");

            string rawFieldName = nameNode.NextSiblingWhere(n => n.Name == "strong")!.InnerText;
            bool isFn = rawFieldName.Contains('('); // a name containing a bracket ought to be a function right?

            string fieldName = nameNode.Attributes["name"].Value;
            var detailsNode = headerNode.NextSiblingWhere(n => n.Name == "dd")!;
            var descriptionParts = detailsNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text || n.Name == "em" || n.Name == "strong").Select(n => n.InnerText);
            string description = string.Concat(descriptionParts).TrimAll();

            if (isFn)
            {
                Function fn = new()
                {
                    Description = description,
                    Static = !rawFieldName.Contains(':') // a : indicates a non-static function like script:disable
                };
                if (isBuildingModule) // override for building module as it's not the same in the site
                    fn.Static = false;

                fn.Name = fn.Static
                    ? fieldName
                    : fieldName.Split(':').Last(); // filter the modulename:functionname prefix of the function name out

                fn.Parameters = ParseParameters(detailsNode);
                fn.Returns = ParseReturnTypes(detailsNode);
                module.Functions.Add(fn);
                Console.WriteLine("parsed function: "  + rawFieldName);
            }
            else // the object is not a function but rather a field.
            {
                var field = ParseField(fieldName, detailsNode);
                module.Fields.Add(field);
                Console.WriteLine("parsed field: " + fieldName);
            }
        }
        return module;
    }

    private static List<Parameter> ParseParameters(HtmlNode detailsNode)
    {
        var parameters = new List<Parameter>();
        
        // Get parameters, if any
        var parameterHeaderNode = detailsNode.ChildNodes.FirstOrDefault(n => n.InnerText == "Parameters:");
        if (parameterHeaderNode != null)
        {
            var paramListNode = parameterHeaderNode.NextSiblingWhere(n => n.Name == "ul")!;
            var paramBaseNodes = paramListNode.ChildNodes.Where(n => n.Name == "li");
            foreach (var paramBaseNode in paramBaseNodes)
            {
                Parameter param = new();
                        
                var paramNameNode = paramBaseNode.ChildNodes.First(n => n.GetClasses().Contains("parameter"));
                param.Name = paramNameNode.InnerText;
                        
                var paramTypeNodes = paramBaseNode.ChildNodes.Where(n => n.GetClasses().Contains("types"));
                param.Types.AddRange(paramTypeNodes.SelectMany(n => n.InnerText.Split(" or ")));
                if (param.Types.Count == 0)
                    param.Types.Add("any");

                param.Optional = paramBaseNode.ChildNodes.Any(n => n.Name == "em" && n.InnerText is "optional" or "default");

                var paramDescParts = paramBaseNode.ChildNodes
                    .Where(n => n.Name != "span" && !string.IsNullOrEmpty(n.InnerText)).Select(n => n.InnerText)
                    .ToArray();
                var desc = string.Concat(paramDescParts).TrimAll();
                param.Description = desc;
                        
                parameters.Add(param);
            }
        }

        return parameters;
    }

    private static List<Parameter> ParseReturnTypes(HtmlNode detailsNode)
    {
        var values = new List<Parameter>();
        
        // get return value(s)
        var returnsHeaderNode = detailsNode.ChildNodes.FirstOrDefault(n => n.InnerText == "Returns:");
        if (returnsHeaderNode != null)
        {
            var returnsListNode = returnsHeaderNode.NextSiblingWhere(n => n.Name == "ol")!;
            var returnBaseNodes = returnsListNode.ChildNodes.Where(n => n.Name == "li").ToArray();
            if (returnBaseNodes.Length == 0) // functions with only 1 return value will not have the returns wrapped in a <li>
                returnBaseNodes = [returnsListNode];
            foreach (var returnBaseNode in returnBaseNodes)
            {
                Parameter returns = new();
                        
                var returnTypeNodes = returnBaseNode.ChildNodes.Where(n => n.GetClasses().Contains("types"));
                returns.Types.AddRange(returnTypeNodes.SelectMany(n => n.InnerText.Split(" or ")));
                if (returns.Types.Count == 0)
                    returns.Types.Add("any");
                        
                var returnDescParts = returnBaseNode.ChildNodes
                    .Where(n => n.Name != "span" && !string.IsNullOrEmpty(n.InnerText)).Select(n => n.InnerText)
                    .ToArray();
                var desc = string.Concat(returnDescParts).TrimAll();
                returns.Description = desc;
                        
                values.Add(returns);
            }
        }

        return values;
    }

    private static Field ParseField(string fieldName, HtmlNode detailsNode)
    {
        bool isStatic = true;
        if (fieldName.Contains('.'))
        {
            isStatic = false;
            fieldName = fieldName.Remove(0, fieldName.IndexOf('.') + 1);
        }
        Field field = new()
        {
            Name = fieldName,
            Description = detailsNode.InnerText.TrimAll(),
            Static = isStatic
        };
        
        return field;
    }
}