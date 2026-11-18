using Stormaid.Domain.Entities;

namespace Stormaid.Application.Interfaces;

public interface IUnitAssignmentRepository
{
    Task<UnitAssignment?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<UnitAssignment?> GetTrackedByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<UnitAssignment>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default
    );
}
