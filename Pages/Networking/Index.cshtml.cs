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

    public async Task OnGetAsync()
    {
        await LoadProperties();
    }

    public async Task<IActionResult> OnPostCreateConnectionAsync([FromForm(Name = "CreateConnectionInput")] CreateConnectionInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        if (input.IntroducedById.HasValue &&
            !await _context.Connections.AnyAsync(c => c.Id == input.IntroducedById))
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
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var connection = await _context.Connections
            .Include(c => c.Companies)
            .FirstOrDefaultAsync(c => c.Id == input.Id);

        if (connection == null)
        {
            return NotFound();
        }

        // Can't be introduced by yourself, and the introducer has to exist
        if (input.IntroducedById.HasValue &&
            (input.IntroducedById == input.Id ||
             !await _context.Connections.AnyAsync(c => c.Id == input.IntroducedById)))
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
        {
            return NotFound();
        }

        _context.Connections.Remove(connection);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateCompanyAsync([FromForm(Name = "CreateCompanyInput")] CreateCompanyInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var company = new Models.Company
        {
            Name = input.Name,
            Website = input.Website,
            ReferralChance = input.ReferralChance,
            Description = input.Description
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateCompanyAsync([FromForm(Name = "UpdateCompanyInput")] UpdateCompanyInput input)
    {
        if (!ModelState.IsValid)
        {
            await LoadProperties();
            return Page();
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == input.Id);

        if (company == null)
        {
            return NotFound();
        }

        company.Name = input.Name;
        company.Website = input.Website;
        company.ReferralChance = input.ReferralChance;
        company.Description = input.Description;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCompanyAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound();
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}