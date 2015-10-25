using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;

namespace ParaJinx
{
    class Program
    {
        static Menu menu;
        
        static System.Collections.Generic.IEnumerator<AIHeroClient> i;

        static System.Collections.Generic.IEnumerator<AIHeroClient> j;

        static System.Collections.Generic.IEnumerator<AIHeroClient> k;

        static System.Collections.Generic.IEnumerator<AIHeroClient> l;
		
        static bool Blitz { get { return EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank") == null; } }
        
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 1500);
        
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 100)
        {
            AllowedCollisionCount = 0, MinimumHitChance = HitChance.High
        };
        
        static readonly Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1)
        {
            AllowedCollisionCount = 100, MinimumHitChance = HitChance.Low
        };
        
        static bool BlitzGrabOnTarget;
        
        static float lastaa, blitzgrab;
        
        static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
        
        static bool AttackIsDone { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000; } }
        
        static bool TargetsQ(AIHeroClient unit) { return EntityManager.Heroes.Enemies.Count(x=>x.IsValidTarget(1500f) && x.Distance(unit) < 250f && !x.IsZombie) >= 2; }
        
        static bool NormalRange { get { return ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525
        	                                        || (ushort)ObjectManager.Player.AttackRange == 526); } }
        
        static bool CanNotMove(AIHeroClient target)
        {
            return (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup)
        	        || target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback)
        	        || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) || target.IsStunned);
        }
        
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Blitz)
            {
                var blitzcrank = EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank");
                if (sender==blitzcrank && (args.SData.Name=="RocketGrab"||args.SData.Name=="RocketGrabMissile"))
                {
                    blitzgrab = Game.Time * 1000;
                }
            }
        }

        static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            for (l = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(1200f)).GetEnumerator(); l.MoveNext();)
            {
                var enemy = l.Current;
                BlitzGrabOnTarget |= enemy == sender && (args.Buff.Name == "Stun" || args.Buff.Name == "rocketgrab2");
            }
        }

        static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLower() == "jinx")
            {
                menu=MainMenu.AddMenu("ParaJinx","parajinx");
                menu.Add("combo",new KeyBind("Combo",false,KeyBind.BindTypes.HoldActive,' '));
                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Game.OnTick += Spells;
                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
                Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Chat.Print("<font color=\"#00BFFF\">Para </font>Jinx<font color=\"#000000\"> by Paranoid </font> - <font color=\"#FFFFFF\">Loaded</font>");
            }
        }
        
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (E.IsReady())
            {
                for (k = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(600f)).GetEnumerator(); k.MoveNext();)
                {
                    var enemy = k.Current;
                    var s = args.SData.Name.ToLower();
                    if (enemy == sender && (s == "katarinar" || s == "drain" || s == "consume" || s == "absolutezero" || s == "volibearqattack" || s == "staticfield"
                                            || s == "reapthewhirlwind" || s == "jinxw" || s == "jinxr" || s == "shenstandunited" || s == "threshe" || s == "threshrpenta"
                                            || s == "threshq" || s == "meditate" || s == "caitlynpiltoverpeacemaker" || s == "cassiopeiapetrifyinggaze"
                                            || s == "ezrealtrueshotbarrage" || s == "galioidolofdurand" || s == "luxmalicecannon" || s == "missfortunebullettime"
                                            || s == "infiniteduress" || s == "alzaharnethergrasp" || s == "velkozr"))
                    {
                        E.Cast(enemy.Position);
                    }
                }
            }
        }
		
        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe) { lastaa = Game.Time * 1000; }
        }

        static void Spells(EventArgs args)
        {
            if (BlitzGrabOnTarget && Game.Time * 1000 > blitzgrab + 1000f)
            {
                BlitzGrabOnTarget = false;
            }
            if (E.IsReady())
            {
                Elogic();
            }
            var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 120f,DamageType.Physical);
            if(menu["combo"].Cast<KeyBind>().CurrentValue)
            {
                switch (unit.IsValidTarget() && !unit.IsZombie)
                {
                    case true:
                    {
                        if (Q.IsReady())
                        {
                            switch (TargetsQ(unit))
                            {
                                case true:
                                {
                                    if (NormalRange && !CanAttack && AttackIsDone)
                                    {
                                        Q.Cast();
                                    }
                                }
                                break;
                                case false:
                                {
                                    if (unit.Distance(ObjectManager.Player)<=600f && ObjectManager.Player.AttackRange>550f && !CanAttack && AttackIsDone)
                                    {
                                        Q.Cast();
                                    }
                                    else if (unit.Distance(ObjectManager.Player)>600f && NormalRange && !CanAttack && AttackIsDone)
                                    {
                                        Q.Cast();
                                    }
                                }
                                break;
                            }
                        }
                        if (W.IsReady())
                        {
                            if (unit.Distance(ObjectManager.Player)>200f && !CanAttack && AttackIsDone)
                            {
                                W.AllowedCollisionCount=0;
                                W.Cast(unit);
                            }
                        }
                    }
                    break;
                    case false:
                    {
                        var unit2 = TargetSelector.GetTarget(1500f,DamageType.Physical);
                        switch (unit2.IsValidTarget() && !unit2.IsZombie)
                        {
                            case true:
                            {
                                if (Q.IsReady())
                                {
                                    if (NormalRange)
                                    {
                                        Q.Cast();
                                    }
                                }
                                if (W.IsReady())
                                {
                                    if (unit2.Distance(ObjectManager.Player)>200f)
                                    {
                                        W.AllowedCollisionCount=0;
                                        W.Cast(unit2);
                                    }
                                }
                            }
                            break;
                            case false:
                            {
                                if (Q.IsReady())
                                {
                                    if (ObjectManager.Player.AttackRange>550f)
                                    {
                                        Q.Cast();
                                    }
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        static void Elogic()
        {
            switch (Blitz)
            {
                case false:
                {
                    var blitzcrank = EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank");
                    if (BlitzGrabOnTarget && blitzcrank.Distance(ObjectManager.Player)<2500f)
                    {
                        E.Cast(blitzcrank.Position);
                    }
                    else if (Game.Time * 1000 > blitzgrab + 1000f)
                    {
                        for (i = EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range).GetEnumerator(); i.MoveNext();)
                        {
                            var enemy = i.Current;
                            if (enemy.HasBuff("teleport_target") || enemy.HasBuff("Pantheon_GrandSkyfall_Jump"))
                            {
                                E.Cast(enemy.Position);
                            }
                            else if (enemy.IsValidTarget() && CanNotMove(enemy))
                            {
                                E.Cast(enemy.Position);
                            }
                        }
                    }
                }
                break;
                case true:
                {
                    for (j = EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range).GetEnumerator(); j.MoveNext();)
                    {
                        var enemy = j.Current;
                        if (enemy.HasBuff("teleport_target") || enemy.HasBuff("Pantheon_GrandSkyfall_Jump"))
                        {
                            E.Cast(enemy.Position);
                        }
                        else if (enemy.IsValidTarget() && CanNotMove(enemy))
                        {
                            E.Cast(enemy.Position);
                        }
                    }
                }
                break;
            }
        }
    }
}