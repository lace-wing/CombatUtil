using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace CombatUtil.Common.FightSystem
{
    public struct FightRecordHead
    {
        /*
         * 目标: 实现支持多人&多Boss的战斗纪录
         * 因为没有多世界的战斗, 所以把纪录以字典套结构体存在世界里
         * 字典的key是纪录的头, value是纪录的主体
         * 头包含所有参与战斗的玩家的filename的HashSet和Boss类名的HashSet
         * 主体包含纪录的详细信息
         * 使用 Main.ActivePlayerFileData.Name 查询某玩家在某世界的战斗纪录
         * 用Dictionary存储, 每一个 玩家-敌怪 组合都是独特的
         */
        /// <summary>
        /// Key: player's filename, value: player's display name
        /// </summary>
        public Dictionary<string, string> PlayerNames;
        public Dictionary<string, int> EnemyInfo;

        public bool Valid => PlayerNames.Count > 0 && EnemyInfo.Count > 0;
        public bool HasPlayer(string filename) => PlayerNames.Keys.Contains(filename);
        public bool HasEnemy(string className, int count) => EnemyInfo.TryGetValue(className, out int c) && c == count;
        /// <summary>
        /// Checks if the provided HashSet matches the record's player filenames
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public bool MatchPlayer(HashSet<string> players)
        {
            return players == PlayerNames.Keys.ToHashSet();
        }
        /// <summary>
        /// Checks if the provided Dictionary matches the record's enemy info
        /// </summary>
        /// <param name="enemies"></param>
        /// <returns></returns>
        public bool MatchEnemy(Dictionary<string, int> enemies)
        {
            return enemies == EnemyInfo;
        }
    }
    public struct FightRecordBody
    {
        public FightRemark[][] Remarks;
        public int[] DeathCounts; 
        public int WinCount;
        public int LossCount;
        public int TimeMax;
        public int TimeMin;
    }
    public enum FightRemark
    {
        Nohit,
        Nodamage
    }

    public class BossFightRecord : ModSystem
    {
        public static Dictionary<FightRecordHead, FightRecordBody> BFightRecord = new Dictionary<FightRecordHead, FightRecordBody>();

        /// <summary>
        /// Get the first occurence of the matching FightRecordHead
        /// </summary>
        /// <param name="playerFilenames"></param>
        /// <param name="enemyInfo"></param>
        /// <returns>Returns a matching FightRecordHead or an empty FightRecordHead if no matching case is found</returns>
        public static FightRecordHead GetRecordHead(HashSet<string> playerFilenames, Dictionary<string, int> enemyInfo)
        {
            FightRecordHead head = new FightRecordHead();
            for (int i = 0; i < BFightRecord.Count; i++)
            {
                head = BFightRecord.Keys.ToArray()[i];
                if (head.MatchPlayer(playerFilenames) && head.MatchEnemy(enemyInfo))
                {
                    return head;
                }
            }
            return new FightRecordHead();
        }
        /// <summary>
        /// Get a player's records from a 
        /// </summary>
        /// <param name="head"></param>
        /// <param name="filename"></param>
        /// <param name="deaths"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public static bool TryGetPlayerRecords(FightRecordHead head, string filename, out int deaths, out FightRemark[] remarks)
        {
            deaths = 0;
            remarks = new FightRemark[0];
            int i = head.PlayerNames.Keys.ToList().IndexOf(filename);
            if (i == -1)
            {
                return false;
            }
            deaths = BFightRecord.GetValueOrDefault(head).DeathCounts[i];
            remarks = BFightRecord.GetValueOrDefault(head).Remarks[i];
            return true;
        }
        /// <summary>
        /// Get an enemy's count
        /// </summary>
        /// <param name="className">The enemy's class name</param>
        /// <returns>Returns number of the enemy in the fight record or -1 if the enemy is not in the fight</returns>
        public static int GetEnemyCount(FightRecordHead head, string className)
        {
            int i = head.EnemyInfo.Keys.ToList().IndexOf(className);
            if (i != -1)
            {
                return head.EnemyInfo.Values.ToArray()[i];
            }
            return -1;
        }

        public override void OnWorldLoad()
        {
            BFightRecord = new Dictionary<FightRecordHead, FightRecordBody>();
        }
        public override void OnWorldUnload()
        {
            BFightRecord = new Dictionary<FightRecordHead, FightRecordBody>();
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("BFightRecord"))
            {
                tag.Add("BFightRecord", BFightRecord);
            }
            else
            {
                tag["BFightRecord"] = Utils.ToJSON(BFightRecord);
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            BFightRecord = Utils.FromJSON<Dictionary<FightRecordHead, FightRecordBody>>(tag["BFightRecord"].ToString());
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Utils.ToJSON(BFightRecord));
        }
        public override void NetReceive(BinaryReader reader)
        {
            string bf = reader.ReadString();
            BFightRecord = Utils.FromJSON<Dictionary<FightRecordHead, FightRecordBody>>(bf); //TODO Test it
        }
    }
}
