using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPlayingCardsState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
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
                if (context.cardManager.PlayCardFromHand(false, cardIndex))
                {
                    Debug.Log($"적 AI가 카드 [{cardToPlay.Name}]를 배치했습니다.");
                    yield return new WaitForSeconds(0.25f);
                }
                else break;
            }
            else break;
        }

        // 자신의 역할이 끝나면, 다음 상태로 전환
        context.ChangeState(new EnemyAttackingState());
    }
}
