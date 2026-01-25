\# AI Security Rules (Claude/MCP)



\## 절대 공유 금지(Secrets)

\- AWS Access Key / Secret Key

\- EC2 SSH Key (\*.pem)

\- PostgreSQL Connection String / Password

\- Redis Password

\- JWT Secret / Token signing key

\- GitHub Actions secrets 값

\- Production appsettings / env files



\## 레포에서 secrets가 있을 수 있는 위치

\- .env, .env.\*

\- appsettings.Production.json

\- secrets.json

\- \*.pem / \*.pfx / \*.key

\- GitHub Actions workflow에서 참조되는 secrets.\*



\## AI(MCP/Claude) 사용 규칙

\- Production 리소스에 직접 접속/조작 금지

\- 배포/인프라 변경은 PR + 리뷰 + CI 통과 후 진행

\- 로그/DB 덤프 등 민감 데이터는 그대로 붙여넣지 말 것 (마스킹 후 공유)



\## 유출 의심 시 대응

1\) 즉시 키/비밀번호 로테이션

2\) GitHub Secrets 재발급 및 교체

3\) EC2 접근 키 교체

4\) DB/Redis 비밀번호 변경

5\) 배포 파이프라인 점검



