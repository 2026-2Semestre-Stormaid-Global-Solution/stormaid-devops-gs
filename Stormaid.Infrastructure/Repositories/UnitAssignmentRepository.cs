using Microsoft.EntityFrameworkCore;
using Stormaid.Application.Interfaces;
using Stormaid.Domain.Entities;
using Stormaid.Infrastructure.Data;

namespace Stormaid.Infrastructure.Repositories;

public class UnitAssignmentRepository : IUnitAssignmentRepository
{
    private readonly StormaidDbContext _context;

    public UnitAssignmentRepository(StormaidDbContext context)
    {
        _context = context;
    }

    public async Task<UnitAssignment?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UnitAssignment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );
    }

    public async Task<UnitAssignment?> GetTrackedByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UnitAssignment>()
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<UnitAssignment>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

        if (pageSize < 1 || pageSize > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var offset = (long)(page - 1) * pageSize;

        if (offset > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(page));

        return await _context.Set<UnitAssignment>()
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UnitAssignment>()
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<UnitAssignment>()
            .AddAsync(assignment, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        UnitAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        _context.Set<UnitAssignment>().Remove(assignment);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
