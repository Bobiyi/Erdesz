using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BitcoinScript : MonoBehaviour
{
    private bool moving = false;

    [SerializeField] private TMP_Text counter;


    void Start()
    {
        counter = FindFirstObjectByType<TMP_Text>();
        if(counter == null)
        {
            Debug.LogError("BitcoinScript: No TMP_Text found.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving) return;

        transform.position = Vector3.Lerp(transform.position, new Vector2(-9.75f, 4.5f), Time.deltaTime*2);

       if(Vector2.Distance(transform.position, new Vector2(-9.75f, 4.5f)) < 0.1f)
        {
            Destroy(gameObject);

            var current_Coin =int.Parse(counter.text);

            current_Coin += 25;

            counter.text = current_Coin+"";
        }
    }

    public void OnMouseDown()
    {
        moving = true;
    }
}
