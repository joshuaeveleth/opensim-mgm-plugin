using System;
using OpenSim.Framework;
using OpenMetaverse;
using OpenSim.Region.OptionalModules.World.NPC;
using OpenSim.Region.Framework.Scenes;
using System.Collections.Generic;

namespace MOSES.MGM
{
	public class MGMClientManager
	{
		private Scene scene;
		private Dictionary<UUID, MGMClient> m_avatars =
			new Dictionary<UUID, MGMClient>();
		private MGMLog log;

		public MGMClientManager(Scene scene, MGMLog log){
			this.scene = scene;
			this.log = log;
		}

		public void RemoveClient(UUID agentID){
			lock (m_avatars) {
				if(m_avatars.ContainsKey(agentID)){
					scene.CloseAgent (agentID, true);
					m_avatars.Remove(agentID);
				}
			}
		}

		public MGMClient NewClient(string firstname, string lastname, UUID agentID, Vector3 position){
			MGMClient client = null;
			UUID owner = agentID;
			bool senseAsAgent = true;
			try
			{
				if (agentID == UUID.Zero)
					client = new MGMClient(firstname, lastname, position,
					                          owner, senseAsAgent, scene, log);
				else
					client = new MGMClient(firstname, lastname, agentID, position,
					                          owner, senseAsAgent, scene, log);
			}
			catch (Exception e)
			{
				log("[NPC MODULE]: exception creating NPC avatar: " + e.ToString());
				return null;
			}

			client.CircuitCode = (uint)Util.RandomClass.Next(0, int.MaxValue);

			log(String.Format(
				"[NPC MODULE]: Creating NPC {0} {1} {2}, owner={3}, senseAsAgent={4} at {5} in {6}",
				firstname, lastname, client.AgentId, owner,
				senseAsAgent, position, scene.RegionInfo.RegionName));

			AgentCircuitData acd = new AgentCircuitData();
			acd.AgentID = client.AgentId;
			acd.firstname = firstname;
			acd.lastname = lastname;
			acd.IPAddress = "0.0.0.0";
			acd.ServiceURLs = new Dictionary<string, object>();

			AvatarAppearance npcAppearance = new AvatarAppearance();//  (appearance, true);
			acd.Appearance = npcAppearance;

			lock (m_avatars)
			{
				scene.AuthenticateHandler.AddNewCircuit(client.CircuitCode, acd);
				scene.AddNewAgent(client, PresenceType.Npc);

				ScenePresence sp;
				if (scene.TryGetScenePresence(client.AgentId, out sp))
				{
					sp.CompleteMovement(client, false);
					m_avatars.Add(client.AgentId, client);
					log(string.Format("[MGM MODULE]: Created NPC {0} {1}", client.AgentId, sp.Name));

					return client;
				}
				else
				{
					log(string.Format(
						"[MGM MODULE]: Could not find scene presence for NPC {0} {1}",
						sp.Name, sp.UUID));

					return null;
				}
			}
		}
	}

	public class MGMClient : NPCAvatar
	{
		private MGMLink link;
		private MGMLog log;

		public MGMClient(string firstName, string lastName, Vector3 position, UUID owner, bool senseAsAgent, Scene scene, MGMLog log) : base(firstName, lastName, position, owner, senseAsAgent, scene)
		{
			this.log = log;
		}

		public MGMClient(string firstName, string lastName, UUID agentID, Vector3 position, UUID owner, bool senseAsAgent, Scene scene, MGMLog log) : base(firstName, lastName, agentID, position, owner, senseAsAgent, scene)
		{
			this.log = log;
		}

		public void registerCallbacks(MGMLink link)
		{
			this.link = link;
			this.OnInstantMessageToNPC += onInstantMessage;
		}

		#region Callbacks

		private void onInstantMessage(GridInstantMessage msg)
		{
			if(msg.dialog != 0)
			{
				return;
			}

			string sender = msg.fromAgentID.ToString();
			string target = msg.toAgentID.ToString();
			int isGroup;
			if(msg.fromGroup)
			{
				isGroup = 1;//
			}
			else
			{
				isGroup = 0;
			}
			string body = msg.message;
			String message = MGMJson.InstantMessage(sender, target,isGroup,body);
			link.send(message);
		}

		#endregion
	}
}

