using System.Collections;
using System.Collections.Generic;
using System.Threading;
using PBBattle;
using PBCommon;
using UnityEngine;
using System;
using System.Linq;

public class BattleCon {
	private int battleID;
	int randSeed;
	public  Dictionary<int,int> dic_battleUserUid;
	public Dictionary<int,ClientUdp> dic_udp;

	private Dictionary<int,bool> dic_battleReady;
	Dictionary<int, int> matchUserRole= new Dictionary<int, int>();
	private bool _isRun = false;
	private bool isBeginBattle = false;
	private int frameNum;
	private int lastFrame;

	private PlayerOperation[] frameOperation;//记录当前帧的玩家操作
	private int[] playerMesNum;//记录玩家的包id
	private bool[] playerGameOver;//记录玩家游戏结束
	private bool oneGameOver;
	private bool allGameOver;
	public bool isFinishBS;
	private Dictionary<int,AllPlayerOperation> dic_gameOperation = new Dictionary<int, AllPlayerOperation>();
	  Dictionary<int, PlayerOperation>[] dic_playerOp  ;

	private Timer waitBattleFinish;
	private float finishTime;//结束倒计时

	public void AnalyzeMessage(CSID messageId, byte[] bodyData)
	{
		Debug.LogError("AnalyzeMessage   messageId == " + messageId);
		switch (messageId)
		{
			case CSID.UDP_BATTLE_READY:
				{
					//接收战斗准备

					UdpBattleReady _mes = CSData.DeserializeData<UdpBattleReady>(bodyData);
					if (_mes.isRecon)
					{
						dic_udp[_mes.battleID].ReConnectRecvClientReady(_mes.uid);
					}
					else
					{
						CheckBattleBegin(_mes.battleID);
						Debug.Log("准备1 _mes.battleID : " + _mes.battleID + " , dic_udp[_mes.battleID] : " + dic_udp[_mes.battleID]);
						Debug.LogWarning("创建用户端口的时间 " + System.DateTime.Now);
						dic_udp[_mes.battleID].RecvClientReady(_mes.uid);
						//	Debug.Log("pb_ReceiveMes.mesID2 *** " );
					}




				}
				break;
			case CSID.UDP_UP_PLAYER_OPERATIONS:
				{
					UdpUpPlayerOperations pb_ReceiveMes = CSData.DeserializeData<UdpUpPlayerOperations>(bodyData);
					Debug.LogWarning(pb_ReceiveMes.mesID + "  move " + pb_ReceiveMes.operation.move);
					// Debug.Log("pb_ReceiveMes.mesID == " + pb_ReceiveMes.mesID + "  move " + pb_ReceiveMes.operation.move);
					UpdatePlayerOperation(pb_ReceiveMes.operation, pb_ReceiveMes.mesID);
					Debug.LogWarning("UDP_UP_PLAYER_OPERATIONS  0");
					// 临时测试用，用完记得删掉下面的。

					//dic_udp[pb_ReceiveMes.operation.battleID].RecvClientReady(pb_ReceiveMes.mesID);

					//UdpDownFrameOperations _dataPb = new UdpDownFrameOperations(); 
					//Debug.LogError("UDP_UP_PLAYER_OPERATIONS  1");
					//_dataPb.operations = new AllPlayerOperation();
					//Debug.LogError("UDP_UP_PLAYER_OPERATIONS  2");

					//_dataPb.operations.operations.Add(pb_ReceiveMes.operation);
					//_dataPb.frameID = frameNum;
					////dic_gameOperation[frameNum] = _dataPb.operations;
					//lastFrame = frameNum;
					//frameNum++; 
					//Debug.LogError("22222");
					//byte[] _data = CSData.GetSendMessage<UdpDownFrameOperations>(_dataPb, SCID.UDP_DOWN_FRAME_OPERATIONS);
					//Debug.LogError("3333");
					//dic_udp[pb_ReceiveMes.operation.battleID].SendMessage(_data);
					//Debug.LogError("44444");
				}
				break;
			case CSID.UDP_UP_DELTA_FRAMES:

				UdpUpDeltaFrames pb_DeltaMes = CSData.DeserializeData<UdpUpDeltaFrames>(bodyData);
				GetRequestFrames(pb_DeltaMes.battleID, pb_DeltaMes.frames[0], pb_DeltaMes.frames[1]);
				break;
			default:
				break;
		}
	}
	public void GetRequestFrames(int battleID, int start, int num)
	{

		UdpDownDeltaFrames rsl = new UdpDownDeltaFrames();
		List<UdpDownFrameOperations> mes = new List<UdpDownFrameOperations>();
		try
		{
			for (int i = start, j = 0; i <= num; i++)
			{

				mes.Add(new UdpDownFrameOperations());
				mes[j].frameID = i;
				mes[j].operations = dic_gameOperation[i];
				j++;
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
		}

		rsl.framesData = mes;
		byte[] _data = CSData.GetSendMessage<UdpDownDeltaFrames>(rsl, SCID.UDP_DOWN_DELTA_FRAMES);
		Debug.LogError("发送给客户端请求的丢失帧数据 battleID: " + battleID);
		dic_udp[battleID].SendMessage(_data);


	}
	public void ReConnectBattle(int _battleID,int re_userUid) // 重新开始对局
	{
		battleID = _battleID;  
		TcpEnterBattle _mes = new TcpEnterBattle();
		_mes.randSeed = randSeed;  
        foreach (KeyValuePair<int, int> kv in dic_battleUserUid)
		{
			BattleUserInfo _bUser = new BattleUserInfo();
			_bUser.uid = kv.Key;
		    _bUser.battleID = dic_battleUserUid[kv.Key];
			_bUser.roleID = matchUserRole[kv.Key];
			_mes.battleUserInfo.Add(_bUser);
		} 
		string _ip = UserManage.Instance.GetUserInfo(re_userUid).socketIp;
		ServerTcp.Instance.SendMessage(_ip, CSData.GetSendMessage<TcpEnterBattle>(_mes, SCID.TCP_ENTER_BATTLE)); 
	}
	public void CreatBattle(int _battleID,List<MatchUserInfo> _battleUser){
		  randSeed = UnityEngine.Random.Range (0, 100);
		 
		ThreadPool.QueueUserWorkItem((obj)=>{
			battleID = _battleID;
			dic_battleUserUid = new Dictionary<int, int> ();
			dic_udp = new Dictionary<int, ClientUdp> ();
			dic_battleReady = new Dictionary<int, bool>();
		
			int userBattleID = 0;

			TcpEnterBattle _mes = new TcpEnterBattle();
			_mes.randSeed = randSeed;
			dic_playerOp = new Dictionary<int, PlayerOperation>[_battleUser.Count];
			for (int i = 0; i < _battleUser.Count; i++) {
				int _userUid = _battleUser [i].uid;
				userBattleID++;  // 为每个user设置一个battleID， 这里就从1开始。

				dic_battleUserUid [_userUid] = userBattleID;
				dic_playerOp[userBattleID-1] = new Dictionary<int, PlayerOperation>();
				string _ip = UserManage.Instance.GetUserInfo (_userUid).socketIp;

				UserManage.Instance.SetMatchInfo(_userUid, battleID); // 记录比赛信息。

				Debug.Log("开始创建客户端 " + i+" UDP");
				var _udp = new ClientUdp ();

				_udp.StartClientUdp (_ip,_userUid);
				Debug.Log("启动客户端 _ip： " + _ip + " ， _userUid ："+ _userUid);
				_udp.delegate_analyze_message = AnalyzeMessage;
				dic_udp [userBattleID] = _udp;
				dic_battleReady[userBattleID] = false;

				BattleUserInfo _bUser = new BattleUserInfo();
				_bUser.uid = _userUid;
				_bUser.battleID = userBattleID;
				_bUser.roleID = _battleUser [i].roleID;
				matchUserRole[_userUid ] = _battleUser[i].roleID;
				_mes.battleUserInfo.Add(_bUser);
			}

			for (int i = 0; i < _battleUser.Count; i++) {
				int _userUid = _battleUser [i].uid;
				string _ip = UserManage.Instance.GetUserInfo (_userUid).socketIp;
				ServerTcp.Instance.SendMessage(_ip,CSData.GetSendMessage<TcpEnterBattle>(_mes,SCID.TCP_ENTER_BATTLE));
			}
		},null);
	}

	public void DestroyBattle(){
		foreach (var item in dic_udp.Values) {
			item.EndClientUdp ();
		}
		isFinishBS = false;
		_isRun = false;
	}

	 
	private void CheckBattleBegin(int _userBattleID){

		if (isBeginBattle) {
			return;
		}

		dic_battleReady[_userBattleID] = true;

		isBeginBattle = true;
	//	Debug.Log("dic_battleReady.Count  == " + dic_battleReady.Count);
		foreach (var item in dic_battleReady.Values) {
			isBeginBattle = (isBeginBattle && item);
		}

		if (isBeginBattle) {
			Debug.Log("BeginBattle  **!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ");
			//开始战斗
			BeginBattle();
		}
	}
	int num;
	void BeginBattle(){
		frameNum = 0;
		lastFrame = 0;
		_isRun = true;
		oneGameOver = false;
		allGameOver = false;

		int playerNum = dic_battleUserUid.Keys.Count;

		frameOperation = new PlayerOperation[playerNum];
		playerMesNum = new int[playerNum];
		playerGameOver = new bool[playerNum];
		for (int i = 0; i < playerNum; i++) {
			frameOperation [i] = null;
			playerMesNum [i] = 0;
			playerGameOver [i] = false;
		}
			
		Thread _threadSenfd = new Thread(Thread_SendFrameData);  
		_threadSenfd.Start();
	}


	private void Thread_SendFrameData(){
		PlayerOperation po = new PlayerOperation();
		//向玩家发送战斗开始
		  isFinishBS = false;
		Debug.LogError("Thread_SendFrameData dic_udp.Count****  " + dic_udp.Count);
		while (!isFinishBS) {
			UdpBattleStart _btData = new UdpBattleStart ();
			byte[] _data = CSData.GetSendMessage<UdpBattleStart> (_btData, SCID.UDP_BATTLE_START);
			foreach (var item in dic_udp) {
				Debug.LogError("向玩家发送战斗 " + item.Value.userUid);
				item.Value.SendMessage (_data);
			}

			bool _allData = true;
			for (int i = 0; i < frameOperation.Length; i++) {
				if (frameOperation[i] == null) {
					Debug.Log("Thread_SendFrameData 没有收到全部玩家的第一帧数据 ****  i= =" + i);
					_allData = false;// 有一个玩家没有发送上来操作 则判断为false
					break;
				}
			}

		
			if (_allData) {
				Debug.LogError("战斗服务器:收到全部玩家的第一次操作数据 ");
				frameNum = 1;

				isFinishBS = true;
			}
			else
			{
				Debug.LogError("NO 收到全部玩家的第一次操作数据 ");
			}

			//Thread.Sleep (1000);
		}

		Debug.LogError("开始发送帧数据 ");

		while (_isRun) {
			UdpDownFrameOperations _dataPb = new UdpDownFrameOperations ();
			if (oneGameOver) {
				_dataPb.frameID = lastFrame;
				_dataPb.operations = dic_gameOperation [lastFrame];
			} else {
				_dataPb.operations = new AllPlayerOperation ();
				//if (frameOperation==null )
				//{
				////	Debug.LogError("frameOperation  为空");
				//}
				//if (  frameOperation.Length == 0)
				//{
				////	Debug.LogError("frameOperation  为0");
				//}

				for (int i = 0; i < frameOperation.Length; i++)
				{
					po = new PlayerOperation();
					po.battleID = i + 1;
					if (dic_playerOp[i].ContainsKey(frameNum) && 1 == 2) //&&1==2
					{
						po.move = dic_playerOp[i ][frameNum].move; 
					}
					else
					{ 
						po.battleID = i + 1;
						if (frameNum >= 15 && frameNum <= 17)
						{
							po.move = 60;  //121 测试，向右走
						}
						else
							po.move = 0;  //121 测试，向右走
					}
					_dataPb.operations.operations.Add(po); 
				} 
				//for (int i=0;i< frameOperation.Length;i++ )
				//{
				//	if(playerMesNum[i] < frameNum) // 客户端没有提交对应帧，丢帧。
				//	{
				//		PlayerOperation po = new PlayerOperation();
				//		po.battleID = i + 1;
				//		po.move = 121; 
				//		_dataPb.operations.operations.Add(po);
				//	}
				//	else
				//	{
				//		_dataPb.operations.operations.Add(frameOperation[i]);
				//	}
				//}

			//	_dataPb.operations.operations.AddRange (frameOperation);
				_dataPb.frameID = frameNum;
				dic_gameOperation [frameNum] = _dataPb.operations;
				lastFrame = frameNum;
				frameNum++;
			//	Debug.LogError("frameNum: " + frameNum);
			}


			foreach ( var a in frameOperation)
			{
				PlayerOperation temp = (PlayerOperation)a;
				//Debug.Log("id: " + temp.battleID + "  move: " + temp.move);
			}


			byte[] _data = CSData.GetSendMessage<UdpDownFrameOperations> (_dataPb, SCID.UDP_DOWN_FRAME_OPERATIONS);
			num++;
			foreach (var item in dic_udp) {
				int _index = item.Key - 1;
				if (!playerGameOver [_index]) {
					//Debug.Log("发送帧操作 序号 " + num + " item.key " + item.Key );
					item.Value.SendMessage (_data);	
				}
			}

			Thread.Sleep (ServerConfig.frameTime);
		}

		Debug.Log ("帧数据发送线程结束.....................");
	}

	public void UpdatePlayerOperation(PlayerOperation _operation,int _mesNum){
       

        int _index = _operation.battleID - 1;
		Debug.LogError("UpdatePlayerOperation1********************************");
		dic_playerOp[_index][_mesNum] = _operation;
		Debug.LogError("UpdatePlayerOpe+ration2********************************");
		if (_mesNum > playerMesNum [_index]) {  
			frameOperation [_index] = _operation;
			playerMesNum [_index] = _mesNum; 

		}
		else
		{
			Debug.LogError("_mesNum 小于或者等于 服务器最大的帧。没有加入 playerMesNum");
		}
	}
	 
 

}
