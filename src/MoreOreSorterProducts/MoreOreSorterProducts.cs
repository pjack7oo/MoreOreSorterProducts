using System;
using System.Reflection;
using System.Reflection.Emit;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Mods;
using HarmonyLib;
namespace MoreOreSorterProducts;

public sealed class MoreOreSorterProducts : DataOnlyMod {

	// Name of this mod. It will be eventually shown to the player.
	public override string Name => "MoreOreSorterProducts mod";

	// Version, currently unused.
	public override int Version => 1;


	// Mod constructor that lists mod dependencies as parameters.
	// This guarantee that all listed mods will be loaded before this mod.
	// It is a good idea to depend on both `Mafi.Core.CoreMod` and `Mafi.Base.BaseMod`.
	public MoreOreSorterProducts(CoreMod coreMod, BaseMod baseMod) {
		// You can use Log class for logging. These will be written to the log file
		// and can be also displayed in the in-game console with command `also_log_to_console`.
		Log.Info("MoreOreSorterProducts: constructed");
		var harmony = new Harmony("coi-harmony");
        harmony.PatchAll();
	}


	public override void RegisterPrototypes(ProtoRegistrator registrator) {

	}

}