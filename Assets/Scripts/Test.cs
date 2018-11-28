using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

  //  private string btn_name;
    private Text btn_text;
	// Use this for initialization
	void Start () {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
        btn_text = btn.transform.Find("Text").GetComponent<Text>();
        btn_text.text = "start";
    }
	
	// Update is called once per frame
	void Update () {
        //if (!btn_text.text.Equals(btn_name))
        //{
        //    btn_text.text = btn_name;
        //}


        Action[] array;
        lock (this)
        {
            array = actions.ToArray();
            actions.Clear();
        }
        foreach(Action each in array){
            each();
        }
    }

    private List<Action> actions = new List<Action>();

    private void OnClick()
    {
        Debug.Log("Test init");

        btn_text.text = "1";
        try
        {
            Ice.InitializationData initData = new Ice.InitializationData();

            initData.properties = Ice.Util.createProperties();
            initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
            //initData.properties.setProperty("Ice.RetryIntervals", "-1");
            initData.properties.setProperty("Ice.FactoryAssemblies", "client");
            initData.properties.setProperty("Ice.Trace.Network", "0");
            initData.properties.setProperty("Player.Proxy", "player:tcp -h 192.168.206.1 -p 12000 -t 5000");

            initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
            {
                lock (this)
                {
                    actions.Add(action);
                }
                // Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            };

            var communicator = Ice.Util.initialize(initData);

           

            Debug.Log("communicator inited!");
            btn_text.text = "2";
            var player = PlayerPrxHelper.uncheckedCast(communicator.propertyToProxy("Player.Proxy"));
            Debug.Log("player inited!");
            btn_text.text = "3";

            //
            // Create an object adapter with no name and no endpoints for receiving callbacks
            // over bidirectional connections.
            //
         //   var adapter = communicator.createObjectAdapter("");

            //
            // Register the callback receiver servant with the object adapter
            //
       //     var proxy = CallbackReceiverPrxHelper.uncheckedCast(adapter.addWithUUID(new CallbackReceiverI()));

            //
            // Associate the object adapter with the bidirectional connection.
            //
          //  server.ice_getConnection().setAdapter(adapter);


            for (int i = 0; i < 5; ++i)
            {
                player.begin_getPlayerInfo(Guid.NewGuid().ToString()).whenCompleted(
                (playerInfo) =>
                {
                    btn_text.text = playerInfo.name;
                    Debug.Log(btn_text.text);
                },
                (Ice.Exception ex) =>
                {
                    btn_text.text = "getPlayerInfo:" + ex.StackTrace;
                    Debug.Log(ex.Message);
                });
            }
            //}, 
            //(Ice.Exception ex) =>
            //{
            //    btn_name = "ping:"+ ex.StackTrace;
            //    Debug.Log(ex.Message);
            //});
            Thread thread = new Thread(new ThreadStart(() =>
             {
                 communicator.waitForShutdown();
             }));
            thread.Start();
        }
        catch (Ice.Exception ex)
        {
            btn_text.text = ex.StackTrace;
            Debug.Log(ex.Message);
        }
    }
}
