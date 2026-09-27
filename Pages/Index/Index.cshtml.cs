using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerFlow.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackerFlow.Pages.Index;

public class IndexModel : PageModel
{
    public List<JobApplication> Applications { get; private set; } = [];

    public void OnGet()
    {
        Applications =
        [
            new JobApplication
            {
                Id = 1,
                Company = "Shopify",
                Role = "Software Developer",
                Url = "https://www.shopify.com/careers",
                DateApplied = new DateTime(2026, 9, 18),
                Method = "Company site",
                Status = "Applied",
                Contact = "Jane Smith",
                Notes = "Follow up next week"
            },
            new JobApplication
            {
                Id = 2,
                Company = "Microsoft",
                Role = "Software Engineer",
                Url = "https://careers.microsoft.com",
                DateApplied = new DateTime(2026, 9, 20),
                Method = "Company site",
                Status = "Applied",
                Contact = "Alex Johnson",
                Notes = "Referred by former colleague"
            },
            new JobApplication
            {
                Id = 3,
                Company = "Amazon",
                Role = "Backend Developer",
                Url = "https://www.amazon.jobs",
                DateApplied = new DateTime(2026, 9, 15),
                Method = "LinkedIn",
                Status = "Interviewing",
                Contact = "Sarah Wilson",
                Notes = "Technical interview scheduled"
            },
            new JobApplication
            {
                Id = 4,
                Company = "Google",
                Role = "Full Stack Engineer",
                Url = "https://www.google.com/about/careers",
                DateApplied = new DateTime(2026, 9, 12),
                Method = "Company site",
                Status = "Negotiating",
                Contact = "Michael Brown",
                Notes = "Waiting on compensation package"
            },
            new JobApplication
            {
                Id = 5,
                Company = "Nova Scotia Power",
                Role = "Application Developer",
                Url = "https://www.nspower.ca",
                DateApplied = null,
                Method = "Company site",
                Status = "Bookmarked",
                Contact = "",
                Notes = "Interesting local opportunity"
            },
            new JobApplication
            {
                Id = 6,
                Company = "Clio",
                Role = "Junior Software Developer",
                Url = "https://www.clio.com/careers",
                DateApplied = null,
                Method = "Company site",
                Status = "Applying",
                Contact = "",
                Notes = "Need to finish tailored resume"
            }
        ];
    }

    public IActionResult OnPostCreate(JobApplication Application)
    {
        // Database/file-system logic will go here.

        return RedirectToPage();
    }

    public IActionResult OnPostUpdate(JobApplication Application)
    {
        // Database/file-system logic will go here.

        return RedirectToPage();
    }

}
