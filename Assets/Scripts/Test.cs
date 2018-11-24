using FootStone.GrainInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    private string btn_name;
    private Text btn_text;
	// Use this for initialization
	void Start () {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
        btn_text = btn.transform.Find("Text").GetComponent<Text>();
        btn_name = "start";
    }
	
	// Update is called once per frame
	void Update () {
        if (!btn_text.text.Equals(btn_name))
        {
            btn_text.text = btn_name;
        }
    }


    private void OnClick()
    {
        Debug.Log("Test init");
        Thread thread = new Thread(new ThreadStart(() =>
       {
           btn_name = "1";
           try
           {
               Ice.InitializationData initData = new Ice.InitializationData();

               initData.properties = Ice.Util.createProperties();
               // initData.properties.setProperty("Ice.ACM.Client", "0");
               // initData.properties.setProperty("Ice.RetryIntervals", "-1");
               initData.properties.setProperty("Ice.FactoryAssemblies", "client");
               initData.properties.setProperty("Ice.Trace.Network", "0");
               initData.properties.setProperty("Player.Proxy", "player:tcp -h 192.168.3.3 -p 12000");

               //
               // using statement - communicator is automatically destroyed
               // at the end of this statement
               //
               using (var communicator = Ice.Util.initialize(initData))
               {
                   Debug.Log("communicator inited!");
                   btn_name = "2";
                   var player = PlayerPrxHelper.uncheckedCast(communicator.propertyToProxy("Player.Proxy"));
                   Debug.Log("player inited!");
                   btn_name = "3";
                   //player.begin_ice_ping().whenCompleted(
                   //    () =>
                   //    {
                           player.begin_getPlayerInfo(Guid.NewGuid().ToString()).whenCompleted(
                                       (playerInfo) =>
                                       {
                                           btn_name = playerInfo.Name;
                                           Debug.Log(btn_name);
                                       },
                                       (Ice.Exception ex) =>
                                       {
                                           btn_name = ex.Message;
                                           Debug.Log(ex.Message);
                                       });
                       //},
                       //(Ice.Exception ex) =>
                       //{
                       //    btn_name = ex.Message;
                       //    Debug.Log(ex.Message);
                       //});
                   communicator.waitForShutdown();

               }
           }
           catch (Exception ex)
           {
               btn_name = ex.Message;
               Debug.Log(ex.Message);

           }
       }));
        thread.Start();
    }
}
