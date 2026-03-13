# Stage & Reward System

## 전체 흐름

```
AdventureLobbyPopup       스테이지 목록 (챕터별, 잠금/해제 상태)
        ↓
AdventureDetailPopup      입장 전 보상 미리보기
        ↓
PartySetupPopup           파티 편성 및 서버 저장
        ↓
BattleMapManager          전투 시작 API → 전투 진행 → 종료 API
        ↓
BattleMapPopup            결과 화면 (획득 보상 표시)
        ↓
StageProgressManager      클라이언트 진행 상태 갱신
```

---

## 주요 클래스

| 클래스 | 위치 | 역할 |
|--------|------|------|
| `AdventureLobbyPopup` | Features/Battle/UI | 챕터/스테이지 목록 UI |
| `AdventureDetailPopup` | Features/Battle/UI | 스테이지 입장 전 상세 팝업 |
| `PartySetupPopup` | Features/Stage/UI | 파티 편성 UI |
| `BattleMapManager` | Features/Combat/Manager | 전투 흐름 총괄 |
| `BattleMapPopup` | Features/Combat/UI | 전투 중 UI / 결과 화면 |
| `StageProgressManager` | Shared/Data | 스테이지 진행 상태 캐시 |
| `CombatNetwork` | Features/Combat/Network | 전투 관련 API 호출 |

---

## 1. 스테이지 잠금 & 해제

챕터 내 스테이지는 순서대로만 개방된다.

- 클리어한 스테이지 → 재입장 가능, 별점 표시
- 첫 번째 미클리어 스테이지 → 입장 가능 (활성화)
- 이후 스테이지 → 잠금 (비활성화)

```csharp
// AdventureLobbyPopup.cs - RenderStagesForChapter()
int? firstLockedStageId = null;
foreach (var s in stages)
{
    if (!cleared.Contains(s.Id))
    {
        firstLockedStageId = s.Id;
        break;
    }
}

// Odd order → Row1, Even order → Row2
Transform parent = (s.Order % 2 == 1) ? Row1 : Row2;

bool isActive = cleared.Contains(s.Id) || firstLockedStageId == s.Id;
```

스테이지 버튼(`StageButtonPopup`)은 오브젝트 풀로 관리하며, 챕터 전환 시 반환 후 재사용한다.

---

## 2. 보상 구조

스테이지 보상은 두 종류로 구분된다.

| 구분 | 필드 | 지급 조건 | UI 색상 |
|------|------|----------|---------|
| 첫 클리어 보상 | `StagePb.FirstRewards` | 최초 클리어 1회 | 초록 |
| 일반 드롭 | `StagePb.Drops` | 매 클리어 | 흰색 |

입장 전(`AdventureDetailPopup`)과 결과 화면(`BattleMapPopup.ShowResult`) 모두 동일한 색상 구분으로 표시한다.

---

## 3. 전투 종료 & 클라이언트 갱신

전투 종료 시 `/api/pb/combat/{combatId}/finish` 응답(`FinishCombatResponsePb`)을 기준으로 클라이언트 상태를 즉시 갱신한다. 별도 재조회 없음.

```csharp
// BattleMapManager.cs - BattleFlow()

// 재화 갱신
var updatedProfile = currentProfile.Clone();
updatedProfile.Gem += (int)result.Gem;
updatedProfile.Gold += (int)result.Gold;
updatedProfile.Token += (int)result.Token;
GameState.Instance.ApplyProfile(updatedProfile);

// 스테이지 진행 상태 갱신
StageProgress.ApplyClear(stageId, result.Stars);
```

서버가 별점과 보상을 계산하며, 클라이언트는 결과를 신뢰하고 그대로 반영한다.

---

## 4. StageProgressManager (진행 상태 캐시)

로그인 시 전체 스테이지 진행 데이터를 서버에서 일괄 수신 후 로컬에 캐시한다.

```
_stageProgress      stageId → UserStageProgressPb
_byBattleType       battleType → 진행 목록
_chapterProgress    chapterId → ChapterProgressInfo (클리어 수 / 전체 수)
```

클리어 시 `ApplyClear(stageId, stars)`로 캐시를 즉시 업데이트하므로, 로비로 돌아왔을 때 서버 재조회 없이 UI가 갱신된 상태를 반영한다.

---

## 5. API

| 엔드포인트 | 설명 |
|-----------|------|
| `POST /api/pb/combat/start` | 전투 세션 생성, 초기 스냅샷 반환 |
| `POST /api/pb/combat/{id}/finish` | 전투 종료, 별점/보상 반환 |
