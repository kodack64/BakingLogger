using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace HH309 {
	class HH309Logger {
		const string comName = "COM5";
		const int intervalMs = 5000;
		static int dataBlock = 100;
		void Run() {
			HH309Communicate port = new HH309Communicate(comName);
			int dataCount = 0;
			int blockCount = 0;
			float[,] data = new float[4, dataBlock];
			if (port.isOpen) {
				do {
					var temps = port.getTemperature();
					if (temps == null) {
						Console.WriteLine("Error in communication");
						break;
					}
					Console.Write("{0} {1} : ",blockCount,dataCount);
					for (int ch = 0; ch < temps.Length; ch++) {
						data[ch, dataCount] = temps[ch];
						Console.Write("{0:0.0}",temps[ch]);
						if (ch + 1 == temps.Length) Console.WriteLine();
						else Console.Write(" ");
					}
					dataCount++;

					if (dataCount >= dataBlock) {
						var fname = "HH309_" + (blockCount).ToString() + ".txt";
						var sw = new StreamWriter(fname);
						for (int ind = 0; ind < dataBlock; ind++) {
							for (int ch = 0; ch < 4; ch++) {
								sw.Write("{0:0.0}",data[ch,ind]);
								if (ch + 1 == 4) sw.WriteLine();
								else sw.Write(" ");
							}
						}
						sw.Close();

						dataCount = 0;
						blockCount++;
					}

					Thread.Sleep(Math.Max(intervalMs-HH309Communicate.waitResponseMs,0));
				} while (!Console.KeyAvailable);
				port.close();
			}
		}

		static void Main(string[] args) {
			(new HH309Logger()).Run();
		}
	}

	class HH309Communicate {
		public const int waitResponseMs = 1000;
		private SerialPort port;
		public HH309Communicate(string comStr) {
			port = new SerialPort(comStr, 9600, Parity.None, 8, StopBits.One);
			port.Open();
			if (!port.IsOpen) {
				port = null;
				Console.WriteLine("Cannot open port");
			}
		}
		~HH309Communicate() {
			if (port != null) {
				port.Close();
			}
		}
		public bool isOpen {
			get {
				return port.IsOpen;
			}
		}
		public void close() {
			port.Close();
			port = null;
		}
		const int bufferSize = 1024;
		byte[] buf = new byte[bufferSize];
		public float[] getTemperature() {
			var temps = new float[4];
			// communicate
			port.Write("A");
			Thread.Sleep(waitResponseMs);
			try {
				port.Read(buf, 0, 1024);
				for (int ch = 0; ch < 4; ch++) {
					temps[ch] = (256 * buf[7 + 2 * ch] + buf[8 + 2 * ch]) / 10.0f;
				}
				return temps;
			} catch {
				return null;
			}
		}
	}
}
