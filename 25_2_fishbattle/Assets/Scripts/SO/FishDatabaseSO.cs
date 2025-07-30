using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDatabase", menuName = "FishDatabase")]
public class FishDatabaseSO : ScriptableObject
{
    public List<FishSO> fishItems = new List<FishSO>();        

    private Dictionary<int, FishSO> fishItemsById;                
    private Dictionary<string, FishSO> fishItemsByName;           

    public void Initialize()
    {
        fishItemsById = new Dictionary<int, FishSO>();             
        fishItemsByName = new Dictionary<string, FishSO>();

        foreach (var item in fishItems)                       
        {
            fishItemsById[item.Id] = item;
            fishItemsByName[item.FishName] = item;
        }
    }

    public FishSO GetItemById(int id)
    {
        if (fishItemsById == null)                                   
        {
            Initialize();
        }

        if (fishItemsById.TryGetValue(id, out FishSO item))
            return item;                                            

        return null;                                              
    }

    public FishSO GetItemByName(string name)
    {
        if (fishItemsByName == null)                                  
        {
            Initialize();
        }

        if (fishItemsByName.TryGetValue(name, out FishSO item))            
            return item;

        return null;
    }

    public List<FishSO> GetItemByType(FishHabitatType type)
    {
        return fishItems.FindAll(stage => stage.HabitatType == type);
    }
}
