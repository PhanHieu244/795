using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OnefallGames;


public class RewardedCoinsController : MonoBehaviour {

    [SerializeField]
    private GameObject sunbrust;
    [SerializeField]
    private Text coinTxt;



    public void StartReward(int coinAmount)
    {
        StartCoroutine(Rewarding(coinAmount));
    }
    
    IEnumerator Rewarding(int amount)
    {
        sunbrust.SetActive(false);

        //Reward coins
        for(int i = 1; i <= amount; i++)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
            CoinManager.Instance.AddCoins(1);
            coinTxt.text = i.ToString();
            yield return new WaitForSeconds(0.05f);
        }

        SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded);

        //Rotate the sunbrust
        sunbrust.SetActive(true);
        RectTransform rect = sunbrust.GetComponent<RectTransform>();
        float t = 0;
        while (t < 3f)
        {
            t += Time.deltaTime;
            rect.eulerAngles += new Vector3(0, 0, 100f * Time.deltaTime);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
