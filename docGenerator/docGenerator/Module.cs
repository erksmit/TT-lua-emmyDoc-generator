namespace docGenerator;

public record struct ModuleHeader
{
    public string Name { get; set; }
    
    public string Path { get; set; }
    
    public string Description { get; set; }
}

public record struct Module()
{
    public ModuleHeader Header { get; set; }
    
    public List<Field> Fields { get; set; } = [];
    
    public List<Function> Functions { get; set; } = [];

    public Module GetParts(bool statics)
    {
        return new Module
        {
            Header = Header,
            Fields = Fields.Where(f => f.Static == statics).ToList(),
            Functions = Functions.Where(f => f.Static == statics).ToList()
        };
    }

    public bool HasParts()
    {
        return Fields.Any() || Functions.Any();
    }
}

public record struct Field()
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public bool Static { get; set; } = true;
}

public record struct Function()
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public List<Parameter> Parameters { get; set; } = [];

    public List<Parameter> Returns { get; set; } = [];
    
    public bool Static { get; set; }
}

public record struct Parameter()
{
    public string Name { get; set; }

    public List<string> Types { get; set; } = [];
    
    public string Description { get; set; }
    
    public bool Optional { get; set; }
}