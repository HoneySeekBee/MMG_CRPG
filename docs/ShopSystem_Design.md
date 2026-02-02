# 상점 시스템 기획서

| 항목 | 내용 |
|------|------|
| 문서 버전 | v1.0 |
| 작성일 | 2026-02-02 |
| 상태 | 검토 대기 |

---

## 1. 개요

### 1.1 목적
유저가 게임 내 재화(골드, 젬, 토큰 등)를 사용하여 아이템을 구매할 수 있는 상점 시스템.

### 1.2 상점 유형

| 유형 | 설명 | 예시 |
|------|------|------|
| **일반 상점** | 상시 운영, 고정 상품 목록 | 장비 상점, 소모품 상점 |
| **기간 한정 상점** | 운영 기간이 있는 이벤트 상점 | 설날 이벤트 상점, 주간 특가 |

### 1.3 핵심 규칙
- **서버 권한**: 가격 검증, 재화 차감, 아이템 지급 모두 서버에서 처리
- **복수 재화**: 상품별로 골드/젬/토큰 등 다른 재화로 가격 설정 가능 (기존 `ItemPrice` 구조 활용)
- **구매 제한**: 상품별 유저당 일일/주간/총 구매 횟수 제한
- **동시성 제어**: Redis 분산 락으로 동일 유저의 동시 구매 요청 방지

---

## 2. 데이터 모델

### 2.1 ERD 관계도

```
Shop (1) ──── (N) ShopProduct (N) ──── (1) Item
                      │
                      └──── (N) UserPurchaseLog
```

### 2.2 Shop (상점)

| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | int (PK) | 상점 ID |
| Code | string (UNIQUE) | 상점 코드 (예: `GENERAL_EQUIP`, `EVENT_2026_LUNAR`) |
| Name | string | 상점명 |
| ShopType | enum | `General` = 0, `TimeLimited` = 1 |
| StartsAt | DateTimeOffset? | 운영 시작 (General은 null) |
| EndsAt | DateTimeOffset? | 운영 종료 (General은 null) |
| IsActive | bool | 활성화 여부 |
| SortOrder | int | 클라이언트 표시 순서 |
| CreatedAt | DateTimeOffset | 생성일 |
| UpdatedAt | DateTimeOffset | 수정일 |

### 2.3 ShopProduct (상점 상품)

| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | int (PK) | 상품 ID |
| ShopId | int (FK → Shop) | 소속 상점 |
| ItemId | int (FK → Item) | 판매 아이템 |
| CurrencyId | int (FK → Currency) | 결제 재화 |
| Price | long | 가격 |
| QuantityPerPurchase | int | 1회 구매 시 지급 수량 (기본: 1) |
| DailyLimit | int? | 일일 구매 제한 (null = 무제한) |
| WeeklyLimit | int? | 주간 구매 제한 (null = 무제한) |
| TotalLimit | int? | 총 구매 제한 (null = 무제한) |
| SortOrder | int | 표시 순서 |
| IsActive | bool | 활성화 여부 |
| CreatedAt | DateTimeOffset | 생성일 |
| UpdatedAt | DateTimeOffset | 수정일 |

### 2.4 UserPurchaseLog (유저 구매 기록)

| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | long (PK) | 기록 ID |
| UserId | int (FK → User) | 구매 유저 |
| ShopProductId | int (FK → ShopProduct) | 구매 상품 |
| Quantity | int | 구매 수량 |
| PricePaid | long | 실제 지불 금액 |
| CurrencyCode | string | 결제 재화 코드 |
| PurchasedAt | DateTimeOffset | 구매 시각 |

---

## 3. 구매 플로우

### 3.1 클라이언트-서버 시퀀스

```
[Client]                         [Server]                          [Redis]          [DB]
   │                                │                                │               │
   │  1. GET /api/pb/shop           │                                │               │
   │ ─────────────────────────────► │                                │               │
   │                                │  상점 목록 + 상품 조회          │               │
   │  ◄───── ShopListResponse ──── │                                │               │
   │  (상점별 상품 목록, 잔여 구매 횟수)                               │               │
   │                                │                                │               │
   │  2. POST /api/pb/shop/purchase │                                │               │
   │ ─────────────────────────────► │                                │               │
   │                                │  ① Redis 분산 락 획득           │               │
   │                                │ ──────────────────────────────► │               │
   │                                │  ◄─── lock:shop:purchase:{uid} │               │
   │                                │                                │               │
   │                                │  ② 검증                        │               │
   │                                │  - 상점 존재/활성 여부           │               │
   │                                │  - 기간 한정이면 기간 내인지     │               │
   │                                │  - 상품 존재/활성 여부           │               │
   │                                │  - 구매 제한 초과 여부           │               │
   │                                │  - 재화 잔액 충분한지            │               │
   │                                │                                │               │
   │                                │  ③ 트랜잭션 시작                │               │
   │                                │ ─────────────────────────────────────────────► │
   │                                │  - 재화 차감 (WalletService)    │               │
   │                                │  - 아이템 지급 (InventoryService)│               │
   │                                │  - 구매 기록 저장               │               │
   │                                │  - COMMIT                      │               │
   │                                │  ◄──────────────────────────────────────────── │
   │                                │                                │               │
   │                                │  ④ 락 해제                     │               │
   │                                │ ──────────────────────────────► │               │
   │                                │                                │               │
   │  ◄─── PurchaseResponse ────── │                                │               │
   │  (결과, 갱신된 잔액, 인벤토리)  │                                │               │
```

### 3.2 서버 검증 체크리스트

| 순서 | 검증 항목 | 실패 시 에러 코드 |
|------|----------|-----------------|
| 1 | 상점 존재 여부 | `SHOP_NOT_FOUND` |
| 2 | 상점 활성 상태 | `SHOP_NOT_ACTIVE` |
| 3 | 기간 한정 상점의 운영 기간 | `SHOP_NOT_IN_PERIOD` |
| 4 | 상품 존재 여부 | `PRODUCT_NOT_FOUND` |
| 5 | 상품 활성 상태 | `PRODUCT_NOT_ACTIVE` |
| 6 | 일일 구매 제한 | `DAILY_LIMIT_EXCEEDED` |
| 7 | 주간 구매 제한 | `WEEKLY_LIMIT_EXCEEDED` |
| 8 | 총 구매 제한 | `TOTAL_LIMIT_EXCEEDED` |
| 9 | 재화 잔액 확인 | `INSUFFICIENT_CURRENCY` |

### 3.3 동시성 제어

```csharp
// Redis 분산 락 키 패턴
var lockKey = $"lock:shop:purchase:{userId}";
var acquired = await _lock.AcquireAsync(lockKey, TimeSpan.FromSeconds(5));
if (!acquired) → PURCHASE_IN_PROGRESS 에러 반환

try
{
    // UnitOfWork 트랜잭션 내에서:
    // 1) 구매 제한 조회 (DB)
    // 2) 재화 차감 (WalletService.SpendAsync)
    // 3) 아이템 지급 (UserInventoryService.GrantAsync)
    // 4) 구매 기록 저장 (UserPurchaseLog INSERT)
    // 5) COMMIT
}
finally
{
    await _lock.ReleaseAsync(lockKey);
}
```

---

## 4. API 스펙

### 4.1 게임 클라이언트용 (Protobuf)

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/pb/shop` | 활성 상점 목록 + 상품 조회 |
| GET | `/api/pb/shop/{shopId}` | 특정 상점 상세 (상품 + 유저 구매 현황) |
| POST | `/api/pb/shop/purchase` | 상품 구매 |

### 4.2 운영툴용 (JSON REST)

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/shop` | 상점 목록 (페이징, 필터) |
| GET | `/api/shop/{id}` | 상점 상세 |
| POST | `/api/shop` | 상점 생성 |
| PUT | `/api/shop/{id}` | 상점 수정 |
| DELETE | `/api/shop/{id}` | 상점 삭제 |
| GET | `/api/shop/{shopId}/products` | 상점 상품 목록 |
| POST | `/api/shop/{shopId}/products` | 상품 추가 |
| PUT | `/api/shop/{shopId}/products/{productId}` | 상품 수정 |
| DELETE | `/api/shop/{shopId}/products/{productId}` | 상품 삭제 |
| GET | `/api/shop/purchase-logs` | 구매 기록 조회 (관리자용) |

### 4.3 Protobuf 메시지 정의

```protobuf
// shop.proto

enum ShopTypePb {
  SHOP_TYPE_GENERAL      = 0;
  SHOP_TYPE_TIME_LIMITED  = 1;
}

message ShopPb {
  int32       id          = 1;
  string      code        = 2;
  string      name        = 3;
  ShopTypePb  shop_type   = 4;
  int64       starts_at_ms = 5;  // 0이면 상시
  int64       ends_at_ms   = 6;  // 0이면 상시
  int32       sort_order   = 7;
  repeated ShopProductPb products = 10;
}

message ShopProductPb {
  int32  id                    = 1;
  int32  item_id               = 2;
  string item_name             = 3;
  string item_icon_url         = 4;
  int32  rarity_id             = 5;
  string currency_code         = 6;
  int64  price                 = 7;
  int32  quantity_per_purchase  = 8;
  int32  daily_limit           = 9;   // 0 = 무제한
  int32  weekly_limit          = 10;  // 0 = 무제한
  int32  total_limit           = 11;  // 0 = 무제한
  int32  daily_purchased       = 12;  // 유저의 오늘 구매 횟수
  int32  weekly_purchased      = 13;  // 유저의 이번주 구매 횟수
  int32  total_purchased       = 14;  // 유저의 총 구매 횟수
}

message ShopListResponse {
  repeated ShopPb shops = 1;
  int64 server_unix_ms  = 2;
}

message PurchaseRequest {
  int32 shop_product_id = 1;
  int32 quantity        = 2;  // 보통 1
}

message PurchaseResponse {
  bool   success         = 1;
  string error_code      = 2;  // 실패 시
  int64  currency_after  = 3;  // 구매 후 잔액
  int32  item_count_after = 4; // 구매 후 아이템 수량
  int32  daily_purchased  = 5; // 갱신된 일일 구매 횟수
  int32  weekly_purchased = 6;
  int32  total_purchased  = 7;
}
```

---

## 5. 구매 제한 계산 로직

### 5.1 기간 기준

| 제한 유형 | 기간 기준 | 리셋 시점 |
|----------|----------|----------|
| 일일 | UTC 00:00 ~ 23:59 | 매일 UTC 00:00 |
| 주간 | 월요일 UTC 00:00 ~ 일요일 UTC 23:59 | 매주 월요일 UTC 00:00 |
| 총 | 제한 없음 | 리셋 없음 |

### 5.2 쿼리 예시

```sql
-- 일일 구매 횟수
SELECT COALESCE(SUM("Quantity"), 0)
FROM "UserPurchaseLogs"
WHERE "UserId" = @userId
  AND "ShopProductId" = @productId
  AND "PurchasedAt" >= @todayUtcStart;

-- 주간 구매 횟수
SELECT COALESCE(SUM("Quantity"), 0)
FROM "UserPurchaseLogs"
WHERE "UserId" = @userId
  AND "ShopProductId" = @productId
  AND "PurchasedAt" >= @weekStartUtc;
```

---

## 6. 운영툴 요구사항

### 6.1 상점 관리 페이지

| 페이지 | 기능 |
|--------|------|
| 상점 목록 | 필터(유형, 활성), 검색(이름, 코드), 페이징 |
| 상점 생성/수정 | 코드, 이름, 유형, 기간(기간 한정일 때만), 활성 여부 |
| 상품 관리 | 상점 내 상품 추가/수정/삭제, 아이템/재화 드롭다운, 가격, 구매 제한 설정 |
| 구매 기록 조회 | 유저별/상품별 구매 이력 검색 |

---

## 7. 구현 작업 목록

### Phase 1: 서버 — Domain + Infrastructure

| # | 작업 | 파일 |
|---|------|------|
| 1 | `Shop` 엔티티 작성 | `Domain/Entities/Shop/Shop.cs` |
| 2 | `ShopProduct` 엔티티 작성 | `Domain/Entities/Shop/ShopProduct.cs` |
| 3 | `UserPurchaseLog` 엔티티 작성 | `Domain/Entities/Shop/UserPurchaseLog.cs` |
| 4 | `ShopType` enum 작성 | `Domain/Enum/ShopType.cs` |
| 5 | EF Configuration 작성 | `Infrastructure/Persistence/Configurations/Shop/` |
| 6 | `DbContext`에 DbSet 추가 | `Infrastructure/Persistence/GameDBContext.cs` |
| 7 | Migration 생성 | EF Core Migration |

### Phase 2: 서버 — Application + Presentation

| # | 작업 | 파일 |
|---|------|------|
| 8 | Repository 인터페이스/구현 | `Application/Repositories/IShop*Repository.cs`, `Infrastructure/Repositories/Shop*Repository.cs` |
| 9 | DTO / Request / Response | `Application/Shop/Dtos.cs`, `Requests.cs` |
| 10 | `ShopService` (상점 CRUD) | `Application/Shop/ShopService.cs` |
| 11 | `PurchaseService` (구매 로직 + 동시성 제어) | `Application/Shop/PurchaseService.cs` |
| 12 | Protobuf 메시지 정의 | `Protocol/Proto/shop.proto` |
| 13 | Game Controller (Protobuf) | `Presentation/Controllers/Game/GameShopController.cs` |
| 14 | Admin Controller (JSON) | `Presentation/Controllers/Admin/AdminShopController.cs` |
| 15 | DI 등록 | `Presentation/Extensions/AppExtensions.cs` |

### Phase 3: 테스트

| # | 작업 |
|---|------|
| 16 | `PurchaseService` 단위 테스트 (정상 구매, 잔액 부족, 구매 제한 초과, 기간 외 구매) |
| 17 | 동시성 테스트 (동일 유저 동시 구매 요청) |

### Phase 4: 운영툴

| # | 작업 | 파일 |
|---|------|------|
| 18 | ViewModel 작성 | `AdminTool/Models/ShopVm.cs` |
| 19 | 상점 관리 Controller | `AdminTool/Controllers/ShopController.cs` |
| 20 | Razor View (목록, 생성, 수정, 상품 관리) | `AdminTool/Views/Shop/` |

### Phase 5: 클라이언트

| # | 작업 |
|---|------|
| 21 | Proto 메시지 생성 (Unity용) |
| 22 | `ShopService.cs` (API 호출, 응답 파싱) |
| 23 | `ShopPresenter.cs` (UI 데이터 바인딩 로직) |
| 24 | UI 프리팹 작업 (직접 작업 필요) |

---

## 8. 포트폴리오 어필 포인트

이 상점 시스템에서 보여줄 수 있는 서버 개발 역량:

| 역량 | 구현 내용 |
|------|----------|
| **서버 권한 구조** | 가격/재화/구매제한 검증을 전부 서버에서 처리, 클라이언트는 결과만 수신 |
| **동시성 제어** | Redis 분산 락으로 동일 유저 동시 구매 방지 |
| **트랜잭션** | UnitOfWork로 재화 차감 + 아이템 지급 + 기록 저장을 원자적 처리 |
| **기존 시스템 연동** | WalletService, UserInventoryService 재사용 |
| **전투와 다른 패턴** | 전투 = 틱 기반 실시간, 상점 = 요청-응답 트랜잭션 → 두 가지 서버 권한 패턴 |
