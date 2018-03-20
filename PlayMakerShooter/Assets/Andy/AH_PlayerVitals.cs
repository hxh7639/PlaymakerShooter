using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AH_PlayerVitals : MonoBehaviour {

    #region Health Variables
    public Slider healthSlider;
    public int maxHealth;
    public int healthFallRate;
    #endregion

    #region Thirst Variables
    [Space(10)]
    public Slider thirstSlider;
    public int maxThirst;
    public int thirstFallRate;
    #endregion

    #region Hunger Variables
    [Space(10)]
    public Slider hungerSlider;
    public int maxHunger;
    public int hungerFallRate;
    #endregion

    #region Stamina Variables
    [Space(10)]
    public Slider staminaSlider;
    public int normMaxStamina = 100;
    public float fatigueMaxStamina = 100;
    public int staminaFallRate;
    public int staminaFallMultiplier;
    private int staminaRegainRate;
    public int staminaRegainMultiplier;
    #endregion

    #region Fatigue Region
    [Space(10)]
    public Slider fatigueSlider;
    public int maxFatigue = 100;
    public int fatigueFallRate;

    public bool fatigueStage1 = true;
    #endregion

    #region Temperature Variables
    [Header("Temperature Settings")]
    public float freezingTemp;
    public float currentTemp;
    public float normalTemp;
    public float overheatTemp;
    public Text tempNumber;
    public Image tempBG;
    public int healthFallDueTempMulti = 1;
    #endregion

    private CharacterController charController;
    private vp_FPController playerController;

    // Use this for initialization
    void Start ()
    {
        #region Starting Sliders
        fatigueSlider.maxValue = maxFatigue;
        fatigueSlider.value = maxFatigue;
            
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        thirstSlider.maxValue = maxThirst;
        thirstSlider.value = maxThirst;

        hungerSlider.maxValue = maxHunger;
        hungerSlider.value = maxHunger;

        staminaSlider.maxValue = normMaxStamina;
        staminaSlider.value = normMaxStamina;
        #endregion

        staminaFallRate = 1;
        staminaRegainRate = 1;

        charController = GetComponent<CharacterController>();
        playerController = GetComponent<vp_FPController>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        #region Fatigue Region
        if (fatigueSlider.value <= 30 && fatigueSlider.value >10)
        {
            fatigueMaxStamina = 50;
            staminaSlider.value = fatigueMaxStamina;

        }
        else if (fatigueSlider.value <=10)
        {
            fatigueMaxStamina = 0;
            staminaSlider.value = fatigueMaxStamina;
        }

        if (fatigueSlider.value >=0)
        {
            fatigueSlider.value -= Time.deltaTime * fatigueFallRate;
        }
        else if (fatigueSlider.value <= 0)
        {
            fatigueSlider.value = 0;
        }
        else if (fatigueSlider.value >= maxFatigue)
        {
            fatigueSlider.value = maxFatigue;
        }

        #endregion

        # region Temperature Region        
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
        #endregion

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
        if (staminaSlider.value >= fatigueMaxStamina)
        {
            staminaSlider.value = fatigueMaxStamina;
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
