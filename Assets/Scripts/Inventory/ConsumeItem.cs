﻿using UnityEngine;

public class ConsumeItem : MonoBehaviour, IConsume
{
    public void Use(ConsumableItem item)
    {
        if (item is ItemHealthPotion)
        {
            Debug.Log("I have been healed!");
        }
    }
}