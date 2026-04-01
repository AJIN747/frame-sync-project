using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PBBattle;
public class GameCon : MonoBehaviour {

	public bool isBattle = true;

	public UIGameOver uiGameOver;
	public GameObject uiReady;
	public Transform mapTranform;


	void Start () {
		if (isBattle) {
			uiReady.SetActive (true);
			if (BattleData.Instance.isReplay)
			{
				ReplayCon _battleCon2 = gameObject.AddComponent<ReplayCon>();
				_battleCon2.delegate_readyOver = ReadyFinish;
				_battleCon2.delegate_gameOver = GameOver;
				_battleCon2.InitData(mapTranform);       // 战场初始化。	

				return;
			}
			if (BattleData.Instance.isReconnect) //|| 1 == 1
			{
				BattleReCon _battleCon1 = gameObject.AddComponent<BattleReCon>();
				_battleCon1.delegate_readyOver = ReadyFinish;
				_battleCon1.delegate_gameOver = GameOver;
				_battleCon1.InitData(mapTranform);       // 战场初始化。	
				return;
			}
			BattleCon _battleCon = gameObject.AddComponent<BattleCon> ();
			_battleCon.delegate_readyOver = ReadyFinish;
			_battleCon.delegate_gameOver = GameOver;
			_battleCon.InitData (mapTranform);		// 战场初始化。	
		}
	}

	void ReadyFinish(){
		uiReady.SetActive (false);
	}

	void GameOver(){
		BattleData.randSeed_static = BattleData.Instance.randSeed;
		BattleData.list_battleUser_Static = new List<BattleUserInfo>(BattleData.Instance.list_battleUser.ToArray()) ; 
		BattleData.dic_frameDate_Static = new Dictionary<int, AllPlayerOperation>();


		foreach (KeyValuePair<int, AllPlayerOperation> kv in BattleData.Instance.dic_frameDate )
		{
			BattleData.dic_frameDate_Static.Add(kv.Key,kv.Value);
		}
		uiGameOver.ShowSelf ();


		foreach (KeyValuePair<int, AllPlayerOperation> kv in BattleData.dic_frameDate_Static)
		{
			Debug.Log("k== " + kv.Key +" v==" + kv.Value);
		}


	}
}
