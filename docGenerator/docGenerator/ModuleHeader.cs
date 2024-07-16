namespace docGenerator;

public record struct ModuleHeader
{
    public string Name { get; set; }
    
    public string Path { get; set; }
    
    public string Description { get; set; }
}