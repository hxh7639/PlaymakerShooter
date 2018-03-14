using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AH_PlayerVitals : MonoBehaviour {

    
    public Slider healthSlider;
    public int maxHealth;
    public int healthFallRate;

    public Slider thirstSlider;
    public int maxThirst;
    public int thirstFallRate;

    public Slider hungerSlider;
    public int maxHunger;
    public int hungerFallRate;

    public Slider staminaSlider;
    public int maxStamina;
    public int staminaFallRate;
    public int staminaFallMultiplier;
    private int staminaRegainRate;
    public int staminaRegainMultiplier;

    [Header("Temperature Settings")]
    public float freezingTemp;
    public float currentTemp;
    public float normalTemp;
    public float overheatTemp;
    public Text tempNumber;
    public Image tempBG;
    public int healthFallDueTempMulti = 1;

    private CharacterController charController;
    private vp_FPController playerController;

    // Use this for initialization
    void Start ()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        thirstSlider.maxValue = maxThirst;
        thirstSlider.value = maxThirst;

        hungerSlider.maxValue = maxHunger;
        hungerSlider.value = maxHunger;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;

        staminaFallRate = 1;
        staminaRegainRate = 1;

        charController = GetComponent<CharacterController>();
        playerController = GetComponent<vp_FPController>();
    }
	
	// Update is called once per frame
	void Update ()
    {

        //TEMPERATURE SECTION
        UpdateTemp();
        if(currentTemp <= freezingTemp)
        {
            tempBG.color = Color.blue;
        }
        else if (currentTemp >= overheatTemp - 0.1)
        {
            tempBG.color = Color.Lerp(new Color(1f,0.5f,0.2f), Color.red, Mathf.PingPong(Time.time, 1));
        }
        else
        {
            tempBG.color = Color.green;
        }

        //TODO combine this with the ones below and make different multiplier
        if (currentTemp <= freezingTemp || currentTemp >= overheatTemp)
        {
            healthSlider.value -= Time.deltaTime / healthFallRate * healthFallDueTempMulti;
        }


        //TODO implement multiplier (for running, working.. etc)
        // HEALTH CONTROL SECTION
        if (hungerSlider.value <= 0 && (thirstSlider.value <= 0))
        {
            healthSlider.value -= Time.deltaTime / healthFallRate * 2;
        }

        else if (hungerSlider.value <= 0 || thirstSlider.value <= 0)
        {
            healthSlider.value -= Time.deltaTime / healthFallRate;
        }

        if (healthSlider.value <= 0)
        {
            CharacterDeath();
        }

        // THIRST CONTROLLER
        if (thirstSlider.value >= 0)
        {
            thirstSlider.value -= Time.deltaTime / thirstFallRate;
        }

        if (thirstSlider.value <= 0)
        {
            thirstSlider.value = 0;
        }
        else if (thirstSlider.value >= maxThirst)
        {
            thirstSlider.value = maxThirst;
        }

        // HUNGER CONTROLLER
        if (hungerSlider.value >= 0)
        {
            hungerSlider.value -= Time.deltaTime / hungerFallRate;
        }

        if(hungerSlider.value <= 0 )
        {
            hungerSlider.value = 0;
        }
        else if (hungerSlider.value >= maxHunger)
        {
            hungerSlider.value = maxHunger;
        }

        //STAMINA CONTROL SECTION

        if (charController.velocity.magnitude > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            staminaSlider.value -= Time.deltaTime / staminaFallRate * staminaFallMultiplier;

            // TODO take out and change it to fit season/weather and the else method too.
            if (staminaSlider.value > 0)
            {
                currentTemp += Time.deltaTime / 5;
            }
        }

        else
        {
            staminaSlider.value += Time.deltaTime / staminaRegainRate * staminaRegainMultiplier;
            // TODO take out and change it to fit season/weather.
            if (currentTemp >= normalTemp)
            {
                currentTemp -= Time.deltaTime / 10;
            }
        }
        if (staminaSlider.value >= maxStamina)
        {
            staminaSlider.value = maxStamina;
        }
        else if (staminaSlider.value <= 0)
        {
            staminaSlider.value = 0;            
        }

    }

    void UpdateTemp()
    {
        tempNumber.text = currentTemp.ToString("00.0");
    }

    void CharacterDeath()
    {
        // TODO link up with UFPS
    }
}
