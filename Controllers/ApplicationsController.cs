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

    [Route("{id}/generate-resume")]
    public async Task<IActionResult> GenerateResume(int id, string group)
    {
        return Ok();
    }

    [Route("{id}/generate-cover-letter")]
    public async Task<IActionResult> GenerateCoverLetter(int id, string group)
    {
        return Ok();
    }
    
    [HttpPost("{id}/create-prompt")]
    public async Task<IActionResult> CreatePrompt(int id)
    {
        return Ok(new { prompt = "Test" });
    }

    [Route("{id}/create-application-package")]
    public async Task<IActionResult> CreateApplicationPacakge(int id)
    {
        return Ok();
    }
}