using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManage : MonoBehaviour {
	
	public bool initFinish;
	private int bulletID;
	private Transform bulletParent;
	private GameObject prefabBullet;

	private Dictionary<int,BulletBase> dic_bullets;
	private List<BulletBase> list_destoryBullet;


	public GameObject skill1;


	public void InitData(Transform _bulletParent){
		initFinish = false;
		bulletParent = _bulletParent;
		bulletID = 0;
		dic_bullets = new Dictionary<int, BulletBase> ();
		list_destoryBullet = new List<BulletBase> ();
		StartCoroutine (LoadBullet());
	}


	IEnumerator LoadBullet(){

		skill1 = Resources.Load<GameObject>("BattleScene/Skill/prefabSkill1");
		prefabBullet = Resources.Load<GameObject> ("BattleScene/Bullet/Bullet");
		yield return new WaitForEndOfFrame ();
		initFinish = true;
	}
	public void AddNormalBullet(int _owerID, GameVector2 _logicPos, int _moveDir)
	{
		bulletID++;

		GameObject _bulletBase = Instantiate(prefabBullet, bulletParent);
		BulletBase _bullet = _bulletBase.GetComponent<BulletBase>();
	 
		_bullet.InitDataNormalAtk(_owerID, bulletID, _logicPos, _moveDir);
		dic_bullets[bulletID] = _bullet;
	}
	public void AddSkill2(int _owerID, GameVector2 _logicPos, int _moveDir)
	{
		bulletID++;

		GameObject _bulletBase = Instantiate(prefabBullet, bulletParent);
		_bulletBase.GetComponent<MeshRenderer>().enabled = true;
		BulletBase _bullet = _bulletBase.GetComponent<BulletBase>();

		_bullet.InitDataSkill2(_owerID, bulletID, _logicPos, _moveDir);
		dic_bullets[bulletID] = _bullet;
	}
	public void AddSkill1(int _owerID, GameVector2 _logicPos, int _moveDir)
	{ 

		GameObject _bulletBase = Instantiate(skill1, bulletParent);
		ObstacleBase _bullet = _bulletBase.GetComponent<ObstacleBase>();
		_bullet.InitData2(ObstacleManage.getId() + 1, _logicPos , _moveDir) ;
		ShapeRectRotate shapeRectRotate = _bulletBase.GetComponent<ShapeRectRotate>();
		shapeRectRotate.dir_ = _moveDir;
		//	_bullet.InitData
		//_bullet.InitDataSkill1(_owerID, bulletID, _logicPos, _moveDir);
		//dic_bullets[bulletID] = _bullet;
	}




	public void AddBullet(int _owerID,GameVector2 _logicPos,int _moveDir){
		bulletID++; 
		GameObject _bulletBase = Instantiate (prefabBullet,bulletParent);
		BulletBase _bullet = _bulletBase.GetComponent<BulletBase> ();
		Debug.Log("_moveDir  "  + _moveDir);
		_bullet.InitData (_owerID,bulletID,_logicPos,_moveDir);
		dic_bullets [bulletID] = _bullet;
	}
		
	public void Logic_Move(){
		foreach (var item in dic_bullets.Values) {
			item.Logic_Move ();
		}
	}

	public void Logic_Collision(){
		foreach (var item in dic_bullets.Values) {
			item.Logic_Collision ();
		}
	}

	public void Logic_Destory(){
		foreach (var item in dic_bullets.Values) {
			if (item.Logic_Destory ()) {
				list_destoryBullet.Add (item);
			}
		}


		foreach (var item in list_destoryBullet) {
			dic_bullets.Remove (item.GetBulletID());
		}

		list_destoryBullet.Clear ();
	}
}
