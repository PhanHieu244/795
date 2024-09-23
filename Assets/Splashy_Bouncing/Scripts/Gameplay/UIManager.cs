using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using OnefallGames;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager Instance { private set; get; }

    //Gameplay UI
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private Text bestScoreTxt;
    [SerializeField] private Text scoreTxt;
    [SerializeField] private Text coinsTxt;

    //Revive UI
    [SerializeField] private GameObject reviveUI;
    [SerializeField] private Image reviveCoverImg;

    //GameOver UI
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private RawImage shareImage;
    [SerializeField] private GameObject playBtn;
    [SerializeField] private GameObject restartBtn;
    [SerializeField] private Button dailyRewardBtn;
    [SerializeField] private Text rewardText;
    [SerializeField] private GameObject watchAdForCoinsBtn;
    [SerializeField] private GameObject soundOnBtn;
    [SerializeField] private GameObject soundOffBtn;
    [SerializeField] private GameObject shareBtn;

    //Rewarded coin UI
    [SerializeField] private RewardedCoinsController rewardedCoinsControl;

    //References
    [SerializeField] private AnimationClip settingBtns_Hide;
    [SerializeField] private AnimationClip settingBtns_Show;
    [SerializeField] private AnimationClip servicesBtns_Hide;
    [SerializeField] private AnimationClip servicesBtns_Show;
    [SerializeField] private Animator settingAnim = null;
    [SerializeField] private Animator servicesAnim = null;

    private void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }


    private void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    private void GameManager_GameStateChanged(GameState obj)
    {
        if (obj == GameState.GameOver)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
        else if (obj == GameState.Revive)
        {
            Invoke("ShowReviveUI", 0.5f);
        }
        else if (obj == GameState.Playing)
        {
            gameplayUI.SetActive(true);

            reviveUI.SetActive(false);
            gameOverUI.SetActive(false);
            rewardedCoinsControl.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }




    // Use this for initialization
    void Start () {

        if (!GameManager.isRestart) //This is the first load
        {
            gameplayUI.SetActive(false);
            reviveUI.SetActive(false);
            rewardedCoinsControl.gameObject.SetActive(false);

            restartBtn.SetActive(false);
            shareImage.gameObject.SetActive(false);
            dailyRewardBtn.gameObject.SetActive(false);
            watchAdForCoinsBtn.gameObject.SetActive(false);
            shareBtn.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {

        UpdateMuteButtons();

        scoreTxt.text = ScoreManager.Instance.Score.ToString();
        coinsTxt.text = CoinManager.Instance.Coins.ToString();
        bestScoreTxt.text = ScoreManager.Instance.BestScore.ToString();

        if (DailyRewardManager.Instance.CanRewardNow())
        {
            rewardText.text = "Grap";
            dailyRewardBtn.interactable = true;
        }
        else
        {
            string hours = DailyRewardManager.Instance.TimeUntilNextReward.Hours.ToString();
            string minutes = DailyRewardManager.Instance.TimeUntilNextReward.Minutes.ToString();
            string seconds = DailyRewardManager.Instance.TimeUntilNextReward.Seconds.ToString();
            rewardText.text = hours + ":" + minutes + ":" + seconds;
            dailyRewardBtn.interactable = false;
        }

    }


    ////////////////////////////Publish functions
  
    public void PlayButtonSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void NativeShareBtn()
    {
        ShareManager.Instance.NativeShare();
    }

    public void RateAppBtn()
    {
        Application.OpenURL(ShareManager.Instance.AppUrl);
    }


    public void TwitterShareBtn()
    {
        ShareManager.Instance.TwitterShare();
    }
    public void FacebookShareBtn()
    {
        ShareManager.Instance.FacebookShare();
    }

    public void BackBtn()
    {
        settingAnim.Play(settingBtns_Hide.name);
        servicesAnim.Play(servicesBtns_Show.name);
    }

    public void PlayBtn()
    {
        playBtn.SetActive(false);
        gameOverUI.SetActive(false);
        shareImage.gameObject.SetActive(false);

        GameManager.Instance.PlayGame();
    }

    public void RestartBtn(float delay)
    {
        GameManager.Instance.LoadScene(SceneManager.GetActiveScene().name, delay);
    }

    public void SettingBtn()
    {
        settingAnim.Play(settingBtns_Show.name);
        servicesAnim.Play(servicesBtns_Hide.name);
    }

    public void DailyRewardBtn()
    {
        DailyRewardManager.Instance.ResetNextRewardTime();
        StartReward(0.5f, DailyRewardManager.Instance.GetRandomReward());
    }

    public void CharacterBtn()
    {
        GameManager.Instance.LoadScene("Character", 0.5f);
    }

    public void WatchAdForCoinsBtn()
    {
        watchAdForCoinsBtn.gameObject.SetActive(false);
        AdManager.Instance.ShowRewardedVideoAd();
    }

    public void SkipReviveBtn()
    {
        reviveUI.SetActive(false);
        GameManager.Instance.GameOver();
    }

    public void ReviveBtn()
    {
        reviveUI.SetActive(false);
        AdManager.Instance.ShowRewardedVideoAd();
    }

    /////////////////////////////Private functions
    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }



    void ShowReviveUI()
    {
        reviveUI.SetActive(true);
        reviveCoverImg.fillAmount = 1;
        StartCoroutine(ReviveCountDown());
    }

    IEnumerator ReviveCountDown()
    {
        float t = 0;
        while (t < GameManager.Instance.ReviveWaitTime)
        {
            if (!reviveUI.activeInHierarchy)
                yield break;
            t += Time.deltaTime;
            float factor = t / GameManager.Instance.ReviveWaitTime;
            reviveCoverImg.fillAmount = Mathf.Lerp(1, 0, factor);
            yield return null;
        }
        reviveUI.SetActive(false);
        GameManager.Instance.GameOver();
    }



    void ShowGameOverUI()
    {
        gameplayUI.SetActive(false);
        reviveUI.SetActive(false);
        rewardedCoinsControl.gameObject.SetActive(false);
        gameOverUI.SetActive(true);

        shareImage.gameObject.SetActive(true);
        shareImage.texture = GameManager.Instance.LoadedScrenshot();

        playBtn.SetActive(false);
        restartBtn.SetActive(true);

        dailyRewardBtn.gameObject.SetActive(true);
        watchAdForCoinsBtn.gameObject.SetActive(AdManager.Instance.IsRewardedVideoAdReady());

        shareBtn.SetActive(true);
    }

    public void StartReward(float delay, int coins)
    {
        StartCoroutine(RewardingCoins(delay, coins));
    }

    IEnumerator RewardingCoins(float delay, int coins)
    {
        yield return new WaitForSeconds(delay);
        rewardedCoinsControl.gameObject.SetActive(true);
        rewardedCoinsControl.StartReward(coins);
    }
}
