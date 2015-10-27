using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;

namespace ParaJinx {
    class Program {
        public static void Main(string[] args) { Loading.OnLoadingComplete += Loading_OnLoadingComplete; }
        
        
        // MENU
        static Menu menu, qm, wm;
        static void Loading_OnLoadingComplete(EventArgs args) {
            if (ObjectManager.Player.ChampionName.ToLower() == "jinx") {
                menu=MainMenu.AddMenu("ParaJinx","parajinx");
                menu.Add("combo",new KeyBind("Combo",false,KeyBind.BindTypes.HoldActive,' '));
                menu.AddSeparator();
                menu.AddGroupLabel("Para Jinx");
                menu.AddSeparator();
                menu.AddLabel("made by Paranoid");
                qm = menu.AddSubMenu("Q Config", "qconfig");
                    qm.AddGroupLabel("Q combo:");
                    qm.Add("qcombo", new CheckBox("Q Combo"));
                    qm.Add("qtargets", new Slider("Q if splash can hit [x] targets", 3, 0, 5));
                wm = menu.AddSubMenu("W Config", "wconfig");
                    wm.AddGroupLabel("Prediction:");
                    wm.Add("whit", new Slider("Minimum Hitchance", 80, 1, 100));
                    wm.Add("wpred", new Slider("Prediction Mode: [1]=target [2]=pred.castposition", 2, 1, 2));
                    wm.AddGroupLabel("W combo On: [ks = all champions] ");
                    foreach (var enemy in EntityManager.Heroes.Enemies)
                        wm.Add(enemy.ChampionName, new CheckBox(enemy.ChampionName));
                    wm.AddGroupLabel("W combo and ks");
                    wm.Add("wcombo", new CheckBox("W combo"));
                    wm.Add("wks", new CheckBox("W KS"));
                    wm.Add("waa", new Slider("In attack range -> Don't use W if can kill target in [x] auto attacks", 2, 2, 6));
                    wm.Add("wrange", new Slider("Minimum range to use W", 600, 300, 900));
                Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
                Game.OnUpdate += Spells;
                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
                Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Chat.Print("<font color=\"#00BFFF\">Para </font>Jinx<font color=\"#000000\"> by Paranoid </font> - <font color=\"#FFFFFF\">Loaded</font>"); } }
        
        
        // ON UPDATE
        static void Spells(EventArgs args) {
            if (W.IsReady() && wm["wks"].Cast<CheckBox>().CurrentValue) Wks();
            if (BlitzGrabOnTarget && Game.Time * 1000 > blitzgrab + 1000f) BlitzGrabOnTarget = false;
            if (E.IsReady()) Elogic();
            if(menu["combo"].Cast<KeyBind>().CurrentValue) {
                if (Q.IsReady() && qm["qcombo"].Cast<CheckBox>().CurrentValue) Qcombo();
                if (W.IsReady() && wm["wcombo"].Cast<CheckBox>().CurrentValue) Wcombo(); } }
        
        
        // Q, W COMBO
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 1500);
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 60);
        static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if (sender.IsMe) lastaa = Game.Time * 1000; }
        static bool NormalRange { get { return ((ushort)ObjectManager.Player.AttackRange == 524 || (ushort)ObjectManager.Player.AttackRange == 525
        	                                        || (ushort)ObjectManager.Player.AttackRange == 526); } }
        static float lastaa;
        static bool CanAttack { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackDelay * 1000 - 150f; } }
        static bool AttackIsDone { get { return Game.Time * 1000 > lastaa + ObjectManager.Player.AttackCastDelay * 1000; } }
        static bool TargetsQ(AIHeroClient unit) { return EntityManager.Heroes.Enemies.Count(x=>x.IsValidTarget(1500f) && x.Distance(unit) < 250f && !x.IsZombie) >= qm["qtargets"].Cast<Slider>().CurrentValue; }
        static void Qcombo() {
            var unit = TargetSelector.GetTarget(ObjectManager.Player.AttackRange + 130f,DamageType.Physical);
            switch (unit.IsValidTarget() && !unit.IsZombie) {
                case true:
                    switch (TargetsQ(unit)) {
                        case true:
                            if (NormalRange && !CanAttack && AttackIsDone && Q.Cast()) return; break;
                        case false:
                            if (unit.Distance(ObjectManager.Player) <= 600f && ObjectManager.Player.AttackRange > 550f && !CanAttack && AttackIsDone && Q.Cast()) return;
                            if (unit.Distance(ObjectManager.Player) > 600f && NormalRange && !CanAttack && AttackIsDone && Q.Cast()) return; break; } break;
                case false:
                    var unit2 = TargetSelector.GetTarget(1500f,DamageType.Physical);
                    switch (unit2.IsValidTarget() && !unit2.IsZombie) {
                        case true:
                            if (NormalRange && Q.Cast()) return; break;
                        case false:
                            if (ObjectManager.Player.AttackRange > 550f && Q.Cast()) return; break; } break; } }
        static void Wcombo() { var target = TargetSelector.GetTarget(1450f,DamageType.Physical); if (target.Distance(ObjectManager.Player) > wm["wrange"].Cast<Slider>().CurrentValue && wm[target.ChampionName].Cast<CheckBox>().CurrentValue) WCast(target); }
        static void Wks() { foreach (var enemy in EntityManager.Heroes.Enemies.Where(x=>x.IsValidTarget(1450f) && !x.IsZombie && x.Distance(ObjectManager.Player) > wm["wrange"].Cast<Slider>().CurrentValue && x.Health < ObjectManager.Player.CalculateDamageOnUnit(x, DamageType.Physical, (float)(new [] {10, 60, 110, 160, 210}[W.Level - 1] + 1.4*(ObjectManager.Player.TotalAttackDamage))))) WCast(enemy); }
        static void WCast(AIHeroClient unit) {
            var pred = W.GetPrediction(unit);
            switch (wm["wpred"].Cast<Slider>().CurrentValue) {
                case 1:
                    if (unit.Distance(ObjectManager.Player) <= ObjectManager.Player.AttackRange + 170f && unit.Health > wm["waa"].Cast<Slider>().CurrentValue * ObjectManager.Player.CalculateDamageOnUnit(unit, DamageType.Physical, (float)(1.1 * ObjectManager.Player.TotalAttackDamage)) && !CanAttack && AttackIsDone && pred.HitChancePercent>=wm["whit"].Cast<Slider>().CurrentValue && W.Cast(unit)) return;
                    if (unit.Distance(ObjectManager.Player) > ObjectManager.Player.AttackRange + 170f && pred.HitChancePercent>=wm["whit"].Cast<Slider>().CurrentValue && W.Cast(unit)) return; break;
                case 2:
                    if (unit.Distance(ObjectManager.Player) <= ObjectManager.Player.AttackRange + 170f && unit.Health > wm["waa"].Cast<Slider>().CurrentValue * ObjectManager.Player.CalculateDamageOnUnit(unit, DamageType.Physical, (float)(1.1 * ObjectManager.Player.TotalAttackDamage)) && !CanAttack && AttackIsDone && pred.HitChancePercent>=wm["whit"].Cast<Slider>().CurrentValue && W.Cast(pred.CastPosition)) return;
                    if (unit.Distance(ObjectManager.Player) > ObjectManager.Player.AttackRange + 170f && pred.HitChancePercent>=wm["whit"].Cast<Slider>().CurrentValue && W.Cast(pred.CastPosition)) return; break; } }

        
        // E LOGIC
        static readonly Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1);
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(500f))) {
                var s = args.SData.Name.ToLower();
                if (enemy == sender && (s == "katarinar" || s == "drain" || s == "crowstorm" || s == "absolutezero" || s == "reapthewhirlwind" || s == "shenstandunited" || s == "meditate" || s == "galioidolofdurand"
				                || s == "infiniteduress" || s == "alzaharnethergrasp" || s == "velkozr") && E.Cast(enemy.Position)) return; } }
        static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args) {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(2500f))) BlitzGrabOnTarget |= enemy == sender && (args.Buff.Name == "Stun" || args.Buff.Name == "rocketgrab2"); }
        static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if (!Blitz) {
                var blitzcrank = EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank");
                if (sender == blitzcrank && (args.SData.Name == "RocketGrab" || args.SData.Name == "RocketGrabMissile")) blitzgrab = Game.Time * 1000; } }       
        static bool CanNotMove(AIHeroClient target) {
            return (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup)
        	        || target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback)
        	        || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) || target.IsStunned); }
        static float blitzgrab;
        static bool BlitzGrabOnTarget;
        static bool Blitz { get { return EntityManager.Heroes.Allies.FirstOrDefault(x=>x.ChampionName == "Blitzcrank") == null; } }
        static void Elogic() {
            switch (Blitz) {
                case false:
                    var blitzcrank = EntityManager.Heroes.Allies.FirstOrDefault(x => x.ChampionName == "Blitzcrank");
                    if (BlitzGrabOnTarget && blitzcrank.Distance(ObjectManager.Player) < 2500f && E.Cast(blitzcrank.Position)) return;
                    if (Game.Time * 1000 > blitzgrab + 1000f)
                        foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range)) {
                            if (enemy.HasBuff("teleport_target") || enemy.HasBuff("Pantheon_GrandSkyfall_Jump") && E.Cast(enemy.Position)) return;
                            if (enemy.IsValidTarget() && CanNotMove(enemy) && E.Cast(enemy.Position)) return; }
                break;
                case true:
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(x => x.Distance(ObjectManager.Player) < E.Range)) {
                        if (enemy.HasBuff("teleport_target") || enemy.HasBuff("Pantheon_GrandSkyfall_Jump") && E.Cast(enemy.Position)) return;
                        if (enemy.IsValidTarget() && CanNotMove(enemy) && E.Cast(enemy.Position)) return; }
                break; } } } }