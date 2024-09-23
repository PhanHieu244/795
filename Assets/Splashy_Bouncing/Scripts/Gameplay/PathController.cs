using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathController : MonoBehaviour {

    [SerializeField]
    private GameObject centerPoint;
    public Transform rightPoint;
    public Transform leftPoint;

    public float PathSpeed { private set; get; }

    private Renderer groundRender = null;
    private Renderer centerPointRender = null;
    private Coroutine moving = null;
    private Color centerPointOriginalColor = Color.white;
    private Color groundOriginalColor = Color.white;
    private void Start()
    {
        groundRender = GetComponent<Renderer>();
        centerPointRender = centerPoint.GetComponent<Renderer>();
        centerPointOriginalColor = centerPointRender.material.color;
        groundOriginalColor = groundRender.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            //Reset ground original color
            groundRender.material.color = groundOriginalColor;

            //Reset center point
            centerPoint.transform.localScale = Vector3.one;
            centerPointRender.material.color = centerPointOriginalColor;

            //Disable coins and obstacles
            MeshRenderer[] meshRender = GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer o in meshRender)
            {
                if (o.CompareTag("Coin") || o.CompareTag("Finish"))
                {
                    o.gameObject.SetActive(false);
                    o.transform.SetParent(null);
                }
            }


            //Reset this object
            gameObject.SetActive(false);
            transform.position = Vector3.zero;
        }
    }

    public void FadeCenterPoint()
    {
        StartCoroutine(FadingCenterPoint());
    }

    IEnumerator FadingCenterPoint()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * GameManager.Instance.GroundLargeScale;
        Color startColor = centerPointRender.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        float t = 0;
        while (t < GameManager.Instance.ObjectFadingTime)
        {
            t += Time.deltaTime;
            float factor = t / GameManager.Instance.ObjectFadingTime;
            centerPoint.transform.localScale = Vector3.Lerp(startScale, endScale, factor);
            centerPointRender.material.color = Color.Lerp(startColor, endColor, factor);
            yield return null;
        }
    }



    /// <summary>
    /// Change color of this ground
    /// </summary>
    /// <param name="targetColor"></param>
    public void ChangeGroundColor(Color targetColor, float time)
    {
        groundOriginalColor = targetColor;
        StartCoroutine(ChangingGroundColor(targetColor, time));
    }

    IEnumerator ChangingGroundColor(Color targetColor, float time)
    {
        Color startColor = groundRender.material.color;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            groundRender.material.color = Color.Lerp(startColor, targetColor, factor);
            yield return null;
        }
    }



    /// <summary>
    /// Move this platform from current x position to center (0)
    /// </summary>
    /// <param name="time"></param>
    public void MoveToCenter(float time)
    {
        if (moving != null)
            StopCoroutine(moving);
        StartCoroutine(MovingToCenter(time));
    }
    IEnumerator MovingToCenter(float time)
    {
        float startX = transform.position.x;
        float endX = 0;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            Vector3 newPos = transform.position;
            newPos.x = Mathf.Lerp(startX, endX, factor);
            transform.position = newPos;
            yield return null;
        }
    }



    /// <summary>
    /// Move this platform left anf right (repeat)
    /// </summary>
    public void Move(float amount, float speed, LerpType lerpType)
    {
        moving = StartCoroutine(MovingLeftAndRight(amount, speed, lerpType));
    }
    private IEnumerator MovingLeftAndRight(float amount, float speed, LerpType lerpType)
    {
        float t = 0;
        float time = amount / speed;
        Vector3 startPos = transform.position;
        Vector3 endPos;
        if (Random.value <= 0.5f)
            endPos = startPos + Vector3.left * amount;
        else
            endPos = startPos + Vector3.right * amount;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = EasyType.MatchedLerpType(lerpType, t / time);
            transform.position = Vector3.Lerp(startPos, endPos, factor);
            yield return null;
        }

        
        while (true)
        {
            t = 0;
            startPos = transform.position;
            if (startPos.x > 0) //On right
            {
                endPos = new Vector3(-amount, startPos.y, startPos.z);
            }
            else //On left
            {
                endPos = new Vector3(amount, startPos.y, startPos.z);
            }

            while (t < time)
            {
                t += Time.deltaTime;
                float factor = EasyType.MatchedLerpType(lerpType, t / time);
                transform.position = Vector3.Lerp(startPos, endPos, factor);
                yield return null;
            }
        }
    }
}
