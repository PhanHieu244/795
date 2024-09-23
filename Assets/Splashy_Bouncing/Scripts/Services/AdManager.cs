using System.Collections.Generic;
using UnityEngine;

enum RewardedAdType
{
    UNITY,
    ADMOB,
}

enum BannerAdType
{
    NONE,
    UNITY,
    ADMOB,
}

enum InterstitialAdType
{
    UNITY,
    ADMOB,
}

[System.Serializable]
class ShowAdConfig
{
    public GameState GameStateForShowingAd = GameState.GameOver;
    public int GameStateCountForShowingAd = 3;
    public float ShowingAdDelay = 0.2f;
    public List<InterstitialAdType> ListInterstitialAdType = new List<InterstitialAdType>();
}


namespace OnefallGames
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; set; }

        [Header("Show Banner Ad config")]
        [SerializeField] private BannerAdType bannerAdType = BannerAdType.NONE;
        [SerializeField] private float showingBannerAdDelay = 0.5f;


        [Header("Show Interstitial Ad Config")]
        [SerializeField] private List<ShowAdConfig> listShowInterstitialAdConfig = new List<ShowAdConfig>();

        [Header("Show Rewarded Video Ad Config")]
        [SerializeField] private float showingRewardedVideoAdDelay = 0.2f;
        [SerializeField] private List<RewardedAdType> listRewardedAdType = new List<RewardedAdType>();

        [Header("Rewarded Coins Config")]
        [SerializeField] private int minRewardedCoins = 40;
        [SerializeField] private int maxRewardedCoins = 80;
        [SerializeField] private float rewardDelay = 0.2f;

        private List<int> listShowAdCount = new List<int>();
        private RewardedAdType readyAdType = RewardedAdType.UNITY;

        private bool isCalledback = false;
        private bool isRewarded = false;
        private void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
        }

        private void Awake()
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


        // Use this for initialization
        void Start()
        {
            foreach (ShowAdConfig o in listShowInterstitialAdConfig)
            {
                listShowAdCount.Add(o.GameStateCountForShowingAd);
            }

        }

        private void Update()
        {
            if (isCalledback)
            {
                isCalledback = false;
                if (isRewarded)
                {
                    if (GameManager.Instance.GameState == GameState.Revive)
                    {
                        GameManager.Instance.SetContinueGame();
                    }
                    else
                    {
                        int coins = Random.Range(minRewardedCoins, maxRewardedCoins) / 5 * 5;
                        UIManager.Instance.StartReward(rewardDelay, coins);
                    }
                }
                else
                {
                    if (GameManager.Instance.GameState == GameState.Revive)
                        GameManager.Instance.GameOver();
                }
            }
        }


        private void GameManager_GameStateChanged(GameState obj)
        {
        }


        /// <summary>
        /// Determines whether rewarded video ad is ready.
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedVideoAdReady()
        {
            return false;
        }


        /// <summary>
        /// Show the rewarded video ad with delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowRewardedVideoAd()
        {
            
        }

        public void OnRewardedVideoClosed(bool isFinishedVideo)
        {
            isCalledback = true;
            isRewarded = isFinishedVideo;
        }
    }
}
