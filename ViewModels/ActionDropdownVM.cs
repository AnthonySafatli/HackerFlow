namespace HackerFlow.ViewModels;

public class ActionDropdownVM
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Handler { get; set; }   
    public string Method { get; set; } = "POST";
    public string ParamName { get; set; } = "group";  
    public IEnumerable<string> Items { get; set; } = [];
    public string CssClass { get; set; } = "btn-hf-primary";
}