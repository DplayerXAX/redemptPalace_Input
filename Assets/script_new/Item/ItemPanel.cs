using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI title;
    public string buffDescript;
    ItemData myItem;
    int myPrice;
    [SerializeField] private TextMeshProUGUI descriptText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image img;

    public void Renew(ItemData item, int price) 
    {
        title.text = item.name;
        buffDescript = item.description;
        descriptText.text = item.description;
        priceText.text = $"${price}";
        myPrice = price;
        myItem = item;
    }

    public void BeBought() 
    {
        if (Player.Instance.TryPurchaseItem(myItem, myPrice)) 
        {
            this.gameObject.SetActive(false);
        }
    }

}
