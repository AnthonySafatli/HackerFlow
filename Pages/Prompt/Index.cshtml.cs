using HackerFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HackerFlow.Pages.Prompt;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
}