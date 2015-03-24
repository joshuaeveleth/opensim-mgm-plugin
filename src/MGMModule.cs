using System;
using System.Reflection;
using log4net;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

using Mono.Addins;

using System.Net;

[assembly: Addin("MGMModule","0.1")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]
[assembly: AddinDescription("MOSES Grid Manager Integration")]
[assembly: AddinAuthor("Michael Heilmann")]

namespace MOSES.MGM
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "MGMModule")]
	public class MGMModule : INonSharedRegionModule
	{
		private static ILog m_log;
		private bool enabled = false;
		private IPAddress mgmAddress;
		private int mgmPort;
		private MGMLink mgmLink;
		private MGMClient client;

		public string Name { get { return "MGMModule"; } }
		public Type ReplaceableInterface { get { return null; } }

		public MGMModule(){}

		#region INonSharedRegionModule

		public void Initialise(IConfigSource source)
		{
			IConfig config = source.Configs["MGMModule"];
			m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			//read config for enabled or Enabled
			enabled = (config != null && (config.GetBoolean("Enabled",false) || config.GetBoolean("enabled",false)));
			if(enabled)
			{
				log("Initialising");
				if(!IPAddress.TryParse(config.GetString("mgmUrl", ""), out mgmAddress))
				{
					log("Error parsing mgm url");
					enabled = false;
					return;
				}
				mgmPort = config.GetInt("mgmPort", 80);
			} else {
				log("Disabled");
			}
		}

		public void Close(){}

		public void AddRegion(Scene scene)
		{
			if(!enabled) return;
			log("Adding region to MGM");
			scene.AddCommand("mgm",this,"mgm status","status","Print the status of the MGM module", consoleStatus);
			mgmLink = new MGMLink(new IPEndPoint(mgmAddress, mgmPort), delegate(string s){log(s);});
			mgmLink.start();
			registerEvents(scene.EventManager);
			string regMsg = MGMJson.Register(scene.Name, scene.RegionInfo.RegionLocX, scene.RegionInfo.RegionLocY, scene.RegionInfo.RegionSizeX);
			mgmLink.send(regMsg);
		}

		public void RemoveRegion(Scene scene)
		{
			if(!enabled) return;
			mgmLink.stop();
		}

		public void RegionLoaded(Scene scene)
		{
			if(!enabled) return;
			log("Adding user manually");
			client = new MGMClient(scene);
			scene.AddNewAgent(client, PresenceType.User);
		}

		#endregion

		private void log(string msg)
		{
			m_log.DebugFormat("[MGM]: {0}", msg);
		}

		#region console

		private void consoleStatus(string module, string[] args)
		{
			log(String.Format("MGM url: {0}", mgmAddress));
			if(mgmLink.isConnected)
			{
				log("MGM link is active");
			} else {
				log("MGM link is not active");
			}
		}

		#endregion

		private void registerEvents(EventManager ev)
		{
			ev.OnFrame += onFrame;

			/* Actor Events */
			//ev.OnNewPresence += onAddAvatar;
			ev.OnRemovePresence += onRemoveAvatar;
			ev.OnAvatarAppearanceChange += onAvatarAppearanceChanged;
			ev.OnScenePresenceUpdated += onAvatarPresenceChanged;
			ev.OnMakeChildAgent += onRemoveAvatar;
			ev.OnMakeRootAgent += onAddAvatar;

			/* Object Events */
			ev.OnObjectAddedToScene += onAddObject;
			ev.OnSceneObjectLoaded += onAddObject;
			ev.OnObjectBeingRemovedFromScene += onRemoveObject;
			ev.OnSceneObjectPartUpdated += onUpdateObject;

			/* Chat Events */
			//local chat
			ev.OnChatFromClient += onChatBroadcast;
			//owner say  //channel say
			ev.OnChatFromWorld += onChatBroadcast;
			//IM
			ev.OnIncomingInstantMessage += onInstantMessage;
			ev.OnChatFromClient += onChatBroadcast;
			log("Registered for events");
		}

		private void onFrame(){
			String msg = MGMJson.Frame();
			mgmLink.send(msg);
		}

		#region ActorEvents

		private void onAddAvatar(ScenePresence client)
		{
			string uuid = client.UUID.ToString();
			string msg = MGMJson.AddAvatar(uuid);
			mgmLink.send(msg);
		}

		private void onAvatarAppearanceChanged(ScenePresence client)
		{

		}

		private void onAvatarPresenceChanged(ScenePresence client)
		{

		}

		public void onRemoveAvatar(ScenePresence client){	onRemoveAvatar(client.UUID);	}
		public void onRemoveAvatar(OpenMetaverse.UUID uuid){
			string msg = MGMJson.RemoveAvatar(uuid.ToString());
			mgmLink.send(msg);
		}

		#endregion
	
		#region ObjectEvents

		private void onAddObject(SceneObjectGroup sog)
		{

		}

		private void onRemoveObject(SceneObjectGroup sog)
		{

		}

		private void onUpdateObject(SceneObjectPart sop, bool flag)
		{

		}

		//only has broadcasts to local chat, not IMs
		private void onChatBroadcast(object obj, OSChatMessage msg)
		{
			if(msg.Message == "")
			{
				//these messages are generated on keyboard input, and have not data
				return;
			}
			string sender = msg.SenderUUID.ToString();
			string target = msg.TargetUUID.ToString();
			string pos = msg.Position.ToString();
			string msgType = Enum.GetName(typeof(ChatTypeEnum),msg.Type);
			String message = MGMJson.TextMessage(sender,target,msg.Channel,msgType,pos,msg.Message);
			mgmLink.send(message);
		}

		//IM Messages
		private void onInstantMessage (GridInstantMessage msg)
		{
			log("Instant Message");
			//string sender = msg.fromAgentID.ToString();
			//string target = msg.toAgentID.ToString();
			//bool isGroup = msg.fromGroup;
			//string body = msg.message;
			//String message = MGMJson.InstantMessage(sender, target,isGroup,body);
			//mgmLink.send(message);
		}

		#endregion
	}
}
