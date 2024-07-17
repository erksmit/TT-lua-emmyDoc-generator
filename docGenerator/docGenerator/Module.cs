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
}

public record struct Field
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public record struct Function()
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string ReturnType { get; set; }

    public List<Parameter> Parameters { get; set; } = [];
    
    public bool Static { get; set; }
}

public record struct Parameter
{
    public string Name { get; set; }
    
    public string Type { get; set; }
    
    public string Description { get; set; }
    
    public bool Optional { get; set; }
}