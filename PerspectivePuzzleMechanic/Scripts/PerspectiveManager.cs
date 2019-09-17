// Forced Perspective Illusion
// Project from Daniel C Menezes
// V1.32 - 17/09/2019
// GitHub: https://github.com/danielcmcg/Forced-Perspective-Illusion-Mechanic-for-Unity

using UnityEngine;

public class PerspectiveManager : MonoBehaviour {
    
    public Material yellowToon;
    public Material blueToon;
    public Material redToon;
    
    private Camera mainCamera;
    private Transform targetForTakenObjects;
    private GameObject pointer;
    private GameObject takenObject;
    private RaycastHit hit;
    private Ray ray;
    private float distanceMultiplier;
    private Vector3 scaleMultiplier;
    private LayerMask layerMask = ~(1 << 8);
    private float cameraHeight = 0;
    private float cosine;
    private float positionCalculation;
    private float lastPositionCalculation = 0;
    private Vector3 lastHitPoint = Vector3.zero;
    private Vector3 lastRotation = Vector3.zero;
    private float rayMaxRange = 1000f;
    private bool isRayTouchingSomething = true;
    private float lastRotationY;
    
    private Vector3 lastHit = Vector3.zero;
    private Vector3 centerCorrection = Vector3.zero;
    private float takenObjSize = 0;
    private int takenObjSizeIndex = 0;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        targetForTakenObjects = GameObject.Find("targetForTakenObjects").transform;
        pointer = GameObject.Find("Pointer");
        pointer.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height / 2) + (Screen.height / 10), 1));
        pointer.transform.parent = mainCamera.transform;
    }

    void Update()
    {
        ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, (Screen.height / 2) + (Screen.height / 10), 0));
        Debug.DrawRay(ray.origin, ray.direction * 200, Color.yellow);

        if (Physics.Raycast(ray, out hit, rayMaxRange, layerMask))
        {
            if (hit.transform.tag == "Getable")
            {
                pointer.GetComponent<MeshRenderer>().material = blueToon;
            }
            else
            {
                pointer.GetComponent<MeshRenderer>().material = yellowToon;
            }
        }

        isRayTouchingSomething = Physics.Raycast(ray, out hit, rayMaxRange, layerMask);  

        if (takenObject != null)
        {
            pointer.GetComponent<MeshRenderer>().material = redToon;
        }
        else
        {
            targetForTakenObjects.position = hit.point;
        }

        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) && isRayTouchingSomething)
        {
            if (hit.transform.tag == "Getable")
            {
                takenObject = hit.transform.gameObject;

                distanceMultiplier = Vector3.Distance(mainCamera.transform.position, takenObject.transform.position);
                scaleMultiplier = takenObject.transform.localScale;
                lastRotation = takenObject.transform.rotation.eulerAngles;
                lastRotationY = lastRotation.y - mainCamera.transform.eulerAngles.y;
                takenObject.transform.transform.parent = targetForTakenObjects; 

                if (takenObject.GetComponent<Rigidbody>() == null)
                {
                    takenObject.AddComponent<Rigidbody>();
                }
                takenObject.GetComponent<Rigidbody>().isKinematic = true;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = true;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    takenObject.GetComponent<MeshRenderer>().receiveShadows = false;
                }
                takenObject.gameObject.layer = 8;
                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = true;
                    takenObject.GetComponent<Collider>().isTrigger = true;
                    child.gameObject.layer = 8;
                }

                takenObjSize = takenObject.GetComponent<Collider>().bounds.size.y;
                takenObjSizeIndex = 1;
                if (takenObject.GetComponent<Collider>().bounds.size.x > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.x;
                    takenObjSizeIndex = 0;
                }
                if (takenObject.GetComponent<Collider>().bounds.size.z > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.z;
                    takenObjSizeIndex = 2;
                }
            }
        }

        if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0))
        {
            if (takenObject != null)
            {
                // recenter the object to the center of the mesh regardless  real pivot point
                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    centerCorrection = takenObject.transform.position - takenObject.GetComponent<MeshRenderer>().bounds.center;
                }

                takenObject.transform.position = Vector3.Lerp(takenObject.transform.position, targetForTakenObjects.position + centerCorrection, Time.deltaTime * 5);
                takenObject.transform.rotation = Quaternion.Lerp(takenObject.transform.rotation, Quaternion.Euler(new Vector3(0, lastRotationY + mainCamera.transform.eulerAngles.y, 0)), Time.deltaTime * 5);
                
                cosine = Vector3.Dot(ray.direction, hit.normal);
                cameraHeight = Mathf.Abs(hit.distance * cosine);
                
                takenObjSize = takenObject.GetComponent<Collider>().bounds.size[takenObjSizeIndex];
                
                positionCalculation = (hit.distance * takenObjSize / 2) / (cameraHeight);
                if (positionCalculation < rayMaxRange)
                {
                    lastPositionCalculation = positionCalculation;
                }
                
                // if the wall is more distant then the raycast max range, increase the size only untill the max range
                if (isRayTouchingSomething)
                {
                    lastHitPoint = hit.point;
                }
                else
                {
                    lastHitPoint = mainCamera.transform.position + ray.direction * rayMaxRange;
                }
                
                targetForTakenObjects.position = Vector3.Lerp(targetForTakenObjects.position, lastHitPoint
                        - (ray.direction * lastPositionCalculation), Time.deltaTime * 10);
                
                takenObject.transform.localScale = scaleMultiplier * (Vector3.Distance(mainCamera.transform.position, takenObject.transform.position) / distanceMultiplier);
            }
        }

        if (Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0))
        {
            if (takenObject != null)
            {
                takenObject.GetComponent<Rigidbody>().isKinematic = false;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = false;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    takenObject.GetComponent<MeshRenderer>().receiveShadows = true;
                }
                takenObject.transform.parent = null;
                takenObject.gameObject.layer = 0;
                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = false;
                    takenObject.GetComponent<Collider>().isTrigger = false;
                    child.gameObject.layer = 0;
                }

                takenObject = null;
            }
        }
    }
}


