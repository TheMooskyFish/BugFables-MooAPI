using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MooAPI.CustomAttack
{
    public class Core
    {
        public delegate IEnumerator Callback(EntityControl entity, CustomAttack attack);
        public class CustomAttack(string name, string desc, int cost, BattleControl.AttackArea attackArea, bool vi, bool kabbu, bool leif, bool targetground, bool targetfront, int actioncommand, bool targetalive, bool excludeself, bool targetfainted, Callback callback)
        {
            public string name = name;
            public string desc = desc;
            public int cost = cost;
            public BattleControl.AttackArea AttackArea = attackArea;
            public bool vi = vi;
            public bool kabbu = kabbu;
            public bool leif = leif;
            public bool targetground = targetground;
            public bool targetfront = targetfront;
            public int actioncommand = actioncommand;
            public bool targetalive = targetalive;
            public bool excludeself = excludeself;
            public bool targetfainted = targetfainted;
            public int id;
            public Callback callback = callback;
        }
        internal static List<CustomAttack> Skills = [];
        public static IEnumerator Handler(EntityControl entity, int actionid)
        {
            Plugin.Logger.LogInfo($"id: {actionid} entity: {entity}");
            if (actionid == -555 || entity.tag != "Player") { yield break; }
            while (MainManager.battle.checkingdead != null)
            {
                yield return null;
            }
            var attack = Skills.FirstOrDefault(i => i.id == actionid);
            if (attack is not null)
            {
                Plugin.Logger.LogInfo($"Custom Skill: {attack.name} COST: {attack.cost} ID: {attack.id} ENTITY: {entity}");
                yield return attack.callback(entity, attack);
            }
            else
            {
                Plugin.Logger.LogInfo($"Non-Custom Skill: ID: {actionid} ENTITY: {entity}");
            }
        }
        public static void RefreshCustomMoves()
        {
            var tempskilldata = new string[Plugin.OG_skilldata.GetLength(0) + Skills.Count, 13];
            Array.Copy(Plugin.OG_skilldata, tempskilldata, Plugin.OG_skilldata.Length);
            var customindexattack = 0;
            for (var i = 0; i < tempskilldata.GetLength(0); i++)
            {
                if (tempskilldata[i, 0] is null)
                {
                    var fields = typeof(CustomAttack).GetFields();
                    Plugin.Logger.LogInfo($"Adding to SkillData: {fields[0].GetValue(Skills[customindexattack])}");
                    for (var i2 = 0; i2 < fields.Length - 2; i2++)
                    {
                        tempskilldata[i, i2] = fields[i2].GetValue(Skills[customindexattack]).ToString();
                    }
                    fields[13].SetValue(Skills[customindexattack], i);
                    customindexattack++;
                }
            }
            MainManager.skilldata = tempskilldata;
        }
        public static bool AddToSkills(CustomAttack attack)
        {
            Skills.Add(attack);
            return true;
        }
    }
}
