using System;
using System.Linq;
using System.Drawing;
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
        static Menu menu, qm, wm, om;
        
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 1500);
        
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3200, 80);
        
        static readonly Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Circular, 1200, 1750, 1);
        
        static readonly AIHeroClient blitz = EntityManager.Heroes.Allies.FirstOrDefault(x => x.ChampionName == "Blitzcrank");
        
        static bool CanAttack
        {
            get
            {
                return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f;
            }
        }
        
        static bool AttackIsDone
        {
            get
            {
                return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000;
            }
        }
        
        static float blitzgrab, lastaa;
        
        static bool BlitzGrabOnTarget;
        
        static int wcount, wontarget;
        
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLower() == "jinx")
            {
                menu = MainMenu.AddMenu("ParaJinx", "parajinx");
                menu.Add("combo", new KeyBind("Combo", false, KeyBind.BindTypes.HoldActive, ' '));
                menu.AddSeparator();
                menu.AddGroupLabel("Para Jinx");
                menu.AddSeparator();
                menu.AddLabel("made by Paranoid");
                om = menu.AddSubMenu("Orb Config", "orbconfig");
                om.AddGroupLabel("ParaOrb Settings:");
                om.Add("useorb", new CheckBox("ParaOrb"));
                om.Add("cancel", new Slider("If you have aa cancel", 0, 0, 10));
                qm = menu.AddSubMenu("Q Config", "qconfig");
                qm.AddGroupLabel("Q combo:");
                qm.Add("qcombo", new CheckBox("Q Combo"));
                qm.Add("qtargets", new Slider("Q if splash can hit [x] targets", 3, 0, 5));
                wm = menu.AddSubMenu("W Config", "wconfig");
                wm.Add("wtest", new CheckBox("Draw -> % W accuracy"));
                wm.Add("wcombo", new CheckBox("W combo"));
                Game.OnUpdate += Game_OnUpdate;
                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
                Drawing.OnDraw += Drawing_OnDraw;
                Chat.Print("<font color=\"#00BFFF\">Para </font>Jinx<font color=\"#000000\"> by Paranoid </font> - <font color=\"#FFFFFF\">Loaded</font>");
            }
        }
            
        static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                if (Orbwalker.DisableAttacking)
                {
                    Orbwalker.DisableAttacking = false;
                }
                if (Orbwalker.DisableMovement)
                {
                    Orbwalker.DisableMovement = false;
                }
            }
            
            if (BlitzGrabOnTarget && Game.Time * 1000 > blitzgrab + 1000f)
            {
                BlitzGrabOnTarget = false;
            }
            
            // E
            if (E.IsReady())
            {
                if (blitz == null)
                {
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range))
                    {
                        if (enemy.IsValidTarget() && (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) || enemy.HasBuffOfType(BuffType.Knockup) || enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) || enemy.HasBuffOfType(BuffType.Knockback) || enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression)))
                        {
                            E.Cast(enemy.Position);
                        }
                    }
                }
                else
                {
                    if (BlitzGrabOnTarget && blitz.Distance(ObjectManager.Player) < 2500f)
                    {
                        E.Cast(blitz.Position);
                    }
                    if (Game.Time * 1000 > blitzgrab + 1000f)
                    {
                        foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range))
                        {
                            if (enemy.IsValidTarget() && (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) || enemy.HasBuffOfType(BuffType.Knockup) || enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) || enemy.HasBuffOfType(BuffType.Knockback) || enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression)))
                            {
                              E.Cast(enemy.Position);
                            }
                        }
                    }
                }
            }
            // END
            
            if (menu["combo"].Cast<KeyBind>().CurrentValue)
            {
                // Q
                if (Q.IsReady() && qm["qcombo"].Cast<CheckBox>().CurrentValue)
                {
                    var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 100f, DamageType.Physical);
                    if (unit.IsValidTarget() && !unit.IsZombie)
                    {
                        if (EntityManager.Heroes.Enemies.Count(x => x.IsValidTarget(1500f) && x.Distance(unit) < 250f && !x.IsZombie) >= qm["qtargets"].Cast<Slider>().CurrentValue)
                        {
                            if (!(ObjectManager.Player.AttackRange>525) && !CanAttack && AttackIsDone)
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            if (unit.Distance(ObjectManager.Player) <= 600f && ObjectManager.Player.AttackRange>525 && !CanAttack && AttackIsDone)
                            {
                                Q.Cast();
                            }
                            if (unit.Distance(ObjectManager.Player) > 600f && !(ObjectManager.Player.AttackRange>525) && !CanAttack && AttackIsDone)
                            {
                                Q.Cast();
                            }
                        }
                    }
                    else
                    {
                        var unit2 = TargetSelector.GetTarget(1500f, DamageType.Physical);
                        if (unit2.IsValidTarget() && !unit2.IsZombie)
                        {
                            if (!(ObjectManager.Player.AttackRange>525))
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            if (ObjectManager.Player.AttackRange>525)
                            {
                                Q.Cast();
                            }
                        }
                    }
                }
                // END
                
                // W
                if (W.IsReady() && wm["wcombo"].Cast<CheckBox>().CurrentValue)
                {
                    var target = TargetSelector.GetTarget(1450f, DamageType.Physical);
                    if (target.IsValidTarget() && target.Distance(ObjectManager.Player) > 475f && target.Distance(ObjectManager.Player) < 730f + 25f * Q.Level && !CanAttack && AttackIsDone && target.Health > 3 * ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Physical, (float)(1.1 * ObjectManager.Player.TotalAttackDamage)))
                    {
                        W.Cast(target);
                    }
                    else if (target.IsValidTarget() && target.Distance(ObjectManager.Player) > 475f && target.Distance(ObjectManager.Player) > 730f + 25f * Q.Level)
                    {
                        W.Cast(target);
                    }
                }
                // END
                
                // Para Orb
                if (om["useorb"].Cast<CheckBox>().CurrentValue)
                {
                    if (!Orbwalker.DisableAttacking)
                    {
                        Orbwalker.DisableAttacking = true;
                    }
                    if (!Orbwalker.DisableMovement)
                    {
                        Orbwalker.DisableMovement = true;
                    }
                    var qt = TargetSelector.GetTarget(730f + 25f * Q.Level, DamageType.Physical);
                    var nt = TargetSelector.GetTarget(655f, DamageType.Physical);
                    if (ObjectManager.Player.AttackRange>525)
                    {
                        if (qt.IsValidTarget() && !qt.IsZombie)
                        {
                            if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, qt);
                            }
                            else
                            {
                                if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 30f + (float)(om["cancel"].Cast<Slider>().CurrentValue))
                                {
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }
                    else
                    {
                        if (nt.IsValidTarget() && !nt.IsZombie)
                        {
                            if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, nt);
                            }
                            else
                            {
                                if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 40f + (float)(om["cancel"].Cast<Slider>().CurrentValue))
                                {
                                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                }
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }
                }
                else
                {
                    if (Orbwalker.DisableAttacking)
                    {
                        Orbwalker.DisableAttacking = false;
                    }
                    if (Orbwalker.DisableMovement)
                    {
                        Orbwalker.DisableMovement = false;
                    }
                }
                // END
            }
        }
        
        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                lastaa = Game.Time * 1000;
            }
        }
        
        static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(2500f)))
            {
                if (enemy == sender)
                {
                    if (args.Buff.Name.ToLower() == "jinxwsight")
                    {
                        wontarget = wontarget + 1;
                    }
                  
                    if (args.Buff.Name == "Stun" || args.Buff.Name == "rocketgrab2")
                    {
                        BlitzGrabOnTarget = true;
                    }
                }
            }
        }
        
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(500f)))
            {
                var s = args.SData.Name.ToLower();
                if (enemy == sender && (s == "katarinar" || s == "drain" || s == "crowstorm" || s == "absolutezero" || s == "reapthewhirlwind" || s == "shenstandunited" || s == "meditate" || s == "galioidolofdurand" || s == "infiniteduress" || s == "alzaharnethergrasp" || s == "velkozr"))
                {
                    E.Cast(enemy.Position);
                }
            }
        }
        
        static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name.ToLower() == "jinxwmissile")
            {
                wcount = wcount + 1;
            }
            
            if (sender == blitz && (args.SData.Name == "RocketGrab" || args.SData.Name == "RocketGrabMissile"))
            {
                blitzgrab = Game.Time * 1000;
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (wm["wtest"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawText(Drawing.WorldToScreen(Player.Instance.Position).X, Drawing.WorldToScreen(Player.Instance.Position).Y, Color.Bisque, "Casted W: " + wcount + ". On target: " + wontarget + ".", 4);
                Drawing.DrawText(Drawing.WorldToScreen(Player.Instance.Position).X, Drawing.WorldToScreen(Player.Instance.Position).Y + 15, Color.Bisque, (wontarget * 100) / wcount + "% accuracy.", 4);
            }
        }
    }
}