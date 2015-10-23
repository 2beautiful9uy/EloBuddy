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
        
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 60)
        {
            AllowedCollisionCount = 0, MinimumHitChance = HitChance.High
        };
            
            static float lastaa;
            
            static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
            
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
                    menu.Add("cancel", new Slider("if you have aa cancel change 0", 0, 0, 30));
                    Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                    Game.OnTick += Game_OnTick;
                    Game.OnTick += Spells;
                    Chat.Print(ObjectManager.Player.ChampionName+" Loaded");
                }
            }
            
            static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                if (sender.IsMe) { lastaa = Game.Time * 1000; }
            }

            static void Game_OnTick(EventArgs args)
            {
                if(menu["combo"].Cast<KeyBind>().CurrentValue)
                {
                    var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 200f,DamageType.Physical);
                    switch (unit.IsValidTarget() && !unit.IsZombie)
                  {
                      case true:
                      {
                          switch (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                          {
                              case true:
                              {
                                  Player.IssueOrder(GameObjectOrder.AttackUnit, unit);
                              }
                              break;
                              case false:
                              {
                                if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 70f + (float)(menu["cancel"].Cast<Slider>().CurrentValue))
                                  {
                                      Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                                  }
                              }
                              break;
                          }
                      }
                      break;
                      case false:
                      {
                          Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                      }
                      break;
                  }
                }
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
                        if (!CanAttack)
                        {
                            if (Q.IsReady())
                            {
                                if (unit.Distance(ObjectManager.Player)<=650f && ObjectManager.Player.AttackRange>550f)
                                {
                                    Core.DelayAction(Qcast, (int)(ObjectManager.Player.AttackCastDelay * 1000)-50);
                                }
                                else if (unit.Distance(ObjectManager.Player)>650f && ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525 || (ushort)ObjectManager.Player.AttackRange == 526))
                                {
                                    Core.DelayAction(Qcast, (int)(ObjectManager.Player.AttackCastDelay * 1000)-50);
                                }
                            }
                            if (W.IsReady())
                            {
                                if (unit.Distance(ObjectManager.Player)>200f)
                                {
                                    Core.DelayAction(Wcast, (int)(ObjectManager.Player.AttackCastDelay * 1000)-50);
                                }
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
        
        static void Qcast()
        {
            Q.Cast();
        }
        
        static void Wcast()
        {
            var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 120f,DamageType.Physical);
            if (unit.IsValidTarget() && !unit.IsZombie && unit.Distance(ObjectManager.Player)>200f)
            {
                W.Cast(unit);
            }
        }
    }
}