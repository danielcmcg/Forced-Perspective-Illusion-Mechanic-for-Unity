using UnityEngine;
using System.Collections;

public class SparksPlayer : MonoBehaviour {

    private GameObject relatedObject;
    public ParticleSystem sparkPS;
    public GameObject sparkLight;

    private Vector3 position;

	// Use this for initialization
	void Start () {
        position = sparkPS.transform.position;
        relatedObject = this.transform.parent.gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (relatedObject.layer == 8 && this != null)
        {
            sparkPS.transform.parent = null;
            sparkPS.transform.position = position;
            sparkPS.Play();
            sparkLight.SetActive(true);
            sparkPS.gameObject.layer = 0;
            this.GetComponent<SparksPlayer>().enabled = false;
        }

	}
}
