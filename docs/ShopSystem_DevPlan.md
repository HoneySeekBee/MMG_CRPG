# 상점 시스템 — 서버 개발 진행 계획

> 목표: 서버측 코드 작성 완료 + 운영툴에서 기능 검증

---

## 전체 진행 흐름

```
Step 1  DB 레이어 (Entity → EF Config → Migration → 테이블 생성)
  ↓
Step 2  데이터 접근 레이어 (Repository 인터페이스 + 구현)
  ↓
Step 3  비즈니스 로직 (ShopService, PurchaseService)
  ↓
Step 4  Admin API (JSON REST → 운영툴이 호출할 엔드포인트)
  ↓
Step 5  운영툴 화면 (AdminTool CRUD 페이지)
  ↓
  ── 여기서 1차 검증: 운영툴로 상점/상품 CRUD + 구매 테스트 ──
  ↓
Step 6  Game API (Protobuf → 클라이언트용 엔드포인트)
  ↓
Step 7  Proto 정의 + 매퍼
```

핵심은 **아래에서 위로 쌓아가면서, 각 단계마다 동작 확인이 가능한 상태를 유지**하는 것.

---

## Step 1. DB 레이어

### 할 일
1. `ShopType` enum 추가 (`Domain/Enum/`)
2. `Shop`, `ShopProduct`, `UserPurchaseLog` 엔티티 작성 (`Domain/Entities/Shop/`)
3. EF Configuration 3개 작성 (`Infrastructure/Persistence/Configurations/Shop/`)
4. `GameDBContext`에 DbSet 3개 추가 + `OnModelCreating`에 ApplyConfiguration
5. EF Migration 생성 → DB 반영

### 확인 포인트
- Migration 파일이 정상 생성되는가
- `dotnet ef database update` 후 PostgreSQL에 테이블 3개(`Shops`, `ShopProducts`, `UserPurchaseLogs`)가 만들어지는가
- FK, 인덱스, Unique 제약이 의도대로 걸렸는가

### 현재 상태
> 엔티티 3개 + enum + EF Config 3개는 이미 작성됨.
> **남은 것:** GameDBContext 수정, Migration 생성/적용

---

## Step 2. 데이터 접근 레이어

### 할 일
1. `IShopRepository` — Shop CRUD (GetById, GetByCode, GetAll, Add, SaveChanges)
2. `IShopProductRepository` — ShopProduct CRUD (GetById, GetByShopId, Add, Remove, SaveChanges)
3. `IUserPurchaseLogRepository` — 구매 기록 저장 + **구매 횟수 집계 쿼리**
   - `GetDailyCountAsync(userId, productId, todayUtcStart)`
   - `GetWeeklyCountAsync(userId, productId, weekStartUtc)`
   - `GetTotalCountAsync(userId, productId)`
   - `GetPurchaseCountsAsync(userId, productId, todayUtcStart, weekStartUtc)` — 3개를 한번에 조회하는 통합 메서드
4. 위 인터페이스들의 EF 구현체 작성 (`Infrastructure/Repositories/`)
5. DI 등록 (`AppExtensions.cs`)

### 설계 판단
- 구매 횟수 조회를 개별 3쿼리로 할지, 한방 쿼리로 할지
  → **한방 쿼리 추천.** 구매 시 매번 3번 DB 왕복은 불필요. `GetPurchaseCountsAsync`로 daily/weekly/total을 한 번에 반환.

### 확인 포인트
- DI 컨테이너에서 resolve 되는가 (앱 기동 시 에러 없는가)

---

## Step 3. 비즈니스 로직

### 3-A. ShopService (상점/상품 CRUD)

단순 CRUD 서비스. 운영툴 Admin API가 호출한다.

```
IShopService
├── GetAllAsync(filter) → PagedResult<ShopDto>
├── GetByIdAsync(id) → ShopDetailDto (상품 포함)
├── CreateAsync(req) → ShopDto
├── UpdateAsync(id, req) → ShopDto
├── DeleteAsync(id)
├── AddProductAsync(shopId, req) → ShopProductDto
├── UpdateProductAsync(shopId, productId, req) → ShopProductDto
├── DeleteProductAsync(shopId, productId)
└── GetPurchaseLogsAsync(filter) → PagedResult<PurchaseLogDto>
```

### 3-B. PurchaseService (구매 로직)

**이 프로젝트의 핵심.** 기존 `WalletService`, `UserInventoryService`를 조합한다.

```
IPurchaseService
├── GetShopListForUserAsync(userId) → 상점 목록 + 유저별 구매 현황
└── PurchaseAsync(userId, shopProductId, quantity) → PurchaseResult
```

`PurchaseAsync` 내부 흐름:
```
1. Redis 분산 락 획득 (lock:shop:purchase:{userId})
2. 검증 체인 (상점 → 상품 → 구매제한 → 잔액)
3. UnitOfWork.ExecuteInTransactionAsync 내에서:
   a. WalletService.SpendAsync (재화 차감)
   b. UserInventoryService.GrantAsync (아이템 지급)
   c. UserPurchaseLog 저장
4. 락 해제
5. 결과 반환 (잔액, 아이템 수량, 갱신된 구매 횟수)
```

### 의존성
- `IShopRepository`
- `IShopProductRepository`
- `IUserPurchaseLogRepository`
- `IWalletService` (기존)
- `IUserInventoryService` (기존)
- `ICurrencyRepository` (기존 — 코드→ID 변환용)
- `IDistributedLock` (기존)
- `IUnitOfWork` (기존)
- `IClock` (기존)

### 확인 포인트
- 정상 구매 시 재화 차감 + 아이템 지급 + 로그 기록이 모두 되는가
- 잔액 부족 시 아무것도 변경 안 되는가 (트랜잭션 롤백)
- 구매 제한 초과 시 적절한 에러 코드 반환하는가

---

## Step 4. Admin API (JSON REST)

### 할 일
`AdminShopController` 작성 — 기존 Admin 컨트롤러 패턴 따름.

```
[ApiController]
[Route("api/shop")]

GET    /api/shop                              → 상점 목록
GET    /api/shop/{id}                         → 상점 상세 (상품 포함)
POST   /api/shop                              → 상점 생성
PUT    /api/shop/{id}                         → 상점 수정
DELETE /api/shop/{id}                         → 상점 삭제

GET    /api/shop/{shopId}/products            → 상품 목록
POST   /api/shop/{shopId}/products            → 상품 추가
PUT    /api/shop/{shopId}/products/{pid}      → 상품 수정
DELETE /api/shop/{shopId}/products/{pid}      → 상품 삭제

GET    /api/shop/purchase-logs                → 구매 기록 조회
```

### 확인 포인트
- Swagger에서 각 엔드포인트 호출해서 정상 동작 확인
- 상점 생성 → 상품 추가 → 조회까지 Swagger로 한 사이클 돌려보기

---

## Step 5. 운영툴 화면

### 할 일
AdminTool 프로젝트에 상점 관리 페이지 추가.
기존 Items 관리 페이지 패턴을 그대로 따른다.

| 페이지 | 경로 | 설명 |
|--------|------|------|
| 상점 목록 | `/admin/shop` | 필터(유형, 활성), 검색, 페이징 |
| 상점 생성 | `/admin/shop/new` | 코드, 이름, 유형, 기간, 활성 |
| 상점 수정 | `/admin/shop/{id}` | + 하단에 상품 목록 테이블 |
| 상품 추가/수정 | 상점 수정 페이지 내 | 아이템/재화 드롭다운, 가격, 구매제한 |
| 구매 기록 | `/admin/shop/logs` | 유저ID/상품별 검색 |

### 사이드바
`_Sidebar.cshtml`에 "Shop" 링크 추가

---

### ★ 1차 검증 (여기까지 하면 가능)

운영툴에서 아래 시나리오를 수동으로 테스트:

```
1. 상점 생성 (일반 상점 1개, 기간한정 상점 1개)
2. 각 상점에 상품 2~3개 추가 (아이템, 재화, 가격, 구매제한 설정)
3. 상점 목록에서 필터/검색 동작 확인
4. 상품 수정/삭제 확인
5. (선택) Swagger에서 PurchaseService 호출하여 구매 테스트
   - 테스트용 유저에게 재화 지급 → 구매 → 잔액/인벤토리/구매기록 확인
   - 구매 제한 초과 테스트
   - 잔액 부족 테스트
6. 구매 기록 조회 페이지에서 로그 확인
```

---

## Step 6. Game API (Protobuf)

> 1차 검증 이후, 클라이언트 연동이 필요할 때 진행

### 할 일
1. `shop.proto` 작성 (`Protocol/Proto/`)
2. Proto 빌드 → C# 코드 생성
3. `GameShopController` 작성
   - `GET /api/pb/shop` — 활성 상점 + 유저별 구매 현황
   - `GET /api/pb/shop/{shopId}` — 특정 상점 상세
   - `POST /api/pb/shop/purchase` — 구매
4. Protobuf ↔ DTO 매퍼 작성

---

## Step 7. (나중) 단위 테스트

PurchaseService의 핵심 분기를 테스트:

| 케이스 | 기대 결과 |
|--------|----------|
| 정상 구매 | 재화 차감 + 아이템 지급 + 로그 기록 |
| 잔액 부족 | `INSUFFICIENT_CURRENCY`, 아무것도 변경 안 됨 |
| 일일 제한 초과 | `DAILY_LIMIT_EXCEEDED` |
| 주간 제한 초과 | `WEEKLY_LIMIT_EXCEEDED` |
| 총 제한 초과 | `TOTAL_LIMIT_EXCEEDED` |
| 비활성 상점 | `SHOP_NOT_ACTIVE` |
| 기간 외 상점 | `SHOP_NOT_IN_PERIOD` |
| 동시 구매 요청 | `PURCHASE_IN_PROGRESS` (락 실패) |

---

## 파일 생성 순서 요약

```
[이미 완료]
  Domain/Enum/ShopType.cs
  Domain/Entities/Shop/Shop.cs
  Domain/Entities/Shop/ShopProduct.cs
  Domain/Entities/Shop/UserPurchaseLog.cs
  Infrastructure/Persistence/Configurations/Shop/ShopConfiguration.cs
  Infrastructure/Persistence/Configurations/Shop/ShopProductConfiguration.cs
  Infrastructure/Persistence/Configurations/Shop/UserPurchaseLogConfiguration.cs

[Step 1 — 남은 것]
  Infrastructure/Persistence/GameDBContext.cs              ← 수정 (DbSet + Config 등록)
  Migration 생성/적용

[Step 2]
  Application/Repositories/IShopRepository.cs
  Application/Repositories/IShopProductRepository.cs
  Application/Repositories/IUserPurchaseLogRepository.cs
  Infrastructure/Repositories/ShopRepository.cs
  Infrastructure/Repositories/ShopProductRepository.cs
  Infrastructure/Repositories/UserPurchaseLogRepository.cs

[Step 3]
  Application/Shop/Dtos.cs
  Application/Shop/Requests.cs
  Application/Shop/IShopService.cs
  Application/Shop/ShopService.cs
  Application/Shop/IPurchaseService.cs
  Application/Shop/PurchaseService.cs

[Step 4]
  Presentation/Controllers/Admin/AdminShopController.cs

[Step 5]
  AdminTool/Models/ShopVm.cs
  AdminTool/Controllers/ShopController.cs
  AdminTool/Views/Shop/Index.cshtml
  AdminTool/Views/Shop/Create.cshtml
  AdminTool/Views/Shop/Edit.cshtml
  AdminTool/Views/Shop/Logs.cshtml
  AdminTool/Views/Shared/_Sidebar.cshtml                  ← 수정

[Step 6]
  Protocol/Proto/shop.proto
  Presentation/Controllers/Game/GameShopController.cs

[공통]
  Presentation/Extensions/AppExtensions.cs                ← 수정 (DI 등록)
```
