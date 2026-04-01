using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public class BattleManage {

	private int battleID;
	private Dictionary<int,BattleCon> dic_battles;

	private static BattleManage instance = null;
	public static BattleManage Instance
	{
		get{ 
			if (instance == null) {
				instance = new BattleManage ();
			}
			return instance;
		}
	}

	private BattleManage(){
		battleID = 0;
		dic_battles = new Dictionary<int, BattleCon> ();
	}

	public void Creat(){

	}


	public void Destroy(){

		foreach (var item in dic_battles) {
			item.Value.DestroyBattle ();
		}

		dic_battles.Clear ();
		instance = null;
	}

	public void BeginBattle(List<MatchUserInfo> _battleUser){
		
		battleID++;
		BattleCon _battle = new BattleCon ();
		_battle.CreatBattle (battleID,_battleUser);

		dic_battles [battleID] = _battle;

		Debug.Log ("开始战斗 本局的battleID是。。。。。" + battleID);	
	}


	public void FinishBattle(int _battleID){
		dic_battles.Remove (_battleID);
		Debug.Log ("战斗结束。。。。。" + _battleID);
	}
	public void reconnectClient(int _userUid)// 重新连接客户端
	{
		string _ip = UserManage.Instance.GetUserInfo(_userUid).socketIp; 
		BattleCon _battle = dic_battles[ UserManage.Instance.GetUserInfo(_userUid).currentBattleId  ]; 
		//  更新对局的UDP信息。
		var _udp = new ClientUdp(); 
		_udp.StartClientUdp(_ip, _userUid); 
		_udp.delegate_analyze_message = _battle.AnalyzeMessage; 
		_battle.dic_udp[_battle.dic_battleUserUid[_userUid]  ] = _udp; 
		_battle.ReConnectBattle(UserManage.Instance.GetUserInfo(_userUid).currentBattleId, _userUid); 
		 
	}

 
}
