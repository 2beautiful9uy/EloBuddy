using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using System.Collections.Generic;

namespace ParaJinx
{
    class Program
    {
        static Menu menu, qm, wm;
        
        static IEnumerator<AIHeroClient> i;

        static IEnumerator<AIHeroClient> j;

        static IEnumerator<AIHeroClient> k;

        static IEnumerator<AIHeroClient> l;

        static IEnumerator<AIHeroClient> n;
		
        static bool Blitz { get { return EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank") == null; } }
        
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 1500);
        
        static readonly Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1)
        {
            AllowedCollisionCount = 100, MinimumHitChance = HitChance.Low
        };
        
        static bool BlitzGrabOnTarget;
        
        static float lastaa, blitzgrab;
        
        static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
        
        static bool AttackIsDone { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000; } }
        
        static bool TargetsQ(AIHeroClient unit) { return EntityManager.Heroes.Enemies.Count(x=>x.IsValidTarget(1500f) && x.Distance(unit) < 250f && !x.IsZombie) >= qtargets; }
        
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
                menu.AddSeparator();
                menu.AddGroupLabel("Para Jinx");
                menu.AddSeparator();
                menu.AddLabel("made by Paranoid");
                qm = menu.AddSubMenu("Q Config", "qconfig");
                    qm.Add("qcombo", new CheckBox("Q Combo"));
                    qm.AddSeparator();
                    qm.Add("qtargets", new Slider("Q if splash can hit [x] targets", 3, 0, 5));
                wm = menu.AddSubMenu("W Config", "wconfig");
                    wm.Add("wcombo", new CheckBox("W combo"));
                    wm.AddSeparator();
                    wm.Add("wks", new CheckBox("W KS"));
                    wm.AddSeparator();
                    wm.Add("wrange", new Slider("Minimum range to use W", 300, 0, 1500));
                    wm.AddSeparator();
                    wm.AddGroupLabel("W On:");
                    foreach (var enemy in EntityManager.Heroes.Enemies)
                        wm.Add(enemy.ChampionName, new CheckBox(enemy.ChampionName));
                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Game.OnTick += Spells;
                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
                Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Chat.Print("<font color=\"#00BFFF\">Para </font>Jinx<font color=\"#000000\"> by Paranoid </font> - <font color=\"#FFFFFF\">Loaded</font>");
            }
        }
        
        static bool wcombo { get { return wm["wcombo"].Cast<CheckBox>().CurrentValue; } }
        
        static bool wks { get { return wm["wks"].Cast<CheckBox>().CurrentValue; } }
        
        static bool qcombo { get { return qm["qcombo"].Cast<CheckBox>().CurrentValue; } }
        
        static float wrange { get { return wm["wrange"].Cast<Slider>().CurrentValue; } }
        
        static int qtargets { get { return qm["qtargets"].Cast<Slider>().CurrentValue; } }
        
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
            if(menu["combo"].Cast<KeyBind>().CurrentValue)
            {
                if (qcombo)
                {
                    Qlogic();
                }
                if (wcombo)
                {
                    Wlogic();
                }
            }
        }
        
        static void Qlogic()
        {
            var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 120f,DamageType.Physical);
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
        
        static void Wlogic()
        {
            for (n = EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < 1500f && !x.IsZombie).GetEnumerator(); n.MoveNext();)
            {
                var target = n.Current;
                if (target.IsValidTarget(1500f) && target.Distance(ObjectManager.Player) > wrange && wks && target.Health < GetKsDamageW(target))
                {
                    Wcast(target);
                }
                else if (wm[target.ChampionName].Cast<CheckBox>().CurrentValue)
                {
                    if (target.IsValidTarget(ObjectManager.Player.AttackRange + 100f) && target.Distance(ObjectManager.Player) > wrange && !CanAttack && AttackIsDone)
                    {
                        Wcast(target);
                    }
                    else if (target.IsValidTarget(1500f) && target.Distance(ObjectManager.Player) > wrange)
                    {
                        Wcast(target);
                    }
                }
            }
        }
        
        static void Wcast(AIHeroClient unit)
        {
            var W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 90);
            var predictedPositions = new Dictionary<int, Tuple<int, PredictionResult>>();
            var prediction = W.GetPrediction(unit);
            if (W.IsReady())
            {
                predictedPositions[unit.NetworkId] = new Tuple<int, PredictionResult>(Environment.TickCount, prediction);
                if (prediction.HitChance >= HitChance.High)
                {
                    W.Cast(prediction.CastPosition);
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
        
        static float Wdamage(AIHeroClient unit)
        {
            var W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 90);
            return ObjectManager.Player.CalculateDamageOnUnit(unit, DamageType.Physical, (float)(new [] {10, 60, 110, 160, 210}[W.Level - 1] + 1.4*(ObjectManager.Player.TotalAttackDamage)));
        }
        
        static float GetKsDamageW(AIHeroClient unit)
        {
            var wdmg = Wdamage(unit);
            if (ObjectManager.Player.HasBuff("summonerexhaust"))
            {
                wdmg = wdmg * 0.6f;
            }
            if (ObjectManager.Player.HasBuff("ferocioushowl"))
            {
                wdmg = wdmg * 0.7f;
            }

            if (unit == EntityManager.Heroes.Enemies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank"))
            {
                if (!unit.HasBuff("BlitzcrankManaBarrierCD") && !unit.HasBuff("ManaBarrier"))
                {
                    wdmg -= unit.Mana / 2f;
                }
            }

            wdmg -= unit.HPRegenRate;
            wdmg -= unit.PercentLifeStealMod * 0.005f * unit.FlatPhysicalDamageMod;

            return wdmg;
        }
    }
}