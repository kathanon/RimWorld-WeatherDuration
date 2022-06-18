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

        private static string AddDuration(WeatherInfo info)
        {
            var location = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            var started = GenDate.DateFullStringAt(GenDate.TickGameToAbs(info.startTick), location);
            // GameCondition does Colorize, but it does not seem to take effect.
            var lasted = (nowTick - info.startTick).ToStringTicksToPeriod() /* .Colorize(ColoredText.DateTimeColor) */;
            return $"{info.def.LabelCap}\n{startedStr}: {started}\n{lastedStr}: {lasted}\n\n{info.def.description}";
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
                    tip.text = AddDuration(info);
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
