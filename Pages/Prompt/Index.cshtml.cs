using HackerFlow.Data;
using HackerFlow.InputModels.Prompt;
using Microsoft.AspNetCore.Http.HttpResults;
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

    private async Task LoadProperties() => 
        Prompts = await _context.Prompts.ToListAsync();

    public async Task OnGet()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreatePromptInput input)
    {
        if (await _context.Prompts.AnyAsync(p => p.Name == input.Name))
        {
            ModelState.AddModelError("CreateInput.Name", "A prompt with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var prompt = new Models.Prompt
        {
            Name = input.Name,
            Description = input.Description,
            PromptString = input.PromptString,
            CreatedAt = DateTime.Now
        };

        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm(Name = "UpdateInput")] UpdatePromptInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var prompt = _context.Prompts.Find(input.Id);
        if (prompt == null)
        {
            return NotFound();
        }

        prompt.Name = input.Name;
        prompt.Description = input.Description;
        prompt.PromptString = input.PromptString;

        _context.SaveChanges();

        return RedirectToPage();   
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var prompt = await _context.Prompts.FindAsync(id);
        if (prompt == null)
            return NotFound();

        _context.Prompts.Remove(prompt);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}