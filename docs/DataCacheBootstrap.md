# E. 데이터 캐시 & 부팅 최적화

## 캐시 구조

앱 부팅 시 마스터 데이터와 이미지를 미리 캐시해두고, 이후 모든 화면에서 서버 요청 없이 로컬에서 즉시 조회한다. 모든 캐시는 `DontDestroyOnLoad` Singleton으로 앱 종료 전까지 유지된다.

| 캐시 | 내용 |
|------|------|
| MasterDataCache | 희귀도·원소·역할 메타데이터 + 아이콘/초상화 이미지 |
| CharacterCache | 캐릭터 마스터 데이터 |
| ItemCache | 아이템 데이터 |
| BattleContentsCache | 챕터·스테이지·스킬 데이터 |
| UIImageCache | UI 스프라이트 (Addressables) |

---

## 부팅 최적화 - 2단계 병렬 로드

```
Phase 1 (로비 진입 전, 완료까지 대기)       Phase 2 (백그라운드)
    MasterData  ─┐                               Skill          ─┐
    Item        ─┤ 병렬                           BattleContents ─┤ 병렬
    Character   ─┤                               Monster        ─┘
    UIImage     ─┘
         ↓
    TryAutoLogin → 로비 진입
```

Phase 1만 완료되면 로비에 진입할 수 있고, Phase 2는 로비 사용 중 백그라운드에서 완료된다.

각 캐시는 `RunParallel()`로 동시에 로드하며, 카운터 방식으로 완료를 감지한다.

```csharp
IEnumerator RunParallel((IEnumerator routine, string label)[] routines)
{
    int remaining = routines.Length;
    foreach (var (routine, label) in routines)
        StartCoroutine(WrapTimed(routine, label, () => remaining--));
    yield return new WaitUntil(() => remaining <= 0);
}
```

`WrapTimed`로 각 캐시 로드 시간을 측정해 부팅 병목을 파악할 수 있다.

---

## 런타임 캐시 갱신

서버 응답값을 로컬 캐시에 직접 반영한다. 재조회 API 호출 없음.

**스테이지 클리어**
```
FinishCombatResponsePb 수신
    ├─ 재화(Gem·Gold·Token) → GameState.ApplyProfile()
    │                          → OnCurrencyChanged 이벤트 → UI 자동 갱신
    └─ 별점·진행도          → StageProgressManager.ApplyClear()
```

**가챠**
```
GachaDrawResultPb 수신
    ├─ 재화 차감   → GameState.ApplyProfile(afterProfile)
    └─ 신규 캐릭터 → UserData.AddOrUpdateCharacter()
                     → UpdatedAt 타임스탬프로 오래된 데이터 덮어쓰기 방지
                     (중복 획득 시 파편 처리, isNew=false는 캐시 갱신 없음)
```
