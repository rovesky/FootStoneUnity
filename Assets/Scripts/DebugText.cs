using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    private Text debug;
    // Start is called before the first frame update    
    void Start()
    {
        debug = this.GetComponent<Text>();
        debug.text = "init";
    }

    // Update is called once per frame
    void Update()
    {
        debug.text = SampleClient.Logger.Instance.Text;

       
        
    }
}
