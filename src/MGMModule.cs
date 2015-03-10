using System;
using System.Reflection;
using log4net;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

using Mono.Addins;


[assembly: Addin("MGMModule","0.1")]
[assembly: AddinDependency("OpenSim", OpenSim.VersionInfo.VersionNumber)]

namespace MOSES.MGM
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "MGMModule")]
	public class MGMModule : INonSharedRegionModule
	{
		private static ILog m_log;
		private bool enabled = false;

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
			} else {
				log("Disabled");
			}
		}

		public void Close(){}

		public void AddRegion(Scene scene)
		{
			if(!enabled) return;
			scene.AddCommand("mgm",this,"mgm status","status","Print the status of the MGM module", consoleStatus);
		}

		public void RemoveRegion(Scene scene)
		{
			if(!enabled) return;
			log("Remove Scene");
		}

		public void RegionLoaded(Scene scene)
		{
			if(!enabled) return;
			log("Region Loaded");
		}

		#endregion

		private void log(string msg)
		{
			m_log.DebugFormat("[MGM]: {0}", msg);
		}

		#region console

		private void consoleStatus(string module, string[] args)
		{
			log("Status Unknown");
		}

		#endregion
	}
}
