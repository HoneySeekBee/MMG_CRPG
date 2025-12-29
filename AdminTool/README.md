# AdminTool (ASP.NET Core / Admin Web) - MMG_CRPG

MMG_CRPG의 운영 및 관리용 어드민 도구입니다.  
게임 서버(WebServer)와 동일한 도메인 모델을 공유하며,  
운영자가 게임 데이터를 안전하게 조회·관리할 수 있도록 설계되었습니다.

본 도구는 **게임 플레이 로직을 직접 수행하지 않으며**,  
운영·모니터링·데이터 관리 목적에 집중합니다.

---

## 기술 스택
- ASP.NET Core Web API
- Clean Architecture (AdminTool / Application / Domain / Infrastructure)
- REST API (JSON)
- JWT 인증
- PostgreSQL (EF Core)
- Redis (Session / Cache)
- WebServer와 Domain / Application 계층 공유

---

## 핵심 설계 포인트

### 1. 서버 로직 재사용 구조
- AdminTool은 WebServer와 **Domain / Application 계층을 공유**
- 전투 규칙, 상태 모델, 비즈니스 로직은 단일 소스로 유지
- 운영툴과 게임 서버 간 **로직 불일치 및 중복 구현 방지**

---

### 2. Admin 전용 API Surface
- AdminTool은 JSON 기반 REST API 제공
- 엔드포인트 예시:
  - `/api/admin/auth/*`
  - `/api/admin/users/*`
  - `/api/admin/contents/*`
  - `/api/admin/combat/*`
- 고빈도 Tick API, 실시간 전투 제어 기능은 노출하지 않음

---

### 3. 인증 및 권한 관리
- JWT 기반 인증 사용
- Admin 계정은 별도 Role 또는 Claim으로 구분
- WebServer와 동일한 인증 인프라를 재사용

---

### 4. 전투 데이터 조회 구조
- 전투는 서버 메모리에서 진행되며,
  AdminTool은 **DB에 저장된 전투 로그(Event)** 만 조회
- 전투 상태 재현, 로그 분석, 이슈 추적 목적
- 실시간 전투 개입 기능은 제공하지 않음

---

### 5. Redis 사용 범위
- 관리자 로그인 세션 관리
- 캐시 데이터
- 서버 상태 확인용 정보

전투 시뮬레이션 및 Tick 처리에는 Redis를 사용하지 않음

---

## 아키텍처 구조 (Clean Architecture)

### 계층 구성
- **AdminTool**
  - Controller (JSON API)
  - 인증 / 요청 검증 / DTO 매핑

- **Application**
  - 운영 유스케이스 조합
  - 조회 / 수정 / 관리 흐름 제어

- **Domain**
  - 공통 도메인 모델
  - 전투 / 유저 / 콘텐츠 규칙

- **Infrastructure**
  - PostgreSQL (EF Core)
  - Redis
  - 외부 서비스 연동

---

## 운영 기능 범위

AdminTool은 라이브 게임 운영에 필요한 주요 기능을 제공합니다.

### 서버 상태 관리
- 서버 기본 상태 확인
- DB / Redis 연결 상태 확인
- 운영 지표 모니터링

### 콘텐츠 관리
- 캐릭터 생성 및 관리
- 몬스터 생성 및 관리
- 아이템 생성 및 관리
- 스테이지 데이터 관리
- 밸런스 및 콘텐츠 데이터 검증

### 가차 시스템 운영
- 가차 풀(Gacha Pool) 구성
- 가차 배너 생성 및 관리
- 확률 및 기간 설정
- 운영 중 가차 데이터 변경 반영

### 유저 관리
- 회원가입 정보 조회
- 유저 계정 관리
- 유저 데이터 확인
- 운영 이슈 대응

### 리소스 관리
- 서버에서 사용하는 이미지 및 정적 리소스 업로드
- 클라이언트에서 사용하는 콘텐츠 리소스 관리

---

## 실행 방법 (로컬)

### 요구 사항
- .NET SDK 8.0.x
- PostgreSQL
- Redis

### 환경 설정
- `appsettings.json`
- `appsettings.Development.json` (Git 추적 제외 권장)

### 필수 설정 항목 예시
- ConnectionStrings:Postgres
- Redis:Host
- Jwt:Issuer
- Jwt:Audience
- Jwt:Key
---

## 설계상 제공하지 않는 기능

AdminTool은 서버 권한(Server-authoritative) 구조를 유지하기 위해,  
다음 기능들은 의도적으로 제공하지 않습니다.

- 실시간 전투 상태 개입
- Tick Loop 제어
- 전투 결과 강제 수정
- 서버 메모리(Runtime Combat State) 직접 접근

운영툴은 영속화된 데이터(DB)를 기준으로  
조회 및 관리만 수행하도록 설계되었습니다.

---

## 트러블슈팅

### 401 Unauthorized
- JWT 만료 여부 확인
- 관리자 계정 권한(Role/Claim) 설정 확인

### 데이터 불일치
- AdminTool은 DB 기준 조회만 수행
- 실시간 전투 상태와 차이가 있을 수 있음 (정상 동작)

---

## 목적 요약
- 게임 서버 로직을 훼손하지 않는 운영 도구
- 서버 권한 구조를 유지한 상태에서의 안전한 관리
- 장기 운영 및 분석을 고려한 설계
