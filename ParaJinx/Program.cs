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
        
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 1500);
        
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 100)
        {
            AllowedCollisionCount = 0, MinimumHitChance = HitChance.High
        };
            
        static float lastaa;
        
        static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
        
        static bool CanMove { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000; } }

        
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        
        static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLower() == "jinx")
            {
                menu=MainMenu.AddMenu("ParaJinx","parajinx");
                menu.Add("combo",new KeyBind("Combo",false,KeyBind.BindTypes.HoldActive,' '));
                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Game.OnTick += Spells;
                Chat.Print(ObjectManager.Player.ChampionName+" Loaded");
            }
        }
        
        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe) { lastaa = Game.Time * 1000; }
        }

        static void Spells(EventArgs args)
        {
            var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 120f,DamageType.Physical);
            if(menu["combo"].Cast<KeyBind>().CurrentValue)
            {
                switch (unit.IsValidTarget() && !unit.IsZombie)
                {
                    case true:
                    {
                        if (Q.IsReady())
                        {
                            if (unit.Distance(ObjectManager.Player)<=600f && ObjectManager.Player.AttackRange>550f && !CanAttack && CanMove)
                            {
                                Q.Cast();
                            }
                            else if (unit.Distance(ObjectManager.Player)>600f && ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525 || (ushort)ObjectManager.Player.AttackRange == 526) && CanMove && !CanAttack)
                            {
                                Q.Cast();
                            }
                        }
                        if (W.IsReady())
                        {
                            if (unit.Distance(ObjectManager.Player)>200f && CanMove && !CanAttack)
                            {
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
                                    if ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525 || (ushort)ObjectManager.Player.AttackRange == 526)
                                    {
                                        Q.Cast();
                                    }
                                }
                                if (W.IsReady())
                                {
                                    if (unit2.Distance(ObjectManager.Player)>200f)
                                    {
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
    }
}