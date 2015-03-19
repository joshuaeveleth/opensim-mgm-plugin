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
			string regMsg = MGMJson.register(scene.Name, scene.RegionInfo.RegionLocX, scene.RegionInfo.RegionLocY, scene.RegionInfo.RegionSizeX);
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
			//I dont think we need this
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
			ev.OnNewPresence += onAddAvatar;
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
			ev.OnChatFromClient += onChatBroadcast;
		}

		private void onFrame(){
			String msg = MGMJson.frame();
			mgmLink.send(msg);
		}

		#region ActorEvents

		private void onAddAvatar(ScenePresence client)
		{

		}

		private void onAvatarAppearanceChanged(ScenePresence client)
		{

		}

		private void onAvatarPresenceChanged(ScenePresence client)
		{

		}

		public void onRemoveAvatar(ScenePresence client){	onRemoveAvatar(client.UUID);	}
		public void onRemoveAvatar(OpenMetaverse.UUID uuid){}

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

		}

		#endregion
	}
}
