using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using BansheeGz.BGDatabase;

/// <summary>
/// BG Database의 Dialog 엔티티들을 DialogSO/ChoiceSO로 변환하는 커스텀 에디터 윈도우
/// </summary>
public class BgDbToDialogSoConverterWindow : EditorWindow
{
    #region 설정 필드
    private string m_outputFolder = "Assets/DialogSOs";
    private DialogDatabaseSO m_targetDatabaseSo;
    private bool m_overwriteExisting = true;
    private bool m_addToDatabase = true;
    private bool m_verboseLog = true;
    #endregion

    [MenuItem("Tools/BG DB → Create DialogSO")]
    private static void OpenWindow()
    {
        var window = GetWindow<BgDbToDialogSoConverterWindow>(true, "BG DB -> DialogSO Converter");
        window.minSize = new Vector2(520, 220);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BG Database → DialogSO 변환기", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("출력 폴더 (Assets/에서 시작)", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        m_outputFolder = EditorGUILayout.TextField(m_outputFolder);
        if (GUILayout.Button("폴더 선택", GUILayout.Width(100)))
        {
            string select = EditorUtility.OpenFolderPanel("Select output folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(select))
            {
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
        m_targetDatabaseSo = (DialogDatabaseSO)EditorGUILayout.ObjectField("Target DialogDatabaseSO (선택)", m_targetDatabaseSo, typeof(DialogDatabaseSO), false);
        m_addToDatabase = EditorGUILayout.ToggleLeft("생성된 DialogSO들을 Target Database에 추가", m_addToDatabase);
        m_overwriteExisting = EditorGUILayout.ToggleLeft("기존 DialogSO/ChoiceSO 덮어쓰기", m_overwriteExisting);
        m_verboseLog = EditorGUILayout.ToggleLeft("상세 로그 출력", m_verboseLog);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("변환 실행", GUILayout.Height(38)))
        {
            ConvertAllBgDialogToSo();
        }

        if (GUILayout.Button("새 DatabaseSO 생성", GUILayout.Height(38), GUILayout.Width(180)))
        {
            CreateNewDatabaseSo();
        }
        EditorGUILayout.EndHorizontal();
    }

    #region 변환 로직
    private void ConvertAllBgDialogToSo()
    {
        try
        {
            // DialogSO와 DialogChoiceSO를 저장할 폴더
            string dialogsOutputFolder = m_outputFolder.Replace("\\", "/");
            string choicesOutputFolder = Path.Combine(m_outputFolder, "Choices").Replace("\\", "/");

            // 폴더가 없으면 생성
            if (!Directory.Exists(dialogsOutputFolder)) AssetDatabase.CreateFolder(Path.GetDirectoryName(dialogsOutputFolder), Path.GetFileName(dialogsOutputFolder));
            if (!Directory.Exists(choicesOutputFolder)) AssetDatabase.CreateFolder(dialogsOutputFolder, "Choices");
            AssetDatabase.Refresh();

            var entities = GatherAllDialogEntities();
            if (entities.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "BG Database에서 Dialog 엔티티를 찾지 못했습니다.", "확인");
                return;
            }

            // Target DatabaseSO가 없는 경우 처리
            if (m_addToDatabase && m_targetDatabaseSo == null)
            {
                if (!EditorUtility.DisplayDialog("DatabaseSO 미지정", "Target DialogDatabaseSO가 지정되어 있지 않습니다. 변환 후 DialogDatabaseSO를 자동으로 생성하시겠습니까?", "네", "아니요"))
                {
                    m_addToDatabase = false;
                }
                else
                {
                    CreateNewDatabaseSo();
                }
            }

            List<DialogSO> createdSoList = new List<DialogSO>();
            int total = entities.Count;
            int processed = 0;

            for (int i = 0; i < entities.Count; i++)
            {
                EditorUtility.DisplayProgressBar("BG DB → DialogSO 변환중...", $"처리중: {i + 1}/{total}", (float)(i) / total);
                var e = entities[i];

                var dialogEntity = (DB_GameDialog)e;
                int dialogId = dialogEntity.DialogId;
                string charName = dialogEntity.name;

                string safeName = SanitizeFileName($"{dialogId}_{charName}.asset");
                string assetPath = Path.Combine(dialogsOutputFolder, safeName).Replace("\\", "/");

                DialogSO existing = AssetDatabase.LoadAssetAtPath<DialogSO>(assetPath);
                DialogSO so = null;

                if (existing != null && m_overwriteExisting)
                {
                    so = existing;
                    PopulateDialogSOFromEntity(so, e, choicesOutputFolder);
                }
                else if (existing != null && !m_overwriteExisting)
                {
                    if (m_verboseLog) Debug.Log($"이미 존재 (덮어쓰기 OFF): {assetPath}");
                    so = existing; // 기존 SO를 리스트에 추가
                }
                else
                {
                    so = ScriptableObject.CreateInstance<DialogSO>();
                    PopulateDialogSOFromEntity(so, e, choicesOutputFolder); // SO 생성 후 데이터 채우기
                    AssetDatabase.CreateAsset(so, AssetDatabase.GenerateUniqueAssetPath(assetPath));
                }

                EditorUtility.SetDirty(so);
                createdSoList.Add(so);
                processed++;
            }

            // DB에 추가
            if (m_addToDatabase && m_targetDatabaseSo != null)
            {
                AddSOsToDialogDatabase(createdSoList);
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
            EditorUtility.DisplayDialog("오류", $"변환 중 오류가 발생했습니다. Console을 확인하세요.\n\n{ex.Message}", "확인");
            throw;
        }
    }

    /// <summary>
    /// BG DB의 메인 Dialog 엔티티들을 모두 수집합니다.
    /// </summary>
    private List<BGEntity> GatherAllDialogEntities()
    {
        var result = new List<BGEntity>();

        try
        {
            int count = DB_GameDialog.CountEntities;
            for (int i = 0; i < count; i++)
            {
                result.Add(DB_GameDialog.GetEntity(i));
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Dialog 테이블 로드 중 오류 (무시됨): {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// BG 엔티티 -> DialogSO 필드 매핑
    /// </summary>
    private void PopulateDialogSOFromEntity(DialogSO so, BGEntity entity, string choicesOutputFolder)
    {
        if (so == null || entity == null) return;

        var dialogEntity = (DB_GameDialog)entity;

        so.DialogId = dialogEntity.DialogId;
        so.name = dialogEntity.name;
        so.text = dialogEntity.text;
        so.nextId = dialogEntity.nextId;

        string prefabPath = dialogEntity.portraitPath;
        if (!string.IsNullOrEmpty(prefabPath))
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            so.PortraitPath = prefabAsset;
            if (prefabAsset == null && m_verboseLog)
            {
                Debug.LogWarning($"프리팹 로드 실패: {prefabPath} (엔티티: {so.name})");
            }
        }
        else
        {
            so.PortraitPath = null;
        }

        so.choices.Clear();

        var choiceEntities = dialogEntity.choices;

        if (choiceEntities == null || choiceEntities.Count == 0)
        {
            return; // 선택지가 없으면 종료
        }

        foreach (var choiceEntity in choiceEntities) 
        {
            string choiceAssetName = SanitizeFileName($"Choice_{so.DialogId}_{choiceEntity.ChoiceId}.asset");
            string choiceAssetPath = Path.Combine(choicesOutputFolder, choiceAssetName);

            DialogChoiceSO choiceSO = AssetDatabase.LoadAssetAtPath<DialogChoiceSO>(choiceAssetPath);

            // 덮어쓰기 옵션 확인
            if (choiceSO != null && !m_overwriteExisting)
            {
                so.choices.Add(choiceSO); // 기존 에셋을 리스트에 추가
                continue;
            }

            // 새 에셋 생성 또는 기존 에셋 덮어쓰기
            if (choiceSO == null)
            {
                choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
            }

            choiceSO.choiceText = choiceEntity.choiceText;
            choiceSO.choiceNextId = choiceEntity.choiceNextId;

            EditorUtility.SetDirty(choiceSO);
            so.choices.Add(choiceSO);
        }
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

    // 생성된 SO들을 DialogDatabaseSO에 추가
    private void AddSOsToDialogDatabase(List<DialogSO> createdList)
    {
        if (m_targetDatabaseSo == null) return;

        Undo.RecordObject(m_targetDatabaseSo, "Add DialogSOs to Database");
        m_targetDatabaseSo.Initialize();

        foreach (var so in createdList)
        {
            if (so == null) continue;

            var existingById = m_targetDatabaseSo.GetDialogById(so.DialogId);

            if (existingById != null)
            {
                if (m_overwriteExisting)
                {
                    // 기존 항목 교체
                    int idx = m_targetDatabaseSo.dialogs.IndexOf(existingById);
                    if (idx >= 0)
                    {
                        m_targetDatabaseSo.dialogs[idx] = so;
                    }
                    else
                    {
                        m_targetDatabaseSo.dialogs.Add(so); // 인덱스를 못찾으면 그냥 추가
                    }
                }
            }
            else
            {
                m_targetDatabaseSo.dialogs.Add(so);
            }
        }

        m_targetDatabaseSo.Initialize(); // DB 내부 인덱스 사전 갱신
    }

    // 새 DatabaseSO 생성
    private void CreateNewDatabaseSo()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create DialogDatabaseSO", "DialogDatabaseSO", "asset", "Choose location to save DialogDatabaseSO", "Assets");
        if (string.IsNullOrEmpty(path)) return;

        var dbSo = ScriptableObject.CreateInstance<DialogDatabaseSO>();
        AssetDatabase.CreateAsset(dbSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        m_targetDatabaseSo = dbSo;
        EditorUtility.DisplayDialog("완료", "새 DialogDatabaseSO 를 생성했습니다.", "확인");
    }

    #endregion
}