using System;
using System.Collections.Generic;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using Grenades;
using MEC;
using Mirror;
using UnityEngine;
using Utf8Json.Internal.DoubleConversion;

namespace Physics
{
	public class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		List<string> GrenadeLauncher = new List<string>();
		List<string> FlashLauncher = new List<string>();
		List<Item> GrenadeHolder = new List<Item>();
		public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		public void OnWaitingForPlayers()
		{
			PortalPos = new Vector3();
			GrenadeLauncher.Clear();
		}

		public void OnRoundEnd()
		{
			foreach (CoroutineHandle handle in Coroutines)
				Timing.KillCoroutines(handle);		
		}

		public void OnShoot(ref ShootEvent ev)
		{
			try
			{
				if (UnityEngine.Physics.Linecast(ev.Shooter.PlayerCameraReference.transform.position, ev.TargetPos, out RaycastHit raycastHit))
				{
					var grenade = raycastHit.transform.GetComponentInParent<FragGrenade>();
					if (grenade != null)
					{
						NetworkServer.Destroy(grenade.gameObject);
						SpawnGrenade(ev.TargetPos, false, 0f, ev.Shooter);				
					}
				}

				if(GrenadeLauncher.Contains(ev.Shooter.GetUserId()))
				{
					if (UnityEngine.Physics.Linecast(ev.Shooter.PlayerCameraReference.transform.position, ev.TargetPos, out RaycastHit info))
					{
						SpawnGrenade(ev.TargetPos, false, .1f, ev.Shooter);
					}
				}
				if(FlashLauncher.Contains(ev.Shooter.GetUserId()))
                {
					if (UnityEngine.Physics.Linecast(ev.Shooter.PlayerCameraReference.transform.position, ev.TargetPos, out RaycastHit info))
					{
						SpawnGrenade(ev.TargetPos, true, .1f, ev.Shooter);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error($"Exception in OnShoot: {e}");
			}
		}

		public static void SpawnGrenade(Vector3 position, bool isFlash = false, float fusedur = -1, ReferenceHub player = null)
		{
			if (player == null) player = ReferenceHub.GetHub(PlayerManager.localPlayer);
			var gm = player.GetComponent<GrenadeManager>();
			Grenade component = UnityEngine.Object.Instantiate(gm.availableGrenades[isFlash ? 1 : 0].grenadeInstance).GetComponent<Grenade>();
			if (fusedur != -1) component.fuseDuration = fusedur;
			component.FullInitData(gm, position, Quaternion.Euler(component.throwStartAngle), Vector3.zero, component.throwAngularVelocity);
			NetworkServer.Spawn(component.gameObject);
		}

		public void OnCommand(ref RACommandEvent ev)
		{
			try
			{
				if (ev.Command.Contains("REQUEST_DATA PLAYER_LIST SILENT"))
					return;

				string[] args = ev.Command.Split(' ');
				ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? PlayerManager.localPlayer.GetPlayer() : Player.GetPlayer(ev.Sender.SenderId);

				switch (args[0].ToLower())
				{
					case "launcher":
						ev.Allow = false;
						if(!sender.CheckPermission("launcher"))
						{
							ev.Sender.RAMessage("No boom boom for you.");
							return;
						}
						if(args.Length != 3)
						{
							ev.Sender.RAMessage("USAGE: launcher (player) (flash/frag)");
							return;
						}
						ReferenceHub f = Player.GetPlayer(args[1]);
						if (f == null)
						{
							ev.Sender.RAMessage("Player not found.");
							return;
						}

						if(args[2] == "flash")
                        {
							if (!FlashLauncher.Contains(f.GetUserId()))
                            {
								FlashLauncher.Add(f.GetUserId());
								ev.Sender.RAMessage($"Flash launcher toggled on for {f.GetNickname()}");
							}								
							else
                            {
								FlashLauncher.Remove(f.GetUserId());
								ev.Sender.RAMessage($"Flash launcher toggled off for {f.GetNickname()}");
							}
                        }
						else if(args[2] == "frag")
                        {
							if (!GrenadeLauncher.Contains(f.GetUserId()))
							{
								GrenadeLauncher.Add(f.GetUserId());
								ev.Sender.RAMessage($"Grenade launcher toggled on for {f.GetNickname()}");
							}
							else
							{
								GrenadeLauncher.Remove(f.GetUserId());
								ev.Sender.RAMessage($"Grenade launcher toggled off for {f.GetNickname()}");
							}
						}
						else
                        {
							ev.Sender.RAMessage("USAGE: launcher (player) (flash/frag)");
                        }
						break;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Exception in OnCommand: {e}");
			}
		}
	}
}