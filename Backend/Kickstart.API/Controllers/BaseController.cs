using Kickstart.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Kickstart.Application.Common.Results;

namespace Kickstart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(new { success = true, data = result.Value });
        }

        return BadRequest(new { success = false, error = result.Error });
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true, message = "Operation completed successfully" });

        return BadRequest(new { success = false, error = result.Error });
    }

    protected IActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    items = result.Items,
                    totalCount = result.TotalItems,
                    totalPages = result.TotalPages,
                    pageNumber = result.PageNumber
                }
            });
        }

        return BadRequest(new { success = false, error = result.Error });
    }

    protected string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    protected string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
