using System;

namespace MOSES.MGM
{
	public static class MGMJson
	{
		public static String frame()
		{
			TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1);
			int secondsSinceEpoch = (int)t.TotalMilliseconds;
			return String.Format("{{\"type\":\"frame\",\"ms\":{0}}}\n",secondsSinceEpoch);
		}

		public static String register(string regionName)
		{
			return String.Format("{{\"type\":\"register\",\"name\":\"{0}\"}}\n",regionName);
		}
	}
}

