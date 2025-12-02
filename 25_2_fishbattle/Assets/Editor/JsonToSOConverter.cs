#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static DialogSO;
using static DialogChoiceSO;

public enum ConversionType
{
    Dialog,
    DialogChoice,
}

public class JsonToSOConverter : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolder = "Assets/ScriptableObjects/";
    private bool createDatabase = true;
    private ConversionType conversionType = ConversionType.Dialog;
    [MenuItem("Tools/Convert Dialog JSON to SO")]
    public static void ShowWindow()
    {
        GetWindow<JsonToSOConverter>("JSON to Scriptable Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to scriptable Object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Sellect JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Sellect JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Select File : ", jsonFilePath);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type :", conversionType);
        if (conversionType == ConversionType.Dialog)
        {
            outputFolder = "Assets/ScriptableObjects/Dialog";
        }
        else if (conversionType == ConversionType.DialogChoice)
        {
            outputFolder = "Assets/ScriptableObjects/Character";
        }
        

        outputFolder = EditorGUILayout.TextField("Output Folder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Object"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a json file firest!", "OK");
                return;
            }

            switch (conversionType)
            {
                case ConversionType.Dialog:
                    ConvertJsonToDialogScriptableObjects();
                    break;
                case ConversionType.DialogChoice:
                    ConvertJsonToDialogChoiceScriptableObject();
                    break;
                
            }
            //ConvertJsonToItemScriptableObjects();
        }
    }
    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            List<DialogSO> dialogDataList = JsonConvert.DeserializeObject<List<DialogSO>>(jsonText);
            List<DialogSO> createdDialog = new List<DialogSO>();

            foreach (var dialogData in dialogDataList)
            {
                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                dialogSO.DialogId = dialogData.DialogId;
                dialogSO.name = dialogData.name;
                dialogSO.text = dialogData.text;
                dialogSO.nextId = dialogData.nextId;
                

                

                string assetPath = $"{outputFolder}/dialog{dialogData.DialogId.ToString("D4")}_{dialogData.name}.asset";
                AssetDatabase.CreateAsset(dialogSO, assetPath);

                dialogSO.name = $"dialog{dialogData.DialogId.ToString("D4")}+{dialogData.name}";
                createdDialog.Add(dialogSO);

                EditorUtility.SetDirty(dialogSO);
            }

            if (createDatabase && createdDialog.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createdDialog;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/dialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Convert Json : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    private void ConvertJsonToDialogChoiceScriptableObject()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            List<DialogChoiceSO> dialogChoiceDataList = JsonConvert.DeserializeObject<List<DialogChoiceSO>>(jsonText);
            List<DialogChoiceSO> createdDialogChoice = new List<DialogChoiceSO>();

            foreach (var dialogChoiceData in dialogChoiceDataList)
            {
                DialogChoiceSO dialogChoiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();

                dialogChoiceSO.choiceId = dialogChoiceData.choiceId;
                dialogChoiceSO.choiceText = dialogChoiceData.choiceText;
                dialogChoiceSO.choiceNextId = dialogChoiceData.choiceNextId;

                string assetPath = $"{outputFolder}/dialogChoice_{dialogChoiceData.choiceId.ToString("D4")}_{dialogChoiceData.name}.asset";
                AssetDatabase.CreateAsset(dialogChoiceSO, assetPath);

                dialogChoiceSO.name = $"dialogChoice_{dialogChoiceData.choiceId.ToString("D4")}+{dialogChoiceData.name}";
                createdDialogChoice.Add(dialogChoiceSO);

                EditorUtility.SetDirty(dialogChoiceSO);
            }

            if (createDatabase && createdDialogChoice.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                //database.dialogchoice = createdDialogChoice;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/dialogChoiceDatabase.asset");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Convert Json : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}
#endif
