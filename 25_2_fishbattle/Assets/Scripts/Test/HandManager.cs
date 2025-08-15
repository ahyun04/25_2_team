using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handTransform;

    public float fanSpread = 5f;
    public float cardSpacing = 5f;
    public float verticalSpacing = 10f;

    public List<GameObject> cardHand = new List<GameObject>();

    void Start()
    {
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
        AddCardToHand();
    }

    private void Update()
    {
        UpdateHandVisuals();
    }

    public void AddCardToHand()
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardHand.Add(newCard);

        UpdateHandVisuals();
    }

    private void UpdateHandVisuals()
    {
        int cardCount = cardHand.Count;

        if (cardCount <= 1)
        {
            // 카드가 한 장일 때는 부채꼴 모양을 적용하지 않고 기본 위치로
            if (cardCount == 1)
            {
                cardHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                cardHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            return;
        }

        // 부채꼴 모양으로 위치 조정
        for (int i = 0; i < cardCount; i++)
        {
            // 0부터 1 사이로 정규화된 위치 (i가 0일 때 -1, 마지막 카드일 때 1)
            float normalizedPosition = (2f * i / (cardCount - 1f)) - 1f;

            // 회전 각도 계산
            float rotationAngle = normalizedPosition * fanSpread * -1f;

            // 수평 간격 계산
            float horizontalOffset = normalizedPosition * cardSpacing * 2f;

            // 수직 간격 계산 (포물선 형태)
            float verticalOffset = -verticalSpacing * (1f - (normalizedPosition * normalizedPosition));

            cardHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
            cardHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}
