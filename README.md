# MMG_CRPG (Lumen Academy)
**Server-authoritative 수집형 RPG (1인 개발)**
Unity 클라이언트와 ASP.NET Core 서버를 직접 설계·구현한  
서버 권한(Server-authoritative) 기반 서브컬쳐 수집형 RPG 프로젝트입니다.  
전투, 데이터, 인증, 운영툴, 배포까지 **실서비스 환경을 가정한 전체 흐름**을 직접 구현하는 것을 목표로 진행했습니다.

## 1. 프로젝트 개요
- 장르: 서브컬쳐 수집형 RPG (자동 전투 기반 PvE)
- 개발 형태: 1인 개발
- 개발 기간: 2025.08.20 ~ 2025.11.18
- 플랫폼: Mobile (Unity)

### 프로젝트 목표
- Unity 클라이언트 경험을 기반으로  
  **게임 서버 구조, 전투 처리, 데이터 설계, 운영 및 배포 환경을 서버 중심 관점에서 직접 설계·구현**
- 클라이언트–서버–운영툴을 포함한 **라이브 서비스 구조 검증**

---
## 2. 핵심 특징 (요약)

### ✔ 서버 권한(Server-authoritative) 기반 전투 구조
- 서버 Tick Loop 기반 전투 시뮬레이션 처리
- 서버가 모든 전투 판정과 상태 변경을 담당
- 클라이언트는 입력 전달 및 Snapshot/Event 기반 결과 렌더링

### ✔ 스킬 시스템 및 전투 로직 확장
- SkillCast / Cooldown / Buff 처리 로직 서버 구현
- Tick 기반 전투 흐름 내 스킬 사용 및 효과 적용
- 스킬/상태 변경은 Event로 분리 전달하여 연출 재생

### ✔ 운영을 고려한 데이터/보상 시스템
- 가챠/보상 지급 로직 서버 검증 중심 처리
- Redis 캐시 및 Warmup 적용
- 내부 운영툴을 통한 배너/확률/기간 데이터 관리

### ✔ 실서비스 환경을 가정한 인프라 구성
- AWS EC2 기반 서버 배포
- PostgreSQL, Redis 사용
- S3 + CloudFront 기반 정적 리소스 관리

---

## 3. 전체 아키텍처
> Client(Unity) ↔ WebServer(API) ↔ Application ↔ Domain ↔ Infrastructure(DB/Cache)
<img width="839" height="494" alt="MMG_CRPG_Archtecture" src="https://github.com/user-attachments/assets/9f0ad939-7681-48be-9956-f13552c35ecc" />

---

## 4. 전투 처리 흐름 요약
- StartCombat → Tick Loop → FinishCombat
- CombatRuntimeState 기반 상태 관리
- AI / Wave / Attack / Skill / Buff 시스템 순차 처리
- CombatTickResponsePb (Snapshot + Events) 생성 후 클라이언트 전달

---

## 5. 기술 스택

### ✔ Client
- Unity (C#)
- Addressables
- DOTween
- 서버 결과 기반 상태 동기화

### ✔ Server
- ASP.NET Core Web API
- Clean Architecture (Web / Application / Domain / Infrastructure)
- Protobuf (Proto3)
- JWT 인증
- PostgreSQL (EF Core)
- Redis (Cache / Presence)

### ✔ Admin Tool
- ASP.NET Core MVC
- Bootstrap
- 관리자 인증 및 권한 관리

### ✔ Infra
- AWS EC2
- Docker 기반 빌드/배포
- S3 + CloudFront

---

## 6. 주요 기능 스크린샷
<img width="346" height="199" alt="combat" src="https://github.com/user-attachments/assets/b32e7119-8fde-4da9-8d72-cc968e621a1b" />
<img width="353" height="193" alt="gacha" src="https://github.com/user-attachments/assets/32e650e8-30f0-4d1d-85a9-5c517e0bc6eb" />
<img width="347" height="194" alt="admin" src="https://github.com/user-attachments/assets/73f9da26-0fed-4c44-ba5d-d93b404fc592" />

## 7. 이 프로젝트를 통해 얻은 것
- 클라이언트–서버–운영툴 전체 흐름을 고려한 설계 경험
- 서버 권한 구조를 통한 데이터 무결성 및 치트 방지 구조 이해
- Tick 기반 전투 처리 및 상태 동기화 설계 경험
- 운영과 배포를 고려한 서버 개발 관점 확립

---

## 8. 한계 및 향후 계획
- 단일 서버 기반 구조 검증에 집중 (대규모 동시 접속/스케일아웃은 후속 과제)
- 실시간 PvP 구조 확장
- 네트워크 구조 고도화(gRPC 검토)
- 서버 분리 및 확장 구조 실험

## 9. 참고 자료 

🎥 Youtube
https://youtu.be/e2f5qjR5SHQ

Notion 
https://www.notion.so/Lumen-Academy-2cedd6b204da81aea245eb503471a500?source=copy_link

Blog (개발 일지)
https://blog.naver.com/12dlfdl12/223977163312

