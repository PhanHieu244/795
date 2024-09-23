using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { private set; get; }

    public static readonly string Selected_Character_Key = "ONEFALL_CURRENT_CHARACTER";

    public int SelectedIndex
    {
        get
        {
            return PlayerPrefs.GetInt(Selected_Character_Key, 0);
        }
        set
        {
            PlayerPrefs.SetInt(Selected_Character_Key, value);
            PlayerPrefs.Save();
        }
    }

    public GameObject[] characters;

    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

