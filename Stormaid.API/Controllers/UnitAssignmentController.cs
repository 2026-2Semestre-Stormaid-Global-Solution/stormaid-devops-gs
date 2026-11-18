using System.Security.Claims;
using Stormaid.Application.DTOs.Request;
using Stormaid.Application.DTOs.Response;
using Stormaid.Application.Services;

namespace Stormaid.API.Controllers;

/// <summary>
/// Rotas de CRUD dos vínculos entre socorristas e unidades de resgate.
/// </summary>
public static class UnitAssignmentController
{
    public static IEndpointRouteBuilder MapUnitAssignmentRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/unit-assignments")
            .WithTags("Vínculos de Unidade")
            .RequireAuthorization(policy => policy.RequireRole("REGULATOR"))
            .RequireRateLimiting("InstitutionalApi");

        group.MapPost("", CreateAsync)
            .WithName("CreateUnitAssignment")
            .WithSummary("Criar vínculo de socorrista com unidade")
            .Produces<UnitAssignmentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:long}", GetByIdAsync)
            .WithName("GetUnitAssignmentById")
            .WithSummary("Consultar vínculo por ID")
            .Produces<UnitAssignmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("", GetPagedAsync)
            .WithName("GetUnitAssignments")
            .WithSummary("Listar vínculos com paginação")
            .Produces<PagedResponse<UnitAssignmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:long}", UpdateAsync)
            .WithName("UpdateUnitAssignment")
            .WithSummary("Trocar o socorrista de um vínculo ativo")
            .Produces<UnitAssignmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:long}", DeleteAsync)
            .WithName("DeleteUnitAssignment")
            .WithSummary("Excluir vínculo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static IResult InvalidId()
    {
        return Results.Problem(
            title: "Identificador inválido.",
            detail: "O identificador deve ser maior que zero.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    private static IResult NotFound()
    {
        return Results.Problem(
            title: "Registro não encontrado.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateAsync(
        CreateUnitAssignmentRequest request,
        ClaimsPrincipal user,
        UnitAssignmentService service,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst("userId")?.Value;

        if (!long.TryParse(userIdClaim, out var regulatorUserId) || regulatorUserId <= 0)
        {
            return Results.Problem(
                title: "Identidade inválida.",
                detail: "O token não contém um userId numérico válido.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            var created = await service.CreateAsync(
                request,
                regulatorUserId,
                cancellationToken
            );

            if (created is null)
            {
                return Results.Problem(
                    title: "Unidade de resgate não encontrada.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.CreatedAtRoute(
                routeName: "GetUnitAssignmentById",
                routeValues: new { id = created.Id },
                value: created
            );
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                title: "Dados inválidos.",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> GetByIdAsync(
        long id,
        UnitAssignmentService service,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return InvalidId();
        }

        var assignment = await service.GetByIdAsync(id, cancellationToken);

        return assignment is null ? NotFound() : Results.Ok(assignment);
    }

    private static async Task<IResult> GetPagedAsync(
        int? page,
        int? pageSize,
        UnitAssignmentService service,
        CancellationToken cancellationToken)
    {
        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 10;

        if (currentPage < 1)
        {
            return Results.Problem(
                title: "Página inválida.",
                detail: "A página deve ser maior ou igual a 1.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (currentPageSize is < 1 or > 100)
        {
            return Results.Problem(
                title: "Tamanho de página inválido.",
                detail: "A quantidade de registros deve estar entre 1 e 100.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await service.GetPagedAsync(
            currentPage,
            currentPageSize,
            cancellationToken
        );

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateAsync(
        long id,
        UpdateUnitAssignmentRequest request,
        UnitAssignmentService service,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return InvalidId();
        }

        try
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);

            return updated is null ? NotFound() : Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                title: "Vínculo não pode ser alterado.",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                title: "Dados inválidos.",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> DeleteAsync(
        long id,
        UnitAssignmentService service,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return InvalidId();
        }

        var deleted = await service.DeleteAsync(id, cancellationToken);

        return deleted ? Results.NoContent() : NotFound();
    }
}
