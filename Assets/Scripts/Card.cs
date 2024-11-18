using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, 
    IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{

    [Header("Card Parameters")] 
    [SerializeField] private float moveSpeedLimit = 2f;
    [SerializeField] private Vector3 offset;

    [Header("Events"), Space]
    public UnityEvent<Card> OnPointerEnterEvent;
    public UnityEvent<Card> OnPointerExitEvent;
    public UnityEvent<Card, bool> OnPointerUpEvent;
    public UnityEvent<Card> OnPointerDownEvent;
    public UnityEvent<Card> OnBeginDragEvent;
    public UnityEvent<Card> OnEndDragEvent;
    public UnityEvent<Card, bool> OnSelectEvent;

    [Space, SerializeField] private bool instantiateVisual = true;
    [Space, SerializeField] private CardVisual cardVisual;
    [Space, SerializeField] private CardVisual visualPrefab;
    private CardVisualHandler _visualHandler;
    
    private Canvas _canvas;
    private Image _image;
    
    public bool selected = false;
    public float selectionOffset;
    
    private bool _isDragging = false;
    private bool _wasDragged = false;
    private bool _isHovering = false;

    public CardVisual Visual => cardVisual;
    
    public bool IsDragging => _isDragging;
    public bool WasDragged => _wasDragged;
    public bool IsHovering => _isHovering;
    
    private float _pointerDownTime;
    private float _pointerUpTime;
    
    void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        _image = GetComponent<Image>();
        
        if (!instantiateVisual)
            return;
        
        _visualHandler = FindObjectOfType<CardVisualHandler>();
        cardVisual = Instantiate(visualPrefab, _visualHandler 
            ? _visualHandler.transform 
            : _canvas.transform)
            .GetComponent<CardVisual>();
        cardVisual.Initialize(this);
    }
    
    void Update()
    {
        ClampPosition();
        
        if (_isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
        
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    #region Callbacks

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragEvent?.Invoke(this);
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        _isDragging = true;
        _canvas.GetComponent<GraphicRaycaster>().enabled = false;
        _image.raycastTarget = false;

        _wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragEvent?.Invoke(this);
        _isDragging = false;
        _canvas.GetComponent<GraphicRaycaster>().enabled = true;
        _image.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent.Invoke(this);
        _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent.Invoke(this);
        _isHovering = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        _pointerUpTime = Time.time;

        OnPointerUpEvent.Invoke(this, _pointerUpTime - _pointerDownTime > .2f);

        if (_pointerUpTime - _pointerDownTime > .2f)
            return;

        if (_wasDragged)
            return;

        selected = !selected;
        OnSelectEvent.Invoke(this, selected);

        // if (selected)
        //     transform.localPosition += (cardVisual.transform.up * selectionOffset);
        // else
        //     transform.localPosition = Vector3.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnPointerDownEvent.Invoke(this);
        _pointerDownTime = Time.time;
    }

    #endregion

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }

    public int SiblingAmount => 
        transform.parent.CompareTag("Slot") 
            ? transform.parent.parent.childCount - 1 
            : 0;

    public float NormalizedPosition => 
        transform.parent.CompareTag("Slot") 
            ? Remap((float)ParentIndex, 0, (float)(transform.parent.parent.childCount - 1), 0, 1) 
            : 0;

    public int ParentIndex => 
        transform.parent.CompareTag("Slot") 
            ? transform.parent.GetSiblingIndex() 
            : 0;

    public float Remap(float value, float from1, float to1, float from2, float to2) =>
        (value - from1) / (to1 - from1) * (to2 - from2) + from2;

    private void OnDestroy()
    {
        if(cardVisual != null)
            Destroy(cardVisual.gameObject);
    }
}
