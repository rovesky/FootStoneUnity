using SampleClient;
using UnityEngine;
using UnityEngine.UI;



public class Btn_Test : MonoBehaviour
{

    //  private string btn_name;
    private Text btn_text;
    // Use this for initialization
    void Start()
    {   

        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
        btn_text = btn.transform.Find("Text").GetComponent<Text>();
        btn_text.text = "test";

    }

    // Update is called once per frame
    void Update()
    {


    }


    private  async void OnClick()
    {
        //SampleClient.Network network = new SampleClient.Network();
        //network.Test(1, 0, true);      

        var network = new NetworkNew();
        await network.Test("192.168.0.128",4061,1, 1, false);

    }
  
    
}
