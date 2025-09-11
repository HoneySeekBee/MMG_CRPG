using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiyCurrency = Domain.Entities.Currency;
using EntitiyUserCurrency = Domain.Entities.UserCurrency;

namespace Application.UserCurrency
{
    public interface ICurrencyRepository
    {
        // 목록 조회 (운영툴/클라 공용)
        Task<IReadOnlyList<Domain.Entities.Currency>> GetAllAsync(CancellationToken ct);

        // 단건 조회
        Task<Domain.Entities.Currency?> GetByIdAsync(short id, CancellationToken ct);

        // Code로 조회 (지급/차감/중복검사에 필요)
        Task<Domain.Entities.Currency?> FindByCodeAsync(string code, CancellationToken ct);

        // 추가/수정은 EF 변경 추적에 맡기고 Save로 커밋
        Task AddAsync(Domain.Entities.Currency row, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
    public interface IUserCurrencyRepository
    {
        Task<EntitiyUserCurrency?> GetAsync(int userId, short currencyId, CancellationToken ct);
        Task<List<EntitiyUserCurrency>> GetByUserAsync(int userId, CancellationToken ct);
        Task AddAsync(EntitiyUserCurrency row, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
        Task InitializeForUserAsync(int userId, CancellationToken ct);
        Task GrantAsync(int userId, string code, long amount, CancellationToken ct);
    }
}
