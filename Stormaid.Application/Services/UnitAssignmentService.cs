using Stormaid.Application.DTOs.Request;
using Stormaid.Application.DTOs.Response;
using Stormaid.Application.Interfaces;
using Stormaid.Domain.Entities;

namespace Stormaid.Application.Services;

/// <summary>
/// Serviço de CRUD dos vínculos entre socorristas
/// e unidades de resgate.
/// </summary>
public sealed class UnitAssignmentService
{
    private readonly IUnitAssignmentRepository _assignments;
    private readonly IRescueUnitRepository _units;

    public UnitAssignmentService(
        IUnitAssignmentRepository assignments,
        IRescueUnitRepository units)
    {
        _assignments = assignments;
        _units = units;
    }

    public async Task<UnitAssignmentResponse?> CreateAsync(
        CreateUnitAssignmentRequest request,
        long regulatorUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var unit = await _units.GetByIdAsync(
            request.RescueUnitId,
            cancellationToken
        );

        if (unit is null)
        {
            return null;
        }

        var assignment = new UnitAssignment(
            request.RescueUnitId,
            request.ResponderUserId,
            regulatorUserId,
            DateTime.UtcNow
        );

        await _assignments.AddAsync(
            assignment,
            cancellationToken
        );

        return UnitAssignmentResponse.FromEntity(assignment);
    }

    public async Task<UnitAssignmentResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _assignments.GetByIdAsync(
            id,
            cancellationToken
        );

        return entity is null
            ? null
            : UnitAssignmentResponse.FromEntity(entity);
    }

    public async Task<PagedResponse<UnitAssignmentResponse>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var entities = await _assignments.GetPagedAsync(
            page,
            pageSize,
            cancellationToken
        );

        var totalItems = await _assignments.CountAsync(
            cancellationToken
        );

        var items = entities
            .Select(UnitAssignmentResponse.FromEntity)
            .ToList();

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)pageSize
        );

        return new PagedResponse<UnitAssignmentResponse>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages
        );
    }

    public async Task<UnitAssignmentResponse?> UpdateAsync(
        long id,
        UpdateUnitAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignment = await _assignments.GetTrackedByIdAsync(
            id,
            cancellationToken
        );

        if (assignment is null)
        {
            return null;
        }

        assignment.ChangeResponder(request.ResponderUserId);

        await _assignments.UpdateAsync(
            assignment,
            cancellationToken
        );

        return UnitAssignmentResponse.FromEntity(assignment);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _assignments.GetTrackedByIdAsync(
            id,
            cancellationToken
        );

        if (assignment is null)
        {
            return false;
        }

        await _assignments.RemoveAsync(
            assignment,
            cancellationToken
        );

        return true;
    }
}
