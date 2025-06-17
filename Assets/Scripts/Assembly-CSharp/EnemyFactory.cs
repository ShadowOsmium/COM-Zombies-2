using System.Collections;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
	protected static EnemyFactory instance;

    public static EnemyFactory Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("EnemyFactory");
                instance = go.AddComponent<EnemyFactory>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private static EnemyController CreateEnemyInternal(
        EnemyType type, Vector3 pos, Quaternion rot,
        int? enemyID = null, bool isTrapped = false)
    {
        GameObject enemyRef = GameSceneController.Instance.enemy_ref_map.Enemy_Set[type];
        GameObject enemyInstance = Object.Instantiate(enemyRef.GetComponent<SinglePrefabReference>().Instance, pos, rot);
        EnemyController controller = Utility.AddEnemyComponent(enemyInstance, GetEnemyTypeControllerName(type));
        EnemyData data = EnemyData.CreateData(GameConfig.Instance.EnemyConfig_Set[type]);
        controller.SetEnemyData(data);
        controller.EnemyID = enemyID ?? GameSceneController.Instance.EnemyIndex;
        controller.Accessory = enemyRef.GetComponent<SinglePrefabReference>().Accessory;
        controller.is_traped = isTrapped;
        enemyInstance.name = "Enemy_" + controller.EnemyID;
        enemyInstance.GetComponent<EnemyAnimationEvent>().SetController(controller);
        GameSceneController.Instance.Enemy_Set.Add(controller.EnemyID, controller);
        GameSceneController.Instance.OnEnemySpawn(data);
        Debug.Log("Created enemy of type " + type + " at position: " + pos.ToString());
        if (!enemyID.HasValue)
            GameSceneController.Instance.EnemyIndex++;

        return controller;
    }

    public static IEnumerator CreateEnemy(EnemyType type, Vector3 pos, Quaternion rot)
    {
        yield return 1;
        EnemyController enemy = CreateEnemyInternal(type, pos, rot);
        yield return 1;
    }

    public static EnemyController CreateRemoteEnemy(EnemyType type, Vector3 pos, Quaternion rot, int enemy_id, bool is_boss = false)
    {
        return CreateEnemyInternal(type, pos, rot, enemy_id, false);
    }

    public static EnemyController CreateEnemyGetEnemyController(EnemyType type, Vector3 pos, Quaternion rot)
    {
        return CreateEnemyInternal(type, pos, rot);
    }

    public static EnemyController CreateEnemyForTrap(EnemyType type, Vector3 pos)
    {
        return CreateEnemyInternal(type, pos, Quaternion.identity, null, true);
    }


    public static string GetEnemyTypeControllerName(EnemyType type)
	{
		string result = "EnemyController";
		switch (type)
		{
		case EnemyType.E_ZOMBIE:
		case EnemyType.E_ZOMBIE_E:
		case EnemyType.E_ZOMBIE_COMMIS:
		case EnemyType.E_ZOMBIE_COMMIS_E:
        case EnemyType.E_ZOMBIE_COWBOY:
		case EnemyType.E_ZOMBIE_COWBOY_E:
			result = "ZombieController";
			break;
		case EnemyType.E_NURSE:
		case EnemyType.E_NURSE_E:
			result = "NurseController";
			break;
		case EnemyType.E_BOOMER:
		case EnemyType.E_BOOMER_E:
			result = "BoomerController";
			break;
		case EnemyType.E_BOOMER_TIMER:
        case EnemyType.E_BOOMER_TIMER_E:
			result = "BoomerTimerController";
			break;
		case EnemyType.E_CROW:
			result = "CrowController";
			break;
		case EnemyType.E_CLOWN:
		case EnemyType.E_CLOWN_E:
			result = "ClownController";
			break;
		case EnemyType.E_FATCOOK:
        case EnemyType.E_FATCOOK_E:
			result = "FatCookController";
			break;
		case EnemyType.E_HAOKE_A:
		case EnemyType.E_HAOKE_B:
			result = "HaokeController";
			break;
		case EnemyType.E_WRESTLER:
        case EnemyType.E_WRESTLER_E:
			result = "WrestlerController";
			break;
		case EnemyType.E_HALLOWEEN:
        case EnemyType.E_HALLOWEEN_E:
			result = "HalloweenController";
			break;
		case EnemyType.E_HALLOWEEN_SUB:
        case EnemyType.E_HALLOWEEN_SUB_E:
			result = "HalloweenSubController";
			break;
		case EnemyType.E_SHARK:
        case EnemyType.E_SHARK_E:
			result = "SharkController";
			break;
		}
		return result;
	}

	public static GameObject CreateEnemyObj(EnemyType type)
	{
		GameObject gameObject = GameSceneController.Instance.enemy_ref_map.Enemy_Set[type];
		return Object.Instantiate(gameObject.GetComponent<SinglePrefabReference>().Instance) as GameObject;
	}
}
