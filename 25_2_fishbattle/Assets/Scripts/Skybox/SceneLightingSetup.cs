using UnityEngine;

public class SceneLightingSetup : MonoBehaviour
{
    #region 레퍼런스
    [Header("해당 씬 skybox")]
    public Material sceneSkybox;
    #endregion

    #region 초기화
    void Start()
    {
        if (SkyboxManager.Instance != null)
            SkyboxManager.Instance.SetSkybox(sceneSkybox);
        else
        {
            Debug.LogError("SkyboxManager가 씬에 존재하지 않습니다! 첫 씬에 SkyboxManager를 배치했는지 확인하세요.");

            // 매니저가 없으면 일단 이 씬의 설정이라도 직접 적용
            RenderSettings.skybox = sceneSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    #endregion
}