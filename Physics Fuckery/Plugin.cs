using Harmony;
using EXILED;
using System.Diagnostics.Tracing;
using System.Collections.Generic;

namespace Physics
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;
		public static int PatchCounter;

		public static HarmonyInstance HarmonyInstance { set; get; }

		internal bool Enabled;
		internal List<string> FastDamage;
		internal List<string> SlowDamage;
		internal int FastDelay;
		internal int SlowDelay;
		internal bool FastAllowFrag;
		internal bool FastAllowFlash;
		internal bool SlowAllowFrag;
		internal bool SlowAllowFlash;
		internal int FastSpeedMultiplier;
		internal int SlowSpeedMultiplier;

		public override void OnEnable()
		{
			ConfigReload();

			EventHandlers = new EventHandlers(this);
			
			Events.RoundEndEvent += EventHandlers.OnRoundEnd;
			Events.WaitingForPlayersEvent += EventHandlers.OnWaitingForPlayers;
			Events.ShootEvent += EventHandlers.OnShoot;
			Events.RemoteAdminCommandEvent += EventHandlers.OnCommand;

			HarmonyInstance = HarmonyInstance.Create($"build.dicore.{PatchCounter}");
			HarmonyInstance.PatchAll();
			Log.Info($"Loaded. Wow nothings broken yet. Amazing.");
		}

		public override void OnDisable()
		{
			Events.RoundStartEvent -= EventHandlers.OnRoundEnd;
			Events.WaitingForPlayersEvent -= EventHandlers.OnWaitingForPlayers;
			Events.ShootEvent -= EventHandlers.OnShoot;
			Events.RemoteAdminCommandEvent -= EventHandlers.OnCommand;

			EventHandlers = null;
			HarmonyInstance.UnpatchAll();
		}

		public override void OnReload()
		{
			
		}

		public void ConfigReload()
		{
			Enabled = Config.GetBool("dn_enabled", true);
			FastDamage = Config.GetStringList("dn_fastdamage");
			SlowDamage = Config.GetStringList("dn_slowdamage");
			FastDelay = Config.GetInt("dn_fastdelay", 1);
			SlowDelay = Config.GetInt("dn_slowdelay", 2);
			FastAllowFrag = Config.GetBool("dn_fastfrag", true);
			FastAllowFlash = Config.GetBool("dn_fastflash", true);
			SlowAllowFrag = Config.GetBool("dn_slowfrag", true);
			SlowAllowFlash = Config.GetBool("dn_slowflash", true);
			FastSpeedMultiplier = Config.GetInt("dn_fastmult", 2);
			SlowSpeedMultiplier = Config.GetInt("dn_slowmult", 1);
		}

		public override string getName => "Physics Fuckery";
	}
}