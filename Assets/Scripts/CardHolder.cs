using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class CardHolder : MonoBehaviour
{

    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    [SerializeField] private CardSlot slotPrefab;
    private RectTransform _rect;

    private bool _isCrossing;
    
    public List<Card> Cards = new();

    [SerializeField] private bool tweenCardReturn;
    
    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        Cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        foreach (var card in Cards)
        {
            card.OnPointerEnterEvent.AddListener(OnPointerEnter);
            card.OnPointerExitEvent.AddListener(OnPointerExit);
            card.OnBeginDragEvent.AddListener(OnBeginDrag);
            card.OnEndDragEvent.AddListener(OnEndDrag);
            card.OnSelectEvent.AddListener(OnSelect);
            card.name = "Card " + cardCount.ToString();
            cardCount++;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                Cards.Remove(hoveredCard);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in Cards)
                card.Deselect();
        }

        if (selectedCard == null || _isCrossing)
            return;

        CheckCardsForSwipe();

    }

    private void OnSelect(Card arg0, bool arg1)
    {
        
    }

    private void CheckCardsForSwipe()
    {
        for (int i = 0; i < Cards.Count; i++)
        {

            if (selectedCard.transform.position.x > Cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex < Cards[i].ParentIndex)
                {
                    SwapCards(i);
                    break;
                }
            }

            if (!(selectedCard.transform.position.x < Cards[i].transform.position.x)) continue;
            if (selectedCard.ParentIndex <= Cards[i].ParentIndex) continue;
            
            SwapCards(i);
            break;
        }
    }
    
    void SwapCards(int index)
    {
        _isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = Cards[index].transform.parent;

        Cards[index].transform.SetParent(focusedParent);
        Cards[index].transform.localPosition = Cards[index].selected ? new Vector3(0, Cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        _isCrossing = false;

        if (Cards[index].Visual == null)
            return;
        
        var swapIsRight = Cards[index].ParentIndex > selectedCard.ParentIndex;
        Cards[index].Visual.Swap(swapIsRight ? -1 : 1);
        
        //Updated Visual Indexes
        foreach (Card card in Cards)
        {
            card.Visual.UpdateIndex(transform.childCount);
        }
    }
    
    
    private void OnPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    private void OnPointerExit(Card card)
    {
        hoveredCard = null;
    }

    private void OnBeginDrag(Card card)
    {
        selectedCard = card;
    }

    private void OnEndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(
            selectedCard.selected 
                ? new Vector3(0,selectedCard.selectionOffset,0) 
                : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        _rect.sizeDelta += Vector2.right;
        _rect.sizeDelta -= Vector2.right;

        selectedCard = null;
    }
}
