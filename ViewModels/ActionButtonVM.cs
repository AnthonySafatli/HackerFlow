namespace HackerFlow.ViewModels;

public class ActionButtonVM
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Handler { get; set; }  
    public string Method { get; set; } = "POST";
    public string CssClass { get; set; } = "btn-outline-secondary";
    public bool Disabled { get; set; }
}