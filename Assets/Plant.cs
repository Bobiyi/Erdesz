using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Plant : MonoBehaviour
{
    private int health;
    private int cost;

    protected Plant(int health, int cost)
    {
        this.Health = health;
        this.Cost = cost;
    }

    public int Health { get => health; protected set => health = value; }
    public int Cost { get => cost; protected set => cost = value; }

    protected void OnMouseDown()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        while(Input.GetMouseButtonDown(0))
        {
            transform.position = mousePos;
        }
    }
}
