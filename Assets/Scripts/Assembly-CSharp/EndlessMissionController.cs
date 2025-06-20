using System;
using System.Collections;
using System.Collections.Generic;
using CoMZ2;
using UnityEngine;
using UnityEngine.UI;

public class EndlessMissionController : MissionController
{
    public float mission_life_time = 50f;
    public float initial_mission_life_time = 0f;

    protected bool mission_started;
    private float last_update_info_time;
    private float update_info_rate = 0.3f;

    public Text enemyCounterText;

    private float spawnIncreaseInterval = 3f;
    private float lastSpawnIncreaseTime = 0f;
    private int maxEnemyCount = 8;
    private float spawnRate = 0.5f;

    public override List<EnemyType> GetMissionEnemyTypeList()
    {
        List<EnemyWaveInfo> list = null;
        foreach (int key in GameConfig.Instance.EnemyWaveInfo_Normal_Set.Keys)
        {
            if (GameSceneController.Instance.DayLevel <= key)
            {
                list = GameConfig.Instance.EnemyWaveInfo_Normal_Set[key].wave_info_list;
                break;
            }
        }

        List<EnemyType> list2 = new List<EnemyType>();
        foreach (EnemyWaveInfo item in list)
        {
            foreach (EnemySpawnInfo item2 in item.spawn_info_list)
            {
                if (!list2.Contains(item2.EType))
                {
                    list2.Add(item2.EType);
                }
            }
        }
        return list2;
    }

    public override IEnumerator Start()
    {
        InitMissionController();
        mission_type = MissionType.Endless;
        CaculateDifficulty();

        yield return 1;

        if (enemyCounterText == null)
        {
            GameObject counterGO = GameObject.Find("EnemyCounterText");
            if (counterGO != null)
                enemyCounterText = counterGO.GetComponent<Text>();
        }

        PlayerController player = GameSceneController.Instance.player_controller;
        while (player == null)
        {
            yield return 1;
            player = GameSceneController.Instance.player_controller;
        }

        List<EnemyWaveInfo> EnemyWaveInfo_Set = null;
        foreach (int level in GameConfig.Instance.EnemyWaveInfo_Normal_Set.Keys)
        {
            if (GameSceneController.Instance.DayLevel <= level)
            {
                EnemyWaveInfo_Set = GameConfig.Instance.EnemyWaveInfo_Normal_Set[level].wave_info_list;
                break;
            }
        }

        while (GameSceneController.Instance.GamePlayingState == PlayingState.CG)
        {
            yield return 1;
        }

        mission_started = true;
        lastSpawnIncreaseTime = Time.time;
        if (mission_type == MissionType.Endless)
        {
            StartCoroutine(ScaleSpawnCap());
        }

        yield return new WaitForSeconds(1f);

        while (mission_life_time > 0f)
        {
            if (GameSceneController.Instance.Enemy_Set.Count < maxEnemyCount)
            {
                int index = UnityEngine.Random.Range(0, EnemyWaveInfo_Set.Count);
                EnemyWaveInfo wave = EnemyWaveInfo_Set[index];

                foreach (EnemySpawnInfo spawn_info in wave.spawn_info_list)
                {
                    if (mission_life_time <= 0f) break;

                    EnemyType EType = spawn_info.EType;
                    int Count = spawn_info.Count;
                    SpawnFromType From = spawn_info.From;

                    for (int i = 0; i < Count; i++)
                    {
                        while (GameSceneController.Instance.Enemy_Set.Count >= maxEnemyCount)
                            yield return new WaitForSeconds(1f);

                        if (mission_life_time <= 0f) break;

                        switch (From)
                        {
                            case SpawnFromType.Grave:
                                GameObject grave = FindClosedGrave(player.transform.position);
                                SpwanZombiesFromGrave(EType, grave);
                                break;
                            case SpawnFromType.Nest:
                                SpwanZombiesFromNest(EType, zombie_nest_array[UnityEngine.Random.Range(0, zombie_nest_array.Length)]);
                                break;
                        }

                        yield return new WaitForSeconds(spawnRate);
                    }

                    yield return new WaitForSeconds(GameConfig.Instance.EnemyWave_Interval_Normal.line_interval);
                }

                yield return new WaitForSeconds(GameConfig.Instance.EnemyWave_Interval_Normal.wave_interval);
            }

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("Mission Life Over~");
    }

    private IEnumerator ScaleSpawnCap()
    {
        if (mission_type != MissionType.Endless) yield break;

        while (mission_started)
        {
            yield return new WaitForSeconds(spawnIncreaseInterval);
            maxEnemyCount += 2;
            Debug.Log("Increased max enemy count to: " + maxEnemyCount);

            if (maxEnemyCount % 4 == 0)
            {
                spawnRate *= 0.60f;
                Debug.Log("Increased spawn rate! New delay: " + spawnRate.ToString("F2") + "s");
            }

            StartCoroutine(AnimateCounterFlash());
        }
    }

    private IEnumerator AnimateCounterFlash()
    {
        if (enemyCounterText == null) yield break;

        Color originalColor = enemyCounterText.color;
        Color flashColor = Color.yellow;
        float duration = 3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            enemyCounterText.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(t * 4f, 1f));
            yield return null;
        }

        enemyCounterText.color = originalColor;
    }

    public override void Update()
    {
        base.Update();
        if (GameSceneController.Instance.GamePlayingState == PlayingState.CG || !mission_started || is_mission_finished || is_mission_paused)
            return;

        mission_life_time += Time.deltaTime;

        if (Time.time - last_update_info_time >= update_info_rate)
        {
            last_update_info_time = Time.time;

            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.FloorToInt(mission_life_time));
            GameSceneController.Instance.game_main_panel.time_alive_panel.SetContent(
                timeSpan.Minutes.ToString("d2") + ":" + timeSpan.Seconds.ToString("d2")
            );

            if (enemyCounterText != null)
            {
                int currentCount = GameSceneController.Instance.Enemy_Set.Count;
                enemyCounterText.text = "Enemies: " + currentCount + " / " + maxEnemyCount;
            }
        }
    }

    public override void CaculateDifficulty()
    {
        mission_life_time = 0f;
    }
}
