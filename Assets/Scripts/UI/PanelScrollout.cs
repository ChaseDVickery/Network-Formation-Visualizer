using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PanelScrollout : MonoBehaviour
{
    RectTransform rt;
    public float scrollTime = 0.3f;
    public RectTransform hiddenPos;
    public RectTransform targetPos;

    private bool atTarget;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    public void TogglePosition() {
        if (atTarget) {
            StopCoroutine("ScrollCoroutine");
            StartCoroutine(ScrollCoroutine(hiddenPos, scrollTime));
            // rt.anchoredPosition = hiddenPos.anchoredPosition;
        } else {
            StopCoroutine("ScrollCoroutine");
            StartCoroutine(ScrollCoroutine(targetPos, scrollTime));
            // rt.anchoredPosition = targetPos.anchoredPosition;
        }
        atTarget = !atTarget;
    }

    public void ClosePanel() {
        StopCoroutine("ScrollCoroutine");
        StartCoroutine(ScrollCoroutine(hiddenPos, scrollTime));
        atTarget = false;
    }

    public void OpenPanel() {
        StopCoroutine("ScrollCoroutine");
        StartCoroutine(ScrollCoroutine(targetPos, scrollTime));
        atTarget = true;
    }

    IEnumerator ScrollCoroutine(RectTransform to, float time) {
        float timer = 0f;
        Vector3 startPos = rt.anchoredPosition;
        while (timer < time) {
            rt.anchoredPosition = Vector3.Lerp(startPos, to.anchoredPosition, timer/time);
            timer += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = to.anchoredPosition;
    }
}