using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OnefallGames;

public class CharacterScroller : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float minCharacterScale = 1f;
    [SerializeField] private float maxCharacterScale = 1.5f;
    [SerializeField] private float spaceBetweenCharacters = 10f;
    [SerializeField] private float movingAmount = 2f;
    [SerializeField] private float thresholdX = 5f;
    [SerializeField] private float rotatingSpeed = 50f;
    [SerializeField] private float snappingTime = 0.3f;
    [SerializeField] [Range(0.1f, 1f)] private float scrollingSpeed = 0.25f;
    [SerializeField] private Color lockedColor = Color.black;
    [SerializeField] private Color unlockColor = Color.white;

    [Header("References")]
    [SerializeField] private Text totalCoinsTxt;
    [SerializeField] private Text characterPriceTxt;
    [SerializeField] private GameObject characterPriceUI;
    [SerializeField] private Button selectButon;
    [SerializeField] private Button unlockButton;




    private List<GameObject> listCharacter = new List<GameObject>();
    private GameObject currentCharacter;
    private GameObject lastCurrentCharacter;
    private IEnumerator rotateCR;
    private Vector3 characterPosition = Vector3.zero;
    private Vector3 originalScale = Vector3.one;
    private Vector3 originalAngles = Vector3.zero;
    private Vector3 firstPos;
    private Vector3 currentPos;
    private float firstTime;
    private float currentTime;
    private bool isCurrentCharRotating = false;
    private bool isMoving = false;

    // Use this for initialization
    void Start()
    {
        int currentIndex = CharacterManager.Instance.SelectedIndex;
    
        currentIndex = Mathf.Clamp(currentIndex, 0, CharacterManager.Instance.characters.Length - 1);

        for (int i = 0; i < CharacterManager.Instance.characters.Length; i++)
        {
            int charIndex = i - currentIndex;

            //Instantiate characters
            GameObject character = Instantiate(CharacterManager.Instance.characters[i], characterPosition, Quaternion.Euler(originalAngles.x, originalAngles.y, originalAngles.z));
            CharacterInfo charData = character.GetComponent<CharacterInfo>();

            //Setup character
            charData.characterSequenceNumber = i;
            listCharacter.Add(character);        
            character.transform.localScale = originalScale;
            character.transform.position = characterPosition + new Vector3(charIndex * spaceBetweenCharacters, 0, 0);

            //Use this code for 3d character
            Renderer[] renders = character.GetComponentsInChildren<Renderer>();
            foreach(Renderer r in renders)
            {
                if (charData.IsUnlocked)
                    r.material.color = unlockColor;
                else
                    r.material.color = lockedColor;
            }
        }

       
        currentCharacter = listCharacter[currentIndex];
        currentCharacter.transform.localScale = maxCharacterScale * originalScale;
        currentCharacter.transform.position += movingAmount * Vector3.forward;
        lastCurrentCharacter = null;

        StartRotateCurrentCharacter();
    }

    // Update is called once per frame
    void Update()
    {
     
        if (Input.GetMouseButtonDown(0)) //Touch the screen
        {
            firstPos = Input.mousePosition;
            firstTime = Time.time;
            isMoving = false;
        }
        else if (Input.GetMouseButton(0)) //Holding
        {
            currentPos = Input.mousePosition;
            currentTime = Time.time;

            float currentDistanceX = Mathf.Abs(firstPos.x - currentPos.x);
            if (currentDistanceX >= thresholdX)
            {
                isMoving = true;

                if (isCurrentCharRotating)
                    StopRotateCurrentCharacter(true);
                
                float speed = currentDistanceX / (currentTime - firstTime);
                Vector3 dir = (firstPos.x - currentPos.x < 0) ? Vector3.right : Vector3.left;
                Vector3 moveVector = dir * (speed / 10) * scrollingSpeed * Time.deltaTime;

                // Move and scale the character
                for (int i = 0; i < listCharacter.Count; i++)
                {
                    MoveAndScaleCharacter(listCharacter[i].transform, moveVector);
                }

                // Update pos and time
                firstPos = currentPos;
                firstTime = currentTime;
            }
        }

        if (Input.GetMouseButtonUp(0)) //Leave touch
        {
            if (isMoving)
            {
                // Save the last character
                lastCurrentCharacter = currentCharacter;

                //Set current character as the nearest char to center
                currentCharacter = GetNearestCharacter();

                // Snap character
                float distance = characterPosition.x - currentCharacter.transform.position.x;
                StartCoroutine(SnappingAndRotating(distance));
            }
        }

        // Update UI
        totalCoinsTxt.text = CoinManager.Instance.Coins.ToString();
        CharacterInfo charData = currentCharacter.GetComponent<CharacterInfo>();

        if (!charData.isFree)
        {
            characterPriceUI.gameObject.SetActive(true);
            characterPriceTxt.text = charData.characterPrice.ToString();
        }
        else
        {
            characterPriceUI.SetActive(false);
        }

        if (currentCharacter != lastCurrentCharacter)
        {
            if (charData.IsUnlocked)
            { 
                unlockButton.gameObject.SetActive(false);
                selectButon.gameObject.SetActive(true);
            }
            else
            {   
                selectButon.gameObject.SetActive(false);
                if (CoinManager.Instance.Coins >= charData.characterPrice)
                {
                    unlockButton.gameObject.SetActive(true);
                    unlockButton.interactable = true;
                }
                else
                {
                    unlockButton.gameObject.SetActive(false);
                    unlockButton.interactable = false;
                }    
            }
        }
    }


    /// <summary>
    /// Get the character nearest to center
    /// </summary>
    /// <returns></returns>
    private GameObject GetNearestCharacter()
    {
        float minDistance = -1;
        GameObject resultObj = null;
        for (int i = 0; i < listCharacter.Count; i++)
        {
            float charDistance = Mathf.Abs(listCharacter[i].transform.position.x - characterPosition.x);
            if (charDistance < minDistance || minDistance < 0)
            {
                minDistance = charDistance;
                resultObj = listCharacter[i];
            }
        }

        return resultObj;
    }



    /// <summary>
    /// Snaping and rotating all characters
    /// </summary>
    /// <param name="snapDistance"></param>
    /// <returns></returns>
    private IEnumerator SnappingAndRotating(float snapDistance)
    {
        float snapDistanceAbs = Mathf.Abs(snapDistance);
        float snapSpeed = snapDistanceAbs / snappingTime;
        float sign = snapDistance / snapDistanceAbs;
        float movedDistance = 0;

        SoundManager.Instance.PlaySound(SoundManager.Instance.tick);

        while (Mathf.Abs(movedDistance) < snapDistanceAbs)
        {
            float d = sign * snapSpeed * Time.deltaTime;
            float remainedDistance = Mathf.Abs(snapDistanceAbs - Mathf.Abs(movedDistance));
            d = Mathf.Clamp(d, -remainedDistance, remainedDistance);

            Vector3 moveVector = new Vector3(d, 0, 0);
            for (int i = 0; i < listCharacter.Count; i++)
            {
                MoveAndScaleCharacter(listCharacter[i].transform, moveVector);
            }

            movedDistance += d;
            yield return null;
        }


        //Stop rotate the current character and start rotate the new character
        if (currentCharacter != lastCurrentCharacter || !isCurrentCharRotating)
        {
            StopRotateCurrentCharacter();
            StartRotateCurrentCharacter();
        }
    }
    /// <summary>
    /// Move and scale given character 
    /// </summary>
    /// <param name="charTrans"></param>
    /// <param name="moveVector"></param>
    private void MoveAndScaleCharacter(Transform charTrans, Vector3 moveVector)
    {
        // Move
        charTrans.position += moveVector;

        // Scale and move forward according to distance from current position to center position
        float distance = Mathf.Abs(charTrans.position.x - characterPosition.x);
        if (distance < (spaceBetweenCharacters / 2))
        {
            float factor = 1 - distance / (spaceBetweenCharacters / 2);
            float scaleFactor = Mathf.Lerp(minCharacterScale, maxCharacterScale, factor);
            charTrans.localScale = scaleFactor * originalScale;

            float fd = Mathf.Lerp(0, movingAmount, factor);
            Vector3 pos = charTrans.position;
            pos.z = characterPosition.z + fd;
            charTrans.position = pos;
        }
        else
        {
            charTrans.localScale = minCharacterScale * originalScale;
            Vector3 pos = charTrans.position;
            pos.z = characterPosition.z;
            charTrans.position = pos;
        }
    }
    







    /// <summary>
    /// Stop rotating current character
    /// </summary>
    /// <param name="isResetRotation"></param>
    private void StopRotateCurrentCharacter(bool isResetRotation = false)
    {
        if (rotateCR != null)
            StopCoroutine(rotateCR);
        isCurrentCharRotating = false;
        if (isResetRotation)
            StartCoroutine(ResetingCharRotation(currentCharacter.transform));        
    }
    /// <summary>
    /// Star rotating current character
    /// </summary>
    private void StartRotateCurrentCharacter()
    {
        StopRotateCurrentCharacter(false);  
        rotateCR = RotatingCharacter(currentCharacter.transform);
        StartCoroutine(rotateCR);
        isCurrentCharRotating = true;
    }





    /// <summary>
    /// Rotating given character's transform
    /// </summary>
    /// <param name="charTrans"></param>
    /// <returns></returns>
    IEnumerator RotatingCharacter(Transform charTrans)
    {
        while (true)
        {
            charTrans.eulerAngles += Vector3.up * rotatingSpeed * Time.deltaTime;
            yield return null;
        }
    }
    /// <summary>
    /// Reseting character rotation
    /// </summary>
    /// <param name="charTrans"></param>
    /// <returns></returns>
    IEnumerator ResetingCharRotation(Transform charTrans)
    {
        Vector3 startAngles = charTrans.rotation.eulerAngles;
        Vector3 endAngles = originalAngles;
        float rotatingTime = Mathf.Abs(endAngles.y - startAngles.y) / 180f;
        float t = 0;
        while (t < rotatingTime)
        {
            t += Time.deltaTime;
            Vector3 eulerAngles = Vector3.Lerp(startAngles, endAngles, t / rotatingTime);
            charTrans.eulerAngles = eulerAngles;
            yield return null;
        }
    }





    ////////////////////////////////////////// Publish functions

    public void UnlockBtn()
    {
        bool isUnlocked = currentCharacter.GetComponent<CharacterInfo>().Unlock();
        if (isUnlocked)
        {

            //Use this code for 3d character
            Renderer[] renders = currentCharacter.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renders)
            {
                r.material.color = unlockColor;
            }

            unlockButton.gameObject.SetActive(false);
            selectButon.gameObject.SetActive(true);

            SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
        }
    }

    public void SelectBtn()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
        CharacterManager.Instance.SelectedIndex = currentCharacter.GetComponent<CharacterInfo>().characterSequenceNumber;
        BackBtn();
    }

    public void BackBtn()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
        SceneManager.LoadScene("Gameplay");
    }
}
