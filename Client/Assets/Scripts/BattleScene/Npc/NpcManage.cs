using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManage : MonoBehaviour
{

	public bool initFinish;
	private int npcID;
	private Transform npcParent;
	private GameObject prefabNpc ,prefabBullet;

	private Dictionary<int, NpcBase> dic_npcs;
	private List<NpcBase> list_destoryNpc;


	int frameNum_ = 74990;

	public void InitData(Transform _npcParent)
	{
		initFinish = false;
		npcParent = _npcParent;
		npcID = 0;
		dic_npcs = new Dictionary<int, NpcBase>();
		list_destoryNpc = new List<NpcBase>();
		StartCoroutine(LoadNpc());
	}


	IEnumerator LoadNpc()
	{

	 
	    prefabNpc = Resources.Load<GameObject>("BattleScene/Npc/Npc");
		prefabBullet = Resources.Load<GameObject>("BattleScene/Bullet/Bullet");
		yield return new WaitForEndOfFrame();
		initFinish = true;
	}

	public void AddSkill2(int _owerID, GameVector2 _logicPos, int _moveDir)
	{
		npcID++;

		GameObject _bulletBase = Instantiate(prefabBullet, npcParent);
		_bulletBase.GetComponent<MeshRenderer>().enabled = true;
		BulletBase _bullet = _bulletBase.GetComponent<BulletBase>();

		_bullet.InitDataSkill2(_owerID, npcID, _logicPos, _moveDir);
	//	dic_npcs[bulletID] = _bullet;
	}

	public void AddNpc( GameVector2 _logicPos, int _moveDir)
	{
		npcID++;
		GameObject _npcBase = Instantiate(prefabNpc, npcParent);  
		NpcBase _npc = _npcBase.GetComponent<NpcBase>();
		Debug.Log("_moveDir  " + _moveDir);
		 _npc.InitData( npcID, _logicPos, _moveDir);
		 dic_npcs[npcID] = _npc;
	}

	public void Logic_Move()
	{
		foreach (var item in dic_npcs.Values)
		{
			item.Logic_Move();
		}
	}
	public void Logic_createNpc()
	{
		frameNum_++;
		if (frameNum_>=300)
		{
			frameNum_ = 0;
		//	AddSkill2(1, new GameVector2(45000, 25000), 1);
		    AddNpc(new GameVector2(45000, 25000), 1); 
		}
		
	}
	public void Logic_Collision()
	{
		foreach (var item in dic_npcs.Values)
		{
			item.Logic_Collision();
		}
	}

	public void Logic_Destory()
	{
		foreach (var item in dic_npcs.Values)
		{
			if (item.Logic_Destory())
			{
				list_destoryNpc.Add(item);
			}
		}


		foreach (var item in list_destoryNpc)
		{
			dic_npcs.Remove(item.GetNpcID());
		}

		list_destoryNpc.Clear();
	}
}
