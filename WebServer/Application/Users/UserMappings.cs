using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    public static class UserMappings
    {
        // 요약 정보로 변환 (User + UserProfile 필요)
        public static UserSummaryDto ToSummaryDto(this User u, UserProfile p) =>
            new(
                Id: u.Id,
                Account: u.Account,
                NickName: p.NickName,
                Level: p.Level,
                Gold: p.Gold,
                Cristal: p.Cristal,
                Status: u.Status,
                CreatedAt: u.CreatedAt,
                LastLoginAt: u.LastLoginAt
            );

        // 프로필 전용
        public static UserProfileDto ToProfileDto(this UserProfile p) =>
            new(
                Id: p.Id,
                UserId: p.UserId,
                NickName: p.NickName,
                Level: p.Level,
                Exp: p.Exp,
                Gold: p.Gold,
                Cristal: p.Cristal,
                IconId: p.IconId
            );

        // 세션 → 브리프
        public static SessionBriefDto ToBriefDto(this Session s) =>
            new(
                Id: s.Id,
                ExpiresAt: s.ExpiresAt,
                RefreshExpiresAt: s.RefreshExpiresAt,
                Revoked: s.Revoked
            );

        // 상세: 계정 + 프로필 (+세션들 옵션)
        public static UserDetailDto ToDetailDto(this User u, UserProfile p, IEnumerable<Session>? recentSessions = null)
        {
            var sessions = recentSessions?.Select(ToBriefDto).ToList();
            return new UserDetailDto(
                Id: u.Id,
                Account: u.Account,
                Status: u.Status,
                CreatedAt: u.CreatedAt,
                LastLoginAt: u.LastLoginAt,
                NickName: p.NickName,
                Level: p.Level,
                Exp: p.Exp,
                Gold: p.Gold,
                Cristal: p.Cristal,
                IconId: p.IconId,
                RecentSessions: sessions
            );
        }
    }
}
