using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemEffectEntry
{
    public string effect;
    public float value;
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name; 
    public List<ItemEffectEntry> effects;
    public string specialEffect;
    public string description;
}

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> items;
    public List<ItemData> availableItems;
    public TextAsset testItems;
    public static ItemDatabase Instance;

    private void Awake()
    {
        Instance = this;
        LoadItems();
    }

    void LoadItems()
    {
        var jsonText = testItems.text;
        items = JsonUtility.FromJson<Wrapper>(jsonText).items;
        availableItems = items;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<ItemData> items;
    }

    public ItemData GetItemById(int id)
    {
        return items.Find(i => i.id == id);
    }

   
}
