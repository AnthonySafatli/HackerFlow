using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using Microsoft.AspNetCore.Mvc;
using HackerFlow.Data;
using Microsoft.EntityFrameworkCore;
using HackerFlow.InputModels.Index;
using HackerFlow.Services;

namespace HackerFlow.Pages.Index;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;
    private readonly IFileService _file;
    private readonly IAppSettingsService _settings;
    private readonly IOpenFileService _openFile;

    public IndexModel(HackerFlowContext context, IFileService file, IAppSettingsService settings, IOpenFileService openFile)
    {
        _context = context;
        _file = file;
        _settings = settings;
        _openFile = openFile;
    }

    public List<JobApplication> Applications { get; private set; } = [];
    public List<string> ResumeGroups { get; private set; } = [];
    public List<string> CoverLetterGroups { get; private set; } = [];
    public List<string> Prompts { get; private set; } = [];
    public List<string> Companies { get; private set; } = [];
    public List<string> Connections { get; private set; } = [];

    private async Task LoadProperties()
    {
        Applications = await _context.Applications
            .Include(a => a.Company)
            .Include(a => a.Connection)
            .ToListAsync();

        ResumeGroups = await _context.Resumes
            .GroupBy(x => x.Name)
            .Select(x => x.First().Name)
            .ToListAsync();

        CoverLetterGroups = await _context.CoverLetters
            .GroupBy(x => x.Name)
            .Select(x => x.First().Name)
            .ToListAsync();

        Prompts = await _context.Prompts
            .Select(x => x.Name)
            .ToListAsync();

        Companies = await _context.Companies
            .Select(x => x.Name)
            .ToListAsync();

        Connections = await _context.Connections
            .Select(x => x.Name)
            .ToListAsync();
    }

    private async Task<Company> ResolveCompanyAsync(string company)
    {
        var name = company.Trim();

        var lower = name.ToLower();
        var companyObj = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == lower);

        if (companyObj == null)
        {
            companyObj = new Models.Company { Name = name };
            _context.Companies.Add(companyObj);
        }

        return companyObj;
    }

    private async Task<Connection> ResolveConnectionAsync(string connection)
    {
        var name = connection.Trim();

        var lower = name.ToLower();
        var connectionObj = await _context.Connections
            .FirstOrDefaultAsync(c => c.Name.ToLower() == lower);

        if (connectionObj == null)
        {
            connectionObj = new Connection { Name = name };
            _context.Connections.Add(connectionObj);
        }

        return connectionObj;
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreatePromptInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();   
            return Page();
        }

        var company = await ResolveCompanyAsync(input.Company);

        var connectionId = (int?)null;
        if (!string.IsNullOrWhiteSpace(input.Connection))
        {
            var connection = await ResolveConnectionAsync(input.Connection);
            connectionId = connection.Id;
        }

        var application = new JobApplication
        {
            CompanyId = company.Id,
            Role = input.Role,
            Url = input.Url,
            Status = input.Status,
            Method = input.Method,
            ConnectionId = connectionId,
            Notes = input.Notes ?? ""
        };

        _context.Applications.Add(application);
        _context.SaveChanges();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm(Name = "UpdateInput")] UpdateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var application = _context.Applications.Find(input.Id);
        if (application == null)
        {
            return NotFound();
        }

        var company = await ResolveCompanyAsync(input.Company);

        var connectionId = (int?)null;
        if (!string.IsNullOrWhiteSpace(input.Connection))
        {
            var connection = await ResolveConnectionAsync(input.Connection);
            connectionId = connection.Id;
        }

        application.CompanyId = company.Id;
        application.Role = input.Role;
        application.Url = input.Url;
        application.Status = input.Status;
        application.Method = input.Method;
        application.DateApplied = input.DateApplied;
        application.FollowUpDate = input.FollowUpDate;
        application.ConnectionId = connectionId;
        application.Notes = input.Notes ?? "";
        application.JobDescription = input.JobDescription ?? "";

        _context.SaveChanges();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostContinuePipelineAsync(int id)
    {
        var application  = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        if ((int)application.Status >= 5)
            return RedirectToPage();

        application.Status += 1;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateResumeAsync(int id, string group)
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

        return RedirectToPage();
    }

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

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var application  = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        application.Status = ApplicationStatus.Archived;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound();

        _context.Applications.Remove(application);

        if (!string.IsNullOrWhiteSpace(application.CoverLetterPath))
            _file.Delete(application.CoverLetterPath);
        if (!string.IsNullOrWhiteSpace(application.ResumePath))
            _file.Delete(application.ResumePath);

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOpenResumeAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (string.IsNullOrWhiteSpace(application?.ResumePath) || !System.IO.File.Exists(application.ResumePath))
            return NotFound();

        _openFile.OpenFile(application.ResumePath);

        // stays on the current page
        return new NoContentResult(); 
    }

    public async Task<IActionResult> OnPostOpenCoverLetterAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (string.IsNullOrWhiteSpace(application?.CoverLetterPath) || !System.IO.File.Exists(application.CoverLetterPath))
            return NotFound();

        _openFile.OpenFile(application.CoverLetterPath);

        // stays on the current page
        return new NoContentResult(); 
    }
}
