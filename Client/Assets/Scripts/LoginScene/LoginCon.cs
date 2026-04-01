using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PBCommon;
using PBLogin;
public class LoginCon : MonoBehaviour {
	public InputField inputField;
	public GameObject waitTip;
	public GameObject Btn_recon;
	void Start(){
	   // IsNetworkReachability();
		//CheckResServerNetWorkReady();
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Application.targetFrameRate = 30;

		NetGlobal.Instance ();
		TcpPB.Instance ().mes_login_result = Message_Login_Result;
		waitTip.SetActive (false);
	}
	void Update()
	{
		//IsNetworkReachability();
	}

	public bool IsNetworkReachability()
	{
		switch (Application.internetReachability)
		{
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				Debug.Log(" WiFi ");
				return true;
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				Debug.Log(" 移动网络 ");
				return true;
			default:
				Debug.Log(" 没有联网 ");
				return false;
		}
	}


	private void CheckResServerNetWorkReady()
	{
		StopCoroutine(PingConnect());
		StartCoroutine(PingConnect());
	}
	enum PingState
	{
		PingIng,
		ConnectFail,
		PingOK,
	}
	PingState m_PingState;
	IEnumerator PingConnect()
	{
		m_PingState = PingState.PingIng;
		//ResServer IP 
		string ResServerIP = "192.168.1.32";
		//Ping服务器 
		Ping ping = new Ping(ResServerIP);

		int nTime = 0;

		while (!ping.isDone)
		{
			yield return new WaitForSeconds(0.1f);

			if (nTime > 20) //2秒
			{
				nTime = 0;
				Debug.Log("连接失败 : " + ping.time);
				m_PingState = PingState.ConnectFail;
				yield break;
			}
			nTime++;
		}
		if (ping.isDone)
		{
			yield return ping.time;
			m_PingState = PingState.PingOK;
			Debug.Log("连接成功");
		}
	}
 
	public void OnClickLogin(){
         
		waitTip.SetActive (true);
        

        string _ip = inputField.text;
        MyTcp.Instance.ConnectServer (_ip,(_result)=>{
			if (_result) {
				Debug.Log("连接成功");
				NetGlobal.Instance().serverIP = _ip;
				TcpLogin _loginInfo = new TcpLogin();
				_loginInfo.token = SystemInfo.deviceUniqueIdentifier; // 客户端凭证
                // 连接成功后发送消息
				MyTcp.Instance.SendMessage(CSData.GetSendMessage<TcpLogin>(_loginInfo,CSID.TCP_LOGIN));
			}
			else{
				Debug.Log("连接失败");
				waitTip.SetActive (false);
			}
		});
	}


	void Message_Login_Result(TcpResponseLogin _mes){
		if (_mes.result && !_mes.reconnect) {
			NetGlobal.Instance().userUid = _mes.uid;
			NetGlobal.Instance().udpSendPort = _mes.udpPort;
			Debug.Log("登录成功～～～" + NetGlobal.Instance().userUid);
			ClearSceneData.LoadScene(GameConfig.mainScene);
		} else if (_mes.result && _mes.reconnect) {
			NetGlobal.Instance().userUid = _mes.uid;
			NetGlobal.Instance().udpSendPort = _mes.udpPort;
			Debug.LogError("判断重连 " + NetGlobal.Instance().serverIP + "   port: " + NetGlobal.Instance().udpSendPort);
			Btn_recon.SetActive(true);
		} else {
			Debug.Log("登录失败～～～");
			waitTip.SetActive(false);
		}
	}
	public void OnClickReconnect()
	{ 
		waitTip.SetActive(true);
		BattleData.Instance.isReconnect = true;
		ClearSceneData.LoadScene(GameConfig.mainScene);
	}
	void OnDestroy(){
		TcpPB.Instance ().mes_login_result = null;
	}
}
