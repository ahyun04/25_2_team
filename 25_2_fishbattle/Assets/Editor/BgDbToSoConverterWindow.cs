using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using BansheeGz.BGDatabase;

/// <summary>
/// BG Database의 Fish 엔티티들을 FishSO로 변환하는 커스텀 에디터 윈도우
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
        EditorGUILayout.LabelField("BG Database → FishSO 변환기", EditorStyles.boldLabel);
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

                // PopulateFishSOFromEntity는 이전에 호출된 곳에서 분리
                // 먼저 FishSO를 생성하고, 필드를 채운 다음, IsPlayerCard 값을 기반으로 경로를 결정
                FishSO tempSo = ScriptableObject.CreateInstance<FishSO>();
                PopulateFishSOFromEntity(tempSo, e, GetHabitatFromEntity(e), GetEntityId(e));

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
                    PopulateFishSOFromEntity(so, e, GetHabitatFromEntity(e), GetEntityId(e));
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
    /// BG DB의 Fish 엔티티들을 모두 수집합니다.
    /// DB 엔티티 타입에 맞게 여기에 추가/수정하세요.
    /// </summary>
    private List<BGEntity> GatherAllFishEntities()
    {
        var result = new List<BGEntity>();

        // FishLake
        try
        {
            int countLake = DB_FishLake.CountEntities;
            for (int i = 0; i < countLake; i++)
            {
                result.Add(DB_FishLake.GetEntity(i));
            }
        }
        catch (Exception) { /* 무시: 테이블 없을 수 있음 */ }

        // FishRiver
        try
        {
            int countRiver = DB_FishRiver.CountEntities;
            for (int i = 0; i < countRiver; i++)
            {
                result.Add(DB_FishRiver.GetEntity(i));
            }
        }
        catch (Exception) { }

        // FishOcean
        try
        {
            int countOcean = DB_FishOcean.CountEntities;
            for (int i = 0; i < countOcean; i++)
            {
                result.Add(DB_FishOcean.GetEntity(i));
            }
        }
        catch (Exception) { }

        return result;
    }

    #endregion

    #region 헬퍼 메서드
    // 엔티티에서 이름 추출. meta에 따라 캐스트해서 _name 필드 사용
    private string GetEntityName(BGEntity entity)
    {
        if (entity == null) return null;
        if (entity is DB_FishLake lake) return lake.name;
        if (entity is DB_FishRiver river) return river.name;
        if (entity is DB_FishOcean ocean) return ocean.name;
        return null;
    }

    /// <summary>
    /// 엔티티에서 가능한 한 실제 FishId를 추출합니다.
    /// 알려진 타입을 우선으로 시도하고, 없으면 리플렉션으로 후보 필드/프로퍼티를 검색합니다.
    /// 그래도 못찾으면 기존의 GetHashCode()를 최후 fallback으로 사용합니다.
    /// </summary>
    private int GetEntityId(BGEntity entity)
    {
        if (entity == null) return 0;

        // 알려진 BG 엔티티 타입에서 직접 읽기
        if (entity is DB_FishLake lake) return Mathf.Abs(lake.FishId);
        if (entity is DB_FishRiver river) return Mathf.Abs(river.FishId);
        if (entity is DB_FishOcean ocean) return Mathf.Abs(ocean.FishId);

        // 리플렉션 후보 검색
        try
        {
            var type = entity.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            string[] candidateNames = new[] { "_FishId", "FishId", "Id", "_id" };

            foreach (var name in candidateNames)
            {
                var field = type.GetField(name, flags);
                if (field != null)
                {
                    var val = field.GetValue(entity);
                    if (val is int vi) return Math.Abs(vi);
                    if (val is long vl) return (int)Math.Abs(vl);
                }

                var prop = type.GetProperty(name, flags);
                if (prop != null)
                {
                    var val = prop.GetValue(entity, null);
                    if (val is int pi) return Math.Abs(pi);
                    if (val is long pl) return (int)Math.Abs(pl);
                }
            }
        }
        catch (Exception ex)
        {
            if (m_verboseLog) Debug.LogWarning($"GetEntityId 리플렉션 실패: {ex.Message}");
        }

        // 최종 fallback: 해시값 (이전 동작 유지)
        try
        {
            return Math.Abs(entity.GetHashCode());
        }
        catch
        {
            return 0;
        }
    }

    // 엔티티의 소속 수역 타입 결정
    private FishHabitatType GetHabitatFromEntity(BGEntity entity)
    {
        if (entity is DB_FishLake) return FishHabitatType.Lake;
        if (entity is DB_FishRiver) return FishHabitatType.River;
        if (entity is DB_FishOcean) return FishHabitatType.Ocean;
        return FishHabitatType.Lake;
    }

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
    private void PopulateFishSOFromEntity(FishSO so, BGEntity entity, FishHabitatType habitat, int idFallback)
    {
        if (so == null || entity == null) return;

        // 기본 공통 필드
        int fishId = 0;
        string name = GetEntityName(entity) ?? $"Fish_{idFallback}";
        int hp = 0;
        int abilityToAct = 0;
        string abilityIconPath = null;
        string skillName = null;
        int damage = 0;
        int heal = 0;
        int support = 0;
        int probability = 0;
        string description = null;
        int maxStack = 1;
        float weight = 1f;
        bool isPlayerCard = false;
        bool isCheck = false;
        string prefabPath = null;

        // 각 엔티티 타입별로 캐스팅하여 필드 읽기
        if (entity is DB_FishLake lake)
        {
            fishId = lake.FishId;
            hp = lake.Hp;
            abilityToAct = lake.AbilityToAct;
            abilityIconPath = lake.AbilityToAct_icon;
            skillName = lake.Skill_name;
            damage = lake.Damage;
            heal = lake.Heal;
            support = lake.Support;
            probability = lake.Probability;
            description = lake.Description;
            maxStack = lake.MaxStackSize;
            weight = lake.Weight;
            isPlayerCard = lake.IsPlayerCard;
            isCheck = lake.Check;
            prefabPath = lake.Prefab;
        }
        else if (entity is DB_FishRiver river)
        {
            fishId = river.FishId;
            hp = river.Hp;
            abilityToAct = river.AbilityToAct;
            abilityIconPath = river.AbilityToAct_icon;
            skillName = river.Skill_name;
            damage = river.Damage;
            heal = river.Heal;
            support = river.Support;
            probability = river.Probability;
            description = river.Description;
            maxStack = river.MaxStackSize;
            weight = river.Weight;
            isPlayerCard = river.IsPlayerCard;
            isCheck = river.Check;
            prefabPath = river.Prefab;
        }
        else if (entity is DB_FishOcean ocean)
        {
            fishId = ocean.FishId;
            hp = ocean.Hp;
            abilityToAct = ocean.AbilityToAct;
            abilityIconPath = ocean.AbilityToAct_icon;
            skillName = ocean.Skill_name;
            damage = ocean.Damage;
            heal = ocean.Heal;
            support = ocean.Support;
            probability = ocean.Probability;
            description = ocean.Description;
            maxStack = ocean.MaxStackSize;
            weight = ocean.Weight;
            isPlayerCard = ocean.IsPlayerCard;
            isCheck = ocean.Check;
            prefabPath = ocean.Prefab;
        }

        // SO에 할당
        so.FishId = fishId;
        so.Name = name;
        so.Hp = hp;
        so.AbilityToAct = abilityToAct;
        so.Skill_name = skillName;
        so.Damage = damage;
        so.Heal = heal;
        so.Support = support;
        so.Probability = probability;
        so.Description = description;
        so.MaxStackSize = Mathf.Max(1, maxStack);
        so.Weight = Mathf.Max(0.0001f, weight);
        so.IsPlayerCard = isPlayerCard;
        so.IsCheck = isCheck;
        so.HabitatType = habitat;

        // 아이콘 로드 시도: 엔티티에 저장된 문자열 경로를 사용하여 Resources.Load<Sprite> 호출
        if (!string.IsNullOrEmpty(abilityIconPath))
        {
            var sprite = LoadIcon(abilityIconPath);
            if (sprite != null)
                so.Icon = sprite;
            else if (m_verboseLog)
                Debug.LogWarning($"아이콘 로드 실패: {abilityIconPath} (엔티티: {name})");
        }

        // 프리팹 로드 시도: Assets/ 경로를 사용하여 AssetDatabase.LoadAssetAtPath 호출
        if (!string.IsNullOrEmpty(prefabPath))
        {
            // AssetDatabase.LoadAssetAtPath는 Resources.Load와 달리 Assets/ 경로 그대로 사용
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                so.Prefab = prefabAsset;
                if (m_verboseLog) Debug.Log($"프리팹 로드 성공: {prefabPath}");
            }
            else
            {
                so.Prefab = null;
                if (m_verboseLog) Debug.LogWarning($"프리팹 로드 실패: {prefabPath} (엔티티: {name})");
            }
        }
        else
        {
            so.Prefab = null; // 경로가 비어있다면 null 할당
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

        // 최초 초기화 루틴 호출 (private 변수 채우기 위해)
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
                    // 기존 항목이 있으면 교체 (이 경우 리스트 내 교체 또는 참조 갱신 필요)
                    // 여기서는 fishItems 리스트에서 기존을 찾아 해당 index에 새로 할당
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
            // 혹은 이름/Id 매칭 실패 시 그냥 추가
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
