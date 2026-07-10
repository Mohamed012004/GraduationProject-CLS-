using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Controllers;

public static class ControllerResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ServiceResult<T> result, ControllerBase controller)
    {
        return result.Status switch
        {
            ServiceStatus.Ok => controller.Ok(result.Data),
            ServiceStatus.Created => controller.CreatedAtAction(result.ActionName, result.RouteValues, result.Data),
            ServiceStatus.NoContent => controller.NoContent(),
            ServiceStatus.NotFound => controller.NotFound(result.Message),
            ServiceStatus.Forbidden => controller.Forbid(),
            ServiceStatus.Unauthorized => controller.Unauthorized(result.Message),
            ServiceStatus.BadRequest => controller.BadRequest(result.Message),
            _ => controller.StatusCode(500)
        };
    }

    public static IActionResult ToActionResult(this ServiceResult result, ControllerBase controller)
    {
        return result.Status switch
        {
            ServiceStatus.Ok => controller.Ok(),
            ServiceStatus.NoContent => controller.NoContent(),
            ServiceStatus.NotFound => controller.NotFound(result.Message),
            ServiceStatus.Forbidden => controller.Forbid(),
            ServiceStatus.Unauthorized => controller.Unauthorized(result.Message),
            ServiceStatus.BadRequest => controller.BadRequest(result.Message),
            _ => controller.StatusCode(500)
        };
    }
}

