//using FootStone.GrainInterfaces;
//using Network;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Btn_CreatePlayer : MonoBehaviour {

//	// Use this for initialization
//	void Start () {
//        Button btn = this.GetComponent<Button>();
//        btn.onClick.AddListener(OnClick);
      
//    }
	
//	// Update is called once per frame
//	void Update () {
		
//	}

//    private async void OnClick()
//    {
//        try
//        {
//            Debug.Log("CreatePlayer init");

//            var accountPrx = NetworkIce.Instance.accountPrx;
//            string name = GameObject.Find("input_account").GetComponent<InputField>().text + "_name";
//           // string password = GameObject.Find("input_password").GetComponent<InputField>().text; 

//            var id = await accountPrx.CreatePlayerAsync(name, 1);
//            var playerPrx = PlayerPrxHelper.uncheckedCast(accountPrx, "player");
//            var ctx = playerPrx.ice_getContext();
//            ctx["playerId"] = id;    
//            NetworkIce.Instance.PlayerPrx = (PlayerPrx)playerPrx.ice_context(ctx);
//            Debug.Log("create player"+ name+" ok!");
//        }
//        catch (AccountException accountEx)
//        {
//            Debug.LogError(accountEx.ice_message_);
//        }
//        catch (System.Exception ex)
//        {
//        //    btn_text.text = "ping:" + ex.StackTrace;
//            Debug.LogError(ex.StackTrace);
//        }

//    }
//}
