using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IElementRepository
    {
        Task<Element?> GetByIdAsync(int id, CancellationToken ct);
        Task<Element?> GetByKeyAsync(string key, CancellationToken ct);
        Task<bool> KeyExistsAsync(string key, CancellationToken ct);
        Task AddAsync(Element entity, CancellationToken ct);
        Task RemoveAsync(Element entity, CancellationToken ct);

        Task<IReadOnlyList<Element>> ListAsync(
            bool? isActive, string? search, int skip, int take,
            CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    } 
}