using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBBattle;
using System;
using System.Threading;

public class BattleData {

	private Mutex mutex_actionList = new Mutex();
	public int randSeed; //随机种子
	public int battleID;
	public bool isReplay;
	public bool isReconnect;
	public const int mapRow = 7;//行数
	public const int mapColumn = 13;//列
	public const int gridLenth = 10000;//格子的逻辑大小
	public const int gridHalfLenth = 5000;//格子的逻辑大小
	public int mapTotalGrid;
	public int mapWidth;
	public int mapHeigh;

	public List<BattleUserInfo> list_battleUser;
	private Dictionary<int, GameVector2> dic_speed;

	private int curOperationID;
	public PlayerOperation selfOperation;

	public int curFramID;
	private int maxFrameID;
	private int clientMaxFrameID;
	private int maxSendNum;

	private List<int> lackFrame;

	public static int randSeed_static; //随机种子
	public static List<BattleUserInfo> list_battleUser_Static;
	public static Dictionary<int, AllPlayerOperation> dic_frameDate_Static;
	public Dictionary<int, AllPlayerOperation> dic_frameDate;
	public Dictionary<int, AllPlayerOperation> client_dic_frameDate;
	private Dictionary<int, int> dic_rightOperationID;


	public Dictionary<int, int>[] clientMove ;
	public Dictionary<int, int>[] serverMove;
	public int[] lastMove ;


	public Dictionary<int, StateSnapShot>[] dic_StateSnapShot; 
	public class StateSnapShot{
		public GameVector2 logicpos;// 逻辑位置
		public GameVector2 logicSpeed;// 
		public int dir; // 摇杆方向
		public int battleUid; 


	} 
	//一些统计数据
	public string log;
	public int fps;
	public int netPack;
	public int sendNum;
	public int recvNum;
	private int lastDir; 
	private static BattleData instance;
	public static BattleData Instance
	{
		get	{ 
			// 如果类的实例不存在则创建，否则直接返回
			if (instance == null) {
				instance = new BattleData ();
			}
			return instance;
		}
	}

	private BattleData(){

		mapTotalGrid = mapRow * mapColumn;
		mapWidth = mapColumn * gridLenth;
		mapHeigh = mapRow * gridLenth;

		curOperationID = 1;
		selfOperation = new PlayerOperation ();
		selfOperation.move = 121;
		ResetRightOperation ();

		dic_speed = new Dictionary<int, GameVector2> ();
		//初始化速度表
		GlobalData.Instance ().GetFileStringFromStreamingAssets ("Desktopspeed.txt", _fileStr => {
			InitSpeedInfo (_fileStr);
		});

		curFramID = 0;
		maxFrameID = 0;
		clientMaxFrameID = 0;
		maxSendNum = 5;

		lackFrame = new List<int> ();
		dic_rightOperationID = new Dictionary<int, int> ();
		dic_frameDate = new Dictionary<int, AllPlayerOperation> ();
		client_dic_frameDate = new Dictionary<int, AllPlayerOperation>();
	}

	public void UpdateBattleInfo(int _randseed,List<BattleUserInfo> _userInfo){
        Debug.Log("UpdateBattleInfo  更新战场信息 "  + Time.realtimeSinceStartup);
		randSeed = _randseed;
		if (_userInfo==null)
		{
			return;
		}
		list_battleUser = new List<BattleUserInfo> (_userInfo);
		clientMove = new Dictionary<int, int>[list_battleUser.Count];
		for (int i =0;i< clientMove.Length;i++)
		{
			clientMove[i] = new Dictionary<int, int>();
		}
		serverMove = new Dictionary<int, int>[list_battleUser.Count];
		for (int i = 0; i < serverMove.Length; i++)
		{
			serverMove[i] = new Dictionary<int, int>();
		}
		lastMove = new int[list_battleUser.Count];
		for (int i = 0; i < lastMove.Length; i++)
		{
			lastMove[i] = 121;
		}

		dic_StateSnapShot = new Dictionary<int, StateSnapShot>[list_battleUser.Count];
		for (int i = 0; i < dic_StateSnapShot.Length; i++)
		{
			dic_StateSnapShot[i] = new Dictionary<int, StateSnapShot>();
		}
		foreach (var item in list_battleUser) {
			if (item.uid == NetGlobal.Instance().userUid) {
				battleID = item.battleID;
				selfOperation.battleID = battleID;
				Debug.LogError ("更新战场信息  自己的战斗id:" + battleID);
			} 
			dic_rightOperationID [item.battleID] = 0; 
		} 
	}

	public void ClearData ()
	{
		curOperationID = 1;
		selfOperation.move = 121;
		ResetRightOperation ();

		curFramID = 0;
		maxFrameID = 0;
		clientMaxFrameID = 0;
		maxSendNum = 5;

		lackFrame.Clear();
		dic_rightOperationID.Clear ();
		dic_frameDate.Clear(); 
		client_dic_frameDate.Clear();
	}


	public void Destory ()
	{
		list_battleUser.Clear ();
		list_battleUser = null;
		instance = null;
	}

	void InitSpeedInfo (string _fileStr)
	{
		string[] lineArray = _fileStr.Split ("\n" [0]); 

		int dir;
		for (int i = 0; i < lineArray.Length; i++) {
			if (lineArray [i] != "") {
				GameVector2 date = new GameVector2 ();
				string[] line = lineArray [i].Split (new char[1]{ ',' }, 3);
				dir = System.Int32.Parse (line [0]);
				date.x = System.Int32.Parse (line [1]);
				date.y = System.Int32.Parse (line [2]);
				dic_speed [dir] = date;
			}
		}
	}

	public GameVector2 GetSpeed (int _dir)
	{
		return dic_speed [_dir];
	}
	//坐标不超出地图
	public GameVector2 GetMapLogicPosition(GameVector2 _pos){
		return new GameVector2 (Mathf.Clamp(_pos.x,0,mapWidth),Mathf.Clamp(_pos.y,0,mapHeigh));
	}

	public GameVector2 GetMapGridCenterPosition(int _row,int _column){
		return new GameVector2 (_column * gridLenth + gridHalfLenth,_row * gridLenth + gridHalfLenth);
	}

	public GameVector2 GetMapGridFromRand(int _randNum){
		int _num1 = _randNum % mapTotalGrid;
		int _row = _num1 / mapColumn;
		int _column = _num1 % mapColumn;
		return new GameVector2 (_row, _column);
	}

	public GameVector2 GetMapGridCenterPositionFromRand(int _randNum){
		GameVector2 grid = GetMapGridFromRand (_randNum);
		return GetMapGridCenterPosition (grid.x, grid.y);
	}


	public void UpdateMoveDir (int _dir)
	{
       // Debug.Log("_dir  ************   "  + _dir);
		selfOperation.move = _dir;
		lastDir = _dir;
	}
	public void UpdateMoveDirUp(int _dir)
	{
		// Debug.Log("_dir  ************   "  + _dir);
		selfOperation.move = _dir;
	}
	public void UpdateRightOperation(RightOpType _type,int _value1,int _value2){
		selfOperation.rightOperation = _type;
		selfOperation.operationValue1 = _value1;
		selfOperation.operationValue2 = _value2;
	//	Debug.Log("curOperationID   "  + curOperationID); //当前操作 每次 UpdateRightOperationID之后++
		selfOperation.operationID = curOperationID;
	}

	public void skill1()
	{
		selfOperation.rightOperation = PBBattle.RightOpType.rop2;
		selfOperation.operationValue1 = lastDir;
		selfOperation.operationValue2 = 0; 
		selfOperation.operationID = curOperationID;
	}
	public void skill2()
	{
		selfOperation.rightOperation = PBBattle.RightOpType.rop3;
		selfOperation.operationValue1 = 0;
		selfOperation.operationValue2 = 0;
		selfOperation.operationID = curOperationID;
	}

	public bool IsValidRightOp(int _battleID,int _rightOpID){
		return _rightOpID > dic_rightOperationID [_battleID];
	}

	public void UpdateRightOperationID(int _battleID,int _opID,RightOpType _type){
		dic_rightOperationID [_battleID] = _opID;
		if (battleID == _battleID) {
			//玩家自己
			curOperationID++;
			if (_type == selfOperation.rightOperation) {
				ResetRightOperation ();
			}
		}
	}

	public void ResetRightOperation(){
		selfOperation.rightOperation = RightOpType.noop;
		selfOperation.operationValue1 = 0;
		selfOperation.operationValue2 = 0;
		selfOperation.operationID = 0;
	}

	public int GetFrameDataNum(){
		if (dic_frameDate == null) {
			return 0;
		} else {
			return dic_frameDate.Count;
		}
	}


	public void AddClientFrameData(int _frameID, UdpUpPlayerOperations _op)
	{
		AllPlayerOperation _aop = new AllPlayerOperation();
		  
		_aop.operations.Add(_op.operation);

		//Debug.LogError("_frameID 增加 " + _frameID);
		mutex_actionList.WaitOne(); 
			client_dic_frameDate[_frameID] = _aop;
		mutex_actionList.ReleaseMutex();
		clientMaxFrameID = _frameID;

		for (int i = 0; i< list_battleUser.Count;i++)
		{
			if (list_battleUser[i].uid == NetGlobal.Instance().userUid&&1==2)  //
			{ 
				clientMove[i].Add(_frameID, _op.operation.move); 
			}
			else
			{
				//if (_frameID==0)
				//{
				//	clientMove[i].Add(_frameID, 121);
				//}else if (_frameID == 1)
				//{
				//	clientMove[i].Add(_frameID, 121);
				//}
				//else
				//{
				//	Debug.Log(_frameID + " _frameID   i ********************** " + i);
					//Debug.Log("i clientMove[i][_frameID - 1] " + clientMove[i][_frameID - 1]);
					clientMove[i].Add(_frameID, lastMove[i]);
				//}
			
			}  
		}
		 
	//	Debug.LogError("C"+_frameID +":" + _op.operation.move);

	}


	public void AddNewFrameData(int _frameID,AllPlayerOperation _op){ 
		dic_frameDate [_frameID] = _op;

		for (int i =0;i<_op.operations.Count;i++)
		{

			//Debug.LogWarning("下发服务器操作 " + _op.operations[i].move);
			if (serverMove[i].ContainsKey(_frameID))
			{
				serverMove[i][_frameID]=  _op.operations[i].move; 
			}
			else
			   serverMove[i].Add(_frameID, _op.operations[i].move);
		}

	
		//Debug.LogError("SAddNewFrameData");
		//Debug.LogWarning("S" + _frameID + ":" + dic_frameDate[_frameID].operations[0].move  );
		// 加入本地的帧，没有收到的一律当作空帧处理
		if (_frameID - maxFrameID > 1)
		{
			Debug.LogError("**** _frameID - maxFrameID : " + (_frameID - maxFrameID)); 
			List<int> sendList = new List<int>();
			sendList.Add(maxFrameID + 1);
			sendList.Add(_frameID - 1); 
			UdpPB.Instance().SendRequestFrames(selfOperation.battleID, sendList); 
		}
	 
		maxFrameID = _frameID;
		checkClientServerBuffer(_frameID);
		//


	}
	public void AddLackFrameData(int _frameID, AllPlayerOperation _newOp)
	{ 
			dic_frameDate[_frameID] = _newOp; 
			Debug.LogWarning("补帧 :" + _frameID); 
	}
	void checkClientServerBuffer(int frameNum__)
	{
		bool rsl = true;
		int j = 0;

		// Debug.LogWarning("检测帧数  " + frameNum__);
		for (int i = 0; i < serverMove.Length; i++)
		{ 
			if (clientMove[i][frameNum__] != serverMove[i][frameNum__])
			{
				j = i;
				rsl = false;
				break;
			}
		}
		//Debug.Log(frameNum__ + "  j:" + j + "比较 C:" + clientMove[j][frameNum__] + "  S:" + serverMove[j][frameNum__]);
		if (!rsl)
		//if (clientMove[frameNum__] != serverMove[frameNum__])
		{
			 
			 Debug.LogError("预测失败：第"+frameNum__ +"帧不同  j:"+j+ "  客户端:" + clientMove[j][frameNum__] +"  服务器:" +serverMove[j][frameNum__]);
			//	Debug.LogError(frameNum__ + "  j:" + j + " 不一样 "  );
			// 回滚; 状态回滚。
			Debug.LogError("回滚前的帧***********curFramID： " + curFramID);
			curFramID = frameNum__-1;
			Debug.LogError("回滚到的帧***********curFramID： " + curFramID);
			lastDoFrame = curFramID - 1;
			//Debug.Log("lastDoFrame***********： " + lastDoFrame);
			int i = 0;
			foreach (var item in list_battleUser)
			{
			//	Debug.LogError("i:"+i +"  count:" + dic_StateSnapShot[i].Count   );
				if (curFramID >= dic_StateSnapShot[i].Count)
				{
					Debug.LogError("没有回滚状态");
					break;
				}
				BattleCon.roleManage.getDic_Role()[item.battleID].objShape.SetPosition(dic_StateSnapShot[i][curFramID].logicpos);
				BattleCon.roleManage.getDic_Role()[item.battleID].roleDirection = dic_StateSnapShot[i][curFramID].dir;
				BattleCon.roleManage.getDic_Role()[item.battleID].logicSpeed = dic_StateSnapShot[i][curFramID].logicSpeed;
				i++; 
			}
		}
		else
		{ 
			Debug.Log("一样frameNum__" + frameNum__); 
		} 

	}





	//public void AddLackFrameData (int _frameID, AllPlayerOperation _newOp)
	//{
	//	//删除缺失的帧记录
	//	if (lackFrame.Contains(_frameID)) {
	//		dic_frameDate [_frameID] = _newOp;
	//		lackFrame.Remove (_frameID);
	//		Debug.Log ("补上 :" + _frameID);
	//	}
	//}
	bool rsl;
	public static int lastDoFrame= -1;
	public bool TryGetNextPlayerOp (out AllPlayerOperation _op)
	{
		int _frameID = curFramID + 1; 
		
	 
		bool rsl = dic_frameDate.TryGetValue(_frameID, out _op);
		bool rsl2=false;
		if (rsl)
		{
			Debug.Log("_frameID   " + _frameID + "   走服务器缓存 "); 
		}
		else
		{
			
			rsl2 = client_dic_frameDate.TryGetValue(_frameID, out _op);
			if (rsl2)
			{
				Debug.Log("_frameID   " + _frameID+ "   走客户端缓存 ");
	 
			}
		}
		if (rsl ||rsl2 )
		{
		
			lastDoFrame = _frameID;
			Debug.LogError("执行新的一帧  ***************TryGetNextPlayerOp  lastDoFrame:" + lastDoFrame);
		}





		return rsl;

		//return dic_frameDate.TryGetValue (_frameID,out _op);
	}
	public bool TryGetNextPlayerOpReplay(out AllPlayerOperation _op)
	{
		int _frameID = curFramID + 1;
		return dic_frameDate_Static.TryGetValue(_frameID, out _op);
	}
	public void RunOpSucces ()
	{
		curFramID++;
	}
}
