using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetInteraction : MonoBehaviour
{
    [Header("Orbitation")]
    public GameObject pivotObject = null;
    
    public bool isOrbiting = false; // Para saber si el jugador est� en �rbita

    [Header("Impulse")]
    [SerializeField] bool isImpulsing;
    [SerializeField] int impulseForce = 70;
    [SerializeField] float deceleration = 0.5f;
    [SerializeField] SpriteRenderer arrowPointer;
    [SerializeField] GameObject holdSpace;
    [SerializeField] GameObject releaseSpace;

    [Header("Gravity")]
    public bool isBeingAttracted;
    public SphereCollider destructionTrigger;

    PlayerMovement playerMovement;
    CameraFollow cameraFollow;
    Rigidbody rb;
    EnergyManagement energyManagement;
    GravityField gravityField;
    [SerializeField] PlanetGeneration planetGeneration;
    private Vector3 randomAxis;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        rb = GetComponent<Rigidbody>();
        energyManagement = GetComponent<EnergyManagement>();
        gravityField = GetComponent<GravityField>();
        destructionTrigger = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        arrowPointer.enabled = false;
        holdSpace.SetActive(false);
        releaseSpace.SetActive(false);

        randomAxis = Random.onUnitSphere;

        destructionTrigger.radius = planetGeneration.destructionThreshold;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlanetOrbit"))
        {
            pivotObject = collision.gameObject; // Guardar el planeta como objeto pivote
            //Debug.Log("Entered collision with: " + pivotObject.transform.parent.name);

            //playerMovement.enabled = false;
            playerMovement.currentSpeed = 0;
            
            rb.useGravity = false;
            rb.velocity = Vector3.zero;

            //cameraFollow.objectToFollow = pivotObject;  //ya veremos
            
            isOrbiting = true; // Activar estado de orbitar
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlanetOrbit"))
        {
            //Debug.Log("Exit collision with: " + collision.gameObject.name);

            cameraFollow.objectToFollow = this.gameObject;
            playerMovement.enabled = true;

            pivotObject = null;
            isOrbiting = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlanetAttractionField")) //el collider
        {
            pivotObject = other.gameObject;
            //Debug.Log("Player enter the gravity field of the: " + pivotObject.transform.parent.name);

            isBeingAttracted = true;
            AttractToPlanet();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // Si el cometa sale del campo gravitacional del planeta
        if (other.gameObject.CompareTag("PlanetAttractionField") && pivotObject != null)
        {
            //Debug.Log("Player exit the gravity field of the: " + pivotObject.transform.parent.name);

            isBeingAttracted = false;
        }

        //ESTO NO LO ENTIENDO
        if (other.CompareTag("Planet")) //destroy planets
        {
            Destroy(other.gameObject);
        }
    }
    private void FixedUpdate()
    {
        if (isBeingAttracted && pivotObject != null)
        {
            AttractToPlanet();
        }
    }

    private void Update()
    {
        if (isOrbiting && pivotObject != null)
        {
            //Debug.Log("Pivote: "+ pivotObject.name + " del " + pivotObject.transform.parent.name);

            transform.RotateAround(pivotObject.transform.position, randomAxis,
                (pivotObject.transform.parent.GetComponent<GravityField>().rotationSpeed
                + playerMovement.currentSpeed) * Time.deltaTime);
        }
        else if(pivotObject == null)
        {
            //Debug.Log("No encuentra el pivote en el Planet Interacrion");
            return;
        }

        if (isOrbiting && energyManagement.isFullEnergised)
        {
            holdSpace.SetActive(true);

            if (Input.GetKey(KeyCode.Space))
            {
                holdSpace.SetActive(false);
                releaseSpace.SetActive(true);
                arrowPointer.enabled = true;
                pivotObject.transform.parent.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                ApplyImpulse();
                arrowPointer.enabled = false;
                isImpulsing = true;

                pivotObject.transform.parent.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 1f);
            }

        }
        if (isImpulsing)
        {
            holdSpace.SetActive(false);
            releaseSpace.SetActive(false);
            ApplyDeceleration();
        }
    }

    private void ApplyDeceleration()
    {
        playerMovement.currentSpeed -= deceleration * Time.fixedDeltaTime;

        if (playerMovement.currentSpeed < 0)
        {
            playerMovement.currentSpeed = 0;
            isImpulsing = false; 
        }

        rb.velocity = transform.forward * playerMovement.currentSpeed;
    }

    private void ApplyImpulse()
    {
        Vector3 forceDirection = transform.forward;
        rb.AddForce(forceDirection * impulseForce, ForceMode.Impulse);
        playerMovement.currentSpeed = impulseForce;
    }

    private void AttractToPlanet()
    {
        if(pivotObject != null)
        {
            Vector3 directionToPlanet = pivotObject.transform.position - transform.position;
            directionToPlanet.Normalize(); 

            // Aplicar una fuerza

            rb.AddForce(directionToPlanet * 
                (pivotObject.transform.parent.GetComponent<GravityField>().gravity) * 
                Time.fixedDeltaTime, ForceMode.Acceleration);
            //Debug.Log(pivotObject.transform.parent);
            //Debug.Log(pivotObject.transform.parent.GetComponent<GravityField>().gravity);
        }
        else
        {
            Debug.Log("Mira me cago en dios");
        }
        
    }
}
