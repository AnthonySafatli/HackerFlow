using HackerFlow.Data;
using HackerFlow.InputModels.Networking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Pages.Networking;

public class IndexModel : PageModel
{
    private readonly HackerFlowContext _context;

    public IndexModel(HackerFlowContext context)
    {
        _context = context;
    }

    public List<Models.Connection> Connections { get; private set; } = [];
    public List<Models.Company> Companies { get; private set; } = [];

    private async Task LoadProperties()
    {
        Connections = await _context.Connections
            .Include(c => c.IntroducedBy)
            .Include(c => c.IntroducedTo)
            .Include(c => c.Companies)
            .OrderBy(c => c.Name)
            .ToListAsync();

        Companies = await _context.Companies
            .Include(c => c.Connections)
            .Include(c => c.Applications)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    private async Task<List<Models.Company>> ResolveCompaniesAsync(IEnumerable<string>? names)
    {
        var companies = new List<Models.Company>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in names ?? [])
        {
            var name = raw?.Trim();
            if (string.IsNullOrEmpty(name) || !seen.Add(name))
            {
                continue;
            }

            var lower = name.ToLower();
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name.ToLower() == lower);

            if (company == null)
            {
                company = new Models.Company { Name = name };
                _context.Companies.Add(company);
            }

            companies.Add(company);
        }

        return companies;
    }

    private Task<bool> CompanyNameTakenAsync(string name, int excludeId = 0)
    {
        var lower = name.Trim().ToLower();
        return _context.Companies.AnyAsync(c => c.Id != excludeId && c.Name.ToLower() == lower);
    }

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCreateConnectionAsync([FromForm(Name = "CreateConnectionInput")] CreateConnectionInput input)
    {
        if (!await _context.Connections.AnyAsync(c => c.Id == input.IntroducedById))
            ModelState.AddModelError(nameof(input.IntroducedById), "That introducer doesn't exist.");
        
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var connection = new Models.Connection
        {
            Name = input.Name,
            Companies = await ResolveCompaniesAsync(input.CompanyNames),
            Closeness = input.Closeness,
            Level = input.Level,
            IntroducedById = input.IntroducedById,
            ConnectedAt = input.ConnectedAt,
            LastContactAt = input.LastContactAt,
            CanIntroduceMe = input.CanIntroduceMe
        };

        _context.Connections.Add(connection);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateConnectionAsync([FromForm(Name = "UpdateConnectionInput")] UpdateConnectionInput input)
    {
        if (input.IntroducedById == input.Id)
            ModelState.AddModelError(nameof(input.IntroducedById), "A connection can't introduce itself.");
        else if (!await _context.Connections.AnyAsync(c => c.Id == input.IntroducedById))
            ModelState.AddModelError(nameof(input.IntroducedById), "That introducer doesn't exist.");
        
        var connection = await _context.Connections
            .Include(c => c.Companies)
            .FirstOrDefaultAsync(c => c.Id == input.Id);
        if (connection == null)
            ModelState.AddModelError(nameof(input.Id), "This connection doesnt exist.");

        if (!ModelState.IsValid || connection == null)
        {
            await LoadProperties();
            return Page();
        }

        connection.Name = input.Name;
        connection.Closeness = input.Closeness;
        connection.Level = input.Level;
        connection.CanIntroduceMe = input.CanIntroduceMe;
        connection.IntroducedById = input.IntroducedById;
        connection.ConnectedAt = input.ConnectedAt;
        connection.LastContactAt = input.LastContactAt;

        connection.Companies.Clear();
        connection.Companies.AddRange(await ResolveCompaniesAsync(input.CompanyNames));

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteConnectionAsync(int id)
    {
        var connection = await _context.Connections
            .Include(c => c.IntroducedTo)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (connection == null)
            ModelState.AddModelError("", "This connection doesnt exist.");

        if (!ModelState.IsValid || connection == null)
        {
            await LoadProperties();
            return Page();
        }

        _context.Connections.Remove(connection);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateCompanyAsync([FromForm(Name = "CreateCompanyInput")] CreateCompanyInput input)
    {
        if (await CompanyNameTakenAsync(input.Name))
            ModelState.AddModelError(nameof(input.Name), "A company with that name already exists.");

        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var company = new Models.Company
        {
            Name = input.Name,
            Website = input.Website ?? "",
            ReferralChance = input.ReferralChance,
            Description = input.Description ?? ""
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return RedirectToPage(null, null, "companies-pane");
    }

    public async Task<IActionResult> OnPostUpdateCompanyAsync([FromForm(Name = "UpdateCompanyInput")] UpdateCompanyInput input)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == input.Id);
        if (company == null)
            ModelState.AddModelError("", "This company doesnt exist.");

        if (await CompanyNameTakenAsync(input.Name, input.Id))
            ModelState.AddModelError(nameof(input.Name), "A company with that name already exists.");

        if (!ModelState.IsValid || company == null)
        {
            await LoadProperties();
            return Page();
        }

        company.Name = input.Name;
        company.Website = input.Website ?? "";
        company.ReferralChance = input.ReferralChance;
        company.Description = input.Description ?? "";

        await _context.SaveChangesAsync();

        return RedirectToPage(null, null, "companies-pane");
    }

    public async Task<IActionResult> OnPostDeleteCompanyAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null)
            ModelState.AddModelError("", "This company doesnt exist.");

        if (await _context.Applications.AnyAsync(a => a.CompanyId == id) ||
            await _context.Connections.AnyAsync(c => c.Companies.Any(x => x.Id == id)))
            ModelState.AddModelError("", "Company is still in use.");

        if (!ModelState.IsValid || company == null)
        {
            await LoadProperties();
            return Page();
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return RedirectToPage(null, null, "companies-pane");
    }
}