using System;
using XRL.World;
using XRL.World.Parts;
using HarmonyLib;

namespace RitaMod.HarmonyPatches{
	[HarmonyPatch(typeof(Brain), "GetFeeling", new Type[] { typeof(GameObject) })]
	public class GetFeeling_Patch
	{
		static void Postfix(ref Brain __instance, ref int  __result, GameObject Target)
		{
			if (__instance.ParentObject.HasRegisteredEvent("GetFeeling"))
			{
				Event eGetFeeling = Event.New("GetFeeling");
				eGetFeeling.SetParameter("Who", Target);
				eGetFeeling.SetParameter("Feeling", __result);
				__instance.ParentObject.FireEvent(eGetFeeling);
				__result = eGetFeeling.GetIntParameter("Feeling");
			}
		}
	}
}