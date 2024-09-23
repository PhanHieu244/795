using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [Header("Camera Config")]
    [SerializeField]
    private float smoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private float originalZDistance = 0;

    private void Start()
    {
        originalZDistance = Mathf.Abs(Mathf.Abs(transform.position.z) - Mathf.Abs(PlayerController.Instance.transform.position.z));
    }

    void Update()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            float currentXDistance = (PlayerController.Instance.transform.position - transform.position).x;
            float currentZDistance = (PlayerController.Instance.transform.position - transform.position).z;
            float distanceZ = Mathf.Abs(originalZDistance - Mathf.Abs(currentZDistance));
            Vector3 targetPos = transform.position + new Vector3(currentXDistance, 0, distanceZ);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        } 
    }

    public void MoveToCenter(float time)
    {
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
}
