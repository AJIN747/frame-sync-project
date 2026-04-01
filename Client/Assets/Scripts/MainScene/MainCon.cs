using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PBCommon;
using PBMatch;
public class MainCon : MonoBehaviour {

	public GameObject waitMatchObj;

	void Start(){
		waitMatchObj.SetActive (false);
		Debug.LogError("maincon start******************");
		TcpPB.Instance ().mes_request_match_result = Message_Reauest_Match_Result; // 请求对局
		TcpPB.Instance ().mes_cancel_match_result = Message_Cancel_Match_Result;
		TcpPB.Instance ().mes_enter_battle = Message_Enter_Battle;   // 进入战场

		if (BattleData.Instance.isReconnect)
		{
			TcpRequestMatch _mes = new TcpRequestMatch();
			_mes.uid = NetGlobal.Instance().userUid;
			_mes.isRecon = true;
			//_mes.roleID = 1; // 取服务器
			MyTcp.Instance.SendMessage(CSData.GetSendMessage<TcpRequestMatch>(_mes, CSID.TCP_REQUEST_MATCH));
		}
	}

	public void OnClickMatch(){
		//开始匹配 
		TcpRequestMatch _mes = new TcpRequestMatch();
		_mes.uid = NetGlobal.Instance().userUid;
		_mes.roleID = 1;
		MyTcp.Instance.SendMessage(CSData.GetSendMessage<TcpRequestMatch>(_mes,CSID.TCP_REQUEST_MATCH));
	}
	public void OnClickReplay()
	{
		BattleData.Instance.isReplay = true;
		BattleData.Instance.UpdateBattleInfo(BattleData.randSeed_static, BattleData.list_battleUser_Static);

		ClearSceneData.LoadScene(GameConfig.replayScene);
	}
	public void OnCliclCancelMatch(){
		//取消匹配
		TcpCancelMatch _mes = new TcpCancelMatch();
		_mes.uid = NetGlobal.Instance().userUid;
		MyTcp.Instance.SendMessage(CSData.GetSendMessage<TcpCancelMatch>(_mes,CSID.TCP_CANCEL_MATCH));
	}

	void Message_Reauest_Match_Result(TcpResponseRequestMatch _result){
		waitMatchObj.SetActive (true);
	}

	void Message_Cancel_Match_Result(TcpResponseCancelMatch _result){
		waitMatchObj.SetActive (false);
	}

    // 进入战场
	void Message_Enter_Battle(PBBattle.TcpEnterBattle _mes){
        Debug.Log(_mes.battleUserInfo + "进入战场");
		BattleData.Instance.UpdateBattleInfo(_mes.randSeed,_mes.battleUserInfo);

		ClearSceneData.LoadScene (GameConfig.battleScene);
	}


	void OnDestroy(){
		TcpPB.Instance ().mes_request_match_result = null;
		TcpPB.Instance ().mes_cancel_match_result = null;
		TcpPB.Instance ().mes_enter_battle = null;
	}
}
