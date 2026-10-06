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

        var introducedByConnection = await _context.Connections
            .FirstOrDefaultAsync(c => c.Id == input.IntroducedById);
        if (input.IntroducedById.HasValue && introducedByConnection == null)
        {
            await LoadProperties();
            return Page();
        }

        var companies = new List<Models.Company>();
        foreach (var companyName in input.CompanyNames)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == companyName);

            if (company == null)
            {
                company = new Models.Company
                {
                    Name = companyName
                };

                _context.Companies.Add(company);
            }
                
            companies.Add(company);
        }

        var connection = new Models.Connection
        {
            Name = input.Name,
            Companies = companies,
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
}