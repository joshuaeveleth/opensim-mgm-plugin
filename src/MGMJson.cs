using System;
using System.Collections.Generic;

namespace MOSES.MGM
{
	public static class MGMJson
	{
		public static String Encode(Dictionary<string, object> dict)
		{
			if(dict.Keys.Count == 0)
			{
				return "{}";
			}
			string[] args = new string[dict.Keys.Count];
			int i = 0;
			foreach(KeyValuePair<string,object> entry in dict){
				if(entry.Value is string)
				{
					args[i] = String.Format("\"{0}\":\"{1}\"",entry.Key, entry.Value);
				}
				else
				{
					args[i] = String.Format("\"{0}\":{1}",entry.Key, entry.Value);
				}
				i++;
			}
			return "{" + string.Join(",",args) + "}";
		}

		public static String Frame()
		{
			TimeSpan t = DateTime.Now - new DateTime(1970, 1, 1);
			int secondsSinceEpoch = (int)t.TotalMilliseconds;
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "frame";
			msg["ms"] = secondsSinceEpoch;
			return MGMJson.Encode(msg);
		}

		public static String Register(string regionName, uint locX, uint locY, uint regionSize)
		{
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "register";
			msg["name"] = regionName;
			msg["locX"] = locX;
			msg["locY"] = locY;
			msg["size"] = regionSize;
			return MGMJson.Encode(msg);
		}

		public static String TextMessage(string sender,string target,int channel,string chatType, string pos,string message)
		{
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "textMessage";
			msg["sender"] = sender;
			msg["target"] = target;
			msg["channel"] = channel;
			msg["chatType"] = chatType;
			msg["pos"] = pos;
			msg["body"] = message;
			return MGMJson.Encode(msg);
		}

		public static string InstantMessage (string sender, string target, int isGroup, string message)
		{
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "InstantMessage";
			msg["sender"] = sender;
			msg["target"] = target;
			msg["isGroup"] = isGroup;
			msg["body"] = message;
			return MGMJson.Encode(msg);
		}

		public static String AddAvatar(string uuid)
		{
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "addAvatar";
			msg["uuid"] = uuid;
			return MGMJson.Encode(msg);
		}

		public static String RemoveAvatar(string uuid)
		{
			Dictionary<string,object> msg = new Dictionary<string, object>();
			msg["type"] = "removeAvatar";
			msg["uuid"] = uuid;
			return MGMJson.Encode(msg);
		}
	}
}

