using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/DialogDatabaseSO")]
public class DialogDatabaseSO : ScriptableObject
{
    public List<DialogSO> dialogs = new List<DialogSO>();
    public List<DialogChoiceSO> dialogchoice = new List<DialogChoiceSO>();

    private Dictionary<int, DialogSO> dialogsById;
    private Dictionary<string, DialogSO> dialogsByName;

    public void Initialize()
    {
        dialogsById = new Dictionary<int, DialogSO>();
        dialogsByName = new Dictionary<string, DialogSO>();

        foreach (var dialog in dialogs)
        {
            dialogsById[dialog.DialogId] = dialog;
            dialogsByName[dialog.name] = dialog;
        }
    }

    public DialogSO GetDialogById(int id)
    {
        if (dialogsById == null)
        {
            Initialize();
        }

        if (dialogsById.TryGetValue(id, out DialogSO dialog))
            return dialog;

        return null;
    }


}
