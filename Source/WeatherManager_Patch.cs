using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WeatherDuration
{
    [HarmonyPatch(typeof(WeatherManager))]
    public static class WeatherManager_Patch
    {
        private struct WeatherInfo
        {
            public WeatherDef def;
            public int startTick;
        }

        private static WeatherInfo current;
        private static WeatherInfo last;
        private static bool active = false;
        private static int nowTick;

        private static readonly string startedStr = "Started".Translate();
        private static readonly string lastedStr = "Lasted".Translate();

        private static string ExpandDescription(WeatherInfo info)
        {
            var def = info.def;
            var location = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            var started = GenDate.DateFullStringAt(GenDate.TickGameToAbs(info.startTick), location);
            // GameCondition does Colorize, but it does not seem to take effect.
            var lasted = (nowTick - info.startTick).ToStringTicksToPeriod() /* .Colorize(ColoredText.DateTimeColor) */;
            var effects = "";
            if (def.accuracyMultiplier != 1.0f || def.moveSpeedMultiplier != 1.0f)
            {
                effects = "\n";
                UpdateEffects(ref effects, def.accuracyMultiplier, StatDefOf.ShootingAccuracyPawn.LabelCap);
                UpdateEffects(ref effects, def.moveSpeedMultiplier, StatDefOf.MoveSpeed.LabelCap);
            }
            return $"{def.LabelCap}\n{startedStr}: {started}\n{lastedStr}: {lasted}\n\n{def.description}{effects}";
        }

        private static void UpdateEffects(ref string effects, float multiplier, string label)
        {
            if (multiplier != 1.0f)
            {
                effects = $"{effects}\n{(multiplier - 1).ToStringByStyle(ToStringStyle.PercentZero)} {label}";
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TooltipHandler), nameof(TooltipHandler.TipRegion), typeof(Rect), typeof(TipSignal))]
        public static void TooltipHandler_TipRegion_Pre(ref TipSignal tip)
        {
            if (active)
            {
                WeatherInfo info = (tip.text == current.def?.description) ? current : last;
                if (info.def != null)
                {
                    tip.text = ExpandDescription(info);
                }
                active = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(WeatherManager.DoWeatherGUI))]
        public static void DoWeatherGUI_Pre(WeatherManager __instance)
        {
            WeatherManager man = __instance;
            nowTick = Find.TickManager.TicksGame;
            if (current.def != man.curWeather)
            {
                if (man.lastWeather == current.def)
                {
                    last = current;
                }
                else
                {
                    last.def = null;
                }
                current.def = man.curWeather;
                current.startTick = nowTick - man.curWeatherAge;
            }

            active = true;
        }
    }
}
