using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using OnefallGames;


public enum PlayerState
{
    Prepare,
    Living,
    Die,
}

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance { private set; get; }
    public static event System.Action<PlayerState> PlayerStateChanged = delegate { };

    public PlayerState PlayerState
    {
        get
        {
            return playerState;
        }

        private set
        {
            if (value != playerState)
            {
                value = playerState;
                PlayerStateChanged(playerState);
            }
        }
    }


    private PlayerState playerState = PlayerState.Die;


    [Header("Player Config")]
    [SerializeField] private int bouncingHeight = 8;
    [SerializeField] private int maxMovingPoints = 40;
    [SerializeField] private int minMovingPoint = 20;
    [SerializeField] private int movingPointDecreaseAmount = 2;
    [SerializeField] private int scoreToDecreaseMovingPoint = 50;
    [SerializeField] private float thresholdSpeed = 30f;

    [Header("Player References")]
    [SerializeField] private CameraController camControl;
    [SerializeField] private ParticleSystem playerExplodeParticle;
    [SerializeField] private MeshRenderer meshRender = null;

    private Rigidbody rigid = null;
    private SphereCollider sphereCollider = null;
    private float firstX = 0;
    private float lastZPos = 0;
    private int bonusScore = 1;
    private int movingPoints = 0;
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
        if (obj == GameState.Playing)
        {
            PlayerLiving();
        }
        else if (obj == GameState.Prepare)
        {
            PlayerPrepare();
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

    // Update is called once per frame
    void Update () {

        if (playerState == PlayerState.Living)
        {
            if (Input.GetMouseButtonDown(0))
            {
                firstX = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)).x;
            }
            else if (Input.GetMouseButton(0))
            {
                float currentX = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)).x;
                float amount = Mathf.Abs(Mathf.Abs(currentX) - Mathf.Abs(firstX));

                if (currentX > firstX)
                {
                    transform.position += new Vector3(amount * thresholdSpeed * Time.deltaTime, 0, 0);
                }
                else
                {
                    transform.position -= new Vector3(amount * thresholdSpeed * Time.deltaTime, 0, 0);
                }

                firstX = currentX;
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                GameManager.Instance.GameOver();
            }

        }
    }

    private void PlayerPrepare()
    {
        //Fire event
        PlayerState = PlayerState.Prepare;
        playerState = PlayerState.Prepare;

        //Add another function here
        meshRender = GetComponent<MeshRenderer>();
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        //Replace player with current character
        GameObject currentChar = CharacterManager.Instance.characters[CharacterManager.Instance.SelectedIndex];
        GetComponent<MeshFilter>().mesh = currentChar.GetComponent<MeshFilter>().sharedMesh;
        meshRender.material = currentChar.GetComponent<MeshRenderer>().sharedMaterial;

        playerExplodeParticle.gameObject.SetActive(false);
        movingPoints = maxMovingPoints;
        lastZPos = transform.position.z;
    }

    private void PlayerLiving()
    {
        //Fire event
        PlayerState = PlayerState.Living;
        playerState = PlayerState.Living;

        //Add another actions here

        meshRender.enabled = true;
        rigid.isKinematic = true;
        sphereCollider.enabled = true;
        sphereCollider.isTrigger = true;
        transform.position = new Vector3(0, 0, lastZPos);
        transform.eulerAngles = Vector3.zero;

        StartCoroutine(MovingForward());
        StartCoroutine(DecreaseMovingPoint());
    }

    private void PlayerDie()
    {
        //Fire event
        PlayerState = PlayerState.Die;
        playerState = PlayerState.Die;

        //Add another actions here
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.explode);
            lastZPos = transform.position.z;
            PlayerDie();
            StartCoroutine(DisablePlayerMesh(0.5f));
            StartCoroutine(SetGamestate(1f));
        }
    }

    //Calculate position for moving player
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 from, Vector3 middle, Vector3 to)
    {
        return Mathf.Pow((1 - t), 2) * transform.position + 2 * (1 - t) * t * middle + Mathf.Pow(t, 2) * to;
    }

    //Move player forward
    private IEnumerator MovingForward()
    {
        int rotateLeft = 0;
        List<Vector3> listPositions = new List<Vector3>();
        while (playerState == PlayerState.Living)
        {
            //Calculate the list position
            listPositions.Clear();
            Vector3 startPoint = transform.position;
            Vector3 endPoint = startPoint + Vector3.forward * GameManager.Instance.pathSpace;
            Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, 0.5f) + Vector3.up * bouncingHeight;
            listPositions.Add(transform.position);
            for (int i = 1; i <= movingPoints; i++)
            {
                float t = i / (float)movingPoints;
                listPositions.Add(CalculateQuadraticBezierPoint(t, startPoint, midPoint, endPoint));
            }

            //Moving player to each point
            rotateLeft += 1;
            float tAngle = 360f / listPositions.Count;
            float startTime = Time.time;
            for (int i = 0; i < listPositions.Count; i++)
            {
                transform.position = new Vector3(transform.position.x, listPositions[i].y, listPositions[i].z);
                transform.RotateAround(transform.position, (rotateLeft > 0) ? Vector3.up : Vector3.down, tAngle);
                yield return null;
            }

            //Reset rotateLeft
            if (rotateLeft == 2)
            {
                rotateLeft = -2;
            }

            if (playerState == PlayerState.Die)
            {
                yield break;
            }

            Ray rayDown = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(rayDown, out hit, 2f))
            {
                if (hit.collider.CompareTag("CenterPoint")) //Hit center point
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitCenterPoint);

                    //Create text mesh for bonus score
                    bonusScore++;
                    GameManager.Instance.CreateTextMesh(hit.point, bonusScore);
                    ScoreManager.Instance.AddScore(bonusScore);

                    Color groundColor = hit.transform.parent.GetComponent<Renderer>().material.color;
                    GameManager.Instance.PlayColorSplash(hit.point);
                    hit.transform.parent.GetComponent<PathController>().FadeCenterPoint();
                    GameManager.Instance.CreateFadingGround(hit.transform.position, groundColor, true, hit.transform.parent);
                    GameManager.Instance.CountCenterPoint();
                }
                else //Hit ground
                {
                    Color groundColor = hit.collider.GetComponent<Renderer>().material.color;
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitGround);
                    bonusScore = 1;
                    ScoreManager.Instance.AddScore(bonusScore);
                    GameManager.Instance.CreateFadingGround(hit.transform.position, groundColor, false, hit.transform);
                }

                GameManager.Instance.CreatePath();
            }
            else //Hit nothing -> fall down
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.fallDown);

                //Call events
                ShareManager.Instance.CreateScreenshot();
                StartCoroutine(SetGamestate(0.5f));

                //Get component
                Rigidbody rigid = GetComponent<Rigidbody>();
                SphereCollider sphereCollider = GetComponent<SphereCollider>();

                //Disable trigger and kinematic
                sphereCollider.isTrigger = false;
                rigid.isKinematic = false;
                lastZPos = transform.position.z;

                //Force player down
                rigid.AddForce(Vector3.down * 1000f);

                yield break;
            }
        }
    }

    //Set game state
    private IEnumerator SetGamestate(float delay)
    {
        yield return null; //Wait to capture screenshot

        if (!GameManager.Instance.IsRevived) //User is not revive
        {
            if (AdManager.Instance.IsRewardedVideoAdReady()) //Call Revive state
            {
                GameManager.Instance.Revive();
            }
            else
            {
                GameManager.Instance.GameOver(); //Call Game Over state
            }
        }
        else //User already revived -> Game Over
        {
            GameManager.Instance.GameOver();
        }
    }


    //Disable player mesh and play explode particle
    private IEnumerator DisablePlayerMesh(float delay)
    {
        ShareManager.Instance.CreateScreenshot();
        yield return null;

        //Disabe mesh render, collider
        meshRender.enabled = false;
        sphereCollider.enabled = false;
        StartCoroutine(PlayParticle());
    }


    //Play player explode particle
    private IEnumerator PlayParticle()
    {
        Vector3 pos = transform.position + Vector3.up * (meshRender.bounds.size.y / 2f);
        playerExplodeParticle.transform.position = pos;
        playerExplodeParticle.gameObject.SetActive(true);
        playerExplodeParticle.Play();
        yield return new WaitForSeconds(playerExplodeParticle.main.startLifetimeMultiplier);
        playerExplodeParticle.gameObject.SetActive(false);
    }



    //Decrease moving points base one scores
    private IEnumerator DecreaseMovingPoint()
    {
        while (true)
        {
            int movingPointTemp = maxMovingPoints;
            int currentScore = ScoreManager.Instance.Score;
            int factor = currentScore / scoreToDecreaseMovingPoint;
            for(int i = 0; i < factor; i++)
            {
                movingPointTemp -= movingPointDecreaseAmount;
            }
            movingPoints = movingPointTemp;
            if (movingPoints <= minMovingPoint)
            {
                movingPoints = minMovingPoint;
                yield break;
            }
            yield return null;
        }
    }
}
