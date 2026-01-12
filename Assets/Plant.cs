using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Plant
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
}
