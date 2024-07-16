namespace docGenerator;

public class Function
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string ReturnType { get; set; }
    
    public List<Parameter> Parameters { get; set; }
}

public class Parameter
{
    public string Name { get; set; }
    
    public string Type { get; set; }
    
    public string Description { get; set; }
    
    public bool Optional { get; set; }
}