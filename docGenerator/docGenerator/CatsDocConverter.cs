using System.Text;

namespace docGenerator;

public class CatsDocConverter
{
    public string GetDocs(List<Module> modules)
    {
        return $$"""
                 -- tt lua docs
                 ---@meta
                 
                 ---@alias frame number
                 ---@alias font number
                 ---@alias bool boolean
                 ---@alias int integer
                 ---@alias float number
                 ---@alias animationdraft draft
                 
                 ---@class Font
                 ---@field BIG font
                 ---@field DEFAULT font
                 ---@field SMALL font
                 Font = {}
                 
                 Icon = {}
                 NinePatch = {}
                 
                 {{string.Join("\r\n", modules.Select(GetModule))}}
                 """;
    }

    public string GetModule(Module module)
    {
        StringBuilder result = new();
        string name = module.Header.Name;
        result.AppendLine($"---@class {name} {module.Header.Description}");
        foreach (var field in module.Fields)
        {
            result.AppendLine($"---@field {field.Name} any {field.Description}");
        }   
        result.AppendLine($"{name} = {{}}");
        if (name != "string")    
            result.AppendLine($"---@alias {name.FirstLetterToLower()} {name}");
        result.AppendLine();
        foreach (var func in module.Functions)
        {
            result.AppendLine(GetFunction(name, func));
        }
        
        return result.ToString();
    }

    public string GetFunction(string moduleName, Function function)
    {
        StringBuilder result = new();
        result.Append("-- ").AppendLine(function.Description);
        foreach (var param in function.Parameters)
        {
            result.Append("---@param ").Append(param.Name);
            if (param.Optional)
                result.Append('?');
            result.AppendSpace();
            result.AppendSpace(string.Join(" | ", param.Types.Select(t => t.FirstLetterToLower())));
            result.AppendLine(param.Description);
        }

        foreach (var returns in function.Returns)
        {
            result.Append("---@return ");
            result.AppendSpace(string.Join(" | ", returns.Types.Select(t => t.FirstLetterToLower())));
            result.AppendSpace('r');
            result.AppendLine(returns.Description);
        }

        result.Append("function ")
            .Append(moduleName)
            .Append(function.Static ? '.' : ':')
            .Append(function.Name)
            .AppendLine($"({string.Join(", ", function.Parameters.Select(p => p.Name))}) end");

        return result.ToString();
    }
}