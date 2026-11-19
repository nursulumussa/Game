using UnityEngine;
using TMPro;
public class CoinCounter : MonoBehaviour
{
    public TMP_Text coinText;
    public bool HasAllCoins() => GameSession.Instance.GoalMet;
    void Start() {UpdateText();}
    public void AddCoin() {GameSession.Instance.AddCoins(GameData.Current.items.coinValue);UpdateText();}
    public void UpdateText()
    {
        if(coinText!=null)coinText.text="Coins: "+GameSession.Instance.Coins+(GameSession.Instance.Level!=null && GameSession.Instance.Level.requiredCoins>0?" / "+GameSession.Instance.Level.requiredCoins:"");
    }
}
