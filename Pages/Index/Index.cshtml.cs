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
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync();

        Connections = await _context.Connections
            .OrderBy(x => x.Name)
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
            companyObj = new Company { Name = name };
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

    public async Task<IActionResult> OnPostCreateAsync([FromForm(Name = "CreateInput")] CreateApplicationInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();   
            return Page();
        }

        var company = await ResolveCompanyAsync(input.Company);

        var connection = null as Connection;
        if (!string.IsNullOrWhiteSpace(input.Connection))
            connection = await ResolveConnectionAsync(input.Connection);

        var application = new JobApplication
        {
            Company = company,
            Role = input.Role,
            Url = input.Url,
            Status = input.Status,
            Method = input.Method,
            Connection = connection,
            Notes = input.Notes ?? ""
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromForm(Name = "UpdateInput")] UpdateApplicationInput input)
    {
        var application = await _context.Applications.FindAsync(input.Id);
        if (application == null)
            ModelState.AddModelError("", "Application not found.");

        if (!ModelState.IsValid || application == null) 
        {
            await LoadProperties();
            return Page();
        }

        var company = await ResolveCompanyAsync(input.Company);

        var connection = null as Connection;
        if (!string.IsNullOrWhiteSpace(input.Connection))
            connection = await ResolveConnectionAsync(input.Connection);

        application.Company = company;
        application.Role = input.Role;
        application.Url = input.Url;
        application.Status = input.Status;
        application.Method = input.Method;
        application.DateApplied = input.DateApplied;
        application.FollowUpDate = input.FollowUpDate;
        application.Connection = connection;
        application.Notes = input.Notes ?? "";
        application.JobDescription = input.JobDescription ?? "";

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostContinuePipelineAsync(int id)
    {
        var application  = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        if (application.Status >= ApplicationStatus.Accepted)
            return RedirectToPage();

        application.Status += 1;
        if (application.Status == ApplicationStatus.Applied)
            application.DateApplied = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateResumeAsync(int id, string group)
    {
        var application = await _context.Applications
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(application.ResumePath))
        {
            ModelState.AddModelError("", "Resume already generated for this application.");
            await LoadProperties();
            return Page();
        }

        var resume = await _context.Resumes
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (resume == null)
        {
            ModelState.AddModelError("", "Resume not found.");
            await LoadProperties();
            return Page();
        }

        var resumeData = await _file.ReadAsync(resume.FilePath);
        if (resumeData == null)
        {
            ModelState.AddModelError("", "Resume file not found.");
            await LoadProperties();
            return Page();
        }

        var ext = Path.GetExtension(resume.FilePath).ToLowerInvariant();
        var resumeFolder = await _settings.GetSetting(AppSetting.GeneratedResumeFolder);
        var resumeName = $"{application.Company.Name}_{application.Role}_Resume_{Guid.NewGuid()}{ext}";

        var resumePath = await _file.SaveAsync(resumeFolder, resumeName, resumeData);
        if (resumePath == null) 
        {
            ModelState.AddModelError("", "Failed to save resume.");
            await LoadProperties();
            return Page();
        }

        application.ResumePath = resumePath;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateCoverLetter(int id, string group)
    {
        var application = await _context.Applications
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(application.CoverLetterPath))
        {
            ModelState.AddModelError("", "Cover letter already generated for this application.");
            await LoadProperties();
            return Page();
        }

        var coverLetter = await _context.CoverLetters
            .Where(x => x.Name == group)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();
        if (coverLetter == null)
        {
            ModelState.AddModelError("", "Cover letter not found.");
            await LoadProperties();
            return Page();
        }

        var coverLetterData = await _file.ReadAsync(coverLetter.FilePath);
        if (coverLetterData == null)
        {
            ModelState.AddModelError("", "Cover letter file not found.");
            await LoadProperties();
            return Page();
        }

        var ext = Path.GetExtension(coverLetter.FilePath).ToLowerInvariant();
        var coverLetterFolder = await _settings.GetSetting(AppSetting.GeneratedCoverLetterFolder);
        var coverLetterName = $"{application.Company.Name}_{application.Role}_CoverLetter_{Guid.NewGuid()}{ext}";

        var coverLetterPath = await _file.SaveAsync(coverLetterFolder, coverLetterName, coverLetterData);
        if (coverLetterPath == null) 
        {
            ModelState.AddModelError("", "Failed to save cover letter.");
            await LoadProperties();
            return Page();
        }

        application.CoverLetterPath = coverLetterPath;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var application  = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        application.Status = ApplicationStatus.Archived;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        _context.Applications.Remove(application);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(application.CoverLetterPath))
            _file.Delete(application.CoverLetterPath);
        if (!string.IsNullOrWhiteSpace(application.ResumePath))
            _file.Delete(application.ResumePath);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOpenResumeAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(application.ResumePath) || !System.IO.File.Exists(application.ResumePath))
        {
            ModelState.AddModelError("", "Resume file not found.");
            await LoadProperties();
            return Page();
        }

        _openFile.OpenFile(application.ResumePath);

        // stays on the current page
        return new NoContentResult(); 
    }

    public async Task<IActionResult> OnPostOpenCoverLetterAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
        {
            ModelState.AddModelError("", "Application not found.");
            await LoadProperties();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(application.CoverLetterPath) || !System.IO.File.Exists(application.CoverLetterPath))
        {
            ModelState.AddModelError("", "Cover letter file not found.");
            await LoadProperties();
            return Page();
        }

        _openFile.OpenFile(application.CoverLetterPath);

        // stays on the current page
        return new NoContentResult(); 
    }
}
