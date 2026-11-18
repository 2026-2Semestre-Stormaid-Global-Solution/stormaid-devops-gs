using Stormaid.Domain.Entities;

namespace Stormaid.Application.DTOs.Response;

public sealed record UnitAssignmentResponse(
    long Id,
    long RescueUnitId,
    long ResponderUserId,
    long AssignedByRegulatorUserId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    DateTime CreatedAt
)
{
    public static UnitAssignmentResponse FromEntity(UnitAssignment entity)
    {
        return new UnitAssignmentResponse(
            entity.Id,
            entity.RescueUnitId,
            entity.ResponderUserId,
            entity.AssignedByRegulatorUserId,
            entity.Status.ToString(),
            entity.StartedAt,
            entity.EndedAt,
            entity.CreatedAt
        );
    }
}
