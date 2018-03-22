using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AH_SleepController : MonoBehaviour {

    [SerializeField] private GameObject sleepUI;
    [SerializeField] private Slider sleepSlider;
    [SerializeField] private Text sleepNumber;

    [SerializeField] private float hourlyRegen;
    [SerializeField] private AH_DisableManager disableManager;
    [SerializeField] private AH_TimeController timeController;

    private void Start()
    {
        sleepUI.SetActive(false);
        disableManager = GameObject.FindObjectOfType<AH_DisableManager>();
        timeController = FindObjectOfType<AH_TimeController>();
    }

    public void EnableSleepUI()
    {
        sleepUI.SetActive(true);
        disableManager.DisablePlayer();
    }

    public void UpdateSlider()
    {
        sleepNumber.text = sleepSlider.value.ToString("0");
    }

    public void SleepBtn(AH_PlayerVitals playerVitals)
    {
        playerVitals.fatigueSlider.value += sleepSlider.value * hourlyRegen;
        //if fatigue is more than 30, give full stamina back
        if (playerVitals.fatigueSlider.value > 30)
        {
            playerVitals.fatigueMaxStamina = playerVitals.normMaxStamina;
        }
        playerVitals.staminaSlider.value = playerVitals.normMaxStamina;
        playerVitals.fatigueStage1 = true;
        timeController.AddSleepHours(sleepSlider.value);
        sleepSlider.value = 1;
        sleepUI.SetActive(false);
        disableManager.EnablePlayer();
    
    }
    public void CancelBtn()
    {
        sleepUI.SetActive(false);
        disableManager.EnablePlayer();
    }
}
