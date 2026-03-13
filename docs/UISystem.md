# D. UI 시스템 구조 (Popup / Pool)

## 설계 결정

모든 게임 컨텐츠를 **단일 LobbyScene에서 팝업으로 관리**하되,
3D 오브젝트가 많이 필요한 전투/가챠는 **별도 씬을 Additive 로드**로 분리한다.

```
LobbyScene (항상 유지)
    ├─ LoginPopup             로그인
    ├─ LobbyPopup             메인 로비
    ├─ InventoryPopup         인벤토리
    ├─ ShopPopup              상점
    ├─ AdventureLobbyPopup    스테이지 선택
    ├─ AdventureDetailPopup   보상 미리보기
    ├─ PartySetupPopup        파티 편성
    └─ BattleMapPopup         전투 UI

    + Additive Load (필요 시 로드 / 언로드)
        ├─ BattleMapScene     전투 3D 오브젝트
        └─ GachaScene         가챠 3D 연출
```

**분리 기준:** 2D UI 중심 컨텐츠 → 팝업 / 3D 오브젝트가 많은 컨텐츠 → Additive 씬

---

## 이점

**1. 씬 전환 없는 빠른 화면 이동**
로그인부터 전투 입장 전까지 씬 로드 없이 팝업 페이드 인/아웃으로 이동.

**2. 이전 상태 즉시 복원**
팝업 단위로 상태가 독립적이라 특정 팝업만 닫으면 아래 팝업이 그대로 유지됨.
(예: 파티 편성 취소 → 스테이지 선택 화면 즉시 복원)

**3. 3D 씬 분리로 메모리 효율 확보**
전투/가챠 씬은 필요할 때만 Additive 로드, 종료 시 언로드.
LobbyScene은 항상 유지되므로 전환 비용 없음.

**4. 풀링으로 재생성 비용 제거**
```
UIPrefabPool (씬 전체)     팝업 인스턴스 재사용
ObjectPool   (팝업 내부)   아이템 슬롯, 탭 등 자식 요소 재사용
```

---

## 트레이드 오프

**1. 팝업 메모리 누적**
한 번 열린 팝업은 풀에 보관되므로 컨텐츠가 늘어날수록 메모리에 누적됨.
자주 쓰지 않는 팝업은 풀에서 제거하는 정책이 별도로 필요.

**2. 팝업 간 의존성 관리**
팝업이 다른 팝업을 직접 열고 닫는 구조라 흐름이 복잡해지면
순서와 상태 관리가 어려워짐.

**3. 명시적 생명주기 관리 필요**
씬 전환 방식은 OnDestroy로 자동 정리되지만,
팝업 방식은 열기/닫기 시 초기화와 정리를 직접 처리해야 함.

---

## 구현 요약

### UIPopup (베이스 클래스)
- CanvasGroup 기반 페이드 인/아웃
- `Time.unscaledDeltaTime` 사용 → 일시정지 중에도 애니메이션 동작
- 서브클래스 훅: `OnBeforeShow / OnAfterShow / OnBeforeHide / OnAfterHide`

### UIPrefabPool (팝업 풀 매니저)
- Addressables 기반 비동기 프리팹 로드 및 캐시
- 키(Key) 기반 단일 인스턴스 보장 → 중복 열기 방지
- `_inflight` 딕셔너리로 동시 비동기 로드 중복 요청 방지

### 사용 예시
```csharp
// 열기
var popup = await popupPool.ShowPopupAsync<AdventureDetailPopup>("StageDetailUI", this.transform);
popup.Set(data);

// 닫기
await popupPool.HidePopupAsync("StageDetailUI", popup);
```
