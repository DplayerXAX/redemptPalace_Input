using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Mammon : NPC
{

    [SerializeField] private GameObject storePanel;
    [SerializeField] private List<ItemPanel> itemDisplays;

    public static Mammon Instance { get; private set; }

    private void Awake()
    {
        /*
        itemDisplays.Add(GameObject.Find("itemArea").GetComponent<ItemPanel>());
        itemDisplays.Add(GameObject.Find("itemArea_1").GetComponent<ItemPanel>());
        itemDisplays.Add(GameObject.Find("itemArea_2").GetComponent<ItemPanel>());*/
    }

    void Start()
    {
        Instance = this;
        currentGID = "npc_Mammon_unseen";
        myName = "Mammon";
        //itemDisplays.
    }

    

    public void EndSelling() 
    {
        storePanel.SetActive(false);
        Player.Instance.PauseControl(true);
    }

    public void SellRandomItems()
    {
        Player.Instance.PauseControl(false);
        storePanel.SetActive(true);
        List<ItemData> available = ItemDatabase.Instance.availableItems;
        if (available.Count == 0)
        {
            return;
        }

        int count = Mathf.Min(3, available.Count);
        List<ItemData> toSell = new();

        while (toSell.Count < count)
        {
            var randomItem = available[Random.Range(0, available.Count)];
            if (!toSell.Contains(randomItem))
                toSell.Add(randomItem);
            //still need to improve
            itemDisplays[toSell.Count - 1].Renew(randomItem,100);
        }
    }

    void Update()
    {
        InteractDetect();
    }
}
