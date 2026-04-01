using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public struct UserInfo{
	public string socketIp;
	public bool isLogin;
	public bool matching;
	public int currentBattleId;

	public UserInfo(string _socketIp,bool _isLogin){
		socketIp = _socketIp;
		isLogin = _isLogin;
		matching = false;
		currentBattleId = -1;
	}

}

public class UserManage {
	private static readonly object umlockObj = new object ();
	private static UserManage instance = null;

	private int userUid;
	private Dictionary<int,UserInfo> dic_userInfo;
	private Dictionary<string,int> dic_tokenUid;

	public static UserManage Instance
	{
		get{ 
			lock (umlockObj) {
				if (instance == null) {
					instance = new UserManage ();
				}	
			}
			return instance;
		}
	}

	private UserManage(){
		userUid = 0;
		dic_userInfo = new Dictionary<int, UserInfo> ();
		dic_tokenUid = new Dictionary<string, int> ();
	}

	public void Creat(){
	
	}


	public void Destroy(){
		instance = null;
	}


	public int UserLogin(string _token,string _socketIp){
		int _uid;
		if (dic_tokenUid.ContainsKey (_token)) {
			_uid = dic_tokenUid [_token];
		} else {
			userUid++;
			_uid = userUid;
			dic_tokenUid [_token] = userUid;
		}

		UserInfo _userInfo = new UserInfo (_socketIp,true);
		dic_userInfo [_uid] = _userInfo;

		return _uid;
	}

	public void UserLogout(string _socketIp){
		//掉线
	}
		
	public void UserLogout(int _uid){
		//自己登出

	}

	public UserInfo GetUserInfo(int _uid){
		return dic_userInfo [_uid];
	}

	public void SetMatchInfo(int _uid , int BattleId_)
	{
		UserInfo ui = dic_userInfo[_uid];
		ui.matching = true;
		ui.currentBattleId = BattleId_;
		dic_userInfo[_uid] = ui;

	}

    public int CheckExistBattle(string _token)
	{
		 
		if (dic_tokenUid.ContainsKey(_token))
		{
			int _uid = dic_tokenUid[_token];
			if (dic_userInfo.ContainsKey(_uid) && dic_userInfo[_uid].matching)
			{ 
				return _uid;
			}
			else
			{
				return -1; 
			} 
		}
		else
		{
			return -1;
		}
	}
	public int UpdateUserInfo(string _token, string _socketIp)
	{
		 
		 int _uid = dic_tokenUid[_token];
		UserInfo ui = dic_userInfo[_uid]; 
		ui.socketIp = _socketIp;
		dic_userInfo[_uid] = ui;
		return _uid;
	}

}
