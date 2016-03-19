using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace M601GC {
	class M601GCLogger {
		const string comName = "COM7";
		const int intervalMs = 5000;
		const int dataBlock = 100;
		char[] delim = { ',' };
		public void Run() {
			SerialPort port = new SerialPort(comName,9600,Parity.None,8,StopBits.One);
			port.NewLine = "\r";
			port.Open();
			if (!port.IsOpen) {
				Console.WriteLine("Cannot open port");
				return;
			}
			int dataCount=0;
			int blockCount = 0;
			double[] data = new double[dataBlock];
			do {
				try {
					port.Write("$PRD\r");
					string rcv = port.ReadLine();
					var elem = rcv.Substring(1).Split(delim);
					var status = Int32.Parse(elem[0]);
					var value = Double.Parse(elem[1]);
					data[dataCount] = value;
					Console.WriteLine("{0} {1} : {2} Pa",blockCount,dataCount,value);
					dataCount++;

					if (dataCount >= dataBlock) {
						var fname = "M-601GC_" + blockCount.ToString() + ".txt";
						StreamWriter sw = new StreamWriter(fname);
						for (int ind = 0; ind < dataBlock; ind++) {
							sw.WriteLine(data[ind]);
						}
						sw.Close();
						dataCount = 0;
						blockCount++;
					}
				}catch{
					Console.WriteLine("Error in communication");
					break;
				}
				Thread.Sleep(intervalMs);
			} while (!Console.KeyAvailable);
			port.Close();
			port.Dispose();
		}
		static void Main(string[] args) {
			(new M601GCLogger()).Run();
		}
	}
}
