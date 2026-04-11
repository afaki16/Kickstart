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
        // Reverse proxy / load balancer header
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip == null) return "Unknown";

        // IPv6 loopback (::1) → localhost
        if (ip.Equals(System.Net.IPAddress.IPv6Loopback))
            return "127.0.0.1";

        // IPv4-mapped IPv6 (::ffff:x.x.x.x) → x.x.x.x
        if (ip.IsIPv4MappedToIPv6)
            return ip.MapToIPv4().ToString();

        return ip.ToString();
    }

    protected string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
