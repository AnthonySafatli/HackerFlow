using Microsoft.AspNetCore.Mvc.RazorPages;
using Venture.Models;
using Venture.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Venture.Services;
using Venture.InputModels;
using Venture.InputModels.CoverLetter;
using Venture.Utils;

namespace Venture.Pages.CoverLetter;

public class IndexModel : PageModel
{
    private readonly VentureContext _context;
    private readonly IAppSettingsService _settings;
    private readonly IFileService _file;
    private readonly IOpenFileService _openFile;

    public IndexModel(VentureContext context, IAppSettingsService settings, IFileService file, IOpenFileService openFile)
    {
        _context = context;
        _settings = settings;
        _file = file;
        _openFile = openFile;
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
        GeneratedCoverLetterFolder = await _settings.GetSetting(AppSetting.GeneratedCoverLetterFolder);
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    private bool TryPrepareFolder(string? raw, out string folder)
    {
        const string key = "ValueInput.Value";

        if (!PathUtils.TryNormalize(raw, out folder, out var error))
        {
            ModelState.AddModelError(key, error!);
            return false;
        }

        try
        {
            Directory.CreateDirectory(folder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ModelState.AddModelError(key, "Can't create or access that folder.");
            return false;
        }

        return true;
    }

    public async Task<IActionResult> OnPostCoverLetterFolderAsync(
        [FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid || !TryPrepareFolder(input?.Value, out var newFolder))
        {
            await LoadProperties();
            return Page();
        }

        var coverLetters = await _context.CoverLetters.ToListAsync();

        // Check everything exists before moving anything
        var missing = coverLetters.FirstOrDefault(c => !System.IO.File.Exists(c.FilePath));
        if (missing != null)
        {
            ModelState.AddModelError("", $"Could not find cover letter file: {missing.FilePath}");
            await LoadProperties();
            return Page();
        }

        foreach (var coverLetter in coverLetters)
        {
            var newPath = await _file.MoveAsync(coverLetter.FilePath, newFolder);

            if (newPath == null)
            {
                // Keep the DB in sync with files already moved
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Could not move cover letter file: {coverLetter.FilePath}");
                await LoadProperties();
                return Page();
            }

            coverLetter.FilePath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.CoverLetterFolder, newFolder);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGeneratedFolderAsync(
        [FromForm(Name = "ValueInput")] SettingUpdateInput input)
    {
        if (!ModelState.IsValid || !TryPrepareFolder(input?.Value, out var newFolder))
        {
            await LoadProperties();
            return Page();
        }

        var applications = await _context.Applications
            .Where(x => !string.IsNullOrWhiteSpace(x.CoverLetterPath))
            .ToListAsync();

        var missing = applications.FirstOrDefault(a => !System.IO.File.Exists(a.CoverLetterPath));
        if (missing != null)
        {
            ModelState.AddModelError("", $"Could not find generated file: {missing.CoverLetterPath}");
            await LoadProperties();
            return Page();
        }

        foreach (var application in applications)
        {
            var newPath = await _file.MoveAsync(application.CoverLetterPath, newFolder);

            if (newPath == null)
            {
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Could not move generated file: {application.CoverLetterPath}");
                await LoadProperties();
                return Page();
            }

            application.CoverLetterPath = newPath;
        }

        await _context.SaveChangesAsync();
        await _settings.SetSetting(AppSetting.GeneratedCoverLetterFolder, newFolder);

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
        
        var ext = Path.GetExtension(input.File.FileName).ToLowerInvariant();
        var coverLetterFolder = await _settings.GetSetting(AppSetting.CoverLetterFolder);
        var fileName = $"{input.Name}_{version}_{Guid.NewGuid()}{ext}";

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
            ModelState.AddModelError("", "Cover letter not found.");
            await LoadProperties();
            return Page();
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
            ModelState.AddModelError("", "Cover letter not found.");
            await LoadProperties();
            return Page();
        }

        _openFile.OpenFile(coverLetter.FilePath);
        
        // stays on the current page
        return new NoContentResult(); 
    }
}