
using UnityEngine;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    public TMP_Text coinText;
    private int coins = 0;
    public bool HasAllCoins()
    {
        return coins >= 3;
    }

    void Start()
    {
        UpdateText();
    }

    public void AddCoin()
    {
        coins++;
        UpdateText();
    }

    void UpdateText()
    {
        coinText.text = "Coins: " + coins;
    }
}
