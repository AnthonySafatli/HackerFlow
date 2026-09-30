using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.Services;
using HackerFlow.InputModels;
using HackerFlow.InputModels.CoverLetter;

namespace HackerFlow.Pages.CoverLetter;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;
    private readonly IAppSettingsService _settings;
    private readonly IFileService _file;

    public IndexModel(HackerFlowContext context, IAppSettingsService settings, IFileService file)
    {
        _context = context;
        _settings = settings;
        _file = file;
    }

    public string CoverLetterFolder { get; set; } = "";
    public string GeneratedCoverLetterFolder { get; set; } = "";
    public List<Models.CoverLetter> CoverLetters { get; private set; } = [];

    public IEnumerable<IGrouping<string, Models.CoverLetter>> CoverLetterGroups =>
        CoverLetters
            .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(r => r.Version)
            .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase);

    private async Task LoadProperties()
    {
        CoverLetters = await _context.CoverLetters.ToListAsync();
        CoverLetterFolder = await _settings.GetSetting(AppSetting.CoverLetterFolder);
        GeneratedCoverLetterFolder = await _settings.GetSetting(AppSetting.GerenatedCoverLetterFolder);
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCoverLetterFolderAsync([FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var newFolder = input.Value;

        Directory.CreateDirectory(newFolder);

        var coverLetters = await _context.CoverLetters.ToListAsync();

        foreach (var coverLetter in coverLetters)
        {
            var newPath = await _file.MoveAsync(
                coverLetter.FilePath,
                newFolder);

            if (newPath == null)
            {
                ModelState.AddModelError(
                    "",
                    $"Could not find cover letter file: {coverLetter.FilePath}");

                await LoadProperties();
                return Page();
            }

            coverLetter.FilePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.CoverLetterFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGeneratedFolderAsync([FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var newFolder = input.Value;

        Directory.CreateDirectory(newFolder);

        var applications = await _context.Applications.Where(x => !string.IsNullOrWhiteSpace(x.CoverLetterPath)).ToListAsync();

        foreach (var coverLetter in applications)
        {
            var newPath = await _file.MoveAsync(
                coverLetter.CoverLetterPath,
                newFolder);

            if (newPath == null)
            {
                ModelState.AddModelError(
                    "",
                    $"Could not find cover letter file: {coverLetter.CoverLetterPath}");

                await LoadProperties();
                return Page();
            }

            coverLetter.CoverLetterPath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.GerenatedCoverLetterFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateCoverLetterInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var version = (await _context.CoverLetters
            .Where(r => r.Name == input.Name)
            .MaxAsync(r => (int?)r.Version) ?? 0) + 1;
        
        var coverLetterFolder = await _settings.GetSetting(AppSetting.CoverLetterFolder);
        var fileName = $"{input.Name}_{version}_{Guid.NewGuid()}.pdf";

        string path = await _file.SaveAsync(coverLetterFolder, fileName, input.File.OpenReadStream());

        var coverLetter = new Models.CoverLetter
        {
            Name = input.Name,
            Version = version,
            FilePath = path,
            Notes = input.Notes ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _context.CoverLetters.Add(coverLetter);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var coverLetter = await _context.CoverLetters.FindAsync(id);
        if (coverLetter == null)
        {
            return NotFound();
        }

        _context.CoverLetters.Remove(coverLetter);
        _file.Delete(coverLetter.FilePath);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOpenAsync(int id)
    {
        var coverLetter = await _context.CoverLetters.FindAsync(id);
        if (coverLetter == null || !System.IO.File.Exists(coverLetter.FilePath))
        {
            return NotFound();
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = coverLetter.FilePath,
            UseShellExecute = true
        });

        // stays on the current page
        return new NoContentResult(); 
    }
}