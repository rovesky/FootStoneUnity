//using FootStone.GrainInterfaces;
//using Network;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Btn_Register : MonoBehaviour {

//	// Use this for initialization
//	void Start () {
//        Button btn = this.GetComponent<Button>();
//        btn.onClick.AddListener(OnClick);
//        //btn_text = btn.transform.Find("Text").GetComponent<Text>();
//        //btn_text.text = "login";
//    }
	
//	// Update is called once per frame
//	void Update () {
		
//	}

//    private async void OnClick()
//    {
//        try
//        {
//            Debug.Log("Register init");

//            var accountPrx = NetworkIce.Instance.accountPrx;
//            string account = GameObject.Find("input_account").GetComponent<InputField>().text ;
//            string password = GameObject.Find("input_password").GetComponent<InputField>().text; 

//            await accountPrx.RegisterRequestAsync(new RegisterInfo(account, password));
//            // btn_text.text = playerInfo.name;
//            Debug.Log("register ok!");
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
