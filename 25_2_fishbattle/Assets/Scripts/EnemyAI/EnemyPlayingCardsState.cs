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
