using UnityEngine;
using System.Collections;

public class TextMeshController : MonoBehaviour {

    private TextMesh textMesh = null;
    private Renderer render = null;

    /// <summary>
    /// Moving up and fading out
    /// </summary>
    /// <param name="bonusScore"></param>
    /// <param name="movingUpSpeed"></param>
    public void SetScoreAndMoveUp(int bonusScore, float movingUpSpeed)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();
        if (render == null)
            render = GetComponent<Renderer>();
        textMesh.text = "+" + bonusScore.ToString();
        StartCoroutine(MovingUp(movingUpSpeed));
    }
    private IEnumerator MovingUp(float speed)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 5f;
        Color startColor = render.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        float movingTime = Vector3.Distance(startPos, endPos) / speed;
        float t = 0;
        while (t < movingTime)
        {
            t += Time.deltaTime;
            float factor = t / movingTime;
            transform.position = Vector3.Lerp(startPos, endPos, factor);
            render.material.color = Color.Lerp(startColor, endColor, factor);
            yield return null;
        }
        render.material.color = startColor;
        gameObject.SetActive(false);
    }
}
