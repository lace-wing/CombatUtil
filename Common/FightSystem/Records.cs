using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatUtil.Common.FightSystem
{
    public struct RecordHeader
    {
        /*目标: 实现支持多人&多Boss的战斗纪录
         *- 因为没有多世界的战斗, 所以把纪录以字典套结构体存在世界里
         *- 字典的key是纪录的头, value是纪录的主体
         *- 头包含所有参与战斗的玩家的filename的HashSet和Boss类名的HashSet
         *- 主体包含纪录的详细信息
         *- 使用Main.ActivePlayerFileData.Name查询某玩家在某世界的战斗纪录
         */
        private HashSet<string> playerFileNames;
        private HashSet<string> enemyNames;
        public RecordHeader(string[] players, string[] enemies)
        {
            playerFileNames = players.ToHashSet();
            enemyNames = enemies.ToHashSet();
        }
    }
    public struct RecordBody
    {
        public string[] PlayerFileNames;
        public FightRemark[][] Remarks;
        public string[] EnemyNames;
        public int[] EnemyCounts; 
        public int WinCount;
        public int LossCount;
        public int TimeMax;
        public int TimeMin;
        public bool TryGetRemarks(string name, out FightRemark[] remarks)
        {
            int i = PlayerFileNames.ToList().IndexOf(name);
            if (i != -1)
            {
                remarks = Remarks[i];
                return true;
            }
            else
            {
                remarks = new FightRemark[] { };
                return false;
            }
        }
        public bool TryGetEnemyCount(string name, out int count)
        {
            int i = EnemyNames.ToList().IndexOf(name);
            if (i != -1)
            {
                count = EnemyCounts[i];
                return true;
            }
            else
            {
                count = -1;
                return false;
            }
        }
        public bool MatchRecord(string[] players, string[] enemies)
        {
            if (players.Length <= 0 || enemies.Length <= 0)
            {
                return false;
            }
            List<string> plist = players.ToList();
            List<string> elist = enemies.ToList();
            for (int i = 0; i < PlayerFileNames.Length; i++)
            {
                if (!plist.Remove(PlayerFileNames[i]))
                {
                    return false;
                }
            }
            if (plist.Count > 0)
            {
                return false;
            }
            for (int i = 0; i < EnemyNames.Length; i++)
            {
                if (!elist.Remove(EnemyNames[i]))
                {
                    return false;
                }
            }
            if (elist.Count > 0)
            {
                return false;
            }
            return true;
        }
    }
    public enum FightRemark
    {
        Nohit,
        Nodamage
    }
}
