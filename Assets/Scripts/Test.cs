using FootStone.GrainInterfaces;
using Ice;
using Network;
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
      
        NetworkIce.Instance.Init();
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

            var sessionPrx = await NetworkIce.Instance.createSession();
            var playerPrx = await sessionPrx.createPlayerAsync();

            var playerInfo = await playerPrx.getPlayerInfoAsync(Guid.NewGuid().ToString());
            btn_text.text = playerInfo.name;
            Debug.Log(btn_text.text);

        }
        catch (System.Exception ex)
        {
            btn_text.text = "ping:" + ex.StackTrace;
            Debug.Log(ex.StackTrace);
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
