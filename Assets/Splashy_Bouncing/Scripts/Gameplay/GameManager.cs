using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnefallGames;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public enum GameState
{
    Prepare,
    Playing,
    Revive,
    GameOver,
}

[System.Serializable]
public struct MovingPlatformConfig
{
    public int MinScore;
    public int MaxScore;
    [Range(0f, 1f)] public float CoinFrequency;
    [Range(0f, 1f)] public float ObstacleFrequency;
    [Range(0f, 1f)] public float MovingFrequency;
    public float MinMovingAmount;
    public float MaxMovingAmount;
    public float MinMovingSpeed;
    public float MaxMovingSpeed;
    public LerpType[] LerpType;
}

public class GameManager : MonoBehaviour {

    public static GameManager Instance { private set; get; }
    public static event System.Action<GameState> GameStateChanged = delegate { };
    public static bool isRestart = false;

    public GameState GameState
    {
        get
        {
            return gameState;
        }
        private set
        {
            if (value != gameState)
            {
                gameState = value;
                GameStateChanged(gameState);
            }
        }
    }

    [Header("Gameplay Config")]
    public int pathSpace = 5;
    [SerializeField] private int groundNumber = 10;
    [SerializeField] private float originalXPathDistance = 1f;
    [SerializeField] private float maxXPathDistance = 4;
    [SerializeField] private float xPathDistanceIncreaseFactor = 0.1f;
    [SerializeField] private float xPathDistanceIncreaseDuration = 1f;
    [SerializeField] private float groundNormalScale = 2f;
    [SerializeField] private float groundLargeScale = 4f;
    [SerializeField] private float objectFadingTime = 1f;
    [SerializeField] private float colorBlendingTime = 0.5f;
    [SerializeField] private int reviveWaitTime = 3;
    [SerializeField] private int centerPointCount = 5;
    [SerializeField] private float pathMovingTime = 0.5f;
    [SerializeField] private float textMeshMovingUpSpeed = 5f;
    [SerializeField] private Color[] groundColors;
    [SerializeField] MovingPlatformConfig[] movingPlatformConfig;


    [Header("Gameplay References")]
    [SerializeField] private CameraController camControl;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject firstGround;
    [SerializeField] private GameObject fadingGroundPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject coinExplodePrefab;
    [SerializeField] private GameObject textMeshPrefab;
    [SerializeField] private GameObject colorSplashPrefab;

    public float GroundBottomPosition { private set; get; }
    public float GroundLargeScale { private set; get; }
    public float ObjectFadingTime { private set; get; }
    public int ReviveWaitTime { private set; get; }
    public bool IsRevived { private set; get; }


    private GameState gameState = GameState.GameOver;

    private List<PathController> listPathControl = new List<PathController>();
    private List<FadingGroundController> listFadingGroundControl = new List<FadingGroundController>();
    private List<CoinController> listCoinControl = new List<CoinController>();
    private List<GameObject> listObstacle = new List<GameObject>();
    private List<ParticleSystem> listCoinExplodeParticle = new List<ParticleSystem>();
    private List<TextMeshController> listTextMeshControl = new List<TextMeshController>();
    private List<ParticleSystem> listColorSplashParticle = new List<ParticleSystem>();
    private float currentZPos = 0;
    private float xPathDistance = 0;
    private int previousColorIndex = 0;
    private int centerCountTemp = 0;
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
        Application.targetFrameRate = 60;
        ScoreManager.Instance.Reset();

        //Fire event
        GameState = GameState.Prepare;
        gameState = GameState.Prepare;

        //Add another actions here

        //Set variables
        GroundBottomPosition = firstGround.transform.position.y;
        GroundLargeScale = groundLargeScale;
        ObjectFadingTime = objectFadingTime;
        ReviveWaitTime = reviveWaitTime;
        currentZPos = firstGround.transform.position.z;
        xPathDistance = originalXPathDistance;
        IsRevived = false;

        //Add first path to the list
        listPathControl.Add(firstGround.GetComponent<PathController>());

        //Create some path first
        for (int i = 0; i < groundNumber; i++)
        {
            float zPos = currentZPos + pathSpace;
            float xPos = 0;
            Vector3 pathPos = new Vector3(xPos, GroundBottomPosition, zPos);
            PathController pathControl = GetPathControl();
            pathControl.gameObject.transform.position = pathPos;
            pathControl.gameObject.SetActive(true);
            currentZPos = pathControl.transform.position.z;
        }

        if (isRestart)
            PlayGame();
    }

    /// <summary>
    /// Actual start the game
    /// </summary>
    public void PlayGame()
    {
        //Fire event
        GameState = GameState.Playing;
        gameState = GameState.Playing;

        //Add another actions here

        //Change grounds color
        if (!IsRevived)
            StartCoroutine(ChangingGroundColor());

        //Increase x path distance
        StartCoroutine(IncreaseXPathDistance());
    }


    /// <summary>
    /// Call Revive event
    /// </summary>
    public void Revive()
    {
        //Fire event
        GameState = GameState.Revive;
        gameState = GameState.Revive;

        //Add another actions here
    }


    /// <summary>
    /// Call GameOver event
    /// </summary>
    public void GameOver()
    {
        //Fire event
        GameState = GameState.GameOver;
        gameState = GameState.GameOver;

        //Add another actions here
        isRestart = true;
    }


    public void LoadScene(string sceneName, float delay)
    {
        StartCoroutine(LoadingScene(sceneName, delay));
    }

    private IEnumerator LoadingScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }


    //Get the inactive path object
    private PathController GetPathControl()
    {
        foreach (PathController o in listPathControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        PathController pathControl = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity).GetComponent<PathController>();
        listPathControl.Add(pathControl);
        pathControl.gameObject.SetActive(false);
        return pathControl;
    }


    //Get the inactive fading ground object
    private FadingGroundController GetFadingGround()
    {
        foreach(FadingGroundController o in listFadingGroundControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        FadingGroundController fadingGroundControl = Instantiate(fadingGroundPrefab, Vector3.zero, Quaternion.identity).GetComponent<FadingGroundController>();
        listFadingGroundControl.Add(fadingGroundControl);
        fadingGroundControl.gameObject.SetActive(false);
        return fadingGroundControl;
    }


    //Get the inactive coin object
    private CoinController GetCoinControl()
    {
        foreach (CoinController o in listCoinControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        CoinController coinControl = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity).GetComponent<CoinController>();
        listCoinControl.Add(coinControl);
        coinControl.gameObject.SetActive(false);
        return coinControl;
    }


    //Get the inactive obstacle
    private GameObject GetObstacle()
    {
        foreach(GameObject o in listObstacle)
        {
            if (!o.activeInHierarchy)
                return o;
        }
        GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);
        obstacle.SetActive(false);
        listObstacle.Add(obstacle);
        return obstacle;
    }

    //Get an inactive TextMeshControl
    private TextMeshController GetTextMeshControl()
    {
        //Find in the list
        foreach(TextMeshController o in listTextMeshControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        TextMeshController textMeshControl = Instantiate(textMeshPrefab, Vector3.zero, Quaternion.identity).GetComponent<TextMeshController>();
        textMeshControl.gameObject.SetActive(false);
        listTextMeshControl.Add(textMeshControl);
        return textMeshControl;
    }

    //Get the inactive coin explode object
    private ParticleSystem GetCoinExplodeParticle()
    {
        foreach (ParticleSystem o in listCoinExplodeParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        ParticleSystem coinExplode = Instantiate(coinExplodePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        coinExplode.gameObject.SetActive(false);
        listCoinExplodeParticle.Add(coinExplode);
        return coinExplode;
    }


    //Get an inactive rocket particle
    private ParticleSystem GetColorSplashParticle()
    {
        //Find in the list
        foreach (ParticleSystem o in listColorSplashParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        ParticleSystem colorSplash = Instantiate(colorSplashPrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        colorSplash.gameObject.SetActive(false);
        listColorSplashParticle.Add(colorSplash);
        return colorSplash;
    }


    //Get the arranged list of path control
    private List<PathController> ArrangedList()
    {
        List<PathController> finalList = new List<PathController>();
        List<PathController> pathsControl = FindObjectsOfType<PathController>().ToList();
        int pathNumber = pathsControl.Count;
        while (finalList.Count < pathNumber)
        {
            float min = 1000;
            PathController minZPathControl = null;
            foreach (PathController o in pathsControl)
            {
                if (o.transform.position.z < min)
                {
                    min = o.transform.position.z;
                    minZPathControl = o;
                }
            }
            finalList.Add(minZPathControl);
            pathsControl.Remove(minZPathControl);
        }
        return finalList;
    }



    //Change the ground color
    private IEnumerator ChangingGroundColor()
    {
        yield return null;
        List<PathController> arrangedList = ArrangedList();
        int currentColorIndex = Random.Range(0, groundColors.Length);
        while (currentColorIndex == previousColorIndex)
        {
            currentColorIndex = Random.Range(0, groundColors.Length);
        }
        previousColorIndex = currentColorIndex;
        Color color = groundColors[currentColorIndex];
        groundPrefab.GetComponent<Renderer>().sharedMaterial.color = color;
        foreach (PathController o in arrangedList)
        {
            o.ChangeGroundColor(color, colorBlendingTime);
            yield return new WaitForSeconds(0.08f);
        }
    }


    private IEnumerator PlayParticle(ParticleSystem par)
    {
        par.Play();
        yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
        par.gameObject.SetActive(false);
    }

    //Increase x distance of the path
    private IEnumerator IncreaseXPathDistance()
    {
        while (xPathDistance < maxXPathDistance)
        {
            yield return new WaitForSeconds(xPathDistanceIncreaseDuration);
            xPathDistance += xPathDistanceIncreaseFactor;
        }
    }

    //Move all path to center, then call Playing event
    private IEnumerator MoveAllPathsToCenter()
    {
        List<PathController> listPath = ArrangedList();
        for(int i = 0; i < listPath.Count; i++)
        {
            listPath[i].MoveToCenter(pathMovingTime);
        }
        yield return new WaitForSeconds(pathMovingTime + 0.1f);
        PlayGame();
    }



    ////////////////////////////////////////////////////////////////Publish functions

    /// <summary>
    /// Load the saved screenshot
    /// </summary>
    /// <returns></returns>
    public Texture LoadedScrenshot()
    {
        byte[] bytes = File.ReadAllBytes(ShareManager.Instance.ScreenshotPath);
        Texture2D tx = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        tx.LoadImage(bytes);
        return tx;
    }

    /// <summary>
    /// Continue the game
    /// </summary>
    public void SetContinueGame()
    {
        IsRevived = true;
        CoinController[] coins = FindObjectsOfType<CoinController>();
        
        //Disable coins
        foreach(CoinController o in coins)
        {
            o.gameObject.SetActive(false);
        }

        //Disable obstacles
        ObstacleController[] obstacles = FindObjectsOfType<ObstacleController>();
        foreach(ObstacleController o in obstacles)
        {
            o.gameObject.SetActive(false);
        }

        camControl.MoveToCenter(pathMovingTime);
        StartCoroutine(MoveAllPathsToCenter());
    }


    /// <summary>
    /// Create next path
    /// </summary>
    public void CreatePath()
    {
        int score = ScoreManager.Instance.Score;
        foreach(MovingPlatformConfig o in movingPlatformConfig)
        {            
            if (score >= o.MinScore && score < o.MaxScore)
            {
                float zPos = currentZPos + pathSpace;
                float xPos = Random.Range(o.MinMovingAmount, o.MaxMovingAmount);
                Vector3 pathPos = new Vector3(xPos, GroundBottomPosition, zPos);
                PathController pathControl = GetPathControl();
                pathControl.transform.position = pathPos;
                currentZPos = pathControl.transform.position.z;

                if (Random.value <= o.CoinFrequency)
                {
                    if (Random.value <= o.ObstacleFrequency) //Create obstacle and coin
                    {
                        if (Random.value <= 0.5f)
                        {
                            //Create coin on left
                            CoinController coinControl = GetCoinControl();
                            coinControl.transform.position = pathControl.leftPoint.position;
                            coinControl.transform.SetParent(pathControl.transform);
                            coinControl.gameObject.SetActive(true);

                            //Create obstacle on right
                            GameObject obstacle = GetObstacle();
                            obstacle.transform.position = pathControl.rightPoint.position;
                            obstacle.transform.SetParent(pathControl.transform);
                            obstacle.SetActive(true);
                        }
                        else
                        {
                            //Create coin on right
                            CoinController coinControl = GetCoinControl();
                            coinControl.transform.position = pathControl.rightPoint.position;
                            coinControl.transform.SetParent(pathControl.transform);
                            coinControl.gameObject.SetActive(true);

                            //Create obstacle on left
                            GameObject obstacle = GetObstacle();
                            obstacle.transform.position = pathControl.leftPoint.position;
                            obstacle.transform.SetParent(pathControl.transform);
                            obstacle.SetActive(true);
                        }
                    }
                    else //Create coin only
                    {
                        //Create coin
                        CoinController coinControl = GetCoinControl();
                        if (Random.value <= 0.5f) //Create on left
                            coinControl.transform.position = pathControl.leftPoint.position;
                        else //Create on right
                            coinControl.transform.position = pathControl.rightPoint.position;
                        coinControl.transform.SetParent(pathControl.transform);
                        coinControl.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (Random.value <= o.ObstacleFrequency) //Create obstacle
                    {
                        if (Random.value <= 0.5f) //Create two obstacles
                        {
                            GameObject obstacle_1 = GetObstacle();
                            obstacle_1.transform.position = pathControl.leftPoint.position;
                            obstacle_1.transform.SetParent(pathControl.transform);
                            obstacle_1.SetActive(true);

                            GameObject obstacle_2 = GetObstacle();
                            obstacle_2.transform.position = pathControl.rightPoint.position;
                            obstacle_2.transform.SetParent(pathControl.transform);
                            obstacle_2.SetActive(true);

                        }
                        else //Create one obstacle
                        {
                            GameObject obstacle = GetObstacle();
                            if (Random.value <= 0.5f)//Create obstacle on left
                                obstacle.transform.position = pathControl.rightPoint.position;
                            else //Create obstacle on right
                                obstacle.transform.position = pathControl.rightPoint.position;
                            obstacle.transform.SetParent(pathControl.transform);
                            obstacle.SetActive(true);
                        }
                    }
                }


                if (Random.value <= o.MovingFrequency) //Create moving path
                {
                    pathControl.transform.position = new Vector3(0, pathControl.transform.position.y, pathControl.transform.position.z);
                    pathControl.gameObject.SetActive(true);
                    pathControl.Move(xPos, Random.Range(o.MinMovingSpeed, o.MaxMovingSpeed), o.LerpType[Random.Range(0, o.LerpType.Length)]);
                }
                else
                {
                    pathControl.gameObject.SetActive(true);
                }
                break;
            }
        }
    }


    /// <summary>
    /// Create fading ground wit given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateFadingGround(Vector3 pos, Color color, bool isHitCenter, Transform parent)
    {
        FadingGroundController fgCotrol = GetFadingGround();
        fgCotrol.gameObject.SetActive(true);
        fgCotrol.transform.position = pos;
        fgCotrol.transform.SetParent(parent);
        float scale = (isHitCenter) ? groundLargeScale : groundNormalScale;
        fgCotrol.FadingGround(color, scale, objectFadingTime);
    }


    /// <summary>
    /// Create a text mesh at given position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="bonusScore"></param>
    public void CreateTextMesh(Vector3 pos, int bonusScore)
    {
        TextMeshController textMeshControl = GetTextMeshControl();
        textMeshControl.transform.position = pos;
        textMeshControl.gameObject.SetActive(true);
        textMeshControl.SetScoreAndMoveUp(bonusScore, textMeshMovingUpSpeed);
    }

    /// <summary>
    /// Play coin explode effect
    /// </summary>
    public void PlayCoinExplode(Vector3 pos)
    {
        ParticleSystem coinExplode = GetCoinExplodeParticle();
        coinExplode.transform.position = pos;
        coinExplode.gameObject.SetActive(true);
        StartCoroutine(PlayParticle(coinExplode));
    }

    /// <summary>
    /// Play color splash effect
    /// </summary>
    public void PlayColorSplash(Vector3 pos)
    {
        ParticleSystem colorSplash = GetColorSplashParticle();
        colorSplash.transform.position = pos;
        colorSplash.transform.eulerAngles = new Vector3(270, 0, 0);
        colorSplash.gameObject.SetActive(true);
        StartCoroutine(PlayParticle(colorSplash));
    }


    /// <summary>
    /// Count center point and change color of the platforms
    /// </summary>
    public void CountCenterPoint()
    {
        centerCountTemp++;
        if (centerCountTemp == centerPointCount)
        {
            centerCountTemp = 0;
            StartCoroutine(ChangingGroundColor());
        }
    }
}
