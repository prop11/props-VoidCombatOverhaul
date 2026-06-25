using HarmonyLib;
using Kingmaker.Controllers.TurnBased;
using UnityEngine;

namespace VoidCombatOverhaul
{
    [HarmonyPatch(typeof(TurnController), "Tick")]
    static class Patch_CombatSpeed
    {
        static float _lastPlayer = -1f;
        static float _lastEnemy = -1f;

        static void Postfix(TurnController __instance)
        {
            if (!Main.Enabled || !Main.Settings.EnableCombatSpeedControls || !InSpaceCombat()) return;

            float global = Mathf.Clamp(Main.Settings.GlobalCombatSpeedMultiplier, 0.5f, 10f);

            if (__instance.IsPlayerTurn)
            {
                if (!Mathf.Approximately(_lastPlayer, global))
                {
                    Traverse.Create(__instance).Property<float>("TimeScaleInPlayerTurn").Value = global;
                    _lastPlayer = global;
                }
            }
            else
            {
                float enemyScale = Main.Settings.AutoSpeedUpEnemyTurns
                    ? global * Mathf.Clamp(Main.Settings.EnemyTurnSpeedMultiplier, 1f, 10f)
                    : global;

                if (!Mathf.Approximately(_lastEnemy, enemyScale))
                {
                    Traverse.Create(__instance).Property<float>("TimeScaleInNonPlayerTurn").Value = enemyScale;
                    _lastEnemy = enemyScale;
                }
            }
        }

        internal static void Reset()
        {
            var tc = Kingmaker.Game.Instance?.TurnController;
            if (tc == null) return;
            Traverse.Create(tc).Property<float>("TimeScaleInPlayerTurn").Value = 1f;
            Traverse.Create(tc).Property<float>("TimeScaleInNonPlayerTurn").Value = 1f;
            _lastPlayer = _lastEnemy = -1f;
        }

        internal static bool InSpaceCombat() =>
            Kingmaker.Game.Instance?.CurrentMode.ToString()?.Contains("SpaceCombat") == true;
    }

    [HarmonyPatch(typeof(Kingmaker.Controllers.Projectiles.Projectile), "get_Speed")]
    static class Patch_ProjectileSpeed
    {
        static void Postfix(ref float __result)
        {
            if (!Main.Enabled || !Main.Settings.EnableCombatSpeedControls || !Patch_CombatSpeed.InSpaceCombat()) return;
            __result *= Mathf.Clamp(Main.Settings.ProjectileSpeedMultiplier, 1f, 10f);
        }
    }

    [HarmonyPatch(typeof(Kingmaker.View.UnitMovementAgent), "get_CombatSpeedMps")]
    static class Patch_ShipMoveSpeed
    {
        static void Postfix(ref float __result)
        {
            if (!Main.Enabled || !Main.Settings.EnableCombatSpeedControls || !Patch_CombatSpeed.InSpaceCombat()) return;
            __result *= Mathf.Clamp(Main.Settings.ShipMoveSpeedMultiplier, 1f, 10f);
        }
    }

    [HarmonyPatch(typeof(Kingmaker.Controllers.SpaceCombat.ExitSpaceCombatController), "OnEventDidTrigger")]
    static class Patch_ResetSpeedOnCombatEnd
    {
        static void Postfix() => Patch_CombatSpeed.Reset();
    }
}
