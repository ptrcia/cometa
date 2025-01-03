using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlanetGeneration : MonoBehaviour
{
    public int minDistanceBetweenPlanets = 50;
    public int sphereCount;
    public int satelliteCount;
    public int maxRadius; //radio os generation
    public float generationThreshold; // Distance to generate new planets
    public float destructionThreshold; // Distance to destroy far away planets
    public bool canGenerate;

    public List<GameObject> generatedPlanets;


    public GameObject[] spheres;
    public Material[] matsPlanets;
    public Material[] matsSatellites;
    public Material[] trailMat;

    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] GameObject player;

    private Vector3 lastPlayerPosition;
    private int planetCounter;

    GravityField gravityField;
    RotateAroundFinal rotateAround;

    private void Awake()
    {
        spheres = new GameObject[sphereCount];
    }
    private void Start()
    {
        lastPlayerPosition = player.transform.position;
        spheres = CreateSpheres(sphereCount, maxRadius);
        canGenerate = true;
    }

    private void Update()
    {
        destructionThreshold = sphereCollider.radius;

        if (canGenerate && Vector3.Distance(player.transform.position, lastPlayerPosition) > generationThreshold)
        {
            //Generate new planets
            Generate();

        }
        // Check distance player

    }
    public void Generate()
    {
            spheres = CreateSpheres(sphereCount, maxRadius);
            lastPlayerPosition = player.transform.position;
    }

    public GameObject[] CreateSpheres(int count, int radius)
    {
        var sphs = new GameObject[count];
        var sphereToCopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        for (int i = 0; i< count; i++)
        {

            //Separation between generated planets
            Vector3 newPosition;
            bool positionValid;

            do //PROBLEMA: los planetas se deben ir presentando delante o en los lados, no atr�s
            {
                newPosition = player.transform.position + new Vector3(
                    Random.Range(-maxRadius, maxRadius), //X
                    Random.Range(-maxRadius, maxRadius), //Y
                    Random.Range(-maxRadius, maxRadius)); //Z

                positionValid = true;

                foreach (GameObject sphere in sphs)
                {
                    if (sphere != null && Vector3.Distance(newPosition, sphere.transform.position) < minDistanceBetweenPlanets) // Distancia m�nima entre planetas
                    {
                        positionValid = false;
                        break;
                    }
                }
            } while (!positionValid);

            //Create copy
            var sp = GameObject.Instantiate(sphereToCopy);
            CreateSatellites(sp);

            //Name
            sp.name = $"Planet - {planetCounter++}";
            sp.tag = "Planet";
            

            //Position
            sp.transform.position = newPosition;

            //Scale
            sp.transform.localScale *= Random.Range(10, 25);

            //Collider
            SphereCollider collider = sp.GetComponent<SphereCollider>();

            //Material
            sp.GetComponent<Renderer>().material = matsPlanets[Random.Range(0, matsPlanets.Length)];

            //Gravity
            gravityField = sp.AddComponent<GravityField>();
            gravityField.optionPlanetSize = (GravityField.planetSize)Random.Range(0, 3);
            gravityField.PlanetSize(gravityField.optionPlanetSize);

            //RotateAround
            rotateAround = sp.AddComponent<RotateAroundFinal>();
            rotateAround.axis = new Vector3(0, 1, 0);
            rotateAround.pivotObject = sp;
            rotateAround.rotationSpeed = Random.Range(3, 10);

            //Colliders Children
            GameObject orbitSphere = new GameObject("ColliderOrbital");
            orbitSphere.transform.SetParent(sp.transform);
            orbitSphere.transform.localPosition = Vector3.zero;
            orbitSphere.transform.localRotation = Quaternion.identity;
            //orbitSphere.tag = "Planet";
            orbitSphere.tag = "PlanetOrbit";
            SphereCollider colliderOrbit = orbitSphere.AddComponent<SphereCollider>();
            colliderOrbit.isTrigger = false;
            //colliderOrbit.radius = Random.Range(collider.radius +10, collider.radius +20);
            colliderOrbit.radius = (collider.radius * sp.transform.localScale.x) + Random.Range(10, 20);



            GameObject attractionSphere = new GameObject("ColliderAtraction");
            attractionSphere.transform.SetParent(sp.transform);
            attractionSphere.transform.localPosition = Vector3.zero;
            attractionSphere.transform.localRotation = Quaternion.identity;
            attractionSphere.tag = "PlanetAttractionField";
            SphereCollider colliderAtraction = attractionSphere.AddComponent<SphereCollider>();
            colliderAtraction.isTrigger = true;
            //colliderAtraction.radius = Random.Range(collider.radius + 30, collider.radius + 60);
            colliderAtraction.radius = (collider.radius * sp.transform.localScale.x) + Random.Range(30, 60);




            generatedPlanets.Add(sp);
        }
        GameObject.Destroy(sphereToCopy);
        return spheres;
    }

    void CreateSatellites(GameObject planet)
    {
        satelliteCount = Random.Range(1, 6);

        for (int i = 0; i < satelliteCount; i++)
        {
            GameObject satellite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            satellite.transform.SetParent(planet.transform);
            //Name
            satellite.name = "Satellite - " + i.ToString();
            satellite.tag = "Satellite";

            //Position
            Vector3 satellitePosition = Random.onUnitSphere * Random.Range(0.8f, 2.0f);  // Distancia aleatoria desde el planeta
            satellite.transform.localPosition = satellitePosition;

            //Scale
            satellite.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

            //RotateAroundPoint
            RotateAroundPoint rotateAroundPoint = satellite.AddComponent<RotateAroundPoint>();
            rotateAroundPoint.pivotObject = satellite.transform.parent.gameObject;

            //Material
            satellite.GetComponent<Renderer>().material = matsSatellites[Random.Range(0, matsSatellites.Length)];

            //Trail
            TrailRenderer tr = satellite.AddComponent<TrailRenderer>(); //adcomponent o getcomoponent?
            tr.time = 1;
            //tr.startWidth = 0.1f;
            tr.endWidth = 0;
            tr.material = trailMat[Random.Range(0, trailMat.Length)];
            tr.startColor = new Color(1, 1, 0, 0.1f);
            tr.endColor = new Color(0, 0, 0, 0);
            tr.startWidth = 5;
            tr.endWidth = 0;

        }
    }
    private void OnDrawGizmos()
    {
        //drawing a circle on the generation threshold
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.transform.position, generationThreshold);

        //drawing a circle on the destruction threshold
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, destructionThreshold);
    }

}
