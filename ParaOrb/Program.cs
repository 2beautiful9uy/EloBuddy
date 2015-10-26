using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ParaOrb
{
    class Program
    {
        static Menu menu;
        
        static float lastaa;
        
        static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
        
        static string Name { get { return ObjectManager.Player.ChampionName.ToLower(); } }
        
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        
        static void Loading_OnLoadingComplete(EventArgs args)
        {
            menu=MainMenu.AddMenu("ParaOrb","paraorb");
            menu.Add("combo",new KeyBind("Combo",false,KeyBind.BindTypes.HoldActive,' '));
            menu.Add("cancel", new Slider("If you have aa cancel change slider to 10 or higher [I have 0 and works perfect TESTED]", 0, 0, 50));
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Game.OnTick += Game_OnTick;
            Chat.Print("<font color=\"#00BFFF\">Para </font>Orb<font color=\"#000000\"> by Paranoid </font> - <font color=\"#FFFFFF\">Loaded</font>");
        }
        
        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                lastaa = Game.Time * 1000;
            }
        }

        static void Game_OnTick(EventArgs args)
        {
            if(menu["combo"].Cast<KeyBind>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 160f,DamageType.Physical);
                switch (ObjectManager.Player.ChampionName.ToLower())
                {
                    case "kalista":
                    {
                        switch (target.IsValidTarget() && !target.IsZombie)
                        {
                            case true:
                            {
                                switch (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                                {
                                    case true:
                                    {
                                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                                    }
                                    break;
                                    case false:
                                    {
                                        if (Game.Time * 1000 > lastaa + 1)
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
                    break;
                    case "ezreal":
                    {
                        Orb(75f, target);
                    }
                    break;
                    case "vayne":
                    {
                        Orb(65f, target);
                    }
                    break;
                    case "jinx":
                    {
                        var Q = new Spell.Active(SpellSlot.Q);
                        var qt = TargetSelector.GetTarget(685f + 50f + 25f * Q.Level, DamageType.Physical);
                        var nt = TargetSelector.GetTarget(655f, DamageType.Physical);
                        if (ObjectManager.Player.AttackRange>550f)
                        {
                            switch (qt.IsValidTarget() && !qt.IsZombie)
                            {
                                case true:
                                {
                                    switch (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                                    {
                                        case true:
                                        {
                                            Player.IssueOrder(GameObjectOrder.AttackUnit, qt);
                                        }
                                        break;
                                        case false:
                                        {
                                            if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 60f + (float)(menu["cancel"].Cast<Slider>().CurrentValue))
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
                        else if ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525 || (ushort)ObjectManager.Player.AttackRange == 526)
                        {
                            switch (nt.IsValidTarget() && !nt.IsZombie)
                            {
                                case true:
                                {
                                    switch (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f)
                                    {
                                        case true:
                                        {
                                            Player.IssueOrder(GameObjectOrder.AttackUnit, nt);
                                        }
                                        break;
                                        case false:
                                        {
                                            if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 60f + (float)(menu["cancel"].Cast<Slider>().CurrentValue))
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
                    break;
                }
                if (Name == "jinx" || Name == "vayne" || Name == "ezreal" || Name == "kalista")
                {
                    return;
                }
                Orb(50f, target);
            }
        }
        
        static void Orb(float move, AIHeroClient unit)
        {
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
                        	if (Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - move + (float)(menu["cancel"].Cast<Slider>().CurrentValue))
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
}