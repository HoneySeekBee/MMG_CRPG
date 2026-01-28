\# AI Usage Rules (Claude Code + MCP)

<!-- Project documentation -->

\## 허용되는 작업

\- 코드 탐색/리뷰/리팩토링 제안

\- 테스트 코드 생성

\- 문서화 (README, API 문서)

\- Unity-Server DTO/에러코드 정리



\## 금지되는 작업

\- Production DB/Redis/AWS 직접 접근

\- 배포 강제 실행(사람 승인 없이)

\- secrets 파일 생성/수정/출력



\## 작업 기본 루틴

1\) 작은 단위로 변경

2\) 변경 후 빌드/테스트 실행

3\) 커밋

4\) PR 생성 후 CI 통과 확인

