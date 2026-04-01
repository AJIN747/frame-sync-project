using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBBattle;
using System.Runtime.CompilerServices;

public class ReplayCon : MonoBehaviour
{

	public delegate void DelegateEvent();
	public DelegateEvent delegate_readyOver;
	public DelegateEvent delegate_gameOver;

	private bool isBattleStart;
	private bool isBattleFinish;

	//[HideInInspector]
	//public RoleManage roleManage;
	//[HideInInspector]
	//public ObstacleManage obstacleManage;
	//[HideInInspector]
	//public BulletManage bulletManage;
	 private float replaySpeed= 0.066f;
	 public float narmalSpeed =1f;
	bool rePlayBegin = false ;
	public void setSpeed(float param)
	{
		 replaySpeed *= param;
		//narmalSpeed /= param;
		//Debug.Log("replaySpeed " + replaySpeed);
	}
	private static ReplayCon instance;
	public static ReplayCon Instance
	{
		get
		{
			return instance;
		}
	}
	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		Debug.Log("录像回放******");
		//UdpPB.Instance().StartClientUdp();
		//UdpPB.Instance().mes_battle_start = Message_Battle_Start;
		//UdpPB.Instance().mes_frame_operation = Message_Frame_Operation;
		//UdpPB.Instance().mes_delta_frame_data = Message_Delta_Frame_Data;
		//UdpPB.Instance().mes_down_game_over = Message_Down_Game_Over;

		isBattleStart = false;
		StartCoroutine("WaitInitData");
	}

	IEnumerator WaitInitData()
	{
		yield return new WaitUntil(() => {
			return  BattleCon.roleManage.initFinish && BattleCon.obstacleManage.initFinish && BattleCon.bulletManage.initFinish;
		});
		BattleStart();
		//	this.InvokeRepeating("Send_BattleReady", 0.5f, 0.2f);
	}

	public void InitData(Transform _map)
	{
		ToolRandom.srand((ulong)BattleData.Instance.randSeed); // 设置随机数种子。
		BattleCon.roleManage = gameObject.AddComponent<RoleManage>();  //角色管理器 生成角色
		BattleCon.obstacleManage = gameObject.AddComponent<ObstacleManage>();  // 障碍物管理 
		BattleCon.bulletManage = gameObject.AddComponent<BulletManage>();     // 子弹管理器

		GameVector2[] roleGrid;// 角色 坐标
		BattleCon.roleManage.InitData(_map.Find("Role"), out roleGrid); // 初始化角色
		BattleCon.obstacleManage.InitData(_map.Find("Obstacle"), roleGrid);  // 初始化障碍物
		BattleCon.bulletManage.InitData(_map.Find("Bullet"));   // 初始化子弹
	}

	//void Send_BattleReady()
	//{
	//	UdpPB.Instance().SendBattleReady(NetGlobal.Instance().userUid, BattleData.Instance.battleID);
	//}

	void Message_Battle_Start(UdpBattleStart _mes)
	{
		BattleStart();
	}

	void BattleStart()
	{
		Debug.Log("BattleStart isBattleStart " + isBattleStart);
		//if (isBattleStart)
		//{
		//	return;
		//}

		isBattleStart = true;
	//	this.CancelInvoke("Send_BattleReady");

		//float _time = NetConfig.frameTime * 0.001f;  // 66ms
													 //	this.InvokeRepeating("Send_operation", _time, _time);  // 循环调用 Send_operation 方法
		rePlayBegin = true;
		//StartCoroutine("WaitForFirstMessage");
		//this.InvokeRepeating("LogicUpdate", 5f, replaySpeed);

		if (delegate_readyOver != null)
		{
			delegate_readyOver();    // 关闭对局等待界面
		}
	}
	 
	void Send_operation()
	{
		UdpPB.Instance().SendOperation();
	}

	//IEnumerator WaitForFirstMessage()
	//{
	//	yield return new WaitUntil(() => {
	//		//	Debug.Log("frameDataNum >0 *** " + BattleData.Instance.GetFrameDataNum());
	//		return BattleData.Instance.GetFrameDataNum() > 0; // 在这里等待第一帧，第一帧没更新之前不会做更新。
	//	});
	//	this.InvokeRepeating("LogicUpdate", 0f, 0.020f);

	//	if (delegate_readyOver != null)
	//	{
	//		delegate_readyOver();    // 关闭对局等待界面
	//	}
	//}

	void Message_Frame_Operation(UdpDownFrameOperations _mes)
	{


		BattleData.Instance.AddNewFrameData(_mes.frameID, _mes.operations);
		BattleData.Instance.netPack++;
	}
	float lasttime;
	//逻辑帧更新
	void LogicUpdate()
	{

		Debug.Log(Time.realtimeSinceStartup - lasttime);
		lasttime = Time.realtimeSinceStartup;
		 
		AllPlayerOperation _op;
		Debug.Log(BattleData.dic_frameDate_Static.Count);
		if (BattleData.Instance.TryGetNextPlayerOpReplay(out _op))
		{

			BattleCon.roleManage.Logic_Operation(_op);
			BattleCon.roleManage.Logic_Move();
			BattleCon.bulletManage.Logic_Move();
			BattleCon.bulletManage.Logic_Collision();
			BattleCon.roleManage.Logic_Move_Correction();
			BattleCon.obstacleManage.Logic_Destory();
			BattleCon.bulletManage.Logic_Destory();
			BattleData.Instance.RunOpSucces();
		}
	}

	//void Message_Delta_Frame_Data(UdpDownDeltaFrames _mes)
	//{
	//	if (_mes.framesData.Count > 0)
	//	{
	//		foreach (var item in _mes.framesData)
	//		{
	//			BattleData.Instance.AddLackFrameData(item.frameID, item.operations);
	//		}
	//	}
	//}

	public void OnClickGameOver()
	{
		BeginGameOver();
	}

	void BeginGameOver()
	{
		this.CancelInvoke("Send_operation");
		this.InvokeRepeating("SendGameOver", 0f, 0.5f);
	}

	void SendGameOver()
	{
		UdpPB.Instance().SendGameOver(BattleData.Instance.battleID);
	}

	void Message_Down_Game_Over(UdpDownGameOver _mes)
	{
		this.CancelInvoke("SendGameOver");
		Debug.Log("游戏结束咯～～～～～～");
		if (delegate_gameOver != null)
		{
			delegate_gameOver();
		}
	}


	void OnDestroy()
	{
		BattleData.Instance.ClearData();
		UdpPB.Instance().Destory();
		instance = null;
	}

	void Update()
	{
		if (!rePlayBegin) return;
		lasttime +=Time.deltaTime;
		if (lasttime>= replaySpeed ) //
		{
			AllPlayerOperation _op;
			Debug.Log(BattleData.dic_frameDate_Static.Count);
			if (BattleData.Instance.TryGetNextPlayerOpReplay(out _op))
			{

				BattleCon.roleManage.Logic_Operation(_op);
				BattleCon.roleManage.Logic_Move();
				BattleCon.bulletManage.Logic_Move();
				BattleCon.bulletManage.Logic_Collision();
				BattleCon.roleManage.Logic_Move_Correction();
				BattleCon.obstacleManage.Logic_Destory();
				BattleCon.bulletManage.Logic_Destory();
				BattleData.Instance.RunOpSucces();
			}
		
			lasttime = 0;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			UdpPB.Instance().MyDestory();
		}
	}

}
