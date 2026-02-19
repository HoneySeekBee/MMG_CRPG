using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// dotnet ef migrations add / update 시 사용되는 팩토리.
    /// 실제 MySQL 접속 없이 마이그레이션 코드를 생성할 수 있도록
    /// ServerVersion을 명시적으로 지정합니다.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GameDBContext>
    {
        public GameDBContext CreateDbContext(string[] args)
        {
            // design-time 전용 더미 연결 문자열 (실제 접속하지 않음)
            var cs = "Server=localhost;Port=3306;Database=MMG_CRPG;User=root;Password=temp";

            var builder = new DbContextOptionsBuilder<GameDBContext>();
            builder.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36)));

            return new GameDBContext(builder.Options);
        }
    }
}
