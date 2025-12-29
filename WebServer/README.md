# WebServer (ASP.NET Core Web API) - MMG_CRPG

MMG_CRPG의 게임 서버입니다.  
서버 권한(Server-authoritative) 구조를 기반으로 전투, 보상, 데이터 무결성을 서버가 책임집니다.  
전투는 Tick Loop로 시뮬레이션되며 결과는 Snapshot + Events 형태로 클라이언트에 전달됩니다.



## 기술 스택
- ASP.NET Core Web API
- Clean Architecture (WebServer / Application / Domain / Infrastructure)
- Protobuf (Proto3, Binary)
- JWT 인증 + RefreshToken
- PostgreSQL (EF Core)
- Redis (Session / Cache)
- AWS EC2 단일 인스턴스 배포
- 정적 리소스: S3 + CloudFront

## 핵심 설계 포인트

### 1. Server-authoritative 구조
- 모든 전투 판정, 상태 변경, 보상 계산은 서버에서 수행
- 클라이언트는 입력(Command) 전달 및 결과(Snapshot/Event) 재생에 집중
- 서버가 상태의 정답을 소유하여 데이터 위변조 및 치트 방지에 유리

### 2. Tick 기반 전투 시뮬레이션

#### Tick Loop 기반 전투 처리 흐름

<img width="3766" height="1730" alt="서버_전투처리구조" src="https://github.com/user-attachments/assets/aec6d4cf-09ed-4e25-beb5-b8c2a93bc1e7" />

서버는 CombatRuntimeState를 authoritative 상태로 유지하며  
Tick Loop를 통해 모든 전투 판정과 상태 변화를 처리합니다.

각 Tick 결과는
- Snapshot: 현재 전투 상태 동기화용 데이터
- Events: 연출 및 로그용 이벤트

로 분리되어 클라이언트에 전달됩니다.

Snapshot은 상태 동기화를, Events는 연출과 로그를 담당하여  
동기화 정확성과 클라이언트 표현 자유도를 분리합니다.

- 기본 Tick 간격: 100ms
- 누락 Tick 보정(Catch-up): 최대 5 Tick
- dt 상한: 200ms (폭주 방지)
- TimeScale을 통한 배속 지원 (1x / 1.5x / 2x)

#### Tick 처리 개요
- 클라이언트 요청 Tick이 서버 Tick보다 앞서면 누락 Tick을 서버에서 보정
- 각 Tick마다 전투 시스템을 순차 실행

### 3. Snapshot + Event 응답 모델
- Snapshot
  - 특정 시점의 전투 전체 상태
  - 클라이언트 동기화 기준 데이터
- Events
  - 공격, 스킬, 사망, 버프 등 연출용 이벤트
  - 클라이언트는 Event 기반으로 연출 재생

### 4. 런타임 전투 상태 관리
#### 전투 런타임 상태 및 이벤트 로그 관리

<img width="4005" height="1766" alt="서버_상태관리 데이터흐름" src="https://github.com/user-attachments/assets/65194b68-5c7a-41e0-9899-02637b5e120f" />

전투 중 상태는 서버 메모리에서 관리되며,  
Tick 처리 중 발생한 이벤트는 DB에 Append-only 방식으로 저장됩니다.

이를 통해 전투 리플레이, 로그 분석, 운영 이슈 추적이 가능하도록 설계했습니다.

- 전투 진행 중 상태는 서버 메모리에서 관리
- ConcurrentDictionary<long, CombatRuntimeState> 사용
- 상태 접근은 lock(SyncRoot)으로 동기화
- 전투 종료 시 런타임 상태 제거하여 메모리 누수 방지

### 5. 이벤트 로그 저장
- CombatLogEvent는 DB에 Append-only 방식으로 저장
- Tick 처리 중 발생한 이벤트 및 커맨드 입력 이벤트 기록
- 전투 재현, 분석, 운영 로그 확인 용도로 활용
- Redis에는 전투 로그를 저장하지 않음

### 6. Redis 사용 범위
- RefreshToken 세션 저장
- 캐시 데이터
- 서버 상태 트래킹 / 분산락

## 아키텍처 구조 (Clean Architecture) 

### 전체 서버 아키텍처

<img width="3866" height="1768" alt="서버_서버클라이언트통신구조" src="https://github.com/user-attachments/assets/4c9178e6-fe49-467e-9c9c-d556594767b1" />

ASP.NET Core Web API를 Entry Point로 하여  
Application 계층에서 유스케이스 흐름을 조합하고,  
Domain 계층에서 전투 규칙과 상태를 관리하며,  
Infrastructure 계층에서 DB 및 Redis와 같은 외부 의존성을 처리합니다.

> WebServer, Application, Domain, Infrastructure 계층을 분리하여  
> 전투 규칙과 비즈니스 로직을 외부 인터페이스(API, DB)로부터 독립적으로 유지하도록 설계했습니다.

- WebServer  
  - Protobuf / JSON Controller  
  - 인증, 요청 검증, Contract 매핑

- Application  
  - 유스케이스 흐름 제어  
  - 전투 시작 / Tick 처리 / 전투 종료 / 보상 지급

- Domain  
  - 전투 규칙 및 상태 모델  
  - Tick 기반 전투 엔진, Snapshot / Event 생성

- Infrastructure  
  - PostgreSQL (EF Core)  
  - Redis (Session / Cache)  
  - 외부 서비스 연동

### 요구 사항
- .NET SDK 8.0.x
- PostgreSQL
- Redis

### 환경 설정
- appsettings.json
- appsettings.Development.json (Git 추적 제외 권장)

### 필수 설정 항목 예시
- ConnectionStrings:Postgres
- Redis:Host
- Jwt:Issuer
- Jwt:Audience
- Jwt:Key
- S3:Bucket
- CloudFront:Domain

### 실행
cd WebServer
dotnet restore
dotnet run

### 데이터베이스 마이그레이션 (EF Core)
dotnet ef database update


## 인증 구조 요약
- 로그인 시 AccessToken(JWT) + RefreshToken 발급
- RefreshToken은 Redis에 저장
- AccessToken 만료 시 RefreshToken으로 재발급
- 로그아웃 또는 차단 시 Refresh 세션 revoke 지원


## 인프라 구성 요약
- AWS EC2 단일 인스턴스 배포
- PostgreSQL / Redis 사용
- 정적 리소스는 S3 저장 후 CloudFront로 서빙


## 트러블슈팅
- 401 Unauthorized
  - JWT 만료 여부 확인
  - Redis에 저장된 Refresh 세션 상태 확인
- 전투 결과 불일치
  - 서버 Tick 처리 로그(Event) 및 Snapshot 생성 흐름 확인
- Redis 연결 오류
  - Redis Host/Port 설정 및 네트워크 보안 그룹 확인
