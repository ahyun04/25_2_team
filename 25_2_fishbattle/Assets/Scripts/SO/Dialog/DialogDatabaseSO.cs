using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/DialogDatabaseSO")]
public class DialogDatabaseSO : ScriptableObject
{
    public List<DialogSO> dialogs = new List<DialogSO>();

    private Dictionary<int, DialogSO> dialogsById;

    public void Initialize()
    {
        dialogsById = new Dictionary<int, DialogSO>();

        foreach (var dialog in dialogs)
        {
            dialogsById[dialog.DialogId] = dialog;
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
