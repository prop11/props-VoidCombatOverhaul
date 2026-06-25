using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;

namespace VoidCombatOverhaul
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool EnableRangePenalties = true;
        public float LongRangeHitPenalty = 0.15f;
        public float ShortRangeHitBonus = 0.10f;

        public bool EnableImprovedShields = true;
        public float ShieldAbsorptionBonus = 0.10f;

        public bool EnableMoraleDegradation = true;
        public int MoraleHitThreshold = 3;
        public int MoraleDegradationPerThreshold = 1;

        public bool EnableRearArcBonus = true;
        public float RearArcHitBonus = 0.25f;

        public bool EnableHarderEscape = true;
        public float EscapeHullThreshold = 0.50f;

        public bool EnableBoardingMilitaryRating = true;
        public int BoardingMilitaryRatingPenalty = 2;

        public bool EnableScrapCostScaling = true;
        public float ScrapCostModerateThreshold = 0.75f;
        public float ScrapCostHeavyThreshold = 0.50f;
        public float ScrapCostCriticalThreshold = 0.25f;
        public float ScrapCostModerateMultiplier = 1.25f;
        public float ScrapCostHeavyMultiplier = 1.75f;
        public float ScrapCostCriticalMultiplier = 2.50f;

        public bool EnableCombatSpeedControls = true;
        public float GlobalCombatSpeedMultiplier = 1.5f;
        public bool AutoSpeedUpEnemyTurns = true;
        public float EnemyTurnSpeedMultiplier = 3.0f;
        public float ProjectileSpeedMultiplier = 2.0f;
        public float ShipMoveSpeedMultiplier = 2.0f;

        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
    }

    public static class Main
    {
        public static Settings Settings;
        public static UnityModManager.ModEntry ModEntry;
        public static bool Enabled;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            new Harmony(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("<b>Void Combat Overhaul</b>");
            GUILayout.Space(8);

            GUILayout.Label("<b>Weapon Range</b>");
            Settings.EnableRangePenalties = GUILayout.Toggle(Settings.EnableRangePenalties, " Range-based hit modifiers");
            if (Settings.EnableRangePenalties)
            {
                FloatField("Long range penalty:", ref Settings.LongRangeHitPenalty);
                FloatField("Short range bonus:", ref Settings.ShortRangeHitBonus);
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Shields</b>");
            Settings.EnableImprovedShields = GUILayout.Toggle(Settings.EnableImprovedShields, " Improved shield absorption");
            if (Settings.EnableImprovedShields)
                FloatField("Absorption bonus:", ref Settings.ShieldAbsorptionBonus);

            GUILayout.Space(6);
            GUILayout.Label("<b>Crew Morale</b>");
            Settings.EnableMoraleDegradation = GUILayout.Toggle(Settings.EnableMoraleDegradation, " Sustained fire morale degradation");
            if (Settings.EnableMoraleDegradation)
            {
                IntField("Hits before morale drop:", ref Settings.MoraleHitThreshold, 1, 10);
                IntField("Morale damage per threshold:", ref Settings.MoraleDegradationPerThreshold, 1, 5);
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Rear Arc</b>");
            Settings.EnableRearArcBonus = GUILayout.Toggle(Settings.EnableRearArcBonus, " Hit bonus attacking from stern (rear 90°)");
            if (Settings.EnableRearArcBonus)
                FloatField("Rear arc hit bonus:", ref Settings.RearArcHitBonus);

            GUILayout.Space(6);
            GUILayout.Label("<b>Escape</b>");
            Settings.EnableHarderEscape = GUILayout.Toggle(Settings.EnableHarderEscape, " Restrict escape to damaged ships");
            if (Settings.EnableHarderEscape)
                FloatField("Max hull % to escape:", ref Settings.EscapeHullThreshold);

            GUILayout.Space(6);
            GUILayout.Label("<b>Boarding</b>");
            Settings.EnableBoardingMilitaryRating = GUILayout.Toggle(Settings.EnableBoardingMilitaryRating, " Boarding reduces enemy military rating");
            if (Settings.EnableBoardingMilitaryRating)
                IntField("Military rating penalty:", ref Settings.BoardingMilitaryRatingPenalty, 1, 10);

            GUILayout.Space(6);
            GUILayout.Label("<b>Repair Costs</b>");
            Settings.EnableScrapCostScaling = GUILayout.Toggle(Settings.EnableScrapCostScaling, " Repair costs scale with hull damage");
            if (Settings.EnableScrapCostScaling)
            {
                FloatField($"Moderate (<{Settings.ScrapCostModerateThreshold:P0}) multiplier:", ref Settings.ScrapCostModerateMultiplier);
                FloatField($"Heavy (<{Settings.ScrapCostHeavyThreshold:P0}) multiplier:", ref Settings.ScrapCostHeavyMultiplier);
                FloatField($"Critical (<{Settings.ScrapCostCriticalThreshold:P0}) multiplier:", ref Settings.ScrapCostCriticalMultiplier);
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Combat Speed</b>");
            Settings.EnableCombatSpeedControls = GUILayout.Toggle(Settings.EnableCombatSpeedControls, " Combat speed controls");
            if (Settings.EnableCombatSpeedControls)
            {
                FloatField("Global speed multiplier:", ref Settings.GlobalCombatSpeedMultiplier);
                Settings.AutoSpeedUpEnemyTurns = GUILayout.Toggle(Settings.AutoSpeedUpEnemyTurns, " Auto speed-up enemy turns");
                if (Settings.AutoSpeedUpEnemyTurns)
                    FloatField("Enemy turn multiplier:", ref Settings.EnemyTurnSpeedMultiplier);
                FloatField("Projectile speed multiplier:", ref Settings.ProjectileSpeedMultiplier);
                FloatField("Ship movement multiplier:", ref Settings.ShipMoveSpeedMultiplier);
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry) => Settings.Save(modEntry);

        static void FloatField(string label, ref float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(240));
            if (float.TryParse(GUILayout.TextField(value.ToString("F2"), GUILayout.Width(60)), out float result))
                value = result;
            GUILayout.EndHorizontal();
        }

        static void IntField(string label, ref int value, int min, int max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(240));
            if (int.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(60)), out int result))
                value = Mathf.Clamp(result, min, max);
            GUILayout.EndHorizontal();
        }
    }
}
