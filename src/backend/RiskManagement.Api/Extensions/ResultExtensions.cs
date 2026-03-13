using Microsoft.AspNetCore.Mvc;
using RiskManagement.Application.Common;
using RiskManagement.Application.DTOs;

namespace RiskManagement.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        var error = result.Error!;

        if (error.ValidationErrors is not null)
            return new BadRequestObjectResult(new ValidationErrorResponse
            {
                Errors = error.ValidationErrors,
                Values = error.Values
            });

        return error.StatusCode switch
        {
            404 => new NotFoundObjectResult(new { error = error.Message }),
            403 => new ObjectResult(new { error = error.Message }) { StatusCode = 403 },
            _ => new BadRequestObjectResult(new { error = error.Message })
        };
    }
}