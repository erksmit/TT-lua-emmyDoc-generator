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
            string description = descNode.InnerText.TrimAll();

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
            var descriptionParts = detailsNode.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Text || n.Name == "em").Select(n => n.InnerText);
            string description = string.Concat(descriptionParts).TrimAll();

            if (isFn)
            {
                Function fn = new()
                {
                    Description = description,
                    Static = !rawFieldName.Contains(':') // a : indicates a non-static function like script:disable
                };

                fn.Name = fn.Static
                    ? fieldName
                    : fieldName.Remove(0, fieldName.IndexOf(':', StringComparison.Ordinal) + 1); // filter the modulename: prefix of the function name out

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
                        param.Types.AddRange(paramTypeNodes.Select(n => n.InnerText));
                        if (param.Types.Count == 0)
                            param.Types.Add("any");

                        param.Optional = paramBaseNode.ChildNodes.Any(n => n.Name == "em" && n.InnerText == "optional");

                        var paramDescParts = paramBaseNode.ChildNodes
                            .Where(n => n.Name != "span" && !string.IsNullOrEmpty(n.InnerText)).Select(n => n.InnerText)
                            .ToArray();
                        var desc = string.Concat(paramDescParts).TrimAll();
                        param.Description = desc;
                        
                        fn.Parameters.Add(param);
                    }
                }

                // get return value(s)
                var returnsHeaderNode = detailsNode.ChildNodes.FirstOrDefault(n => n.InnerText == "Returns:");
                if (returnsHeaderNode != null)
                {
                    var returnsListNode = returnsHeaderNode.NextSiblingWhere(n => n.Name == "ol")!;
                    var returnBaseNodes = returnsListNode.ChildNodes.Where(n => n.Name == "li").ToArray();
                    if (!returnBaseNodes.Any()) // functions with only 1 return value will not have the returns wrapped in a <li>
                        returnBaseNodes = [returnsListNode];
                    foreach (var returnBaseNode in returnBaseNodes)
                    {
                        Parameter returns = new();
                        
                        var returnTypeNodes = returnBaseNode.ChildNodes.Where(n => n.GetClasses().Contains("types"));
                        returns.Types.AddRange(returnTypeNodes.Select(n => n.InnerText));
                        if (returns.Types.Count == 0)
                            returns.Types.Add("any");
                        
                        var returnDescParts = returnBaseNode.ChildNodes
                            .Where(n => n.Name != "span" && !string.IsNullOrEmpty(n.InnerText)).Select(n => n.InnerText)
                            .ToArray();
                        var desc = string.Concat(returnDescParts).TrimAll();
                        returns.Description = desc;
                        
                        fn.Returns.Add(returns);
                    }
                }

                module.Functions.Add(fn);
            }
            else // the object is not a function
            {
                bool stat = true;
                if (fieldName.Contains('.'))
                {
                    stat = false;
                    fieldName = fieldName.Remove(0, fieldName.IndexOf('.', StringComparison.Ordinal) + 1);
                }
                Field field = new()
                {
                    Name = fieldName,
                    Description = detailsNode.InnerText.TrimAll(),
                    Static = stat
                };
                module.Fields.Add(field);
            }
        }
        return module;
    }
}