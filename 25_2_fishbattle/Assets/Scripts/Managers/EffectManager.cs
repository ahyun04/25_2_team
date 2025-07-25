using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : SingletonMono<EffectManager>
{
    #region 클래스
    // 개별 이펙트 설정 정보
    [System.Serializable]
    public class Effect
    {
        public string _name;                // 이펙트 이름 (식별용)
        public GameObject _effectPrefab;    // 이펙트 프리팹
        public int _initialPoolSize = 5;    // 초기 풀 크기
    }

    // 풀에 대한 런타임 데이터
    public class EffectPoolData
    {
        public Queue<GameObject> poolQueue; // 대기 중인 이펙트 오브젝트
        public int currentSize;             // 현재 풀에 생성된 전체 개수

        public EffectPoolData(int initial)
        {
            poolQueue = new Queue<GameObject>();
            currentSize = initial;
        }
    }

    #endregion

    #region 레퍼런스
    protected override bool DontDestroy => true;

    [Header("References")]
    public Effect[] effects; // 인스펙터에서 설정된 이펙트 목록
    private Dictionary<string, EffectPoolData> _effectPools; // 이펙트 이름 → 풀 정보 매핑

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
    }

    #endregion

    #region 이펙트
    // 모든 이펙트 풀을 초기화
    private void InitializeEffectPools()
    {
        _effectPools = new Dictionary<string, EffectPoolData>();

        foreach (var effect in effects)
        {
            var poolData = new EffectPoolData(effect._initialPoolSize);

            // 초기 풀 크기만큼 생성 후 비활성화
            for (int i = 0; i < effect._initialPoolSize; i++)
            {
                GameObject obj = CreateNewInstance(effect._effectPrefab);
                poolData.poolQueue.Enqueue(obj);
            }

            _effectPools.Add(effect._name, poolData);
        }
    }

    // 이펙트 인스턴스를 생성하고 비활성화된 상태로 반환
    private GameObject CreateNewInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);
        instance.transform.SetParent(transform); // 계층 정리용 부모 설정
        return instance;
    }

    // 위치와 회전을 적용하고 이펙트 오브젝트를 활성화
    private void SetupEffectInstance(GameObject instance, Vector3 position, Quaternion rotation)
    {
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetActive(true);
    }

    // 외부에서 호출되는 이펙트 재생 함수
    public void PlayEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        if (!_effectPools.ContainsKey(effectName))
        {
            Debug.LogWarning($"Effect not found: {effectName}");
            return;
        }

        EffectPoolData poolData = _effectPools[effectName];
        GameObject effectInstance;

        // 큐에 남아있는 오브젝트 사용, 없으면 새로 생성
        if (poolData.poolQueue.Count > 0)
        {
            effectInstance = poolData.poolQueue.Dequeue();
        }
        else
        {
            var effectPrefab = effects.First(e => e._name == effectName)._effectPrefab;
            effectInstance = CreateNewInstance(effectPrefab);
            poolData.currentSize++;
        }

        SetupEffectInstance(effectInstance, position, rotation);
        StartCoroutine(ReturnToPoolAfterDelay(effectInstance, effectName));     // 자동 반환 처리
    }

    // 이펙트 재생 후 일정 시간 뒤에 풀로 반환
    private IEnumerator ReturnToPoolAfterDelay(GameObject instance, string effectName)
    {
        if (instance == null) yield break;

        // 파티클 지속 시간 계산
        ParticleSystem particle = instance.GetComponent<ParticleSystem>();
        float duration = particle != null ? particle.main.duration : 1f;

        yield return new WaitForSeconds(duration + 0.5f);                   // 여유 시간 포함

        if (instance != null && _effectPools.ContainsKey(effectName))
        {
            instance.SetActive(false);
            _effectPools[effectName].poolQueue.Enqueue(instance);           // 다시 풀로 반환
        }
    }

    #endregion
}