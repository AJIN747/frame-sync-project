using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour {
	[HideInInspector]
	public ShapeBase objShape;

//	private int owerID;
	public int speed;
	public int life;
	private int curLife;
	private int enableFrame;

	private Vector3 renderPosition;
	private GameVector2 logicSpeed;
	public void InitData(int _owerID,int _id,GameVector2 _logicPos,int _moveDir){

//		owerID = _owerID;

		objShape = GetComponent<ShapeBase> ();
		objShape.InitSelf (ObjectType.bullet,_id);
		objShape.SetPosition (_logicPos);
		transform.position = objShape.GetPositionVec3 (0.5f);
		Debug.Log("攻击碰撞体 " + transform.position);
		Debug.Log("_moveDir " + _moveDir);
		logicSpeed = speed * BattleData.Instance.GetSpeed (_moveDir);
		Debug.Log("攻击碰撞体logicSpeed " + logicSpeed);
		curLife = life;
	}

	public void InitDataNormalAtk(int _owerID, int _id, GameVector2 _logicPos, int _moveDir)
	{
		  
		objShape = GetComponent<ShapeBase>();
		objShape.InitSelf(ObjectType.bullet, _id);
		_logicPos = _logicPos + 50 * BattleData.Instance.GetSpeed(_moveDir);
		objShape.SetPosition(_logicPos);
		transform.position = objShape.GetPositionVec3(0.5f);

		logicSpeed = GameVector2.zero;
		curLife = life;
		enableFrame = 8;
	}
	public void InitDataSkill2(int _owerID, int _id, GameVector2 _logicPos, int _moveDir)
	{

		objShape = GetComponent<ShapeBase>();
		objShape.InitSelf(ObjectType.bullet, _id);
		_logicPos = _logicPos + 50 * BattleData.Instance.GetSpeed(_moveDir);
		objShape.SetPosition(_logicPos);
		transform.position = objShape.GetPositionVec3(0.5f);

		logicSpeed = speed * BattleData.Instance.GetSpeed(_moveDir);
		curLife = life;
		enableFrame = 0;
	}
	public void InitDataSkill1(int _owerID, int _id, GameVector2 _logicPos, int _moveDir)
	{

		objShape = GetComponent<ShapeBase>();
		objShape.InitSelf(ObjectType.obstacle, _id);
		_logicPos = _logicPos + 200 * BattleData.Instance.GetSpeed(_moveDir);
		objShape.SetPosition(_logicPos);
		transform.position = objShape.GetPositionVec3(0.5f);

	//	_moveDir = _moveDir * 3;
		 
		Vector3 _renderDir = ToolGameVector.ChangeGameVectorToVector3(BattleData.Instance.GetSpeed(_moveDir));
		transform.rotation = Quaternion.LookRotation(_renderDir);



		logicSpeed = GameVector2.zero;
		curLife = life;
		enableFrame = 8;
	}



	public int GetBulletID(){
		return objShape.ObjUid.objectID;
	}
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.Lerp(transform.position,renderPosition,0.4f);
	}

	public virtual void Logic_Move(){
		curLife--;
		if (curLife > 0) {
			GameVector2 _targetPos = objShape.GetPosition () + logicSpeed;
			objShape.SetPosition (_targetPos);
			renderPosition = objShape.GetPositionVec3 (0.5f);
		} else {

		}
	}

	public virtual void Logic_Collision(){
		if ((life-curLife) < enableFrame )
		{
			return;
		}
		if (BattleCon.obstacleManage.AttackObstacle(objShape.ObjUid,objShape.GetPosition(),objShape.GetRadius(),1)) {
		 	curLife = 0;
		}
		if (curLife != 0) //检测攻击英雄
		{
			if (  BattleCon.roleManage.Logic_CheckHit(objShape.ObjUid, objShape.GetPosition(), objShape.GetRadius(), 1)) { 
					 
			Debug.Log("攻击了&&&&&&&&& ");
			//遍历全部角色，排除自己，检测是否碰撞。
			curLife = 0;
		}
		}
	}

	public virtual bool Logic_Destory(){
		if (curLife <= 0) {
			Destroy (gameObject);
			return true;
		} else {
			return false;
		}
	}
}
