using FootStone.GrainInterfaces;
using Ice;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

class PlayerPushI : PlayerPushDisp_
{
    public override void hpChanged(int hp, Current current = null)
    {
        Debug.Log("new hp" + hp);
    }
}

public class Test : MonoBehaviour {

  //  private string btn_name;
    private Text btn_text;
	// Use this for initialization
	void Start () {

        //      NetworkIce.Instance.Init("192.168.206.1",12000);
        NetworkIce.Instance.Init("192.168.3.14", 4061);
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
        NetworkIce.Instance.Update();     
    }



    private async void OnClick()
    {
        try
        {
            Debug.Log("Test init");

            //   var sessionPrx = await NetworkIce.Instance.createSession("1");
            //  var playerPrx = await sessionPrx.createPlayerAsync();

            PlayerPrx playerPrx = null;
            try
            {
                playerPrx = PlayerPrxHelper.uncheckedCast(NetworkIce.Instance.Communicator.stringToProxy("player"));
            }
            catch (Ice.NotRegisteredException)
            {
            //    var query =
            //        IceGrid.QueryPrxHelper.checkedCast(NetworkIce.Instance.Communicator.stringToProxy("FootStone/Query"));
            //    playerPrx = PlayerPrxHelper.checkedCast(query.findObjectByType("::FootStone::GrainInterfaces::Player"));
            }
            if (playerPrx == null)
            {
                Console.WriteLine("couldn't find a `::Player' object");
                return ;
            }

            //
            // Create an object adapter with no name and no endpoints for receiving callbacks
            // over bidirectional connections.
            //
            var adapter = NetworkIce.Instance.Communicator.createObjectAdapter("");

            //
            // Register the callback receiver servant with the object adapter
            //
            var proxy = PlayerPushPrxHelper.uncheckedCast(adapter.addWithUUID(new PlayerPushI()));

            //
            // Associate the object adapter with the bidirectional connection.
            //
            (await playerPrx.ice_getConnectionAsync()).setAdapter(adapter);
            string id = Guid.NewGuid().ToString();
            //
            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.
            //
            await playerPrx.addPushAsync(id,proxy);

            var playerInfo = await playerPrx.getPlayerInfoAsync(id);
            btn_text.text = playerInfo.name;
            Debug.Log(btn_text.text);

        }
        catch (System.Exception ex)
        {
            btn_text.text = "ping:" + ex.StackTrace;
            Debug.LogError(ex.StackTrace);
        }

        //for (int i = 0; i < 1; ++i)
        //    {
        //        player.begin_getPlayerInfo(Guid.NewGuid().ToString()).whenCompleted(
        //        (playerInfo) =>
        //        {

        //        },
        //        (Ice.Exception ex) =>
        //        {
        //            btn_text.text = "getPlayerInfo:" + ex.StackTrace;
        //            Debug.Log(ex.Message);
        //        });
        //    }



    }
}
