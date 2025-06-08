using System.Collections.Generic;
using UnityEngine;

public class EnemyMap : MonoBehaviour
{
	public Dictionary<EnemyType, GameObject> Enemy_Set = new Dictionary<EnemyType, GameObject>();

	private void Awake()
	{
	}

    public void ResetEnemyMapInfo(List<EnemyType> enemy_type_list)
    {
        GameObject gameObject = null;

        foreach (EnemyType item in enemy_type_list)
        {
            // Instantiate and store the main enemy
            gameObject = Object.Instantiate(Resources.Load("Prefabs/Enemy/" + GameConfig.Instance.EnemyConfig_Set[item].enemy_name)) as GameObject;
            Enemy_Set[item] = gameObject;

            // Handle special cases
            switch (item)
            {
                case EnemyType.E_FATCOOK:
                    {
                        EnemyType minionType = EnemyType.E_BOOMER_TIMER;
                        GameObject minion = Object.Instantiate(Resources.Load("Prefabs/Enemy/" + GameConfig.Instance.EnemyConfig_Set[minionType].enemy_name)) as GameObject;
                        Enemy_Set[minionType] = minion;
                        break;
                    }

                case EnemyType.E_FATCOOK_E:
                    {
                        EnemyType minionType = EnemyType.E_BOOMER_TIMER;
                        GameObject minion = Object.Instantiate(Resources.Load("Prefabs/Enemy/" + GameConfig.Instance.EnemyConfig_Set[minionType].enemy_name)) as GameObject;
                        Enemy_Set[minionType] = minion;
                        break;
                    }

                case EnemyType.E_HALLOWEEN:
                    {
                        EnemyType subType = EnemyType.E_HALLOWEEN_SUB;
                        GameObject sub = Object.Instantiate(Resources.Load("Prefabs/Enemy/" + GameConfig.Instance.EnemyConfig_Set[subType].enemy_name)) as GameObject;
                        Enemy_Set[subType] = sub;
                        break;
                    }
                case EnemyType.E_HALLOWEEN_E:
                    {
                        EnemyType subType = EnemyType.E_HALLOWEEN_SUB;
                        GameObject sub = Object.Instantiate(Resources.Load("Prefabs/Enemy/" + GameConfig.Instance.EnemyConfig_Set[subType].enemy_name)) as GameObject;
                        Enemy_Set[subType] = sub;
                        break;
                    }
            }
        }

        // Initialize all enemy prefabs as inactive
        foreach (EnemyType key in Enemy_Set.Keys)
        {
            GameObject gameObject2 = EnemyFactory.CreateEnemyObj(key);
            gameObject2.SetActive(false);
        }
    }
}
