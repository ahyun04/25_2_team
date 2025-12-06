using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using BansheeGz.BGDatabase;

/// <summary>
/// BG Database의 DB_Fish 엔티티들을 FishSO로 변환하는 커스텀 에디터 윈도우
/// (통합된 테이블 대응 버전)
/// </summary>
public class BgDbToSoConverterWindow : EditorWindow
{
    #region 설정 필드
    // 출력 폴더 (Assets/ 부터 시작)
    private string m_outputFolder = "Assets/FishSOs";
    // DB에서 만들어진 SO들을 모아둘 FishDatabaseSO (선택 가능)
    private FishDatabaseSO m_targetDatabaseSo;
    // 기존 에셋 덮어쓰기 옵션
    private bool m_overwriteExisting = false;
    // 생성된 항목을 자동으로 Database에 추가
    private bool m_addToDatabase = true;
    // 로그 레벨
    private bool m_verboseLog = true;
    #endregion

    [MenuItem("Tools/BG DB → Create FishSO")]
    private static void OpenWindow()
    {
        var window = GetWindow<BgDbToSoConverterWindow>(true, "BG DB -> FishSO Converter");
        window.minSize = new Vector2(520, 220);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BG Database → FishSO 변환기 (통합본)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("출력 폴더 (Assets/에서 시작)", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        m_outputFolder = EditorGUILayout.TextField(m_outputFolder);
        if (GUILayout.Button("폴더 선택", GUILayout.Width(100)))
        {
            string select = EditorUtility.OpenFolderPanel("Select output folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(select))
            {
                // 절대경로 -> Assets 상대경로 변환
                if (select.StartsWith(Application.dataPath))
                {
                    m_outputFolder = "Assets" + select.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "선택한 폴더는 프로젝트의 Assets 폴더 아래여야 합니다.", "확인");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        m_targetDatabaseSo = (FishDatabaseSO)EditorGUILayout.ObjectField("Target FishDatabaseSO (선택)", m_targetDatabaseSo, typeof(FishDatabaseSO), false);
        m_addToDatabase = EditorGUILayout.ToggleLeft("생성된 FishSO들을 Target Database에 추가", m_addToDatabase);
        m_overwriteExisting = EditorGUILayout.ToggleLeft("기존 FishSO 덮어쓰기(같은 이름/ID 일치시)", m_overwriteExisting);
        m_verboseLog = EditorGUILayout.ToggleLeft("상세 로그 출력", m_verboseLog);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("변환 실행", GUILayout.Height(38)))
        {
            if (!Directory.Exists(m_outputFolder))
            {
                // 폴더 없으면 생성
                AssetDatabase.CreateFolder(Path.GetDirectoryName(m_outputFolder), Path.GetFileName(m_outputFolder));
                AssetDatabase.Refresh();
            }

            ConvertAllBgFishToSo();
        }

        if (GUILayout.Button("새 DatabaseSO 생성", GUILayout.Height(38), GUILayout.Width(180)))
        {
            CreateNewDatabaseSo();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("주의: 아이콘 문자열 경로는 Resources 폴더 경로를 기준으로 로드합니다. Addressables 사용시 LoadIcon 부분을 수정하세요.", MessageType.Info);
    }

    #region 변환 로직
    private void ConvertAllBgFishToSo()
    {
        try
        {
            var entities = GatherAllFishEntities();
            if (entities.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "BG Database에서 Fish 엔티티를 찾지 못했습니다.", "확인");
                return;
            }

            if (m_addToDatabase && m_targetDatabaseSo == null)
            {
                if (!EditorUtility.DisplayDialog("DatabaseSO 미지정", "Target FishDatabaseSO가 지정되어 있지 않습니다. 변환 후 FishDatabaseSO를 자동으로 생성하시겠습니까?", "네", "아니요"))
                {
                    m_addToDatabase = false;
                }
                else
                {
                    CreateNewDatabaseSo();
                }
            }

            // 플레이어/적 폴더 경로 정의
            string playerOutputFolder = Path.Combine(m_outputFolder, "Player").Replace("\\", "/");
            string enemyOutputFolder = Path.Combine(m_outputFolder, "Enemy").Replace("\\", "/");

            // 폴더가 없으면 생성
            if (!Directory.Exists(playerOutputFolder)) AssetDatabase.CreateFolder(m_outputFolder, "Player");
            if (!Directory.Exists(enemyOutputFolder)) AssetDatabase.CreateFolder(m_outputFolder, "Enemy");
            AssetDatabase.Refresh();


            List<FishSO> createdSoList = new List<FishSO>();
            int total = entities.Count;
            int processed = 0;

            for (int i = 0; i < entities.Count; i++)
            {
                EditorUtility.DisplayProgressBar("BG DB → FishSO 변환중...", $"처리중: {i + 1}/{total}", (float)(i) / total);
                var e = entities[i];

                // 임시 SO 생성 후 데이터 채우기 (경로 결정을 위해)
                FishSO tempSo = ScriptableObject.CreateInstance<FishSO>();
                PopulateFishSOFromEntity(tempSo, e);

                // isPlayerCard 값을 기준으로 저장 경로 결정
                string targetFolder = tempSo.IsPlayerCard ? playerOutputFolder : enemyOutputFolder;
                string fishName = tempSo.Name.Trim().Replace(' ', '_');
                string safeName = SanitizeFileName($"{tempSo.FishId}_{fishName}.asset");
                string assetPath = Path.Combine(targetFolder, safeName).Replace("\\", "/");

                FishSO existing = AssetDatabase.LoadAssetAtPath<FishSO>(assetPath);
                FishSO so = null;

                if (existing != null && m_overwriteExisting)
                {
                    so = existing;
                    // 기존 SO에 데이터 덮어쓰기
                    PopulateFishSOFromEntity(so, e);
                }
                else if (existing != null && !m_overwriteExisting)
                {
                    if (m_verboseLog) Debug.Log($"이미 존재 (덮어쓰기 OFF): {assetPath}");
                    so = existing; // 기존 SO를 리스트에 추가
                }
                else
                {
                    // 새 SO 생성
                    so = tempSo;
                    AssetDatabase.CreateAsset(so, AssetDatabase.GenerateUniqueAssetPath(assetPath));
                }

                EditorUtility.SetDirty(so);
                createdSoList.Add(so);

                processed++;
            }

            // DB에 추가
            if (m_addToDatabase && m_targetDatabaseSo != null)
            {
                AddSOsToDatabase(createdSoList);
                EditorUtility.SetDirty(m_targetDatabaseSo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", $"변환 완료: 총 {processed}개 처리됨.", "확인");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"변환 중 오류: {ex}");
            throw;
        }
    }

    /// <summary>
    /// BG DB의 DB_Fish 엔티티들을 모두 수집합니다.
    /// 통합된 DB_Fish 테이블만 조회합니다.
    /// </summary>
    private List<DB_Fish> GatherAllFishEntities()
    {
        var result = new List<DB_Fish>();

        try
        {
            // DB_Fish 테이블 조회
            int count = DB_Fish.CountEntities;
            for (int i = 0; i < count; i++)
            {
                result.Add(DB_Fish.GetEntity(i));
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DB_Fish 테이블 조회 실패 (테이블이 없거나 비어있을 수 있음): {ex.Message}");
        }

        return result;
    }

    #endregion

    #region 헬퍼 메서드

    // 파일명 안전하게
    private string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "_");
        }
        return name;
    }

    // BG 엔티티 -> FishSO 필드 매핑
    private void PopulateFishSOFromEntity(FishSO so, DB_Fish entity)
    {
        if (so == null || entity == null) return;

        // 1. 기본 필드 매핑
        so.FishId = entity.FishId;
        so.Name = string.IsNullOrEmpty(entity.name) ? $"Fish_{entity.FishId}" : entity.name;
        so.Description = entity.Description;
        so.Skill_name = entity.Skill_name;
        so.Damage = entity.Damage;
        so.Heal = entity.Heal;
        so.Hp = entity.Hp;
        so.AbilityToAct = entity.AbilityToAct;
        so.Probability = entity.Probability;
        so.MaxStackSize = Mathf.Max(1, entity.MaxStackSize);
        so.IsPlayerCard = entity.IsPlayerCard;

        // 2. Habitat (String -> Enum 파싱)
        if (!string.IsNullOrEmpty(entity.Habitat))
        {
            if (Enum.TryParse(entity.Habitat, true, out FishHabitatType habitatEnum))
            {
                so.HabitatType = habitatEnum;
            }
            else
            {
                if (m_verboseLog) Debug.LogWarning($"Habitat 파싱 실패 ({entity.Habitat}). 기본값(Lake)으로 설정합니다. - 엔티티: {entity.name}");
                so.HabitatType = FishHabitatType.Lake; // 기본값
            }
        }
        else
        {
            so.HabitatType = FishHabitatType.Lake;
        }

        if (!string.IsNullOrEmpty(entity.Position))
        {
            if (Enum.TryParse(entity.Position, true, out Position positionEnum))
            {
                so.Position = positionEnum;
            }
            else
            {
                if (m_verboseLog) Debug.LogWarning($"Position 파싱 실패 ({entity.Position}). 기본값(Attack)으로 설정합니다. - 엔티티: {entity.name}");
                so.Position = Position.Attack; // 기본값
            }
        }
        else
        {
            so.Position = Position.Attack;
        }

        // 3. 아이콘 로드 (Resources)
        // entity.AbilityToAct_icon 경로 사용
        if (!string.IsNullOrEmpty(entity.AbilityToAct_icon))
        {
            var sprite = LoadIcon(entity.AbilityToAct_icon);
            if (sprite != null)
                so.Icon = sprite;
            else if (m_verboseLog)
                Debug.LogWarning($"아이콘 로드 실패: {entity.AbilityToAct_icon} (엔티티: {entity.name})");
        }

        // 4. 프리팹 로드 (Assets)
        // entity.Prefab 경로 사용
        string prefabPath = entity.Prefab;
        if (!string.IsNullOrEmpty(prefabPath))
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                so.Prefab = prefabAsset;
                if (m_verboseLog) Debug.Log($"프리팹 로드 성공: {prefabPath}");
            }
            else
            {
                so.Prefab = null;
                if (m_verboseLog) Debug.LogWarning($"프리팹 로드 실패: {prefabPath} (엔티티: {entity.name})");
            }
        }
        else
        {
            so.Prefab = null;
        }

        EditorUtility.SetDirty(so);
    }

    // 기본 Resources 기반 아이콘 로드 유틸
    private Sprite LoadIcon(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // 경로가 확장자 포함이면 제거
        string clean = Path.ChangeExtension(path, null);

        // Resources.Load는 Assets/Resources 내부 경로(확장자 제외)여야 함
        try
        {
            var sprite = Resources.Load<Sprite>(clean);
            return sprite;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LoadIcon 예외: {ex.Message}");
            return null;
        }
    }

    // 생성된 SO들을 FishDatabaseSO에 추가(중복 검사)
    private void AddSOsToDatabase(List<FishSO> createdList)
    {
        if (m_targetDatabaseSo == null) return;

        // Undo/Dirty 처리
        Undo.RecordObject(m_targetDatabaseSo, "Add FishSOs to Database");

        // 최초 초기화 루틴 호출
        m_targetDatabaseSo.Initialize();

        foreach (var so in createdList)
        {
            if (so == null) continue;

            // 중복 체크: Id 기반 혹은 Name 기반 검사
            var existingById = m_targetDatabaseSo.GetItemById(so.FishId);
            var existingByName = m_targetDatabaseSo.GetItemByName(so.Name);

            if (existingById != null || existingByName != null)
            {
                if (m_overwriteExisting)
                {
                    // 기존 항목이 있으면 교체
                    ReplaceExistingInDatabase(m_targetDatabaseSo, existingById ?? existingByName, so);
                }
                else
                {
                    if (m_verboseLog) Debug.Log($"Database에 이미 존재함(스킵): {so.Name} (Id:{so.FishId})");
                    continue;
                }
            }
            else
            {
                // 새로 추가
                m_targetDatabaseSo.fishItems.Add(so);
            }
        }

        // DB 내부 인덱스 사전 갱신
        m_targetDatabaseSo.Initialize();
    }

    // Database 내 기존 항목을 찾아 교체
    private void ReplaceExistingInDatabase(FishDatabaseSO db, FishSO existing, FishSO @new)
    {
        if (db == null || existing == null || @new == null) return;

        int idx = db.fishItems.IndexOf(existing);
        if (idx >= 0)
        {
            db.fishItems[idx] = @new;
            if (m_verboseLog) Debug.Log($"Database 항목 교체: {@new.Name} (Id:{@new.FishId})");
        }
        else
        {
            db.fishItems.Add(@new);
        }
    }
    #endregion

    #region 유틸: DatabaseSO 생성
    private void CreateNewDatabaseSo()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create FishDatabaseSO", "FishDatabaseSO", "asset", "Choose location to save FishDatabaseSO", "Assets");
        if (string.IsNullOrEmpty(path)) return;

        var dbSo = ScriptableObject.CreateInstance<FishDatabaseSO>();
        AssetDatabase.CreateAsset(dbSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        m_targetDatabaseSo = dbSo;
        EditorUtility.DisplayDialog("완료", "새 FishDatabaseSO 를 생성했습니다.", "확인");
    }
    #endregion
}