using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using HackerFlow.Data;
using HackerFlow.Models;
using HackerFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
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

    // TODO: Look into moving these into the pages file
    [Route("{id}/generate-resume")]
    public async Task<IActionResult> GenerateResume(int id, string group)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(application.ResumePath))
            return BadRequest();

        var resume = await _context.Resumes
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (resume == null)
            return NotFound();

        var resumeData = await _file.ReadAsync(resume.FilePath);
        if (resumeData == null)
            return BadRequest();

        var ext = Path.GetExtension(resume.FilePath).ToLowerInvariant();
        var resumeFolder = await _settings.GetSetting(AppSetting.GerenatedResumeFolder);
        var resumeName = $"{application.Company}_{application.Role}_Resume_{Guid.NewGuid()}{ext}";

        var resumePath = await _file.SaveAsync(resumeFolder, resumeName, resumeData);
        if (resumePath == null) 
            return BadRequest();

        application.ResumePath = resumePath;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [Route("{id}/generate-cover-letter")]
    public async Task<IActionResult> GenerateCoverLetter(int id, string group)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(application.CoverLetterPath))
            return BadRequest();

        var coverLetter = await _context.CoverLetters
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (coverLetter == null)
            return NotFound();

        var coverLetterData = await _file.ReadAsync(coverLetter.FilePath);
        if (coverLetterData == null)
            return BadRequest();

        var ext = Path.GetExtension(coverLetter.FilePath).ToLowerInvariant();
        var coverLetterFolder = await _settings.GetSetting(AppSetting.GerenatedResumeFolder);
        var coverLetterName = $"{application.Company}_{application.Role}_CoverLetter_{Guid.NewGuid()}{ext}";

        var coverLetterPath = await _file.SaveAsync(coverLetterFolder, coverLetterName, coverLetterData);
        if (coverLetterPath == null) 
            return BadRequest();

        application.CoverLetterPath = coverLetterPath;
        await _context.SaveChangesAsync();

        return Ok();
    }
    

    [HttpPost("{id}/create-prompt")]
    public async Task<IActionResult> CreatePrompt(int id, string name, CancellationToken ct)
    {
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
            result.Replace("{{jobDescription}}", application.JobDescription);
        
        return Ok(new { prompt = result });
    }

    [Route("{id}/create-application-package")]
    public async Task<IActionResult> CreateApplicationPackage(int id)
    {
        var application = await _context.Applications.FindAsync(id);
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

        var companyName = Regex.Replace(application.Company, "[^a-zA-Z0-9_-]", "");
        var roleName = Regex.Replace(application.Role, "[^a-zA-Z0-9_-]", "");

        var packageFolder = $"~/Downloads/{companyName}_{roleName}_Package";
        var resumeName = $"AnthonySafatli_Resume_{companyName}";
        var coverLetterName = $"AnthonySafatli_CoverLetter_{companyName}";

        var saveTasks = new List<Task>();

        if (resumeData != null)
            saveTasks.Add(_file.SaveAsync(packageFolder, resumeName, resumeData));

        if (coverLetterData != null)
            saveTasks.Add(_file.SaveAsync(packageFolder, coverLetterName, coverLetterData));

        await Task.WhenAll(saveTasks);

        return Ok();
    }

    private async Task<string> ReadDocumentAsync(string? path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return string.Empty;

        await using var stream = System.IO.File.OpenRead(path);
        return await _extractor.ExtractTextAsync(stream, path, ct);
    }
}