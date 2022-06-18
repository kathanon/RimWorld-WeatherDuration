using HarmonyLib;
using Verse;

namespace WeatherDuration
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        public const string ModIdentifier = "kathanon.WeatherDuration";

        static Main()
        {
            var harmony = new Harmony(ModIdentifier);
            harmony.PatchAll();
        }
    }
}
