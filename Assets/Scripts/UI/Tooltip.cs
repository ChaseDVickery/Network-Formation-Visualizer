using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// https://answers.unity.com/questions/1199251/onmouseover-ui-button-c.html
public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverTime = 0.5f;
    public Vector2 offset;
    public string tooltipString;
    public float moveTolerance = 5f;
    public GameObject tooltipPrefab;
    private GameObject tooltip;

    private bool newPos = true;
    private Vector2 initMousePos, currMousePos;
    private bool mouse_over = false;
    private bool showing = false;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        tooltip = Instantiate(tooltipPrefab, GameObject.FindGameObjectWithTag("UICanvas").transform);
        tooltip.GetComponentInChildren<TMP_Text>().text = tooltipString;
        
        tooltip.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (mouse_over) {
            currMousePos = Input.mousePosition;
            if (!InTolerance(currMousePos)) {
                initMousePos = currMousePos;
                HideTooltip();
                showing = false;
                timer = 0f;
            } else {
                timer += Time.deltaTime;
            }
        }
        if (timer >= hoverTime && !showing) {
            showing = true;
            ShowTooltip();
        }
    }

    private bool InTolerance(Vector2 newPos) {
        return (initMousePos - newPos).magnitude < moveTolerance;
    }

    private void ShowTooltip() {
        Vector3 mousePos = Input.mousePosition;
        // mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(mousePos.x+offset.x, mousePos.y+offset.y);
        tooltip.SetActive(true);
    }

    private void HideTooltip() {
        tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        showing = false;
        newPos = true;
        timer = 0;
        initMousePos = Input.mousePosition;
        Debug.Log("Mouse enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        showing = false;
        timer = 0;
        HideTooltip();
        Debug.Log("Mouse exit");
    }
}
