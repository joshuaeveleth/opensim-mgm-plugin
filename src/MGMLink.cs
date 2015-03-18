using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace MOSES.MGM
{
	public delegate void MGMLog(string msg);

	public class MGMLink
	{
		private IPEndPoint uplink;
		private Socket socket;
		private MGMLog log;
		private Thread readThread;
		private Thread writeThread;
		private Timer connectionTest;
		private ConcurrentQueue<String> outBox = new ConcurrentQueue<String>();
		private static int connectionTestIntervalms = 1000;
		public bool isConnected { get; private set; }
		

		public MGMLink (IPEndPoint mgmAddress, MGMLog log)
		{
			uplink = mgmAddress;
			this.log = log;
			connectionTest = new Timer(timerTick,null,Timeout.Infinite,connectionTestIntervalms);
		}

		public void send(String message){
			outBox.Enqueue(message);
		}

		public void start()
		{
			//start our timer thread checking on connection state
			connectionTest.Change(0,connectionTestIntervalms);
		}

		public void stop()
		{
			connectionTest.Change(Timeout.Infinite,connectionTestIntervalms);
			if(socket != null)
			{
				socket.Close();
			}
		}

		private void timerTick(Object stateInfo)
		{
			try
			{
				if(socket == null)
				{
					log("creating new socket");
					socket = new Socket(uplink.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
				}
				if(! socket.Connected)
				{
					log("setting isConnected to true");
					isConnected = true;
					log("Reconnecting socket");
					socket.Disconnect(true);
					socket.Connect(uplink);
					readThread = new Thread(readSocket);
					readThread.Start();
					writeThread = new Thread(writeSocket);
					writeThread.Start();
					log("setting isConnected to true");
					isConnected = true;
				}
			} 
			catch(System.Net.Sockets.SocketException e)
			{
				log(String.Format("Error connecting to MGM: {0}", e.Message));
				isConnected = false;
			}
		}

		private void readSocket()
		{
			log("Read thread start");
			byte[] buffer = new byte[1024];
			while(isConnected)
			{
				try {
					int read = socket.Receive(buffer, 0, 1024,SocketFlags.None);
					if(read == 0) continue;
					String result = System.Text.Encoding.UTF8.GetString(buffer,0,read);
					log(result);
				} catch (Exception e){
					log(String.Format("reader: The socket went away: {0}", e.Message));
					isConnected = false;
				}
			}
			log("Read Thread Stop");
		}

		private void writeSocket()
		{
			log("Write thread start");
			while(isConnected)
			{
				try {
					String msg;
					if( !outBox.TryDequeue(out msg)){
						continue;
					}
					socket.Send(System.Text.Encoding.ASCII.GetBytes(msg));
				} catch (Exception e){
					log(String.Format("writer: The socket went away: {0}", e.Message));
					isConnected = false;
					socket.Disconnect(true);
				}
			}
			log("Write thread stop");
		}
	}
}

