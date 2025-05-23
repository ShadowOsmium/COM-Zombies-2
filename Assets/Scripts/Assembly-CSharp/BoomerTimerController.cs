using System.Collections.Generic;
using CoMZ2;
using TNetSdk;
using UnityEngine;

public class BoomerTimerController : EnemyController
{
	private float cur_time;

	private float life_time = 8f;

	private bool is_bombed;

	protected float speed_up_ratio = 1f;

	protected float explodeRange;

	private GameObject platter;

	public override void Init()
	{
		life_time = (float)enemy_data.config.Ex_conf["LifeTime"];
		ANI_SHOW = "Show01";
		ANI_IDLE = "Forward01";
		ANI_ATTACK = "Explode01";
		ANI_INJURED = "Damage01";
		ANI_DEAD = "Death01";
		ANI_RUN = "Forward01";
		base.Init();
		speed_up_ratio = (float)enemy_data.config.Ex_conf["speedUpRatio"];
		explodeRange = (float)enemy_data.config.Ex_conf["explodeRange"];
		head_ori = base.transform.Find("Zombie_Little_Cook_Head").gameObject;
		if (head_ori == null)
		{
			Debug.LogError("head_ori not found!");
		}
		body_ori = base.transform.Find("Zombie_Little_Cook_Body").gameObject;
		if (body_ori == null)
		{
			Debug.LogError("body_ori not founded!");
		}
		head_broken = base.transform.Find("Zombie_Little_Cook_Head_Break").gameObject;
		if (head_broken == null)
		{
			Debug.LogError("head_broken not founded!");
		}
		head_broken.SetActive(false);
		platter = base.transform.Find("Serving_platter").gameObject;
		if (platter == null)
		{
			Debug.LogError("Serving_platter not founded!");
		}
		body_eff_prefab = base.Accessory[0];
		SetState(SHOW_STATE);
	}

	public override void Dologic(float deltaTime)
	{
		if (is_bombed)
		{
			base.Dologic(deltaTime);
			return;
		}
		cur_time += deltaTime;
		if (cur_time >= life_time)
		{
			is_bombed = true;
			SetState(SHOOT_STATE);
		}
		else
		{
			base.Dologic(deltaTime);
		}
	}

	public override void OnDead(float damage, WeaponController weapon, ObjectController player, Vector3 hit_point, Vector3 hit_normal)
	{
		base.OnDead(damage, weapon, player, hit_point, hit_normal);
		if (is_ice_dead)
		{
			OnIceBodyCrash();
		}
		Object.Destroy(base.GetComponent<Collider>());
		StartCoroutine(RemoveOnTime(3f));
		platter.SetActive(false);
		is_bombed = true;
	}

	public override void CheckHit()
	{
		if (base.IsEnchant)
		{
			foreach (EnemyController value in GameSceneController.Instance.Enemy_Set.Values)
			{
				if ((value.centroid - centroid).sqrMagnitude < explodeRange * explodeRange && !GameSceneController.CheckBlockBetween(centroid, value.centroid))
				{
					value.OnHit(enemy_data.damage_val, null, this, value.centroid, Vector3.zero);
				}
			}
		}
		else
		{
			foreach (PlayerController value2 in GameSceneController.Instance.Player_Set.Values)
			{
				if ((value2.centroid - centroid).sqrMagnitude < explodeRange * explodeRange && !GameSceneController.CheckBlockBetween(centroid, value2.centroid))
				{
					value2.OnHit(enemy_data.damage_val, null, this, value2.centroid, Vector3.zero);
				}
			}
			foreach (NPCController value3 in GameSceneController.Instance.NPC_Set.Values)
			{
				if ((value3.centroid - centroid).sqrMagnitude < explodeRange * explodeRange && !GameSceneController.CheckBlockBetween(centroid, value3.centroid))
				{
					value3.OnHit(enemy_data.damage_val, null, this, value3.centroid, Vector3.zero);
				}
			}
			foreach (GuardianForceController value4 in GameSceneController.Instance.GuardianForce_Set.Values)
			{
				if ((value4.centroid - centroid).sqrMagnitude < explodeRange * explodeRange && !GameSceneController.CheckBlockBetween(centroid, value4.centroid))
				{
					value4.OnHit(enemy_data.damage_val, null, this, value4.centroid, Vector3.zero);
				}
			}
			foreach (EnemyController value5 in GameSceneController.Instance.Enemy_Enchant_Set.Values)
			{
				if ((value5.centroid - centroid).sqrMagnitude < explodeRange * explodeRange && !GameSceneController.CheckBlockBetween(centroid, value5.centroid))
				{
					value5.OnHit(enemy_data.damage_val, null, this, value5.centroid, Vector3.zero);
				}
			}
		}
		OnBoom();
		SetState(DEAD_STATE);
		GameSceneController.Instance.boom_m_pool.GetComponent<ObjectPool>().CreateObject(centroid, Quaternion.identity);
	}

	public override void OnHit(float damage, WeaponController weapon, ObjectController player, Vector3 hit_point, Vector3 hit_normal)
	{
		if (enemyState == SHOW_STATE || enemyState == DEAD_STATE)
		{
			return;
		}
		injured_time = Time.time;
		if (!hatred_set.ContainsKey(player))
		{
			hatred_set.Add(player, 1f);
		}
		Dictionary<ObjectController, float> dictionary;
		Dictionary<ObjectController, float> dictionary2 = (dictionary = hatred_set);
		ObjectController key;
		ObjectController key2 = (key = player);
		float num = dictionary[key];
		dictionary2[key2] = num + damage;
		OnHitSound(weapon);
		//if (GameData.Instance.cur_game_type == GameData.GamePlayType.Coop && damage > 0f && tnetObj != null)
		//{
		//	SFSObject sFSObject = new SFSObject();
		//	SFSArray sFSArray = new SFSArray();
		//	sFSArray.AddShort((short)enemy_id);
		//	sFSArray.AddFloat(damage);
		//	if (weapon != null && weapon.weapon_type == WeaponType.IceGun)
		//	{
		//		tem_frozenTime = ((IceGunController)weapon).frozenTime;
		//		sFSArray.AddBool(true);
		//		sFSArray.AddFloat(tem_frozenTime);
		//	}
		//	else
		//	{
		//		sFSArray.AddBool(false);
		//		sFSArray.AddFloat(0f);
		//	}
		//	sFSObject.PutSFSArray("enemyInjured", sFSArray);
		//	tnetObj.Send(new BroadcastMessageRequest(sFSObject));
		//	Dictionary<PlayerID, float> player_damage_Set;
		//	Dictionary<PlayerID, float> dictionary3 = (player_damage_Set = GameSceneController.Instance.Player_damage_Set);
		//	PlayerID player_id;
		//	PlayerID key3 = (player_id = GameSceneController.Instance.player_controller.player_id);
		//	num = player_damage_Set[player_id];
		//	dictionary3[key3] = num + damage;
		//}
		if (enemy_data.OnInjured(damage))
		{
			if (weapon != null && weapon.weapon_type == WeaponType.IceGun)
			{
				is_ice_dead = true;
				PlayPlayerAudio("FreezeBurst");
			}
			GameSceneController.Instance.UpdateEnemyDeathInfo(enemy_data.enemy_type, 1);
			OnDead(damage, weapon, player, hit_point, hit_normal);
			SetState(DEAD_STATE);
			//if (GameData.Instance.cur_game_type == GameData.GamePlayType.Coop && damage > 0f && tnetObj != null)
			//{
			//	SFSObject sFSObject2 = new SFSObject();
			//	SFSArray sFSArray2 = new SFSArray();
			//	sFSArray2.AddShort((short)enemy_id);
			//	sFSArray2.AddFloat(damage);
			//	sFSObject2.PutSFSArray("enemyDead", sFSArray2);
			//	tnetObj.Send(new BroadcastMessageRequest(sFSObject2));
			//}
		}
		else if (weapon != null && weapon.weapon_type == WeaponType.IceGun)
		{
			frozenTime = ((IceGunController)weapon).frozenTime;
			AnimationUtil.Stop(base.gameObject);
			SetState(FROZEN_STATE);
		}
		else
		{
			if (base.Enemy_State.GetStateType() != EnemyStateType.Shoot)
			{
				AnimationUtil.Stop(base.gameObject);
				AnimationUtil.PlayAnimate(base.gameObject, ANI_INJURED, WrapMode.ClampForever);
				SetState(INJURED_STATE);
			}
			CreateInjuredBloodEff(hit_point, hit_normal);
		}
	}

	public override void FireUpdate(float deltaTime)
	{
		if (target_player != null)
		{
			base.transform.LookAt(target_player.transform);
		}
		if (AnimationUtil.IsAnimationPlayedPercentage(base.gameObject, ANI_ATTACK, 0.95f))
		{
			CheckHit();
		}
	}

	public override void Fire()
	{
		ANI_CUR_ATTACK = ANI_ATTACK;
		AnimationUtil.CrossAnimate(base.gameObject, ANI_ATTACK, WrapMode.ClampForever);
	}

	public void OnBoom()
	{
		Object.Destroy(base.GetComponent<Collider>());
		StartCoroutine(RemoveOnTime(0.1f));
	}

	public override bool FinishAttackAni()
	{
		if (AnimationUtil.IsPlayingAnimation(base.gameObject, ANI_ATTACK))
		{
			return false;
		}
		return true;
	}

	public override bool OnChaisawSpecialHit(float damage, WeaponController weapon, ObjectController player, Vector3 hit_point, Vector3 hit_normal)
	{
		return false;
	}

	public override void OnChaisawInjuredResease(ObjectController player)
	{
	}

	public override void OnChaisawSkillDead(ObjectController player)
	{
	}
}
