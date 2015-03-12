using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MOSES.MGM
{
	public delegate void MGMLog(string msg);

	public class MGMLink
	{
		private IPEndPoint uplink;
		private TcpClient socket = new TcpClient();
		private MGMLog log;
		private Thread readThread;
		private Thread writeThread;
		private Timer connectionTest;
		private static int connectionTestIntervalms = 1000;
		public bool isConnected { 
			get
			{
				return socket.Connected;
			}
		}
		

		public MGMLink (IPEndPoint mgmAddress, MGMLog log)
		{
			uplink = mgmAddress;
			this.log = log;
			connectionTest = new Timer(timerTick,null,Timeout.Infinite,connectionTestIntervalms);
		}

		public void start()
		{
			//start our timer thread checking on connection state
			connectionTest.Change(0,connectionTestIntervalms);
		}

		public void stop()
		{
			//halt the thread
			connectionTest.Change(Timeout.Infinite,connectionTestIntervalms);
			//close socket
			socket.Close();
		}

		private void timerTick(Object stateInfo)
		{
			if(isConnected)
			{
				return;
			}
			try
			{
				socket.Connect(uplink);
				readThread = new Thread(readSocket);
				readThread.Start();
				writeThread = new Thread(writeSocket);
				writeThread.Start();
			} 
			catch(System.Net.Sockets.SocketException e)
			{
				log(String.Format("Error connecting to MGM: {0}", e.Message));
			}
		}

		private void readSocket()
		{
			NetworkStream stream = socket.GetStream();
			byte[] buffer = new byte[1024];
			while(isConnected)
			{
				try {
					int read = stream.Read(buffer, 0, 1024);
					char[] chars = new char[read / sizeof(char)];
					System.Buffer.BlockCopy(buffer, 0, chars, 0, read);
					String msg = new string(chars);
					log(msg);
				} catch (Exception e){
					log(String.Format("The socket went away: {0}", e.Message));
					return;
				}
			}
		}

		private void writeSocket()
		{
			NetworkStream stream = socket.GetStream();
			while(isConnected)
			{
				try {
					String msg = "Test Message!";
					byte[] bytes = new byte[msg.Length * sizeof(char)];
					System.Buffer.BlockCopy(msg.ToCharArray(), 0, bytes, 0, bytes.Length);
					stream.Write(bytes,0,bytes.Length);
				} catch (Exception e){
					log(String.Format("The socket went away: {0}", e.Message));
					return;
				}
			}
		}
	}
}

