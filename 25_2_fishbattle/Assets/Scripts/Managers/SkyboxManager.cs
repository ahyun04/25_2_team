using UnityEngine;

public class SkyboxManager : SingletonMono<SkyboxManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    private Material currentSkybox;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        // 이미 Instance가 존재하여 이 객체가 파괴될 운명이라면 초기화를 수행하지 않음
        if (Instance != this) return;

        // 초기 스카이박스 설정
        currentSkybox = RenderSettings.skybox;
    }

    #endregion

    #region Skybox 교체
    // 새로운 스카이박스를 즉시 적용하고 환경광을 업데이트합니다.
    public void SetSkybox(Material newSkybox)
    {
        // null이 아니고, 현재 스카이박스와 다른 경우에만 변경
        if (newSkybox != null && newSkybox != currentSkybox)
        {
            currentSkybox = newSkybox;
            RenderSettings.skybox = currentSkybox;

            // 스카이박스 변경 후 환경광(Ambient Light)을 강제로 업데이트합니다.
            // 이것을 하지 않으면 스카이박스만 바뀌고 조명은 어둡게 유지될 수 있습니다.
            DynamicGI.UpdateEnvironment();

            Debug.Log($"Skybox changed to: {newSkybox.name}");
        }
        else if (newSkybox == null)
        {
            Debug.LogWarning("Attempted to set a null skybox.");
        }
    }

    #endregion
}
