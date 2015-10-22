using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ParaVayne
{
    class Program
    {
        static Menu menu;

        static float lastaa;

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        static void Loading_OnLoadingComplete(EventArgs args)
        {
            menu=MainMenu.AddMenu("ParaVayne","paravayne");
            menu.Add("combo",new KeyBind("Combo",false,KeyBind.BindTypes.HoldActive,' '));
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Game.OnTick += Game_OnTick;
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
                var target = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 200f,DamageType.Physical);
                switch (target.IsValidTarget() && !target.IsZombie)
                {
                    case true:
                    {
                        switch (CanAttack)
                        {
                            case true:
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            }
                            break;
                            case false:
                            {
                            if (CanMove)
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
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

        static bool CanAttack
        {
            get
            {
                return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 180f;
            }
        }

        static bool CanMove
        {
            get
            {
                return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000 - 40f;
            }
        }
    }
}