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

    public ApplicationsController(HackerFlowContext context, IAppSettingsService settings, IFileService file)
    {
        _context = context;
        _settings = settings;
        _file = file;
    }

    [Route("{id}/generate-resume")]
    public async Task<IActionResult> GenerateResume(int id, string group)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        var resume = await _context.Resumes
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (resume == null)
            return NotFound();

        var resumeData = await _file.ReadAsync(resume.FilePath);
        if (resumeData == null)
            return BadRequest();

        var resumeFolder = await _settings.GetSetting(AppSetting.GerenatedResumeFolder);
        var resumeName = $"{application.Company}_{application.Role}_Resume_{Guid.NewGuid()}.pdf";

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

        var coverLetter = await _context.CoverLetters
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (coverLetter == null)
            return NotFound();

        var coverLetterData = await _file.ReadAsync(coverLetter.FilePath);
        if (coverLetterData == null)
            return BadRequest();

        var coverLetterFolder = await _settings.GetSetting(AppSetting.GerenatedResumeFolder);
        var coverLetterName = $"{application.Company}_{application.Role}_CoverLetter_{Guid.NewGuid()}.pdf";

        var coverLetterPath = await _file.SaveAsync(coverLetterFolder, coverLetterName, coverLetterData);
        if (coverLetterPath == null) 
            return BadRequest();

        application.CoverLetterPath = coverLetterPath;
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPost("{id}/create-prompt")]
    public async Task<IActionResult> CreatePrompt(int id)
    {
        return Ok(new { prompt = "Test" });
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
}