using HackerFlow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackerFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly HackerFlowContext _context;

    public ApplicationsController(HackerFlowContext context)
    {
        _context = context;
    }

    [Route("generate-resume")]
    public async Task<IActionResult> GenerateResume()
    {
        return Ok();
    }

    [Route("generate-cover-letter")]
    public async Task<IActionResult> GenerateCoverLetter()
    {
        return Ok();
    }

    [Route("create-prompt")]
    public async Task<IActionResult> CreatePrompt()
    {
        return Ok();
    }

    [Route("create-application-package")]
    public async Task<IActionResult> CreateApplicationPacakge()
    {
        return Ok();
    }
}