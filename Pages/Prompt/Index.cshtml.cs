using HackerFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Pages.Prompt;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public List<Models.Prompt> Prompts { get; private set; } = [];

    private async Task LoadProperties()
    {
        Prompts = await _context.Prompts.ToListAsync();
    }

    public async Task OnGet()
    {
        await LoadProperties();
    }
}