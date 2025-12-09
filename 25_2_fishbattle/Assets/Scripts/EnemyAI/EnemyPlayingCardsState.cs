using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPlayingCardsState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        // AI가 어떤 카드를 낼지 '생각'하는 시간을 표현하는 딜레이
        yield return new WaitForSeconds(0.75f);

        FishUnit injuredUnit = context.cardManager.GetMostInjuredBattleUnit(false); // false = Enemy

        if (injuredUnit != null)
        {
            // 벤치에 있는 힐러 찾기
            List<FishUnit> benchHealers = new List<FishUnit>();
            foreach (var bench in context.cardManager.enemyBenchAreas)
            {
                foreach (var card in bench.GetOccupiedCards())
                {
                    if (card.TryGetComponent<FishUnit>(out var unit) && unit.CardData.Position == Position.Heal)
                    {
                        benchHealers.Add(unit);
                    }
                }
            }

            // 사용 가능한 힐러가 있다면
            foreach (var healer in benchHealers)
            {
                // AP 확인
                if (TurnManager.Instance.EnemyAP >= healer.CardData.AbilityToAct)
                {
                    // 힐 실행
                    if (TurnManager.Instance.SpendAP(false, healer.CardData.AbilityToAct))
                    {
                        Debug.Log($"적 AI가 벤치의 힐러 [{healer.CardData.Name}]를 사용하여 [{injuredUnit.CardData.Name}]을 치료합니다.");
                        injuredUnit.Heal(healer.CardData.Heal);

                        // 힐러 사용 연출 딜레이
                        yield return new WaitForSeconds(1.0f);

                        // 치료 후 더 이상 치료할 대상이 없으면 중단할 수도 있음 (여기선 1회만 시도 예시)
                        break;
                    }
                }
            }
        }

        Debug.Log("적 턴 시작. 현재 AP: " + TurnManager.Instance.EnemyAP);
        int cardsToPlay = 1;

        for (int i = 0; i < cardsToPlay; i++)
        {
            if (context.cardManager._enemyHandCards.Count == 0) break;

            FishSO cardToPlay = context.cardManager._enemyHandCards
                .Where(c => TurnManager.Instance.EnemyAP >= c.AbilityToAct)
                .OrderBy(c => c.AbilityToAct)
                .FirstOrDefault();

            if (cardToPlay != null)
            {
                int cardIndex = context.cardManager._enemyHandCards.IndexOf(cardToPlay);
                if (context.cardActionHandler.PlayCardFromHand(false, cardIndex))
                {
                    Debug.Log($"적 AI가 카드 [{cardToPlay.Name}]를 배치했습니다.");
                    yield return new WaitForSeconds(0.8f);
                }
                else break;
            }
            else break;
        }
        // 카드 내는 단계를 마치고, 공격 단계로 넘어가기 전의 짧은 간격(텀)을 위한 딜레이
        yield return new WaitForSeconds(0.5f);

        // 자신의 역할이 끝나면, 다음 상태로 전환
        context.ChangeState(new EnemyAttackingState());
    }
}
