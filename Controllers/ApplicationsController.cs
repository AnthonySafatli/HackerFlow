using System.Text.RegularExpressions;
using HackerFlow.Data;
using HackerFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private const string PackagesCookie = "packages";

    private string DownloadsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    private List<string> GetPackageFolders() =>
        (Request.Cookies[PackagesCookie] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

    private readonly HackerFlowContext _context;
    private readonly IAppSettingsService _settings;
    private readonly IFileService _file;
    private readonly IDocumentTextExtractor _extractor;

    public ApplicationsController(HackerFlowContext context, IAppSettingsService settings, IFileService file, IDocumentTextExtractor extractor)
    {
        _context = context;
        _settings = settings;
        _file = file;
        _extractor = extractor;
    }

    [HttpPost("{id}/create-prompt")]
    public async Task<IActionResult> CreatePrompt(int id, [FromQuery] string? name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Prompt name is required.");

        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        var prompt = await _context.Prompts.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (prompt == null)
            return NotFound();

        var resumeText = await ReadDocumentAsync(application.ResumePath, ct);
        var coverLetterText = await ReadDocumentAsync(application.CoverLetterPath, ct);

        var result = prompt.PromptString;

        if (resumeText != null)
            result = result.Replace("{{resume}}", resumeText);

        if (coverLetterText != null)
            result = result.Replace("{{coverLetter}}", coverLetterText);

        if (!string.IsNullOrWhiteSpace(application.JobDescription))
            result = result.Replace("{{jobDescription}}", application.JobDescription);
        
        return Ok(new { prompt = result });
    }

    [Route("{id}/create-application-package")]
    public async Task<IActionResult> CreateApplicationPackage(int id)
    {
        var application = await _context.Applications
            .Include(a => a.Company)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (application == null)
            return NotFound();

        var resumeDataTask = string.IsNullOrEmpty(application.ResumePath)
            ? Task.FromResult<byte[]?>(null)
            : _file.ReadAsync(application.ResumePath);

        var coverLetterDataTask = string.IsNullOrEmpty(application.CoverLetterPath)
            ? Task.FromResult<byte[]?>(null)
            : _file.ReadAsync(application.CoverLetterPath);

        var resumeData = await resumeDataTask;
        var coverLetterData = await coverLetterDataTask;

        if (resumeData == null && coverLetterData == null)
            return BadRequest();

        var companyName = Regex.Replace(application.Company.Name, "[^a-zA-Z0-9_-]", "");
        var roleName = Regex.Replace(application.Role, "[^a-zA-Z0-9_-]", "");

        var folderName = $"{companyName}_{roleName}_Package";
        var packageFolder = Path.Combine(DownloadsPath, folderName);
        var resumeExt = Path.GetExtension(application.ResumePath).ToLowerInvariant();
        var coverLetterExt = Path.GetExtension(application.CoverLetterPath).ToLowerInvariant();
        var resumeName = $"AnthonySafatli_Resume_{companyName}{resumeExt}";
        var coverLetterName = $"AnthonySafatli_CoverLetter_{companyName}{coverLetterExt}";

        var saveTasks = new List<Task>();

        if (resumeData != null)
            saveTasks.Add(_file.SaveAsync(packageFolder, resumeName, resumeData));

        if (coverLetterData != null)
            saveTasks.Add(_file.SaveAsync(packageFolder, coverLetterName, coverLetterData));

        await Task.WhenAll(saveTasks);

        var folders = GetPackageFolders();
        if (!folders.Contains(folderName))
            folders.Add(folderName);

        Response.Cookies.Append(PackagesCookie, string.Join(",", folders), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return Ok();
    }

    [HttpDelete("packages")]
    public IActionResult PurgePackages()
    {
        var deleted = new List<string>();

        foreach (var name in GetPackageFolders())
        {
            // Cookies can be tampered with, so only delete names that match our own pattern
            if (!Regex.IsMatch(name, "^[a-zA-Z0-9_-]+_Package$"))
                continue;

            var path = Path.Combine(DownloadsPath, name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                deleted.Add(name);
            }
        }

        Response.Cookies.Delete(PackagesCookie);
        return Ok(deleted);
    }

    private async Task<string> ReadDocumentAsync(string? path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return string.Empty;

        await using var stream = System.IO.File.OpenRead(path);
        return await _extractor.ExtractTextAsync(stream, path, ct);
    }
}