using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
    public Text pageNumber;

    void UpdatePageNumber()
    {
        pageNumber.text = InventoryDisplay.Instance.CurrentPage.ToString();
    }

    public void GoLeftPage()
    {
        if (InventoryDisplay.Instance.CurrentPage == 1)
        {
            InventoryDisplay.Instance.CurrentPage = InventoryDisplay.Instance.MaxPage;
        }
        else
        {
            InventoryDisplay.Instance.CurrentPage--;
        }
        UpdatePageNumber();

        InventoryDisplay.Instance.UpdateInventoryList();
    }

    public void GoRightPage()
    {
        if (InventoryDisplay.Instance.CurrentPage == InventoryDisplay.Instance.MaxPage)
        {
            InventoryDisplay.Instance.CurrentPage = 1;
        }
        else
        {
            InventoryDisplay.Instance.CurrentPage++;
        }
        UpdatePageNumber();

        InventoryDisplay.Instance.UpdateInventoryList();
    }
}
