using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBBattle;
public class BattleReCon : BattleCon {

	  

	public override void Start () { 
		UdpPB.Instance ().StartClientUdp ();
		UdpPB.Instance().mes_battle_start = Message_Battle_Start;
		UdpPB.Instance ().mes_frame_operation = Message_Frame_Operation;
		UdpPB.Instance ().mes_delta_frame_data = Message_Delta_Frame_Data;
		UdpPB.Instance ().mes_down_game_over = Message_Down_Game_Over;

		isBattleStart = false;
		StartCoroutine ("WaitInitData2"); 
	}
	public override void Message_Battle_Start(UdpBattleStart _mes)
	{ 
	}
	IEnumerator WaitInitData2(){
		yield return new WaitUntil (()=>{
			return roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
		});
		Debug.LogError("WaitInitData2    WaitInitData2");
		UdpPB.Instance().SendBattleReady(NetGlobal.Instance().userUid, BattleData.Instance.battleID);
		//this.InvokeRepeating ("Send_BattleReady", 0.2f, 0.2f);

		 BattleStart2();
	}


	//void Send_BattleReady(){
	//	UdpPB.Instance ().SendBattleReady (NetGlobal.Instance().userUid,BattleData.Instance.battleID);
	//}


	void BattleStart2()
	{

		float _time = NetConfig.frameTime * 0.001f;  // 66ms
		this.InvokeRepeating("Send_operation", _time, _time);  // 循环调用 Send_operation 方法

		StartCoroutine("WaitForFirstMessage2");
	}


	IEnumerator WaitForFirstMessage2()
	{
		yield return 1;
		this.InvokeRepeating("LogicUpdate", 0f, 0.020f);

		if (delegate_readyOver != null)
		{
			delegate_readyOver();    // 关闭对局等待界面
		}
	}


}
