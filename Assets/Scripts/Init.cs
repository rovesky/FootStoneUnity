using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Init : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.Find("input_account").GetComponent<InputField>().text = "a1";
        GameObject.Find("input_password").GetComponent<InputField>().text = "111111";
        NetworkIce.Instance.Init("192.168.3.14", 4061);
    }
	
	// Update is called once per frame
	void Update () {
        NetworkIce.Instance.Update();
    }
}
