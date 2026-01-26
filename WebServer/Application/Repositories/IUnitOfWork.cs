using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// 트랜잭션 내에서 작업 실행. 실패 시 자동 롤백.
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);

        /// <summary>
        /// 트랜잭션 내에서 작업 실행 (반환값 없음). 실패 시 자동 롤백.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default);
    }
}
