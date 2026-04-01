using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickBtn : MonoBehaviour {
	private EventTrigger _EventTri;
	private Image btnImage;
	void Start () {
		_EventTri = GetComponent<EventTrigger> ();
		btnImage = GetComponent<Image> ();
	}
	
	public void EnableButton(){
		_EventTri.enabled = true;
		btnImage.raycastTarget = true;
	}

	public void DisableButton(){
		_EventTri.enabled = false;
		btnImage.raycastTarget = true;
	}

	public void OnClickDown(){
		btnImage.color = Color.gray;

		if (gameObject.tag.Equals("NormalAttackButton")) {// 攻击
														  //普通攻击
			Debug.Log("BattleData.Instance.battleID  = "  + BattleData.Instance.battleID);
			RoleBase _role = BattleCon.roleManage.GetRoleFromBattleID ( BattleData.Instance.battleID);
			if (_role.IsCloudAttack()) { //j检测攻击是否Cd结束
				BattleData.Instance.UpdateRightOperation ( PBBattle.RightOpType.rop1,0,0);	
			}
		}
		else if (gameObject.tag.Equals("BtnGameOver")) {
			BattleCon.Instance.OnClickGameOver ();
		}
		else if (gameObject.name.Equals("skill1"))
		{ 
			BattleData.Instance.skill1();
		}
		else if (gameObject.name.Equals("skill2"))
		{
			BattleData.Instance.skill2();
		}

	}

	public void OnClickUp(){
		btnImage.color = Color.white;
	}
	public void speedUp()
	{
		Time.timeScale *= 2;
	//	ReplayCon.Instance.setSpeed(2);
	}
	public void speedDown()
	{
		Time.timeScale *= 0.5f;
		///	ReplayCon.Instance.setSpeed(10f);

	}

}
