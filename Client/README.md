# Client (Unity) - MMG_CRPG

MMG_CRPG의 Unity 클라이언트입니다.  
서버 권한(Server-authoritative) 구조에서 클라이언트는 **입력(Command) 전달**과  
**서버 결과(Snapshot/Event) 재생(Playback)** 역할에 집중합니다.

---

## 핵심 설계 포인트

### 1) 서버-클라이언트 통신 규약

- 프로토콜: HTTPS + Protobuf(Proto3, Binary) + JWT
- 엔드포인트: `/api/pb/*`
- 헤더
  - `Content-Type: application/x-protobuf`
  - `Accept: application/x-protobuf`
  - `Authorization: Bearer <AccessToken>`

<img width="4142" height="1937" alt="클라이언트구조" src="https://github.com/user-attachments/assets/a5bddee3-30b6-487f-8908-af41cd6dbc99" />

- 클라이언트는 Feature 단위(Network Module)에서 요청을 생성합니다.
- 모든 네트워크 요청은 `ProtoHttpClient`를 통해 전송됩니다.
- `ProtoHttpClient`는 HTTPS 전송, JWT 인증, Retry, Timeout, Protobuf 파싱을 공통 처리합니다.
- 서버는 `/api/pb/*` 엔드포인트에서 Protobuf 요청을 처리하고,
  서버 기준 Snapshot + Event 결과를 반환합니다.
- 클라이언트는 서버 응답을 기준으로 상태를 동기화하고 연출만 재생합니다.  

### 2) Tick 기반 전투 동기화 (서버 권한)
- 전투 흐름: `StartCombat → Tick Loop → FinishCombat`
- 서버가 모든 판정과 상태 변경을 결정
- 클라이언트는 Tick 단위로 Snapshot을 반영하고, Event로 연출을 재생

### 3) 씬 및 게임 플로우 구조

<img width="3837" height="1894" alt="서버클라이언트통신구조" src="https://github.com/user-attachments/assets/2b80369a-d736-49cd-b336-fbd94389420e" />

- `AppPersistent` 씬은 앱 시작부터 종료까지 유지되며,
  인증, 네트워크, 캐시, Addressables 등 전역 시스템을 포함합니다.
- `AppBootstrap`이 `LobbyRoot` 씬을 로드하여 전체 게임 플로우를 초기화합니다.
- `LobbyRoot`는 UI(Canvas/Panel/Popup)와 게임 상태 전환을 관리하며 항상 유지됩니다.
- 전투, 가챠, 파티 설정과 같은 콘텐츠 씬은 Additive 방식으로 로드되며,
  사용 종료 시 즉시 언로드됩니다.
- 이 구조를 통해 씬 전환 비용을 최소화하고 전역 상태를 안정적으로 유지합니다.

### 4) Addressables 로딩 & 캐시
- Key 기반 비동기 로딩
- 캐시(Dictionary)로 중복 로드 방지
- 콘텐츠 종료 시 선택적 Release로 메모리 사용량 제어

---

## 폴더 구조

### 1) Unity Project Root
Unity 프로젝트 전체 구성입니다.

- `Assets/` : 게임 리소스 및 스크립트
- `Packages/` : Unity Package Manager 설정
- `ProjectSettings/` : Unity 프로젝트 설정
- `UserSettings/` : 에디터 사용자 설정
- `Logs/` : 실행 로그
- `Recordings/` : 개발 중 녹화/캡처 리소스
- `Client.sln` / `Assembly-CSharp*.csproj` : IDE 연동용 솔루션/프로젝트 파일

`Library/`, `Temp/`, `obj/`는 로컬 캐시이므로 Git 추적 대상에서 제외합니다.

---

### 2) Source Layout (Assets/Script)
실제 게임 로직은 `Assets/Script` 기준으로 구성되어 있으며,  
앱 수명 관리 / 기능 단위 / 공통 시스템을 명확히 분리하는 것을 목표로 설계했습니다.

#### App
- `Assets/Script/App/`
  - `AppBootstrap`
    - 게임 최초 실행 진입점
    - ApiConfig, 로딩 UI, 전역 시스템 초기화
    - `AppPersistent` 기반에서 `LobbyRoot` 로드 및 초기 플로우 진입

App 레이어는 초기화와 씬 플로우 연결만 담당하며,  
실제 게임 로직은 Feature / System 레이어에 위임합니다.

#### Core
- `Assets/Script/Core/`
  - Addressables 래퍼 및 유틸
  - Scene 관리
  - Object Pooling
  - 공용 유틸 및 기반 컴포넌트

#### Features
- `Assets/Script/Features/`
  - 기능(도메인) 단위로 코드 구성
  - 각 Feature는 UI / Network / Presentation / Local State를 내부에 포함

주요 Feature 구성:
- `Auth`
- `Battle`
  - `Character`
  - `Monster`
  - `UI`
- `Combat`
  - `Core` : 전투 상태 및 컨텍스트
  - `Manager` : 전투 흐름 제어
  - `Network` : 전투 Protobuf API
  - `Presentation` : Snapshot 적용 및 Event 기반 연출
  - `Skills`
  - `UI`
- `Lobby`
  - `Managers`
  - `Network`
  - `UI`
- `Login`
- `Gacha`
- `Stage`
  - `Data`
  - `Manager`
  - `Progress`
  - `UI`

서버 권한 구조에 맞춰 클라이언트는 서버 상태를 직접 소유하지 않고,  
Snapshot을 해석하여 화면 표현(Presentation)에 집중하도록 설계했습니다.

#### Shared
- `Assets/Script/Shared/`
  - 여러 Feature에서 공통으로 사용하는 데이터, 캐시, 확장 메서드

#### Systems
- `Assets/Script/Systems/`
  - 여러 Feature에서 공통으로 사용하는 런타임 시스템
  - 예: Network(ProtoHttpClient, ApiConfig, ApiRoutes), Audio, Logging, Cache 등

#### UI
- `Assets/Script/UI/`
  - 공통 UI 프레임워크
  - Panel / Popup / HUD 베이스 및 공통 위젯

---

## 실행 방법 (로컬)

### 요구 사항
- Unity: `2022.3.48`
- 서버 주소: `ApiConfig` ScriptableObject
  - `AppBootstrap`에서 Inspector Reference로 주입
  - 빌드/실행 환경에 따라 서로 다른 ApiConfig 에셋을 할당하여 사용

### 1) 서버 실행
- `WebServer`를 먼저 실행하고, 클라이언트의 ApiConfig가 올바른 서버 주소를 바라보도록 설정합니다.

### 2) Addressables 빌드 (변경이 있는 경우)
Addressables 에셋/그룹/라벨/카탈로그 변경이 있을 경우 실행 전 빌드가 필요합니다.

- Unity 메뉴: `Window > Asset Management > Addressables > Groups`
- `Build > New Build > Default Build Script` 또는 `Build Player Content`

### 3) 클라이언트 실행
1. Unity에서 `Client/` 프로젝트 오픈
2. (필요 시) Addressables 빌드
3. Play

---

## 전투 통신 흐름(개념)
- StartCombat 요청 → 초기 Snapshot 수신
- TickRequest 반복 호출 → Snapshot + Events 수신
- Event로 연출 재생, Snapshot으로 상태 동기화

---

## 트러블슈팅
- 401 Unauthorized: AccessToken 만료 → 재로그인 또는 토큰 갱신 흐름 확인
- Protobuf 파싱 오류: 서버/클라이언트 proto 파일 버전 불일치 여부 확인
- Addressables 로딩 실패: Addressables 빌드 여부 및 원격 경로(S3/CloudFront) 설정 확인
