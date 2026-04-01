using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ShapeCircle))]
public class RoleBase : MonoBehaviour {

	public int moveSpeed;
	public int attackTime;//攻击时间间隔
	private int curAttackTime;
//	private GameObject uiObj;
	private Transform modleParent;

	private Vector3 renderPosition;  // 渲染位置
	private Quaternion renderDir;
	public GameVector2 logicSpeed;
	public int roleDirection;//角色朝向
	[HideInInspector]
	public ShapeBase objShape; // 角色的shapeBase
	public Animator ani;
	public int hp;
	public TextMesh hpText;



	public void InitData(GameObject _ui,GameObject _modle,int _roleID,GameVector2 _logicPos){
		objShape = GetComponent<ShapeBase> ();
		logicSpeed = GameVector2.zero;
		modleParent = transform.Find ("Modle");

		//		uiObj = _ui;
		ani = _modle.transform.GetChild(1).gameObject.GetComponent<Animator>();
		_modle.transform.SetParent (modleParent);
		_modle.transform.localPosition = new Vector3(0,0.5f,0);

		objShape.InitSelf (ObjectType.role,_roleID);
		objShape.SetPosition (_logicPos);
		renderPosition = objShape.GetPositionVec3 ();
		transform.position = renderPosition;

		roleDirection = 0;
		renderDir = Quaternion.LookRotation (new Vector3(1f,0f,0f));
		modleParent.rotation = renderDir;
		hp = 100;
		curAttackTime = 0;
		hpText = transform.Find("hpNum").GetComponent<TextMesh>();
		hpText.text = hp.ToString();
	}

	void Update () {
	    transform.position = Vector3.Lerp(transform.position,renderPosition,0.4f);
		modleParent.rotation = Quaternion.Lerp (modleParent.rotation,renderDir,0.2f);	
	}

	public bool IsCloudAttack(){
		return curAttackTime <= 0;
	}

	public virtual void Logic_UpdateMoveDir(int _dir){
		if (_dir > 120) { 
			ani.SetBool("walk", false);
			logicSpeed = GameVector2.zero;
		} else
		{
			
			ani.SetBool("walk", true);
			roleDirection = _dir * 3;
			logicSpeed = moveSpeed * BattleData.Instance.GetSpeed (roleDirection);
			Vector3 _renderDir = ToolGameVector.ChangeGameVectorToVector3 (logicSpeed);																																																																		//ani.speed *= !!ReplayCon.Instance ? ReplayCon.Instance.narmalSpeed : 1;
			renderDir = Quaternion.LookRotation (_renderDir);
		}

	}

	public virtual void Logic_NormalAttack(){
		curAttackTime = attackTime;
	
		GameVector2 _bulletPos = objShape.GetPosition () + ToolMethod.Logic2Config(objShape.GetRadius ()) * BattleData.Instance.GetSpeed (roleDirection);
	    //BattleCon.Instance.bulletManage.AddBullet(objShape.ObjUid.objectID,_bulletPos, roleDirection);

		BattleCon.bulletManage.AddNormalBullet(objShape.ObjUid.objectID, _bulletPos, roleDirection);
		ani.SetTrigger("attack");
	}

	public virtual void Logic_Skill1(int dir)
	{
		curAttackTime = attackTime;  
		 
		//roleDirection = dir * 3;
		//GameVector2 logicSpeed1 = 10 * moveSpeed * BattleData.Instance.GetSpeed(roleDirection);



		GameVector2 _bulletPos = objShape.GetPosition() +   ToolMethod.Logic2Config(objShape.GetRadius()) * BattleData.Instance.GetSpeed(roleDirection);



		BattleCon.bulletManage.AddSkill1(objShape.ObjUid.objectID, _bulletPos, roleDirection);


		//Vector3 _renderDir = ToolGameVector.ChangeGameVectorToVector3(logicSpeed1);
		//	 skill.transform.position = _renderDir;
		//skill.transform.rotation = Quaternion.LookRotation(_renderDir);


		ani.SetTrigger("attack");
	}

	public virtual void Logic_Skill2()
	{
		curAttackTime = attackTime;

		GameVector2 _bulletPos = objShape.GetPosition() + ToolMethod.Logic2Config(objShape.GetRadius()) * BattleData.Instance.GetSpeed(roleDirection);
		
		BattleCon.bulletManage.AddSkill2(objShape.ObjUid.objectID, _bulletPos, roleDirection);
		ani.SetTrigger("attack");
	}


	public virtual void Logic_Move(){

      //  Debug.Log("Logic_Move  "  + Time.realtimeSinceStartup);
		if (logicSpeed != GameVector2.zero) { // 如果逻辑速度不等于0
			GameVector2 _targetPos = objShape.GetPosition () + logicSpeed; // 计算目标位置
			UpdateLogicPosition (_targetPos); //更新逻辑位置， 
			renderPosition = objShape.GetPositionVec3 (); // 更新渲染位置。 使用算法平滑处理。
		}
		if (curAttackTime > 0) {
			curAttackTime--;	
		}
	}

	public virtual void Logic_Move_Correction(){
		GameVector2 _ccLogicPos;
	    
		if (BattleCon.obstacleManage.CollisionCorrection(objShape.GetPosition (),objShape.GetRadius(),out _ccLogicPos)) {
			UpdateLogicPosition (_ccLogicPos);
		  	renderPosition = objShape.GetPositionVec3(); // 更新渲染位置。 使用算法平滑处理。
		}
	}

	void UpdateLogicPosition(GameVector2 _logicPos){
		 
		objShape.SetPosition (BattleData.Instance.GetMapLogicPosition(_logicPos));
	}

	public bool BeAttack (int _atk)
	{
		hp--;
		if (hp <= 0)
		{
			//isBroken = true;
			return true;
		}
		else
		{
			hpText.text = hp.ToString();
			return true;
		}
	}
	//public virtual void Logic_CheckHit()
	//{

	//	//  Debug.Log("Logic_Move  "  + Time.realtimeSinceStartup);
	//	if (logicSpeed != GameVector2.zero)
	//	{ // 如果逻辑速度不等于0
	//		GameVector2 _targetPos = objShape.GetPosition() + logicSpeed; // 计算目标位置
	//		UpdateLogicPosition(_targetPos); //更新逻辑位置， 使用算法平滑处理。
	//		renderPosition = objShape.GetPositionVec3(); // 获取渲染位置。
	//	}
	//	if (curAttackTime > 0)
	//	{
	//		curAttackTime--;
	//	}
	//}
}
