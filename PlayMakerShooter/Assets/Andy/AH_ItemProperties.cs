using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AH_ItemProperties : MonoBehaviour {

    [Header("Your Consumables")]
    public string itemName;

    [SerializeField] private bool food;
    [SerializeField] private bool water;
    [SerializeField] private bool health;
    [SerializeField] private float value; //andy TODO, seperate out different value so they can have different values

    [SerializeField] private AH_PlayerVitals playerVitals;

    public void Interaction(AH_PlayerVitals playerVitals)
    {
        if(food)
        {
            playerVitals.hungerSlider.value += value;
            this.gameObject.SetActive(false); // make it go away after its used
        }

        if (water)
        {
            playerVitals.thirstSlider.value += value;
            this.gameObject.SetActive(false); // make it go away after its used
        }

        if (health)
        {
            playerVitals.healthSlider.value += value;
            this.gameObject.SetActive(false); // make it go away after its used
        }

    }



}
