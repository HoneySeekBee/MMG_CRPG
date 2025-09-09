using Application.Users;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> ExistsByAccountAsync(string account, CancellationToken ct);
        Task<User?> FindByAccountAsync(string account, CancellationToken ct);
        Task<User?> GetByIdAsync(int userId, CancellationToken ct);

        Task AddAsync(User user, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
    public interface IUserQueryRepository
    {
        /// <summary>계정+프로필 조인 후 페이징 목록 반환</summary>
        Task<(IReadOnlyList<(User User, UserProfile Profile)> Rows, int TotalCount)>
      GetPagedAsync(UserListQuery query, CancellationToken ct);

        /// <summary>상세 조회를 위해 계정+프로필(+선택 세션 브리프) 로드</summary>
        Task<(User? User, UserProfile? Profile)> GetAggregateAsync(int userId, CancellationToken ct);
    }
}
