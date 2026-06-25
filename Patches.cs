using HarmonyLib;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;
using System.Collections.Generic;

namespace VoidCombatOverhaul
{
    [HarmonyPatch(typeof(RuleStarshipCalculateHitChances), "OnTrigger")]
    static class Patch_HitChanceModifiers
    {
        internal static readonly Dictionary<string, int> HitsThisCombat = new();

        static void Postfix(RuleStarshipCalculateHitChances __instance)
        {
            if (!Main.Enabled) return;

            var attacker = __instance.Initiator as StarshipEntity;
            var target = __instance.Target as StarshipEntity;
            if (attacker == null || target == null) return;

            if (Main.Settings.EnableRangePenalties)
            {
                float maxRange = 20f;
                try { maxRange = Traverse.Create(__instance.Weapon?.Blueprint).Property<int>("MaxRangeCells").Value; } catch { }

                float dist = Vector3.Distance(attacker.Position, target.Position);
                if (dist <= maxRange * 0.3f)
                    __instance.ResultHitChance += Mathf.RoundToInt(Main.Settings.ShortRangeHitBonus * 100f);
                else if (dist >= maxRange * 0.7f)
                    __instance.ResultHitChance = Mathf.Max(__instance.ResultHitChance - Mathf.RoundToInt(Main.Settings.LongRangeHitPenalty * 100f), 5);
            }

            if (Main.Settings.EnableRearArcBonus)
            {
                float dot = Vector3.Dot((attacker.Position - target.Position).normalized, target.Forward);
                if (dot < -0.7f)
                    __instance.ResultHitChance += Mathf.RoundToInt(Main.Settings.RearArcHitBonus * 100f);
            }
        }
    }

    [HarmonyPatch(typeof(RuleStarshipRollShieldAbsorption), "OnTrigger")]
    static class Patch_ShieldAbsorption
    {
        static void Postfix(RuleStarshipRollShieldAbsorption __instance)
        {
            if (!Main.Enabled || !Main.Settings.EnableImprovedShields || __instance.ResultShields <= 0) return;
            __instance.ResultShields += Mathf.RoundToInt(__instance.ResultShields * Main.Settings.ShieldAbsorptionBonus);
        }
    }

    [HarmonyPatch(typeof(RuleStarshipPerformAttack), "OnTrigger")]
    static class Patch_MoraleDegradation
    {
        static void Postfix(RuleStarshipPerformAttack __instance)
        {
            if (!Main.Enabled || !Main.Settings.EnableMoraleDegradation || !__instance.ResultIsHit) return;

            var target = __instance.Target as StarshipEntity;
            if (target == null) return;

            var hits = Patch_HitChanceModifiers.HitsThisCombat;
            if (!hits.ContainsKey(target.UniqueId)) hits[target.UniqueId] = 0;
            hits[target.UniqueId]++;

            if (hits[target.UniqueId] % Main.Settings.MoraleHitThreshold == 0)
            {
                var morale = target.GetOptional<Kingmaker.UnitLogic.Parts.PartStarshipMorale>();
                if (morale != null) morale.MoraleDamage += Main.Settings.MoraleDegradationPerThreshold;
            }
        }
    }

    [HarmonyPatch(typeof(Kingmaker.Controllers.SpaceCombat.ExitSpaceCombatController), "CanExitSpaceCombat")]
    static class Patch_EscapeRestriction
    {
        static void Postfix(ref bool __result)
        {
            if (!Main.Enabled || !Main.Settings.EnableHarderEscape || !__result) return;

            var ship = Kingmaker.Game.Instance?.Player?.PlayerShip as StarshipEntity;
            var hull = ship?.GetOptional<PartStarshipHull>();
            if (hull == null) return;

            if (Traverse.Create(hull).Property<float>("HullIntegrity").Value > Main.Settings.EscapeHullThreshold)
                __result = false;
        }
    }

    [HarmonyPatch(typeof(Kingmaker.Controllers.SpaceCombat.ExitSpaceCombatController), "OnEventDidTrigger")]
    static class Patch_ClearCombatState
    {
        static void Postfix() => Patch_HitChanceModifiers.HitsThisCombat.Clear();
    }

    [HarmonyPatch(typeof(AbilityCustomStarshipBoardingTeam), "Deliver")]
    static class Patch_BoardingMilitaryRating
    {
        static void Postfix(Kingmaker.UnitLogic.Abilities.AbilityExecutionContext context, Kingmaker.Utility.TargetWrapper target)
        {
            if (!Main.Enabled || !Main.Settings.EnableBoardingMilitaryRating) return;

            var ship = target?.Entity as StarshipEntity;
            if (ship == null || ship.IsPlayerFaction) return;

            Kingmaker.RuleSystem.Rulebook.Trigger(new RuleStarshipPerformDecreaseMilitaryRating(
                context.Caster, ship, Main.Settings.BoardingMilitaryRatingPenalty));
        }
    }

    [HarmonyPatch(typeof(Kingmaker.SpaceCombat.Scrap.Scrap), "get_ScrapToRegenOneHp")]
    static class Patch_ScrapCostScaling
    {
        static void Postfix(ref int __result)
        {
            if (!Main.Enabled || !Main.Settings.EnableScrapCostScaling) return;

            var ship = Kingmaker.Game.Instance?.Player?.PlayerShip as StarshipEntity;
            var hull = ship?.GetOptional<PartStarshipHull>();
            if (hull == null) return;

            float integrity = Traverse.Create(hull).Property<float>("HullIntegrity").Value;
            float mult = integrity < Main.Settings.ScrapCostCriticalThreshold ? Main.Settings.ScrapCostCriticalMultiplier
                       : integrity < Main.Settings.ScrapCostHeavyThreshold    ? Main.Settings.ScrapCostHeavyMultiplier
                       : integrity < Main.Settings.ScrapCostModerateThreshold ? Main.Settings.ScrapCostModerateMultiplier
                       : 1f;

            if (mult > 1f) __result = Mathf.RoundToInt(__result * mult);
        }
    }
}
