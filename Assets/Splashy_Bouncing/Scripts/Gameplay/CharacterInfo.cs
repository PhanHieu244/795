using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    [HideInInspector]
    public int characterSequenceNumber;
    [Header("This field must be different than the others")]
    public string characterName;
    [Header("Price of the character")]
    public int characterPrice;
    [Header("Is the character free ?")]
    public bool isFree = false;

    public bool IsUnlocked
    {
        get
        {
            return (isFree || PlayerPrefs.GetInt(characterName, 0) == 1);
        }
    }

    void Awake()
    {
        characterName = characterName.ToUpper();
    }

    public bool Unlock()
    {
        if (IsUnlocked)
            return true;

        if (OnefallGames.CoinManager.Instance.Coins >= characterPrice)
        {
            PlayerPrefs.SetInt(characterName, 1);
            PlayerPrefs.Save();
            OnefallGames.CoinManager.Instance.RemoveCoins(characterPrice);

            return true;
        }

        return false;
    }
}
