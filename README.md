# 프로젝트 이름

## 1. 프로젝트 개요
- MMG_CRPG는 서브컬쳐 수집형 RPG 입니다. 
- 기간 : 2025.08.20 ~ 2025.11.18
- 사용한 기술 스택 : Client - Unity
                    Server - ASP.NET Core API
                    DB - PostgreSQL
                    운영툴 - ASP.NET Core MVC 기반 내부 툴
                    기타 - GitHub, Protobuf
- 프로젝트 목표 : 서브컬쳐 스타일의 수집형 RPG를 Unity 기반으로 개발한 개인 프로젝트 입니다. 클라이언트, 서버, 운영툴까지 전체 시스템을 직접 설계하고 구현하는 것을 목표로 진행하였습니다. 

## 2. 주요 특징 (핵심 기능 요약)
### ✔ 클라이언트
- Unity 기반 실시간 전투 구현
- Addressable 기반 리소스 관리
- UI/Inventory/Stage/가챠/파티 시스템 등

### ✔ 서버 (Web API)
- ASP.NET Core 기반
- DDD 구조 (Domain / Application / Infrastructure / Web)
- 전투 시뮬레이션 서버 직접 구현 (Tick 기반 전투)
- Protobuf 기반 고효율 통신

### ✔ 운영툴 (Admin Tool)
- 마스터 데이터 CRUD
- 유저 데이터 조회/변경
- Stage/Monster/Item/Skill 등 리소스 관리 페이지

### ✔ DB
- PostgreSQL 기반
- Character / Monster / Stage / Item / User 등 약 60개 테이블 설계

---

## 3. 전체 아키텍처
> 클라이언트 ↔ WebServer(API) ↔ Application ↔ Domain ↔ Infrastructure(DB)

<img width="923" height="559" alt="image" src="https://github.com/user-attachments/assets/621023de-83f0-4e7e-9a08-f82e7424998c" />


---

## 4. 게임 전투 흐름 요약
- StartCombat → Tick → FinishCombat  
- CombatRuntimeState 기반 전투 계산  
- AI / Wave / AttackSystem 등 내부 로직 설명  
- TickResponsePb → 클라이언트로 반환

---

## 5. 주요 기술 스택
### ✔ Client
- Unity(C#), Addressable, DOTween 등

### ✔ Server
- ASP.NET Core Web API
- Protobuf
- PostgreSQL
  
### ✔ AdminTool
- ASP.NET Core MVC
- Bootstrap
- 관리자 인증/권한

---

## 6. 주요 기능 스크린샷
<img width="346" height="199" alt="image" src="https://github.com/user-attachments/assets/b32e7119-8fde-4da9-8d72-cc968e621a1b" />
<img width="353" height="193" alt="image" src="https://github.com/user-attachments/assets/32e650e8-30f0-4d1d-85a9-5c517e0bc6eb" />
<img width="347" height="194" alt="image" src="https://github.com/user-attachments/assets/73f9da26-0fed-4c44-ba5d-d93b404fc592" />

클라이언트 동작 동영상 : https://youtu.be/QODgDKIFDW0

---

## 7. 배운 점
- 클라~서버~운영툴 전체 흐름 이해
- DDD 구조 설계 경험 축적
- 서버 기반 전투 시뮬레이션 구현 경험
- 데이터 중심 개발 사고 정착

---

## 8. 앞으로의 계획
- AWS 배포 (EC2 + RDS)
- 전투 스킬 시스템 확장
- 가챠/상점/캐릭터 성장 시스템 고도화
- 로그인/매칭/소셜 기능 추가 예정


