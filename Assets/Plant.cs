using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Plant : MonoBehaviour
{
    private int health;
    private int cost;

    public int Health { get => health; protected set => health = value; }
    public int Cost { get => cost; protected set => cost = value; }

    protected void OnMouseDown()
    {
        
    }
}
