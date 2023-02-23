using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Interactable;

public class Agent : MonoBehaviour, IInteractable
{
    public string agentName;

    private bool isHovered = false;

    SpriteRenderer spriteRenderer;

    public override string ToString() {
        return agentName;
    }

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter() { Hover(); }
    void OnMouseExit() { Unhover(); }

    public void Hover() {
        isHovered = true;
    }
    public void Unhover() {
        isHovered = false;
    }

    public void Select() {
        spriteRenderer.material.SetFloat("_Selected", 1f);
    }
    public void Deselect() {
        spriteRenderer.material.SetFloat("_Selected", 0f);
    }
    public void AltSelect() {
        spriteRenderer.material.SetFloat("_AltSelected", 1f);
    }
    public void AltDeselect() {
        spriteRenderer.material.SetFloat("_AltSelected", 0f);
    }
}
