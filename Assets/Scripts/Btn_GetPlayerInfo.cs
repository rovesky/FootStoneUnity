//using FootStone.GrainInterfaces;
//using Network;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//public class Btn_GetPlayerInfo : MonoBehaviour
//{
//    // Use this for initialization
//    void Start()
//    {
//        Button btn = this.GetComponent<Button>();
//        btn.onClick.AddListener(OnClick);

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    private async void OnClick()
//    {
//        try
//        {
//            Debug.Log("GetPlayerInfo init");

//            var playerPrx = NetworkIce.Instance.PlayerPrx;
//            var playerInfo = await playerPrx.GetPlayerInfoAsync();
         
//            Debug.Log("get player info:" + JsonUtility.ToJson(playerInfo));
//        }
//        catch (AccountException accountEx)
//        {
//            Debug.LogError(accountEx.ice_message_);
//        }
//        catch (System.Exception ex)
//        {
//            //    btn_text.text = "ping:" + ex.StackTrace;
//            Debug.LogError(ex.StackTrace);
//        }

//    }

//}

