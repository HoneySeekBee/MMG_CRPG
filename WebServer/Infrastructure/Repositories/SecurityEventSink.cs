using Application.Contents.Stages;
using Application.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class SecurityEventSink : ISecurityEventSink
    {
        private readonly ISecurityEventRepository _repo;
        private readonly GameDBContext _db;
        private readonly IClock _clock;
        public SecurityEventSink(ISecurityEventRepository repo, GameDBContext db, IClock clock)
        {
            _repo = repo;
            _db = db;
            _clock = clock;
        }

        public async Task LogAsync(string type, int? userId, object meta, CancellationToken ct)
        {
            // string → enum 변환
            var evtType = Enum.TryParse<SecurityEventType>(type, ignoreCase: true, out var t)
                ? t : SecurityEventType.LoginSuccess;

            // jsonb 컬럼용 직렬화
            var metaJson = JsonSerializer.Serialize(meta);

            // 팩토리 사용 (private ctor / setter 보호)
            var e = SecurityEvent.Create(
                type: evtType,
                when: _clock.UtcNow,
                userId: userId,
                metaJson: metaJson
            );

            await _repo.AddAsync(e, ct);

            // 리포에 SaveAsync가 없으므로 DbContext로 커밋
            await _db.SaveChangesAsync(ct);
        }
    }
}
