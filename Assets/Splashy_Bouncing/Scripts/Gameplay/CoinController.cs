using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnefallGames;

public class CoinController : MonoBehaviour {

    private void Update()
    {
        transform.Rotate(Vector3.up, 200 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
            CoinManager.Instance.AddCoins(1);
            GameManager.Instance.PlayCoinExplode(transform.position);
            gameObject.SetActive(false);
        }
    }
}
