using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Boomlagoon.JSON;
using CoMZ2;
using LitJson;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public enum DailyMissionStatus
    {
        Disenable,
        Free,
        CrystalEnable,
        CrystalDisenable
    }

    public enum GamePlayType
    {
        Normal,
        Coop
    }

    public enum GameSceneState
    {
        None,
        Shop,
        Gaming
    }

    private static GameData instance;

    public DateTime last_checked_date_now;

    public DateTime next_cd_date;

    public string cur_save_date = string.Empty;

    public string save_date = string.Empty;

    public int daily_mission_count;

    public int lottery_reset_count;

    public int lottery_count;

    public bool daily_mode_enable;

    public bool is_crazy_daily;

    public GameDataInt total_cash = new GameDataInt(0);

    public GameDataInt total_crystal = new GameDataInt(0);

    public GameDataInt total_voucher = new GameDataInt(0);

    public int tapjoyPoints;

    public int day_level;

    public AvatarType cur_avatar = AvatarType.None;

    public Dictionary<string, WeaponData> WeaponData_Set = new Dictionary<string, WeaponData>();

    public Dictionary<AvatarType, AvatarData> AvatarData_Set = new Dictionary<AvatarType, AvatarData>();

    public Dictionary<string, GameProb> GameStoryProbs_Set = new Dictionary<string, GameProb>();

    public Dictionary<string, GameProb> WeaponFragmentProbs_Set = new Dictionary<string, GameProb>();

    public Dictionary<string, GameProb> WeaponIntensifierProbs_Set = new Dictionary<string, GameProb>();

    public Dictionary<string, int> Enemy_Loading_Set = new Dictionary<string, int>();

    public Dictionary<string, string> Iap_failed_info = new Dictionary<string, string>();

    public int Iap_failed_info_count;

    public GameSceneState scene_state;

    public string loading_to_scene = "test_new1";

    public QuestInfo cur_quest_info;

    public Action reset_nist_time_error;

    public Action reset_nist_time_finish;

    public float sensitivity_ratio = 0.4f;

    public bool is_enter_tutorial = true;

    public bool show_ui_tutorial = true;

    public bool show_ui_tutorial_weapon = true;

    public bool is_daily_cd_crystal;

    protected string user_id = string.Empty;

    public string NickName = string.Empty;

    public int FightStrength;

    protected string game_version = "1.0";

    public int enter_shop_count;

    public string timeserver_url = "http://72.167.165.221:7600/gameapi/GameCommonNo.do?action=groovy&json=%7B%22cmd%22:%22GetServerTime%22%7D";

    public string statistics_url = "http://208.109.176.89:8081/gameapi/turboPlatform2.do?action=logAllInfo&json=";

    public string iap_check_url = "http://192.225.224.97:7600/gameapi/GameCommon.do?action=groovy&json=";

    public string redeem_get_url = "http://184.168.72.188:9218/gameapi/comzb.do?action=comzb/GetGiftPackage&json=";

    public string redeem_accept_url = "http://184.168.72.188:9218/gameapi/comzb.do?action=comzb/AcceptGiftPackage&json=";

    public float redeem_change_ratio = 0.2f;

    protected DateTime finally_save_date;

    public bool TRINITI_IAP_CEHCK;

    public List<UnlockInGame> UnlockList = new List<UnlockInGame>();

    public Dictionary<string, SkillData> Skill_Avatar_Set = new Dictionary<string, SkillData>();

    public bool showUpdatePoster;

    public GamePlayType cur_game_type;

    public CoopBossType cur_coop_boss = CoopBossType.E_NONE;

    public List<string> lottery_seat_state = new List<string>();

    public static GameData Instance
    {
        get
        {
            return instance;
        }
    }

    public string UserId
    {
        get
        {
            return user_id;
        }
    }

    private void Awake()
    {
        if (GameObject.Find("_AndroidPlatform") == null)
        {
            GameObject gameObject = new GameObject("_AndroidPlatform");
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            DevicePlugin.InitAndroidPlatform();
            gameObject.AddComponent<TrinitiAdAndroidPlugin>();
        }
        instance = this;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        finally_save_date = GetCurDateTime();
        TRINITI_IAP_CEHCK = true;
    }

    private void Init()
    {
        daily_mission_count = 0;
        day_level = 1;
        total_cash = new GameDataInt(0);
        total_crystal = new GameDataInt(0);
        total_voucher = new GameDataInt(0);
        cur_avatar = AvatarType.Human;
        foreach (AvatarConfig value in GameConfig.Instance.AvatarConfig_Set.Values)
        {
            AvatarData avatarData = new AvatarData();
            avatarData.show_name = value.show_name;
            avatarData.avatar_name = value.avatar_name;
            avatarData.avatar_type = value.avatar_type;
            avatarData.config = value;
            avatarData.level = 1;
            avatarData.cur_exp = new GameDataInt(0);
            avatarData.exist_state = value.exist_state;
            avatarData.primary_equipment = "MP5";
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    avatarData.skill_list.Add(value.first_skill);
                }
                else
                {
                    avatarData.skill_list.Add("null");
                }
            }
            AvatarData_Set[avatarData.avatar_type] = avatarData;
        }
        foreach (SkillConfig value2 in GameConfig.Instance.Skill_Avatar_Set.Values)
        {
            SkillData skillData = new SkillData();
            skillData.skill_name = value2.skill_name;
            skillData.level = 1;
            skillData.exist_state = value2.exist_state;
            skillData.config = value2;
            skillData.ResetData();
            Skill_Avatar_Set[skillData.skill_name] = skillData;
        }
        foreach (WeaponConfig value3 in GameConfig.Instance.WeaponConfig_Set.Values)
        {
            WeaponData weaponData = new WeaponData();
            weaponData.weapon_name = value3.weapon_name;
            weaponData.weapon_type = value3.wType;
            weaponData.is_secondary = value3.is_secondary;
            weaponData.owner = value3.owner;
            weaponData.config = value3;
            weaponData.total_bullet_count = value3.initBullet;
            weaponData.level = 1;
            weaponData.exist_state = value3.exist_state;
            WeaponData_Set[weaponData.weapon_name] = weaponData;
        }
        foreach (string value4 in GameConfig.Instance.Enemy_Loading_Set.Values)
        {
            Enemy_Loading_Set[value4] = 0;
        }
        for (int j = 0; j < 10; j++)
        {
            lottery_seat_state.Add("null");
        }
        cur_quest_info = new QuestInfo();
        cur_quest_info.mission_type = MissionType.Cleaner;
        is_enter_tutorial = true;
        game_version = "2.1.2";
        user_id = DevicePlugin.GetUUID();
        if (!LoadData())
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer && Screen.height >= 700)
            {
                sensitivity_ratio = 0.6f;
            }
            SaveData();
            UploadStatistics("FirstTime", new Hashtable());
            showUpdatePoster = true;
        }
        if (game_version != "2.1.2")
        {
            game_version = "2.1.2";
            OnGameDataVersionDifferent();
            showUpdatePoster = true;
        }
        foreach (WeaponData value5 in WeaponData_Set.Values)
        {
            if (value5.exist_state == WeaponExistState.Locked && value5.config.unlock_day >= 0 && value5.config.unlock_day < day_level)
            {
                value5.Unlock();
            }
            value5.ResetData();
        }
        foreach (AvatarData value6 in AvatarData_Set.Values)
        {
            if (value6.exist_state == AvatarExistState.Locked && value6.config.unlockDay < day_level)
            {
                value6.Unlock();
            }
            value6.ResetData();
        }
    }

    public static void CheckGameData()
    {
        if (!GameObject.Find("GameData"))
        {
            GameObject gameObject = Instantiate(Resources.Load<GameObject>("GameData"));
            gameObject.name = "GameData";
            Instance.Init();
            TapJoyScript.CreateTapjoyObj();
        }
    }

    public void SaveData()
    {
        Configure configure = new Configure();
        configure.AddSection("Save", string.Empty, string.Empty);
        configure.AddValueSingle("Save", "IAPResend_android", Iap_Resend.Get_IAP_android_list(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "Cash", total_cash.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "Crystal", total_crystal.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "Voucher", total_voucher.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "TapjoyPoints", tapjoyPoints.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "DayLevel", day_level.ToString(), string.Empty, string.Empty);
        int num = (int)cur_avatar;
        configure.AddValueSingle("Save", "AvatarType", num.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "SensitivityRatio", sensitivity_ratio.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "Version", game_version, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "EnterShopCount", enter_shop_count.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "TimeserverUrl", timeserver_url, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "StatisticsUrl", statistics_url, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "IapUrl", iap_check_url, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "RedeemGetUrl", redeem_get_url, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "RedeemAcceptUrl", redeem_accept_url, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "RedeemAwardRatio", redeem_change_ratio.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "NickName", NickName, string.Empty, string.Empty);
        if (is_enter_tutorial)
        {
            configure.AddValueSingle("Save", "EnterTutorial", "1", string.Empty, string.Empty);
        }
        else
        {
            configure.AddValueSingle("Save", "EnterTutorial", "0", string.Empty, string.Empty);
        }
        if (show_ui_tutorial)
        {
            configure.AddValueSingle("Save", "ShowUITutorial", "1", string.Empty, string.Empty);
        }
        else
        {
            configure.AddValueSingle("Save", "ShowUITutorial", "0", string.Empty, string.Empty);
        }
        if (show_ui_tutorial_weapon)
        {
            configure.AddValueSingle("Save", "ShowUITutorialWeapon", "1", string.Empty, string.Empty);
        }
        else
        {
            configure.AddValueSingle("Save", "ShowUITutorialWeapon", "0", string.Empty, string.Empty);
        }
        if (TRINITI_IAP_CEHCK)
        {
            configure.AddValueSingle("Save", "IapCheck", "1", string.Empty, string.Empty);
        }
        else
        {
            configure.AddValueSingle("Save", "IapCheck", "0", string.Empty, string.Empty);
        }
        finally_save_date = GetCurDateTime();
        configure.AddValueSingle("Save", "SaveDate", finally_save_date.ToString(), string.Empty, string.Empty);
        ArrayList arrayList = new ArrayList();
        foreach (WeaponData value in WeaponData_Set.Values)
        {
            StringLine stringLine = new StringLine();
            stringLine.AddString(value.weapon_name);
            int weapon_type = (int)value.weapon_type;
            stringLine.AddString(weapon_type.ToString());
            stringLine.AddString(value.level.ToString());
            stringLine.AddString(value.total_bullet_count.ToString());
            int exist_state = (int)value.exist_state;
            stringLine.AddString(exist_state.ToString());
            stringLine.AddString(value.damage_level.ToString());
            stringLine.AddString(value.frequency_level.ToString());
            stringLine.AddString(value.clip_level.ToString());
            stringLine.AddString(value.range_level.ToString());
            stringLine.AddString(value.stretch_level.ToString());
            arrayList.Add(stringLine.content);
        }
        configure.AddValueArray2("Save", "WeaponsData", arrayList, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "WeaponsDataCount", arrayList.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList2 = new ArrayList();
        foreach (AvatarData value2 in AvatarData_Set.Values)
        {
            StringLine stringLine2 = new StringLine();
            stringLine2.AddString(value2.avatar_name);
            int avatar_type = (int)value2.avatar_type;
            stringLine2.AddString(avatar_type.ToString());
            stringLine2.AddString("null");
            stringLine2.AddString(value2.level.ToString());
            stringLine2.AddString(value2.armor_level.ToString());
            stringLine2.AddString(value2.cur_exp.ToString());
            int exist_state2 = (int)value2.exist_state;
            stringLine2.AddString(exist_state2.ToString());
            stringLine2.AddString(value2.primary_equipment);
            stringLine2.AddString(value2.hp_level.ToString());
            stringLine2.AddString(value2.damage_level.ToString());
            foreach (string item in value2.skill_list)
            {
                stringLine2.AddString(item);
            }
            arrayList2.Add(stringLine2.content);
        }
        configure.AddValueArray2("Save", "AvatarsData", arrayList2, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "AvatarsDataCount", arrayList2.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList3 = new ArrayList();
        foreach (SkillData value3 in Skill_Avatar_Set.Values)
        {
            StringLine stringLine3 = new StringLine();
            stringLine3.AddString(value3.skill_name);
            stringLine3.AddString(value3.level.ToString());
            int exist_state3 = (int)value3.exist_state;
            stringLine3.AddString(exist_state3.ToString());
            arrayList3.Add(stringLine3.content);
        }
        configure.AddValueArray2("Save", "SkillAvatarData", arrayList3, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "SkillAvatarDataCount", arrayList3.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList4 = new ArrayList();
        foreach (string key in GameStoryProbs_Set.Keys)
        {
            StringLine stringLine4 = new StringLine();
            stringLine4.AddString(key);
            stringLine4.AddString(GameStoryProbs_Set[key].count.ToString());
            arrayList4.Add(stringLine4.content);
        }
        configure.AddValueArray2("Save", "StoryProbData", arrayList4, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "StoryProbDataCount", arrayList4.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList5 = new ArrayList();
        foreach (string key2 in WeaponFragmentProbs_Set.Keys)
        {
            StringLine stringLine5 = new StringLine();
            stringLine5.AddString(key2);
            stringLine5.AddString(WeaponFragmentProbs_Set[key2].count.ToString());
            arrayList5.Add(stringLine5.content);
        }
        configure.AddValueArray2("Save", "FragmentProbData", arrayList5, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "FragmentProbDataCount", arrayList5.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList6 = new ArrayList();
        foreach (string key3 in WeaponIntensifierProbs_Set.Keys)
        {
            StringLine stringLine6 = new StringLine();
            stringLine6.AddString(key3);
            stringLine6.AddString(WeaponIntensifierProbs_Set[key3].count.ToString());
            arrayList6.Add(stringLine6.content);
        }
        configure.AddValueArray2("Save", "IntensifierProbData", arrayList6, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "IntensifierProbDataCount", arrayList6.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList7 = new ArrayList();
        foreach (string key4 in Enemy_Loading_Set.Keys)
        {
            StringLine stringLine7 = new StringLine();
            stringLine7.AddString(key4);
            stringLine7.AddString(Enemy_Loading_Set[key4].ToString());
            arrayList7.Add(stringLine7.content);
        }
        configure.AddValueArray2("Save", "EnemyLoadData", arrayList7, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "EnemyLoadDataCount", arrayList7.Count.ToString(), string.Empty, string.Empty);
        StringLine stringLine8 = new StringLine();
        foreach (string item2 in lottery_seat_state)
        {
            stringLine8.AddString(item2);
        }
        configure.AddValueArray("Save", "LotterySeatData", stringLine8.content, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "LotterySeatCount", lottery_seat_state.Count.ToString(), string.Empty, string.Empty);
        ArrayList arrayList8 = new ArrayList();
        foreach (string key5 in GameConfig.Instance.Config_Version_Set.Keys)
        {
            StringLine stringLine9 = new StringLine();
            stringLine9.AddString(key5);
            stringLine9.AddString(GameConfig.Instance.Config_Version_Set[key5]);
            arrayList8.Add(stringLine9.content);
        }
        configure.AddValueArray2("Save", "ConfigVersion", arrayList8, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "ConfigVersionCount", arrayList8.Count.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "IapFailedCount", Iap_failed_info.Count.ToString(), string.Empty, string.Empty);
        if (Iap_failed_info.Count > 0)
        {
            int num2 = 1;
            foreach (string key6 in Iap_failed_info.Keys)
            {
                configure.AddValueSingle("Save", "IapFailedInfoTid" + num2, key6, string.Empty, string.Empty);
                string data = Iap_failed_info[key6];
                data = DataEncipher(data);
                Utils.FileWriteString(Utils.SavePath() + MD5Sample.GetMd5String("IapFailedInfoReceipt" + num2) + ".bytes", data);
                num2++;
            }
        }
        string data2 = configure.Save();
        data2 = DataEncipher(data2);
        Utils.FileWriteString(Utils.SavePath() + MD5Sample.GetMd5String("CoMZ2") + ".bytes", data2);
    }

    public void SaveDailyData()
    {
        Configure configure = new Configure();
        configure.AddSection("Save", string.Empty, string.Empty);
        configure.AddValueSingle("Save", "SavDate", cur_save_date, string.Empty, string.Empty);
        configure.AddValueSingle("Save", "DailyMissionCount", daily_mission_count.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "LotteryResetCount", lottery_reset_count.ToString(), string.Empty, string.Empty);
        configure.AddValueSingle("Save", "LotteryCount", lottery_count.ToString(), string.Empty, string.Empty);
        string data = configure.Save();
        data = DataEncipher(data);
        Utils.FileWriteString(Utils.SavePath() + MD5Sample.GetMd5String("CoMZ2Daily") + ".bytes", data);
    }

    public bool LoadData()
    {
        string content = string.Empty;
        if (!Utils.FileReadString(Utils.SavePath() + MD5Sample.GetMd5String("CoMZ2") + ".bytes", ref content))
            return false;

        try
        {
            Configure configure = new Configure();

            content = DataDecrypt(content);
            configure.Load(content);

            // Game version string
            game_version = configure.GetSingle("Save", "Version");
            if (game_version == null)
            {
                game_version = "0";
            }

            float parsedGameVersion = 0f;
            float.TryParse(game_version, out parsedGameVersion);

            // Load currencies
            string cashStr = configure.GetSingle("Save", "Cash");
            int cash;
            if (!int.TryParse(cashStr, out cash))
            {
                cash = 0;
            }
            total_cash = new GameDataInt(cash);

            string crystalStr = configure.GetSingle("Save", "Crystal");
            int crystal;
            if (!int.TryParse(crystalStr, out crystal))
            {
                crystal = 0;
            }
            total_crystal = new GameDataInt(crystal);

            string voucherStr = configure.GetSingle("Save", "Voucher");
            int voucher;
            if (!int.TryParse(voucherStr, out voucher))
            {
                voucher = 0;
            }
            total_voucher = new GameDataInt(voucher);

            // TapjoyPoints
            string tapjoyPointsStr = configure.GetSingle("Save", "TapjoyPoints");
            if (tapjoyPointsStr != null)
            {
                int tapjoyPointsTemp;
                if (int.TryParse(tapjoyPointsStr, out tapjoyPointsTemp))
                {
                    tapjoyPoints = tapjoyPointsTemp;
                }
            }

            // Day level
            string dayLevelStr = configure.GetSingle("Save", "DayLevel");
            if (dayLevelStr != null)
            {
                int dayLevelTemp;
                if (int.TryParse(dayLevelStr, out dayLevelTemp))
                {
                    day_level = dayLevelTemp;
                }
            }

            // Avatar type
            string avatarTypeStr = configure.GetSingle("Save", "AvatarType");
            if (avatarTypeStr != null)
            {
                int avatarTypeInt;
                if (int.TryParse(avatarTypeStr, out avatarTypeInt))
                {
                    cur_avatar = (AvatarType)avatarTypeInt;
                }
                else
                {
                    cur_avatar = AvatarType.Human; // your default enum value
                }
            }
            else
            {
                cur_avatar = AvatarType.Human;
            }

            // Sensitivity ratio
            string sensitivityStr = configure.GetSingle("Save", "SensitivityRatio");
            if (sensitivityStr != null)
            {
                float sensitivityTemp;
                if (float.TryParse(sensitivityStr, out sensitivityTemp))
                {
                    sensitivity_ratio = sensitivityTemp;
                }
            }

            // Enter tutorial
            string enterTutorialStr = configure.GetSingle("Save", "EnterTutorial");
            if (enterTutorialStr != null)
            {
                int enterTutorialTemp;
                if (int.TryParse(enterTutorialStr, out enterTutorialTemp))
                {
                    is_enter_tutorial = (enterTutorialTemp != 0);
                }
            }

            // Show UI tutorial
            string showUITutorialStr = configure.GetSingle("Save", "ShowUITutorial");
            if (showUITutorialStr != null)
            {
                int showUITutorialTemp;
                if (int.TryParse(showUITutorialStr, out showUITutorialTemp))
                {
                    show_ui_tutorial = (showUITutorialTemp != 0);
                }
            }

            string showUITutorialWeaponStr = configure.GetSingle("Save", "ShowUITutorialWeapon");
            if (showUITutorialWeaponStr != null)
            {
                int showUITutorialWeaponTemp;
                if (int.TryParse(showUITutorialWeaponStr, out showUITutorialWeaponTemp))
                {
                    show_ui_tutorial_weapon = (showUITutorialWeaponTemp != 0);
                }
            }

            // Enter shop count
            string enterShopCountStr = configure.GetSingle("Save", "EnterShopCount");
            if (enterShopCountStr != null)
            {
                int enterShopCountTemp;
                if (int.TryParse(enterShopCountStr, out enterShopCountTemp))
                {
                    enter_shop_count = enterShopCountTemp;
                }
            }

            // URLs
            string timeServerUrl = configure.GetSingle("Save", "TimeserverUrl");
            if (timeServerUrl != null)
            {
                timeserver_url = timeServerUrl;
            }
            else
            {
                timeserver_url = "";
            }

            string statisticsUrl = configure.GetSingle("Save", "StatisticsUrl");
            if (statisticsUrl != null)
            {
                statistics_url = statisticsUrl;
            }
            else
            {
                statistics_url = "";
            }

            iap_check_url = "http://192.225.224.97:7600/gameapi/GameCommon.do?action=groovy&json=";

            string iapCheckStr = configure.GetSingle("Save", "IapCheck");
            if (iapCheckStr != null)
            {
                int iapCheckTemp;
                if (int.TryParse(iapCheckStr, out iapCheckTemp))
                {
                    TRINITI_IAP_CEHCK = (iapCheckTemp != 0);
                }
            }

            string redeemGetUrl = configure.GetSingle("Save", "RedeemGetUrl");
            if (redeemGetUrl != null)
            {
                redeem_get_url = redeemGetUrl;
            }

            string redeemAcceptUrl = configure.GetSingle("Save", "RedeemAcceptUrl");
            if (redeemAcceptUrl != null)
            {
                redeem_accept_url = redeemAcceptUrl;
            }

            string redeemAwardRatioStr = configure.GetSingle("Save", "RedeemAwardRatio");
            if (redeemAwardRatioStr != null)
            {
                float redeemRatioTemp;
                if (float.TryParse(redeemAwardRatioStr, out redeemRatioTemp))
                {
                    redeem_change_ratio = redeemRatioTemp;
                }
            }

            string nickNameStr = configure.GetSingle("Save", "NickName");
            if (!string.IsNullOrEmpty(nickNameStr))
            {
                NickName = nickNameStr;
            }

            string iapResendAndroidStr = configure.GetSingle("Save", "IAPResend_android");
            if (!string.IsNullOrEmpty(iapResendAndroidStr) && iapResendAndroidStr != "false")
            {
                string[] parts = iapResendAndroidStr.Split('|');
                if (parts.Length >= 5)
                {
                    string product_Id = parts[0];
                    string tid = parts[1];
                    string receipt = parts[2];
                    string action = parts[3];
                    string signature = parts[4];
                    IapCenter.Instance.SendIAPVerifyRequest_for_Android(product_Id, tid, receipt, action, signature);
                }
            }

            // Load weapons data
            string weaponsCountStr = configure.GetSingle("Save", "WeaponsDataCount");
            int weaponsCount = 0;
            if (weaponsCountStr != null)
            {
                int.TryParse(weaponsCountStr, out weaponsCount);
            }

            for (int i = 0; i < weaponsCount; i++)
            {
                string weaponId = configure.GetArray2("Save", "WeaponsData", i, 0);
                if (weaponId == null)
                    continue;

                WeaponData weaponData;
                if (!WeaponData_Set.TryGetValue(weaponId, out weaponData))
                    continue;

                int tempInt;

                string levelStr = configure.GetArray2("Save", "WeaponsData", i, 2);
                if (levelStr != null && int.TryParse(levelStr, out tempInt))
                {
                    weaponData.level = tempInt;
                }

                string totalBulletCountStr = configure.GetArray2("Save", "WeaponsData", i, 3);
                if (totalBulletCountStr != null && int.TryParse(totalBulletCountStr, out tempInt))
                {
                    weaponData.total_bullet_count = tempInt;
                }

                string existStateStr = configure.GetArray2("Save", "WeaponsData", i, 4);
                if (existStateStr != null && int.TryParse(existStateStr, out tempInt))
                {
                    weaponData.exist_state = (WeaponExistState)tempInt;
                }

                weaponData.damage_level = ParseOrDefault(configure.GetArray2("Save", "WeaponsData", i, 5), weaponData.level);
                weaponData.frequency_level = ParseOrDefault(configure.GetArray2("Save", "WeaponsData", i, 6), weaponData.level);
                weaponData.clip_level = ParseOrDefault(configure.GetArray2("Save", "WeaponsData", i, 7), weaponData.level);
                weaponData.range_level = ParseOrDefault(configure.GetArray2("Save", "WeaponsData", i, 8), weaponData.level);
                weaponData.stretch_level = ParseOrDefault(configure.GetArray2("Save", "WeaponsData", i, 9), weaponData.level);
            }

            // Load avatars data
            string avatarsCountStr = configure.GetSingle("Save", "AvatarsDataCount");
            int avatarsCount = 0;
            if (avatarsCountStr != null)
            {
                int.TryParse(avatarsCountStr, out avatarsCount);
            }

            for (int j = 0; j < avatarsCount; j++)
            {
                string avatarKeyStr = configure.GetArray2("Save", "AvatarsData", j, 1);
                if (avatarKeyStr == null)
                    continue;

                int avatarKeyInt;
                if (!int.TryParse(avatarKeyStr, out avatarKeyInt))
                    continue;

                AvatarType avatarKey = (AvatarType)avatarKeyInt;

                AvatarData avatarData;
                if (!AvatarData_Set.TryGetValue(avatarKey, out avatarData))
                    continue;

                int tempInt;

                string avatarLevelStr = configure.GetArray2("Save", "AvatarsData", j, 3);
                if (avatarLevelStr != null && int.TryParse(avatarLevelStr, out tempInt))
                {
                    avatarData.level = tempInt;
                }

                string armorLevelStr = configure.GetArray2("Save", "AvatarsData", j, 4);
                if (armorLevelStr != null && int.TryParse(armorLevelStr, out tempInt))
                {
                    avatarData.armor_level = tempInt;
                }

                string expStr = configure.GetArray2("Save", "AvatarsData", j, 5);
                if (expStr != null)
                {
                    int expVal;
                    if (int.TryParse(expStr, out expVal))
                    {
                        avatarData.cur_exp = new GameDataInt(expVal);
                    }
                }

                string existStateStr = configure.GetArray2("Save", "AvatarsData", j, 6);
                if (existStateStr != null && int.TryParse(existStateStr, out tempInt))
                {
                    avatarData.exist_state = (AvatarExistState)tempInt;
                }

                string primaryEquipmentStr = configure.GetArray2("Save", "AvatarsData", j, 7);
                if (primaryEquipmentStr != null)
                {
                    avatarData.primary_equipment = primaryEquipmentStr;
                }

                avatarData.hp_level = ParseOrDefault(configure.GetArray2("Save", "AvatarsData", j, 8), avatarData.level);
                avatarData.damage_level = ParseOrDefault(configure.GetArray2("Save", "AvatarsData", j, 9), avatarData.level);

                for (int k = 0; k < 2; k++)
                {
                    string skillVal = configure.GetArray2("Save", "AvatarsData", j, 10 + k);
                    if (skillVal != null)
                    {
                        avatarData.skill_list[k] = skillVal;
                    }
                }
            }

            // SkillAvatarData loading
            string skillAvatarDataCountStr = configure.GetSingle("Save", "SkillAvatarDataCount");
            int skillAvatarDataCount = 0;
            if (skillAvatarDataCountStr != null)
            {
                int.TryParse(skillAvatarDataCountStr, out skillAvatarDataCount);
            }

            if (skillAvatarDataCount > 0)
            {
                for (int l = 0; l < skillAvatarDataCount; l++)
                {
                    string skillKey = configure.GetArray2("Save", "SkillAvatarData", l, 0);
                    if (skillKey == null)
                        continue;

                    SkillData skillData;
                    if (!Skill_Avatar_Set.TryGetValue(skillKey, out skillData))
                        continue;

                    int tempInt;

                    string levelStr = configure.GetArray2("Save", "SkillAvatarData", l, 1);
                    if (levelStr != null && int.TryParse(levelStr, out tempInt))
                    {
                        skillData.level = tempInt;
                    }

                    string existStateStr = configure.GetArray2("Save", "SkillAvatarData", l, 2);
                    if (existStateStr != null && int.TryParse(existStateStr, out tempInt))
                    {
                        skillData.exist_state = (SkillExistState)tempInt;
                    }

                    skillData.ResetData();
                }
            }
            else
            {
                SkillData skillData2;

                if (Skill_Avatar_Set.TryGetValue("Whirlwind", out skillData2))
                {
                    skillData2.level = WeaponData_Set["Chainsaw"].level;
                    skillData2.ResetData();
                }
                if (Skill_Avatar_Set.TryGetValue("Enchant", out skillData2))
                {
                    skillData2.level = WeaponData_Set["Medicine"].level;
                    skillData2.ResetData();
                }
                if (Skill_Avatar_Set.TryGetValue("BaseballRobot", out skillData2))
                {
                    skillData2.level = WeaponData_Set["Baseball"].level;
                    skillData2.ResetData();
                }
                if (Skill_Avatar_Set.TryGetValue("Grenade", out skillData2))
                {
                    skillData2.level = WeaponData_Set["Shield"].level;
                    skillData2.ResetData();
                }
                if (Skill_Avatar_Set.TryGetValue("Scarecrow", out skillData2))
                {
                    skillData2.level = WeaponData_Set["44Magnum"].level;
                    skillData2.ResetData();
                }
            }

            // StoryProbData loading
            string storyProbDataCountStr = configure.GetSingle("Save", "StoryProbDataCount");
            int storyProbDataCount = 0;
            if (storyProbDataCountStr != null)
            {
                int.TryParse(storyProbDataCountStr, out storyProbDataCount);
            }

            for (int m = 0; m < storyProbDataCount; m++)
            {
                string key = configure.GetArray2("Save", "StoryProbData", m, 0);
                string countStr = configure.GetArray2("Save", "StoryProbData", m, 1);
                int count = 0;
                if (countStr != null)
                {
                    int.TryParse(countStr, out count);
                }

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                GameProb gameProb = new GameProb();
                GameProbsCfg probCfg;
                if (GameConfig.Instance.ProbsConfig_Set.TryGetValue(key, out probCfg))
                {
                    gameProb.prob_cfg = probCfg;
                    gameProb.count = count;
                    GameStoryProbs_Set[key] = gameProb;
                }
            }

            // FragmentProbData loading
            string fragmentProbDataCountStr = configure.GetSingle("Save", "FragmentProbDataCount");
            int fragmentProbDataCount = 0;
            if (fragmentProbDataCountStr != null)
            {
                int.TryParse(fragmentProbDataCountStr, out fragmentProbDataCount);
            }

            for (int n = 0; n < fragmentProbDataCount; n++)
            {
                string key = configure.GetArray2("Save", "FragmentProbData", n, 0);
                string countStr = configure.GetArray2("Save", "FragmentProbData", n, 1);
                int count2 = 0;
                if (countStr != null)
                {
                    int.TryParse(countStr, out count2);
                }

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                GameProbsCfg probCfg;
                if (GameConfig.Instance.ProbsConfig_Set.TryGetValue(key, out probCfg))
                {
                    GameProb gameProb2 = new GameProb();
                    gameProb2.prob_cfg = probCfg;
                    gameProb2.count = count2;
                    WeaponFragmentProbs_Set[key] = gameProb2;
                }
            }

            // IntensifierProbData loading
            string intensifierProbDataCountStr = configure.GetSingle("Save", "IntensifierProbDataCount");
            int intensifierProbDataCount = 0;
            if (intensifierProbDataCountStr != null)
            {
                int.TryParse(intensifierProbDataCountStr, out intensifierProbDataCount);
            }

            for (int num5 = 0; num5 < intensifierProbDataCount; num5++)
            {
                string key = configure.GetArray2("Save", "IntensifierProbData", num5, 0);
                string countStr = configure.GetArray2("Save", "IntensifierProbData", num5, 1);
                int count3 = 0;
                if (countStr != null)
                {
                    int.TryParse(countStr, out count3);
                }

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                GameProbsCfg probCfg;
                if (GameConfig.Instance.ProbsConfig_Set.TryGetValue(key, out probCfg))
                {
                    GameProb gameProb3 = new GameProb();
                    gameProb3.prob_cfg = probCfg;
                    gameProb3.count = count3;
                    WeaponIntensifierProbs_Set[key] = gameProb3;
                }
            }
            return true; // loaded successfully
        }
        catch (Exception ex)
        {
            Debug.LogError("LoadData Exception: " + ex.Message);
            return false;
        }
    }


    // Helper method for parsing int or fallback
    private int ParseOrDefault(string input, int defaultValue)
    {
        int result;
        if (input != null && int.TryParse(input, out result))
        {
            return result;
        }
        return defaultValue;
    }


    public bool LoadDailyData()
    {
        string content = string.Empty;
        string filePath = Utils.SavePath() + MD5Sample.GetMd5String("CoMZ2Daily") + ".bytes";

        if (Utils.FileReadString(filePath, ref content))
        {
            Configure configure = new Configure();
            content = DataDecrypt(content);
            configure.Load(content);

            save_date = configure.GetSingle("Save", "SavDate");

            int missionCount = 0;
            string missionCountStr = configure.GetSingle("Save", "DailyMissionCount");
            if (missionCountStr != null && int.TryParse(missionCountStr, out missionCount))
            {
                daily_mission_count = missionCount;
            }
            else
            {
                daily_mission_count = 0; // fallback default
            }

            int resetCount = 0;
            string resetCountStr = configure.GetSingle("Save", "LotteryResetCount");
            if (resetCountStr != null && int.TryParse(resetCountStr, out resetCount))
            {
                lottery_reset_count = resetCount;
            }
            else
            {
                lottery_reset_count = 0;
            }

            int lCount = 0;
            string lotteryCountStr = configure.GetSingle("Save", "LotteryCount");
            if (lotteryCountStr != null && int.TryParse(lotteryCountStr, out lCount))
            {
                lottery_count = lCount;
            }
            else
            {
                lottery_count = 0;
            }

            if (save_date != cur_save_date)
            {
                Debug.Log("Reset DailyMissionCount!");
                save_date = cur_save_date;
                daily_mission_count = 0;
                lottery_reset_count = 0;
                lottery_count = 0;
                is_daily_cd_crystal = false;

                SaveDailyData();
            }

            return true;
        }

        return false;
    }


    public List<WeaponData> GetOwnerWeapons()
    {
        List<WeaponData> list = new List<WeaponData>();
        foreach (WeaponData value in WeaponData_Set.Values)
        {
            if (value.exist_state == WeaponExistState.Owned)
            {
                list.Add(value);
            }
        }
        return list;
    }

    public List<WeaponData> GetCouldIntensifierWeapons()
    {
        List<WeaponData> list = new List<WeaponData>();
        foreach (WeaponData value in WeaponData_Set.Values)
        {
            if (value.exist_state == WeaponExistState.Owned)
            {
                list.Add(value);
            }
        }
        return list;
    }

    public void DropIntensifierRandom(List<WeaponData> WeaponSet)
    {
        int num = 0;
        foreach (WeaponData item in WeaponSet)
        {
            num += item.intensifier_drop_weight;
        }
        int num2 = UnityEngine.Random.Range(1, num);
        int num3 = 0;
        foreach (WeaponData item2 in WeaponSet)
        {
            num3 += item2.intensifier_drop_weight;
            if (num2 <= num3)
            {
                Debug.Log("drop intensifier, weapon name:" + item2.weapon_name);
                break;
            }
        }
    }

    public bool CheckStoryProbCombine(int level)
    {
        List<GameProb> list = new List<GameProb>();
        foreach (GameProb value in GameStoryProbs_Set.Values)
        {
            GameStoryProbsCfg gameStoryProbsCfg = value.prob_cfg as GameStoryProbsCfg;
            if (gameStoryProbsCfg.level == level)
            {
                list.Add(value);
            }
        }
        int num = 0;
        foreach (GameProb item in list)
        {
            GameStoryProbsCfg gameStoryProbsCfg2 = item.prob_cfg as GameStoryProbsCfg;
            num += gameStoryProbsCfg2.weight * item.count;
        }
        if (num >= 10)
        {
            foreach (GameProb item2 in list)
            {
                GameStoryProbs_Set.Remove(item2.prob_cfg.prob_name);
            }
            return true;
        }
        return false;
    }

    public List<GameProb> GetWeaponFragmentProbs(string weapon_name)
    {
        List<GameProb> list = new List<GameProb>();
        foreach (GameProb value in WeaponFragmentProbs_Set.Values)
        {
            WeaponFragmentProbsCfg weaponFragmentProbsCfg = value.prob_cfg as WeaponFragmentProbsCfg;
            if (weaponFragmentProbsCfg.weapon_name == weapon_name)
            {
                list.Add(value);
            }
        }
        return list;
    }

    public int[] GetWeaponFragmentProbsCountOrder(string weapon_name)
    {
        if (WeaponData_Set[weapon_name].config.combination_count == 0)
        {
            return null;
        }
        List<GameProb> weaponFragmentProbs = GetWeaponFragmentProbs(weapon_name);
        int[] array = new int[WeaponData_Set[weapon_name].config.combination_count];
        for (int i = 0; i < weaponFragmentProbs.Count; i++)
        {
            bool flag = false;
            foreach (GameProb item in weaponFragmentProbs)
            {
                WeaponFragmentProbsCfg weaponFragmentProbsCfg = item.prob_cfg as WeaponFragmentProbsCfg;
                int num = (int)(weaponFragmentProbsCfg.type - 1);
                if (num == i)
                {
                    array[i] = item.count;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                array[i] = 0;
            }
        }
        return array;
    }

    public bool CheckFragmentProbCombine(string weapon_name)
    {
        if (WeaponData_Set[weapon_name].exist_state != 0)
        {
            Debug.Log("Weapon:" + weapon_name + "can not combine,it is already combined.");
            return false;
        }
        int[] weaponFragmentProbsCountOrder = GetWeaponFragmentProbsCountOrder(weapon_name);
        if (weaponFragmentProbsCountOrder == null)
        {
            return false;
        }
        bool result = true;
        for (int i = 0; i < weaponFragmentProbsCountOrder.Length; i++)
        {
            if (weaponFragmentProbsCountOrder[i] == 0)
            {
                result = false;
                break;
            }
        }
        return result;
    }

    public bool WeaponCombine(string weapon_name)
    {
        if (!CheckFragmentProbCombine(weapon_name))
        {
            return false;
        }
        List<GameProb> weaponFragmentProbs = GetWeaponFragmentProbs(weapon_name);
        foreach (GameProb item in weaponFragmentProbs)
        {
            WeaponFragmentProbs_Set.Remove(item.prob_cfg.prob_name);
        }
        Instance.WeaponData_Set[weapon_name].exist_state = WeaponExistState.Owned;
        return true;
    }

    public List<WeaponData> GetPrimaryWeapons()
    {
        List<WeaponData> list = new List<WeaponData>();
        foreach (WeaponData value in WeaponData_Set.Values)
        {
            if (!value.is_secondary)
            {
                list.Add(value);
            }
        }
        return list;
    }

    public List<WeaponData> GetSecondaryWeaponsFor(AvatarType avatar_type)
    {
        List<WeaponData> list = new List<WeaponData>();
        foreach (WeaponData value in WeaponData_Set.Values)
        {
            if (value.is_secondary && value.owner == avatar_type)
            {
                list.Add(value);
            }
        }
        return list;
    }

    public void ResetCurNistTime()
    {
        Debug.Log("ResetCurNistTime...");
        StartCoroutine(ResetCurServerTime());
    }

    public IEnumerator ResetCurServerTime()
    {
        DateTime currentDateTime = DateTime.Now;
        last_checked_date_now = currentDateTime;
        next_cd_date = currentDateTime.AddDays(1.0);
        next_cd_date = new DateTime(next_cd_date.Year, next_cd_date.Month, next_cd_date.Day, 0, 0, 0);
        Debug.Log("last_checked_date_now:" + last_checked_date_now);
        Debug.Log("next_cd_date:" + next_cd_date);
        Debug.Log(string.Concat("cd:", next_cd_date - last_checked_date_now, " totalSec:", (next_cd_date - last_checked_date_now).TotalSeconds));
        string date = (cur_save_date = last_checked_date_now.Year.ToString() + last_checked_date_now.Month + last_checked_date_now.Day);
        daily_mode_enable = true;
        if (!LoadDailyData())
        {
            SaveDailyData();
        }
        if (reset_nist_time_finish != null)
        {
            reset_nist_time_finish();
        }
        yield break;
    }

    public void SetMapMissionList(ref List<QuestInfo> mInfos)
    {
        int key;
        QuestInfo questInfo;
        if (instance.day_level > GameConfig.Instance.Side_Quest_Order.Count)
        {
            key = (instance.day_level - 1) % GameConfig.Instance.Side_Quest_Order_Spare.Count + 1;
            questInfo = GameConfig.Instance.Side_Quest_Order_Spare[key];
        }
        else
        {
            key = instance.day_level;
            questInfo = GameConfig.Instance.Side_Quest_Order[key];
        }
        questInfo.SetQuestComment();
        if (questInfo.mission_type == MissionType.MainMission)
        {
            questInfo = GameConfig.Instance.Main_Quest_Order[key];
        }
        mInfos.Add(questInfo);
        List<string> list = new List<string>();
        list.Add("Church");
        list.Add("Depot");
        list.Add("Junkyard");
        questInfo = new QuestInfo();
        questInfo.mission_type = MissionType.Cleaner;
        questInfo.mission_day_type = MissionDayType.Daily;
        questInfo.SetQuestComment();
        questInfo.scene_name = list[UnityEngine.Random.Range(0, list.Count)];
        mInfos.Add(questInfo);
        questInfo = new QuestInfo();
        questInfo.mission_type = MissionType.Coop;
        questInfo.mission_day_type = MissionDayType.None;
        questInfo.SetQuestComment();
        questInfo.scene_name = list[UnityEngine.Random.Range(0, list.Count)];
        mInfos.Add(questInfo);
    }

    public void MapSceneQuestInfoWrite(QuestInfo info)
    {
        Debug.Log(string.Concat("info:", info.mission_day_type, " ff:", info.mission_type));
        Instance.cur_quest_info = info;
        Instance.loading_to_scene = Instance.cur_quest_info.scene_name;
        if (Instance.cur_quest_info.mission_day_type == MissionDayType.Daily)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("tCrystalNum", GetDailyMissionPrice(is_crazy_daily));
            Instance.UploadStatistics("tCrystal_DaiLy", hashtable);
            total_crystal -= GetDailyMissionPrice(is_crazy_daily);
            if (total_crystal < 0)
            {
                total_crystal = new GameDataInt(0);
                Debug.LogWarning("Enter daily mission not enough crystal.");
            }
            Instance.daily_mission_count++;
            Instance.SaveDailyData();
        }
    }

    public List<WeaponFragmentList> GetWeaponFragmentProbsState(string weapon_name)
    {
        WeaponFragmentList weaponFragmentList = null;
        List<WeaponFragmentList> list = new List<WeaponFragmentList>();
        List<GameProb> weaponFragmentProbs = GetWeaponFragmentProbs(weapon_name);
        List<WeaponFragmentProbsCfg> weaponFragmentProb = GameConfig.Instance.GetWeaponFragmentProb(weapon_name);
        foreach (WeaponFragmentProbsCfg item in weaponFragmentProb)
        {
            weaponFragmentList = new WeaponFragmentList(item.prob_name, 0, item.image_name);
            foreach (GameProb item2 in weaponFragmentProbs)
            {
                WeaponFragmentProbsCfg weaponFragmentProbsCfg = item2.prob_cfg as WeaponFragmentProbsCfg;
                if (weaponFragmentProbsCfg != null && item.prob_name == weaponFragmentProbsCfg.prob_name)
                {
                    weaponFragmentList.count += item2.count;
                }
            }
            list.Add(weaponFragmentList);
        }
        return list;
    }

    public float GetSideEnemyStandardReward()
    {
        float paraA = GameConfig.Instance.Standary_Enemy_Info.ParaA;
        float paraB = GameConfig.Instance.Standary_Enemy_Info.ParaB;
        float paraC = GameConfig.Instance.Standary_Enemy_Info.ParaC;
        float paraK = GameConfig.Instance.Standary_Enemy_Info.ParaK;
        float num = Instance.day_level;
        return paraA * Mathf.Pow(num - paraB, paraK) + paraC;
    }

    public float GetSideEnemyStandardRewardTotal()
    {
        float sideEnemyStandardReward = GetSideEnemyStandardReward();
        return sideEnemyStandardReward * GameConfig.Instance.GetStandaryEnemyCount(Instance.day_level);
    }

    public int GetMissionRewardCash(MissionType mission_type, MissionDayType mission_day_type, int crazy_daily = 0)
    {
        float sideEnemyStandardRewardTotal = GetSideEnemyStandardRewardTotal();
        int result = 0;
        switch (mission_day_type)
        {
            case MissionDayType.Daily:
                switch (crazy_daily)
                {
                    case 0:
                        result = ((!Instance.is_crazy_daily) ? ((int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_a * sideEnemyStandardRewardTotal)) : ((int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_b * sideEnemyStandardRewardTotal)));
                        break;
                    case 1:
                        result = (int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_a * sideEnemyStandardRewardTotal);
                        break;
                    case 2:
                        result = (int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_b * sideEnemyStandardRewardTotal);
                        break;
                }
                break;
            case MissionDayType.Tutorial:
                result = GameConfig.Instance.init_cash;
                break;
            case MissionDayType.Main:
                result = (int)GameConfig.Instance.Main_Quest_Difficulty_Set[Instance.day_level].finish_reward;
                break;
            default:
                result = (int)(GameConfig.Instance.Mission_Finish_Reward_Info.side_ratio * sideEnemyStandardRewardTotal);
                break;
        }
        return result;
    }

    public int GetMissionRewardCrystal(MissionType mission_type, MissionDayType mission_day_type)
    {
        int result = 0;
        switch (mission_day_type)
        {
            case MissionDayType.Main:
                result = 1;
                break;
            case MissionDayType.Tutorial:
                result = GameConfig.Instance.init_crystal;
                break;
        }
        return result;
    }

    public int GetMissionRewardVoucher(MissionType mission_type, MissionDayType mission_day_type, int crazy_daily = 0)
    {
        int result = 0;
        switch (mission_day_type)
        {
            case MissionDayType.Daily:
                switch (crazy_daily)
                {
                    case 0:
                        result = ((!Instance.is_crazy_daily) ? ((int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_base * GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_a)) : ((int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_base * GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_b)));
                        break;
                    case 1:
                        result = (int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_base * GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_a);
                        break;
                    case 2:
                        result = (int)(GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_base * GameConfig.Instance.Mission_Finish_Reward_Info.daily_ratio_voucher_b);
                        break;
                }
                break;
            case MissionDayType.Tutorial:
                result = GameConfig.Instance.init_voucher;
                break;
            default:
                result = 1;
                break;
            case MissionDayType.Main:
                break;
        }
        return result;
    }

    public DailyMissionStatus EnableDailyMission()
    {
        if (daily_mode_enable && daily_mission_count < 1)
        {
            return DailyMissionStatus.Free;
        }
        if (daily_mode_enable && daily_mission_count < 6)
        {
            return DailyMissionStatus.CrystalEnable;
        }
        if (daily_mode_enable && daily_mission_count >= 6)
        {
            return DailyMissionStatus.CrystalDisenable;
        }
        return DailyMissionStatus.Disenable;
    }

    public bool EnableEnterDailyMission()
    {
        if ((EnableDailyMission() == DailyMissionStatus.Free || EnableDailyMission() == DailyMissionStatus.CrystalEnable) && total_crystal >= GetDailyMissionPrice(is_crazy_daily))
        {
            return true;
        }
        return false;
    }

    public int GetDailyMissionPrice(bool is_hard)
    {
        if (is_hard)
        {
            if (EnableDailyMission() == DailyMissionStatus.Free)
            {
                return GameConfig.Instance.daily_price_hard1;
            }
            if (EnableDailyMission() == DailyMissionStatus.CrystalEnable)
            {
                return GameConfig.Instance.daily_price_hard2;
            }
        }
        else
        {
            if (EnableDailyMission() == DailyMissionStatus.Free)
            {
                return GameConfig.Instance.daily_price_easy1;
            }
            if (EnableDailyMission() == DailyMissionStatus.CrystalEnable)
            {
                return GameConfig.Instance.daily_price_easy2;
            }
        }
        return 99999;
    }

    public int GetDailyCDPrice()
    {
        if (EnableDailyMission() == DailyMissionStatus.CrystalEnable)
        {
            return GameConfig.Instance.daily_price_cd;
        }
        return 99999;
    }

    public bool ResetDailyCD()
    {
        if (EnableDailyMission() == DailyMissionStatus.CrystalEnable && total_crystal >= GetDailyCDPrice())
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("tCrystalNum", GetDailyCDPrice());
            Instance.UploadStatistics("tCrystal_SpeedUp", hashtable);
            total_crystal -= GetDailyCDPrice();
            is_daily_cd_crystal = true;
            return true;
        }
        return false;
    }

    public static string Encipher(string data_encipher)
    {
        int num = 17;
        char[] array = data_encipher.ToCharArray();
        char[] array2 = data_encipher.ToCharArray();
        char[] array3 = new char[2] { '\0', '\0' };
        for (int i = 0; i < array.Length; i++)
        {
            char c = (array3[0] = array[i]);
            string s = new string(array3);
            int num2 = char.ConvertToUtf32(s, 0);
            num2 ^= num;
            array2[i] = char.ConvertFromUtf32(num2)[0];
        }
        return new string(array2);
    }

    public static bool IsHighEffect()
    {
        return true;
    }

    public void OnExchgCurrcy(GameCurrencyType src_type, GameCurrencyType des_type, int src_count, int des_count)
    {
        switch (src_type)
        {
            case GameCurrencyType.Crystal:
                total_crystal -= src_count;
                if (total_crystal <= 0)
                {
                    Debug.LogWarning(string.Concat("ExchgCurrcy warning src type:", src_type, " count:", src_count));
                    total_crystal = new GameDataInt(0);
                }
                break;
            case GameCurrencyType.Voucher:
                total_voucher -= src_count;
                if (total_voucher <= 0)
                {
                    Debug.LogWarning(string.Concat("ExchgCurrcy warning src type:", src_type, " count:", src_count));
                    total_voucher = new GameDataInt(0);
                }
                break;
            case GameCurrencyType.Cash:
                total_cash -= src_count;
                if (total_cash <= 0)
                {
                    Debug.LogWarning(string.Concat("ExchgCurrcy warning src type:", src_type, " count:", src_count));
                    total_cash = new GameDataInt(0);
                }
                break;
        }
        Hashtable hashtable = new Hashtable();
        switch (des_type)
        {
            case GameCurrencyType.Crystal:
                total_crystal += des_count;
                break;
            case GameCurrencyType.Voucher:
                total_voucher += des_count;
                hashtable.Add("tCrystalNum", src_count);
                hashtable.Add("GoldNum", des_count);
                Instance.UploadStatistics("tCrystal_Gold", hashtable);
                break;
            case GameCurrencyType.Cash:
                total_cash += des_count;
                hashtable.Add("tCrystalNum", src_count);
                hashtable.Add("VouchersNum", des_count);
                Instance.UploadStatistics("tCrystal_Vouchers", hashtable);
                break;
        }
    }

    public void CheckEnemyFirstShow(string enemy_name)
    {
        if (Enemy_Loading_Set.ContainsKey(enemy_name) && Enemy_Loading_Set[enemy_name] != 1)
        {
            Enemy_Loading_Set[enemy_name] = 1;
        }
    }

    public void UploadStatistics(string action_id, Hashtable action_data)
    {
        Hashtable hashtable = new Hashtable();
        hashtable["gamename"] = "com.trinitigame.callofminizombies2";
        hashtable["uuid"] = user_id;
        hashtable["action"] = action_id;
        string text2 = (string)(hashtable["data"] = JsonMapper.ToJson(action_data));
        string s = JsonMapper.ToJson(hashtable);
        byte[] post_data = XXTEAUtils.Encrypt(Encoding.UTF8.GetBytes(s), Encoding.UTF8.GetBytes("abcd@@##980[]L>."));
        wwwClient.Instance.SendHttpRequest(statistics_url, post_data, OnStatisticsResponse, OnStatisticsRequestError, action_id);
    }

    public void OnStatisticsResponse(string action, byte[] response_data)
    {
        string jsonString = string.Empty;
        if (response_data == null)
        {
            Debug.LogError("OnStatisticsResponse:" + action + " response_data error");
            return;
        }
        byte[] array = XXTEAUtils.Decrypt(response_data, Encoding.UTF8.GetBytes("abcd@@##980[]L>."));
        if (array != null)
        {
            jsonString = Encoding.UTF8.GetString(array);
        }
        JSONObject jSONObject = JSONObject.Parse(jsonString);
        if (jSONObject.ContainsKey("code"))
        {
            string text = jSONObject["code"].ToString();
            if (!(text == "0"))
            {
            }
        }
    }


    public void OnStatisticsRequestError(string action, byte[] post_data)
    {
        Debug.Log("OnRequestError");
    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause:" + pause);
        if (is_enter_tutorial)
        {
            return;
        }
        if (!pause)
        {
            DateTime curDateTime = GetCurDateTime();
            TimeSpan timeSpan = (curDateTime - finally_save_date).Duration();
            if (timeSpan >= TimeSpan.FromMinutes(120.0))
            {
                Debug.Log("Long time no see.");
                TAudioManager.instance.soundVolume = 1f;
                TAudioManager.instance.musicVolume = 1f;
                Time.timeScale = 1f;
                LoadingUIController.FinishedLoading();
                Application.LoadLevel("GameCover");
            }
            PushNotification.ReSetNotifications();
        }
        else
        {
            finally_save_date = GetCurDateTime();
            Hashtable hashtable = new Hashtable();
            hashtable.Add("count", 1);
            Instance.UploadStatistics("Logout", hashtable);
        }
    }

    public static string DataEncipher(string data)
    {
        if (!GameConfig.IsEditorMode())
        {
            string enciphered = Encipher(data);                  // Obfuscate data first
            return XXTEAUtils.Encrypt(enciphered, GetImportContent()); // Then encrypt it
        }
        return data;
    }

    public static string DataDecrypt(string data)
    {
        if (!GameConfig.IsEditorMode())
        {
            string decrypted = XXTEAUtils.Decrypt(data, GetImportContent()); // Decrypt first
            return Encipher(decrypted);                                   // Then de-obfuscate
        }
        return data;
    }

    public static string GetImportContent()
    {
        return "in0yt@n5#f.o71[";
    }

    public static DateTime GetCurDateTime()
    {
        return new DateTime(MiscPlugin.GetIOSYear(), MiscPlugin.GetIOSMonth(), MiscPlugin.GetIOSDay(), MiscPlugin.GetIOSHour(), MiscPlugin.GetIOSMin(), MiscPlugin.GetIOSSec());
    }

    private void OnGameDataVersionDifferent()
    {
        Debug.Log("OnGameDataVersionDifferent...");
        GameConfig.Instance.force_update_local = true;
        GameConfig.Instance.Init();
        SaveData();
    }

    public void SaveIapFailedInfo(string Random, int Rat, string product_Id, string tid, string receipt)
    {
        Iap_failed_info[Random + "|" + Rat + "|" + product_Id + "|" + tid] = receipt;
        SaveData();
    }

    public void RemoveIapFailedInfo(string Random, int Rat, string product_Id, string tid, string receipt)
    {
        if (Iap_failed_info.ContainsKey(Random + "|" + Rat + "|" + product_Id + "|" + tid))
        {
            Iap_failed_info.Remove(Random + "|" + Rat + "|" + product_Id + "|" + tid);
        }
        SaveData();
    }

    public static bool CheckSaveDataVersion()
    {
        string content = string.Empty;
        if (Utils.FileReadString(Utils.SavePath() + MD5Sample.GetMd5String("CoMZ2") + ".bytes", ref content))
        {
            Configure configure = new Configure();
            content = DataDecrypt(content);
            configure.Load(content);
            string single = configure.GetSingle("Save", "Version");
            if (single != "2.1.2")
            {
                return true;
            }
        }
        return false;
    }
}
