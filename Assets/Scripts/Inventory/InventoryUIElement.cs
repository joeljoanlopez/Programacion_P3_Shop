﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image Image;
    public TextMeshProUGUI AmountText;

    private Canvas _canvas;
    private GraphicRaycaster _raycaster;
    private ItemBasic _item;
    private InventoryUI _inventory;
    private int _amount;
    private bool _getStack;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            _getStack = true;
        else
            _getStack = false;
    }

    public void SetStuff(InventorySlot slot, InventoryUI inventory)
    {
        Image.sprite = slot.Item._imageUI;
        AmountText.text = slot.Amount.ToString();
        AmountText.enabled = slot.Amount > 1;

        _item = slot.Item;
        _amount = slot.Amount;
        _inventory = inventory;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Start moving object from the beginning!
        transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y, 0) / transform.lossyScale.x; // Thanks to the canvas scaler we need to devide pointer delta by canvas scale to match pointer movement.

        // We need a few references from UI
        if (!_canvas)
        {
            _canvas = GetComponentInParent<Canvas>();
            _raycaster = _canvas.GetComponent<GraphicRaycaster>();
        }

        // Change parent of our item to the canvas
        transform.SetParent(_canvas.transform, true);

        // And set it as last child to be rendered on top of UI
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convierte la posición del ratón a la posición en el mundo
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint);

        // Asigna la posición al objeto
        transform.position = worldPoint;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        // Find objects within canvas
        var results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);
        foreach (var hit in results)
        {
            // Changing parent to new inventory
            if (hit.gameObject.TryGetComponent<InventoryUI>(out InventoryUI _destInventory))
            {
                int _amountSent = 0;
                if (_getStack)
                    _amountSent = _amount;
                else
                    _amountSent = 1;

                for (int i = 0; i < _amountSent; i++)
                    SendItem(_destInventory);
            }
        }

        // And centering item position
        transform.localPosition = Vector3.zero;
    }

    private void SendItem(InventoryUI destination)
    {
        int _itemValue = _item._cost;
        if (_itemValue < CoinManager.Instance.GetCurrentCoins())
        {
            _inventory.Inventory.RemoveItem(_item);
            destination.Inventory.AddItem(_item);
        }

        if (_inventory._isPlayer)
            CoinManager.Instance.SellItem(_itemValue);
        else
            CoinManager.Instance.BuyItem(_itemValue);
    }
}