using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBBattle;
public class BattleCon : MonoBehaviour {

	public delegate void DelegateEvent();
	public DelegateEvent delegate_readyOver;
	public DelegateEvent delegate_gameOver;

	public bool isBattleStart;
	private bool isBattleFinish;

	[HideInInspector]
	public static RoleManage roleManage;
	[HideInInspector]
	public static ObstacleManage obstacleManage;
	[HideInInspector]
	public static BulletManage bulletManage;
	[HideInInspector]
	public static NpcManage npcManage;





	private static BattleCon instance;
	public static BattleCon Instance {
		get {
			return instance;
		}
	}
	void Awake()
	{
		instance = this;
	}

	public virtual void Start() {
		UdpPB.Instance().StartClientUdp();
		UdpPB.Instance().mes_battle_start = Message_Battle_Start;
		UdpPB.Instance().mes_frame_operation = Message_Frame_Operation;
		UdpPB.Instance().mes_delta_frame_data = Message_Delta_Frame_Data;
		UdpPB.Instance().mes_down_game_over = Message_Down_Game_Over;

		isBattleStart = false;
		StartCoroutine("WaitInitData");




	}

	IEnumerator WaitInitData() {
		yield return new WaitUntil(() => {
			return roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
		});
		SnapShot();
		this.InvokeRepeating("Send_BattleReady", 0.5f, 0.2f);
	}

	public void InitData(Transform _map) {
		ToolRandom.srand((ulong)BattleData.Instance.randSeed); // 设置随机数种子。
		roleManage = gameObject.AddComponent<RoleManage>();  //角色管理器 生成角色
		obstacleManage = gameObject.AddComponent<ObstacleManage>();  // 障碍物管理 
		bulletManage = gameObject.AddComponent<BulletManage>();     // 子弹管理器
		npcManage = gameObject.AddComponent<NpcManage>();     // 子弹管理器

		GameVector2[] roleGrid;// 角色 坐标
		roleManage.InitData(_map.Find("Role"), out roleGrid); // 初始化角色
		obstacleManage.InitData(_map.Find("Obstacle"), roleGrid);  // 初始化障碍物
		bulletManage.InitData(_map.Find("Bullet"));   // 初始化子弹
		npcManage.InitData(_map.Find("Npc"));   // 初始化子弹
	}

	void Send_BattleReady() {
		UdpPB.Instance().SendBattleReady(NetGlobal.Instance().userUid, BattleData.Instance.battleID);
	}

	public virtual void Message_Battle_Start(UdpBattleStart _mes) {
		BattleStart();
	}

	void BattleStart()
	{
		//Debug.Log("BattleStart isBattleStart "  + isBattleStart);
		if (isBattleStart) {
			return;
		}

		isBattleStart = true;
		this.CancelInvoke("Send_BattleReady");

		float _time = NetConfig.frameTime * 0.001f;  // 66ms
		this.InvokeRepeating("Send_operation", _time, _time);  // 循环调用 Send_operation 方法

		StartCoroutine("WaitForFirstMessage");
	}

	void Send_operation()
	{
		UdpPB.Instance().SendOperation();
	}

	IEnumerator WaitForFirstMessage() {
		yield return new WaitUntil(() => {
			//	Debug.Log("frameDataNum >0 *** " + BattleData.Instance.GetFrameDataNum());
			return BattleData.Instance.GetFrameDataNum() > 0; // 在这里等待第一帧，第一帧没更新之前不会做更新。
		});
		this.InvokeRepeating("LogicUpdate", 0f, 0.02f); // 0.020f

		if (delegate_readyOver != null) {
			delegate_readyOver();    // 关闭对局等待界面
		}
	}

	public void Message_Frame_Operation(UdpDownFrameOperations _mes)
	{


		BattleData.Instance.AddNewFrameData(_mes.frameID, _mes.operations);
		BattleData.Instance.netPack++;
	}

	//逻辑帧更新
	void LogicUpdate() {
		AllPlayerOperation _op;
		if (BattleData.Instance.TryGetNextPlayerOp(out _op)) {
		 
			npcManage.Logic_createNpc();
			roleManage.Logic_Operation(_op);
			roleManage.Logic_Move();
			bulletManage.Logic_Move();
			bulletManage.Logic_Collision();

			npcManage.Logic_Move();


			roleManage.Logic_Move_Correction();
			obstacleManage.Logic_Destory();
			bulletManage.Logic_Destory();
			BattleData.Instance.RunOpSucces();
			SnapShot();// 快照
		}
	}
	private void SnapShot()
	{
		BattleData.StateSnapShot sss  ;
		//sss.battleUid =
		//sss.battleUid =
		//Debug.LogError("快照1 ** " + roleManage.getDic_Role().Count);
		//Debug.LogError("快照2 ** " + roleManage.getDic_Role()[1]);
		//Debug.LogError("快照3 ** " + roleManage.getDic_Role()[1].objShape);

		int i = 0;
		foreach (var item in BattleData.Instance.list_battleUser)
		{
			sss = new BattleData.StateSnapShot();
			sss.logicpos = roleManage.getDic_Role()[item.battleID].objShape.GetPosition();
			sss.dir = roleManage.getDic_Role()[item.battleID].roleDirection;
			sss.logicSpeed = roleManage.getDic_Role()[item.battleID].logicSpeed; 
			if (BattleData.Instance.dic_StateSnapShot[i].ContainsKey(BattleData.Instance.curFramID))
			{ 
				BattleData.Instance.dic_StateSnapShot[i][BattleData.Instance.curFramID]= sss;
				i++;
				continue;
			}
			BattleData.Instance.dic_StateSnapShot[i].Add(BattleData.Instance.curFramID, sss);
			i++;
		}
		 
	
		//记录快照
		//	Debug.LogError(" BattleData.Instance.curFramID == " + BattleData.Instance.curFramID);




	


	}
	public void Message_Delta_Frame_Data (UdpDownDeltaFrames _mes)
	{
		if (_mes.framesData.Count > 0) {
			foreach (var item in _mes.framesData) { 
				BattleData.Instance.AddLackFrameData(item.frameID, item.operations);
			}
		}
	}

	public void OnClickGameOver(){
		BeginGameOver ();
	}

	void BeginGameOver ()
	{
		this.CancelInvoke ("Send_operation");
		this.InvokeRepeating ("SendGameOver", 0f, 0.5f);
	}

	void SendGameOver ()
	{
		UdpPB.Instance ().SendGameOver (BattleData.Instance.battleID);
	}

	public void Message_Down_Game_Over (UdpDownGameOver _mes)
	{
		this.CancelInvoke ("SendGameOver");
		Debug.Log ("游戏结束咯～～～～～～");
		if (delegate_gameOver != null) {
			delegate_gameOver ();
		}
	}


	void OnDestroy ()
	{
	//	Debug.Log("清理之前 "  + BattleData.dic_frameDate_Static.Count);
		BattleData.Instance.ClearData ();
	//	Debug.Log("清理之后 " + BattleData.dic_frameDate_Static.Count);
		UdpPB.Instance ().Destory ();
		instance = null;
	}

	void Update()
	{

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			UdpPB.Instance().MyDestory();
		}
	
	}
	
}
