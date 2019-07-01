//using FootStone.GrainInterfaces;
//using Ice;
//using Network;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;
//using UnityEngine.UI;

//class PlayerPushI : PlayerPushDisp_
//{
//    //public override void hpChanged(int hp, Current current = null)
//    {
//        Debug.Log("new hp" + hp);
//    }
//}

//public class Btn_Login : MonoBehaviour {

//  //  private string btn_name;
//    private Text btn_text;
//	// Use this for initialization
//	void Start () {  
 
//        Button btn = this.GetComponent<Button>();
//        btn.onClick.AddListener(OnClick);
//        btn_text = btn.transform.Find("Text").GetComponent<Text>();
//        btn_text.text = "login";

//    }

//    // Update is called once per frame
//    void Update () {
      
        
//    }



//    private async void OnClick()
//    {
//        try
//        {
//            Debug.Log("Login init");

//            var accountPrx = NetworkIce.Instance.accountPrx;

//            string account = GameObject.Find("input_account").GetComponent<InputField>().text;
//            string password = GameObject.Find("input_password").GetComponent<InputField>().text;

//            await accountPrx.LoginRequestAsync(new LoginInfo(account, password));
//           // btn_text.text = playerInfo.name;
//            Debug.Log(btn_text.text);
//        }
//        catch (AccountException accountEx)
//        {
//            Debug.LogError(accountEx.ice_message_);
//        }
//        catch (System.Exception ex)
//        {
//            btn_text.text = "ping:" + ex.StackTrace;
//            Debug.LogError(ex.StackTrace);
//        }

//    }

//    private async System.Threading.Tasks.Task GetPlayerInfo()
//    {
//        try
//        {
//            Debug.Log("Test init");

//            //   var sessionPrx = await NetworkIce.Instance.createSession("1");
//            //  var playerPrx = await sessionPrx.createPlayerAsync();

//            PlayerPrx playerPrx = null;
//            try
//            {
//                playerPrx = PlayerPrxHelper.uncheckedCast(NetworkIce.Instance.Communicator.stringToProxy("player"));
//            }
//            catch (Ice.NotRegisteredException)
//            {
//                //    var query =
//                //        IceGrid.QueryPrxHelper.checkedCast(NetworkIce.Instance.Communicator.stringToProxy("FootStone/Query"));
//                //    playerPrx = PlayerPrxHelper.checkedCast(query.findObjectByType("::FootStone::GrainInterfaces::Player"));
//            }
//            if (playerPrx == null)
//            {
//                Console.WriteLine("couldn't find a `::Player' object");
//                return;
//            }
//            string id = Guid.NewGuid().ToString();
//            var ctx = playerPrx.ice_getContext();
//            ctx["playerId"] = id;
//            playerPrx = (PlayerPrx)playerPrx.ice_context(ctx);
//            //
//            // Create an object adapter with no name and no endpoints for receiving callbacks
//            // over bidirectional connections.
//            //
//            var adapter = NetworkIce.Instance.Communicator.createObjectAdapter("");

//            //
//            // Register the callback receiver servant with the object adapter
//            //
//            var proxy = PlayerPushPrxHelper.uncheckedCast(adapter.addWithUUID(new PlayerPushI()));

//            //
//            // Associate the object adapter with the bidirectional connection.
//            //
//            (await playerPrx.ice_getConnectionAsync()).setAdapter(adapter);

//            //
//            // Provide the proxy of the callback receiver object to the server and wait for
//            // shutdown.
//            //
//          //  await playerPrx.AddPushAsync(proxy);

//            var playerInfo = await playerPrx.GetPlayerInfoAsync();
//            btn_text.text = playerInfo.name;
//            Debug.Log(btn_text.text);

//        }
//        catch (System.Exception ex)
//        {
//            btn_text.text = "ping:" + ex.StackTrace;
//            Debug.LogError(ex.StackTrace);
//        }
//    }
//}
