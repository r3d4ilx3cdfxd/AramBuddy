using System;
using System.Collections.Generic;
using System.Linq;
using AramBuddy.MainCore.Logics.Casting;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Spells;
using GenesisSpellLibrary;
using GenesisSpellLibrary.Spells;
using static AramBuddy.Config;

namespace AramBuddy.MainCore.Logics
{
    internal class ModesManager
    {
        /// <summary>
        ///     Modes enum.
        /// </summary>
        public enum Modes
        {
            Flee,
            LaneClear,
            Harass,
            Combo,
            None
        }

        /// <summary>
        ///     Bot current active mode.
        /// </summary>
        public static Modes CurrentMode;

        /// <summary>
        ///     Gets the spells from the database.
        /// </summary>
        protected static SpellBase Spell => SpellManager.CurrentSpells;

        /// <summary>
        ///     List contains my hero spells.
        /// </summary>
        public static List<Spell.SpellBase> Spelllist = new List<Spell.SpellBase> { Spell.Q, Spell.W, Spell.E, Spell.R };

        public static void OnTick()
        {
            UpdateSpells();

            Orbwalker.DisableAttacking = Flee || None;

            if (Flee)
            {
                CurrentMode = Modes.Flee;
            }
            else if (LaneClear)
            {
                CurrentMode = Modes.LaneClear;
            }
            else if (Harass)
            {
                CurrentMode = Modes.Harass;
            }
            else if (None)
            {
                CurrentMode = Modes.None;
            }

            if (Combo)
            {
                CurrentMode = Modes.Combo;
            }

            // Modes Switch
            switch (CurrentMode)
            {
                case Modes.Combo:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                    break;
                case Modes.Flee:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Flee;
                    break;
                case Modes.Harass:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Harass;
                    break;
                case Modes.LaneClear:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
                    break;
                case Modes.None:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                    break;
                default:
                    Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                    break;
            }

            FlashAndGhost();

            if (!DisableSpellsCasting && !Program.CustomChamp)
            {
                ModesBase();
            }
        }

        public static void FlashAndGhost()
        {
            if (Flee && Player.Instance.PredictHealth() > 0)
            {
                if (SummonerSpells.Ghost.IsReady() && Program.SpellsMenu["Ghost"].Cast<CheckBox>().CurrentValue && SummonerSpells.Ghost.Slot != SpellSlot.Unknown
                    && Player.Instance.CountEnemyHeros(SafeValue) > 0)
                {
                    Logger.Send("Cast Ghost FleeMode CountEnemyHeros " + Player.Instance.CountEnemyHeros(SafeValue));
                    SummonerSpells.Ghost.Cast();
                }
                if (SummonerSpells.Flash.IsReady() && Program.SpellsMenu["Flash"].Cast<CheckBox>().CurrentValue && SummonerSpells.Flash.Slot != SpellSlot.Unknown
                    && (Player.Instance.PredictHealthPercent() < 20 && Player.Instance.CountEnemyHeros(SafeValue, Game.Ping + 250) > 0
                    || Player.Instance.CountEnemyHeros(SafeValue, Game.Ping + 250) - Player.Instance.CountEnemyHeros(SafeValue, Game.Ping + 250) > 2)
                    && ObjectsManager.AllySpawn != null)
                {
                    Logger.Send("Cast Flash FleeMode HealthPercent " + (int)Player.Instance.PredictHealthPercent());
                    Player.CastSpell(SummonerSpells.Flash.Slot, Player.Instance.PredictPosition().Extend(ObjectsManager.AllySpawn, 400).To3D());
                }
            }
        }
        
        /// <summary>
        ///     Update Spell values that needs to be updated.
        /// </summary>
        public static void UpdateSpells()
        {
            if (Player.Instance.Hero == Champion.AurelionSol)
            {
                Spell.E.Range = (uint)(2000 + Spell.E.Level * 1000);
            }
            if (Player.Instance.Hero == Champion.TahmKench)
            {
                Spell.R.Range = (uint)(3500 + Spell.R.Level * 1000);
            }
            if (Player.Instance.Hero == Champion.Ryze)
            {
                Spell.R.Range = (uint)(1500 * Spell.R.Level);
            }
        }

        /// <summary>
        ///     Casts Spells.
        /// </summary>
        public static void ModesBase()
        {
            /* Casting the summoner spells // No Longer Neaded
            if (Player.Instance.CountEnemyHeros(SafeValue) > 0 && Player.Instance.PredictHealthPercent() <= 25 && (Combo || Harass || Flee))
            {
                if (SummonerSpells.Heal.IsReady() && Program.SpellsMenu["Heal"].Cast<CheckBox>().CurrentValue && SummonerSpells.Heal.Slot != SpellSlot.Unknown)
                {
                    Logger.Send("Cast Heal HealthPercent " + (int)Player.Instance.PredictHealthPercent());
                    SummonerSpells.Heal.Cast();
                }
                else
                {
                    if (SummonerSpells.Barrier.IsReady() && Program.SpellsMenu["Barrier"].Cast<CheckBox>().CurrentValue && SummonerSpells.Barrier.Slot != SpellSlot.Unknown)
                    {
                        Logger.Send("Cast Barrier HealthPercent " + (int)Player.Instance.PredictHealthPercent());
                        SummonerSpells.Barrier.Cast();
                    }
                }
            }

            if (Player.Instance.ManaPercent < 50 && SummonerSpells.Clarity.IsReady() && Program.SpellsMenu["Clarity"].Cast<CheckBox>().CurrentValue && SummonerSpells.Clarity.Slot != SpellSlot.Unknown)
            {
                Logger.Send("Cast Clarity ManaPercent " + (int)Player.Instance.ManaPercent);
                SummonerSpells.Clarity.Cast();
            }
            if (SummonerSpells.Cleanse.IsReady() && Program.SpellsMenu["Cleanse"].Cast<CheckBox>().CurrentValue && SummonerSpells.Cleanse.Slot != SpellSlot.Unknown && Player.Instance.IsCC()
                && Player.Instance.CountEnemyHeros(SafeValue) > 0 && Player.Instance.PredictHealthPercent() <= 80)
            {
                Logger.Send("Cast Cleanse FleeMode Player CC'ed HealthPercent " + (int)Player.Instance.PredictHealthPercent() + " CountEnemyHeros " + Player.Instance.CountEnemyHeros(SafeValue));
                SummonerSpells.Cleanse.Cast();
            }*/

            if(CurrentMode == Modes.Combo && !Program.SpellsMenu.CheckBoxValue("combo"))
                return;
            if (CurrentMode == Modes.Harass && !Program.SpellsMenu.CheckBoxValue("harass"))
                return;
            if (CurrentMode == Modes.Flee && !Program.SpellsMenu.CheckBoxValue("flee"))
                return;
            if (CurrentMode == Modes.LaneClear && !Program.SpellsMenu.CheckBoxValue("laneclear"))
                return;

            foreach (var spell in Spelllist.Where(s => s != null && s.IsReady() && !s.IsSaver() && !s.IsTP()))
            {
                if (CurrentMode == Modes.Combo || (CurrentMode == Modes.Harass && (Player.Instance.ManaPercent > 60 || Player.Instance.ManaPercent.Equals(0))))
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget((spell as Spell.Chargeable)?.MaximumRange ?? spell.Range, spell.DamageType));
                }
                if (spell.Slot != SpellSlot.R)
                {
                    if (CurrentMode == Modes.LaneClear)
                    {
                        foreach (var minion in
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsValidTarget(spell.Range) && (Player.Instance.ManaPercent > 60 || Player.Instance.IsNoManaHero())))
                        {
                            SpellsCasting.Casting(spell, minion);
                        }
                    }
                }
                if (CurrentMode == Modes.Flee && spell.IsCC() && spell.IsReady())
                {
                    SpellsCasting.Casting(spell, TargetSelector.GetTarget(spell.Range, spell.DamageType));
                }
            }
        }

        /// <summary>
        ///     Returns True if combo needs to be active.
        /// </summary>
        public static bool Combo
        {
            get
            {
                return (Misc.SafeToAttack && Player.Instance.IsSafe() && Player.Instance.CountEnemyHeros(SafeValue) > 0
                    && (Player.Instance.CountAllyHeros(SafeValue) > 1 || Core.GameTickCount - Brain.LastTeamFight < 1500
                    || Misc.TeamTotal(Player.Instance.PredictPosition()) >= Misc.TeamTotal(Player.Instance.PredictPosition(), true)))
                    || Player.Instance.IsZombie();
            }
        }

        /// <summary>
        ///     Returns True if Harass needs to be active.
        /// </summary>
        public static bool Harass
        {
            get
            {
                return Core.GameTickCount - Brain.LastTeamFight > 1500 && Misc.SafeToAttack && Player.Instance.IsSafe() && Player.Instance.CountEnemyHeros(SafeValue + 100) > 0
                    && ((Player.Instance.IsUnderHisturret() && EntityManager.Heroes.Enemies.Any(e => e.IsKillable() && e.UnderEnemyTurret()))
                        || EntityManager.Heroes.Enemies.Where(e => e.IsKillable()).All(e => e.Distance(Player.Instance) > (SafeValue > 400 ? SafeValue - 400 : 400))
                        || Misc.TeamTotal(Player.Instance.PredictPosition(), true) > Misc.TeamTotal(Player.Instance.PredictPosition())
                        || (EntityManager.Heroes.Enemies.Any(e => e.IsKillable(Player.Instance.GetAutoAttackRange(e)) && Player.Instance.AlliesMoreThanEnemies())));
            }
        }

        /// <summary>
        ///     Returns True if LaneClear needs to be active.
        /// </summary>
        public static bool LaneClear
        {
            get
            {
                return Misc.SafeToAttack && Player.Instance.IsSafe() && Core.GameTickCount - Brain.LastTeamFight > 1500
                    && (Player.Instance.PredictHealthPercent() > 20 || Player.Instance.CountEnemyHeros(SafeValue) < 2)
                    && (Player.Instance.CountEnemyMinionsInRangeWithPrediction(SafeValue) > 1 || AttackObject)
                    && (Player.Instance.CountAllyMinionsInRangeWithPrediction(SafeValue) > 0 || Player.Instance.IsUnderHisturret() || Player.Instance.CountAllyHeros(SafeValue) > 1)
                    && (Player.Instance.CountEnemyHeros(SafeValue) <= 1 || Player.Instance.AlliesMoreThanEnemies() || Player.Instance.IsUnderHisturret()
                    || Misc.TeamTotal(Player.Instance.PredictPosition()) >= Misc.TeamTotal(Player.Instance.PredictPosition(), true)
                    || Player.Instance.CountAllyHeros(SafeValue) >= Player.Instance.CountEnemyHeros(SafeValue));
            }
        }

        /// <summary>
        ///     Returns True if Flee needs to be active.
        /// </summary>
        public static bool Flee
        {
            get
            {
                return !Player.Instance.IsUnderHisturret() && (Player.Instance.EnemiesMoreThanAllies() && Player.Instance.PredictHealthPercent() < 60
                    && Misc.TeamTotal(Player.Instance.PredictPosition(), true) > Misc.TeamTotal(Player.Instance.PredictPosition()) || !Player.Instance.IsSafe());
            }
        }

        /// <summary>
        ///     Returns True if No modes are active.
        /// </summary>
        public static bool None
        {
            get
            {
                return !Combo && !Harass && !LaneClear && !Flee;
            }
        }

        /// <summary>
        ///     Returns True if Can attack objects.
        /// </summary>
        public static bool AttackObject
        {
            get
            {
                return CurrentMode != Modes.Combo && (ObjectsManager.EnemyNexues != null && ObjectsManager.EnemyNexues.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyNexues.BoundingRadius * 3))
                       || (ObjectsManager.EnemyInhb != null && ObjectsManager.EnemyInhb.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyInhb.BoundingRadius * 3))
                       || (ObjectsManager.EnemyTurret != null && ObjectsManager.EnemyTurret.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange() + ObjectsManager.EnemyTurret.BoundingRadius * 3))
                       || EntityManager.Heroes.Enemies.Count(e => e.IsDead) == 5 || (EntityManager.Heroes.Enemies.Count(e => e.IsDead) >= 3
                       && EntityManager.Heroes.Allies.Count(a => a.IsActive() && a.IsValidTarget(SafeValue)) >= EntityManager.Heroes.Enemies.Count(a => a.IsValidTarget(SafeValue)));
            }
        }
    }
}
