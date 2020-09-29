using UnityEngine;
using System.Collections;

public class LampShutOff : MonoBehaviour {

    private Transform light;

	// Use this for initialization
	void Start () {
        light = this.gameObject.transform.GetChild(1);
	}
	
	// Update is called once per frame
	void Update () {
	    if(this.gameObject.GetComponent<Rigidbody>() != null)
        {
            Destroy(light.gameObject);
            this.GetComponent<LampShutOff>().enabled = false;
        }
	}
}
