using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EnergyManagement : MonoBehaviour
{
    [Header("Energy")]
    public float currentEnergy = 50;
    public float maxEnergy = 100;
    [SerializeField] float energyRate = 1;

    public bool companionExist;
    public bool isFullEnergised;
    [SerializeField] TextMeshProUGUI energyText;
    [SerializeField] Slider energySlider;

    PlanetInteraction planetInteraction;
    PlayerMovement playerMovement;
    [SerializeField] PlanetController planetController;


    private void Awake()
    {
        planetInteraction = GetComponent<PlanetInteraction>();
        playerMovement = GetComponent<PlayerMovement>();
        //planetController = GetComponent<PlanetController>();
    }
    private void Start()
    {
        isFullEnergised = false;
    }

    private void Update()
    {
        Energy();
        energyText.text = "Energy -> " + (int)currentEnergy;
        energySlider.value = currentEnergy;

    }
    public void Energy()
    {
        if (planetInteraction.isOrbiting)
        {
            if (!isFullEnergised)
            {
                StartCoroutine(IncreaseEnergy());
            }
        }
        else
        {
            if (currentEnergy > 0)
            {
                playerMovement.canMove = true;
                StartCoroutine(DecreaseEnergy());

            }
            else if (currentEnergy <= 0 && !planetController.isSpecialPlanetEvoked)
            {
                //GameManager.instanciate.GameOver();
                Debug.Log("It's game over.");
            }
            else if (currentEnergy <= 0 && planetController.isSpecialPlanetEvoked)
            {
                playerMovement.canMove = false;               
                if(!companionExist)
                {
                    companionExist = true;
                    planetController.SpawnCompanion();
                }
            }
        }

        IEnumerator DecreaseEnergy()
        {
            while (!planetInteraction.isOrbiting && currentEnergy > 0)
            {
                currentEnergy -= energyRate * Time.deltaTime * (playerMovement.currentSpeed * 0.01f);
                yield return new WaitForSeconds(1);
                isFullEnergised = false;
            }

        }

        IEnumerator IncreaseEnergy()
        {

            WaitForSeconds wait = new WaitForSeconds(1);

            while (planetInteraction.isOrbiting && currentEnergy < maxEnergy)
            {
                currentEnergy += energyRate * Time.deltaTime;
                //Debug.Log("currentEnergy: " + currentEnergy + "// energyRate: " + energyRate + "// time: " + Time.deltaTime);
                //imprimir
                yield return wait;
            }

            if (currentEnergy >= maxEnergy)
            {
                currentEnergy = maxEnergy;
                isFullEnergised = true;
                //UI iluminar y pulsar barra
            }

        }
    }
}
