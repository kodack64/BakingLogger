using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using TC08example;

namespace BakingLogger {

	class TC08Logger {

		// Close device when window is closed
		[DllImport("Kernel32")]
		static extern bool
			SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		delegate bool HandlerRoutine(CtrlTypes CtrlType);
		HandlerRoutine myHandlerDele;
		public enum CtrlTypes {
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}
		bool myHandler(CtrlTypes ctrlType) {
			Imports.TC08Stop(handle);
			Imports.TC08CloseUnit(handle);
			return false;
		}


		const int PICO_OK = 1;
		short USBTC08_MAX_CHANNELS = 8;
		Boolean isStreaming = true;
		static int intervalAcquireMs = 1000*5;
		static int dataBlock = 100;
		short handle=0;
		int blockCount = 0;
		unsafe void Run() {
			myHandlerDele = new HandlerRoutine(myHandler);
			SetConsoleCtrlHandler(myHandlerDele, true);

			// retrieve last final datablock
			DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
			var files = di.GetFiles("tc08_*.txt", SearchOption.TopDirectoryOnly);
			char[] delim = {'.','_'};
			foreach (var file in files) {
				var fileind = Int32.Parse(file.Name.Split(delim)[1]);
				blockCount = Math.Max(fileind+1, blockCount);
			}
			if (blockCount > 0) {
				Console.WriteLine("Continue logging from block index {0]", blockCount);
			}

			// get handle
			handle = Imports.TC08OpenUnit();
			if (handle == 0) {
				Console.WriteLine("Unable to open : {0}", handle);
				return;
			} else {
				Console.WriteLine("Open device : {0}", handle);
			}

			// device info
			var line = new System.Text.StringBuilder(256);
			Imports.TC08GetFormattedInfo(handle, line, 256);
			Console.WriteLine("{0}", line);

			// set channels
			for (short channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++) {
				Imports.TC08SetChannel(handle, channel, 'K');
			}

			float[,] data = new float[USBTC08_MAX_CHANNELS+1,dataBlock];
			int dataCount = 0;
			if (!isStreaming) {
				// loop single acquire until key hit
				do {
					float[] tempbuffer = new float[USBTC08_MAX_CHANNELS];
					short overflow;
					short status = Imports.TC08GetSingle(handle, tempbuffer, &overflow, Imports.TempUnit.USBTC08_UNITS_CENTIGRADE);
					if (status == PICO_OK) {
						for (short chan = 1; chan <= USBTC08_MAX_CHANNELS; chan++) {
							data[chan, dataCount] = tempbuffer[chan];
							Console.Write("{0:0.0000} ", tempbuffer[chan]);
						}
						Console.WriteLine();
					}

					dataCount++;
					if (dataCount >= dataBlock) {
						dataCount = 0;
						var sw = new StreamWriter("tc08_"+blockCount.ToString()+".txt");
						for (int i = 0; i < dataBlock; i++) {
							for (int c = 0; c < USBTC08_MAX_CHANNELS+1; c++) {
								sw.Write("{0:0.0000}", data[c, i]);
								if (c == USBTC08_MAX_CHANNELS) sw.WriteLine();
								else sw.Write(" ");
							}
						}
						sw.Close();
						blockCount++;
					}

					Thread.Sleep(intervalAcquireMs);
				} while (Win32Interop._kbhit() == 0);
				Imports.TC08Stop(handle);
			} else {
				// streaming acquire until key hit
				// researve unsafe array
				float[][] tempbuffer = new float[USBTC08_MAX_CHANNELS + 1][];
				int buffer_size = 1024;
				PinnedArray<float>[] pinned = new PinnedArray<float>[buffer_size];
				for (short channel = 0; channel <= USBTC08_MAX_CHANNELS; channel++) {
					tempbuffer[channel] = new float[buffer_size];
					pinned[channel] = new PinnedArray<float>(tempbuffer[channel]);
				}
				int[] times_ms_buffer = new int[buffer_size];
				short[] overflow = new short[USBTC08_MAX_CHANNELS+1];

				// get interval ms
				short actual_interval_ms = Imports.TC08Run(handle, Imports.TC08GetMinIntervalMS(handle));
				if (actual_interval_ms <= 0) {
					Console.WriteLine("interval is too short");
				} else {
					do {
						// acquire
						int numberOfSamples = 0;
						for (short chan = 0; chan <= USBTC08_MAX_CHANNELS; chan++) {
							numberOfSamples = Imports.TC08GetTemp(handle, tempbuffer[chan], times_ms_buffer, buffer_size,
								out overflow[chan], chan, Imports.TempUnit.USBTC08_UNITS_CENTIGRADE, 0);
						}

						if (numberOfSamples > 0) {
							Console.Write("{0} {1} : ", blockCount, dataCount);
							for (short chan = 0; chan <= USBTC08_MAX_CHANNELS; chan++) {
								float ave = 0;
								for (int i = 0; i < numberOfSamples; i++) {
									ave += pinned[chan].Target[i];
								}
								ave /= numberOfSamples;
								data[chan, dataCount] = ave;
								Console.Write("{0:0.0000} ", ave);
							}
							Console.WriteLine();
							dataCount++;
						}

						if (dataCount >= dataBlock) {
							dataCount = 0;
							string filename = "tc08_" + blockCount.ToString() + ".txt";
							var sw = new StreamWriter(filename);
							for (int i = 0; i < dataBlock; i++) {
								for (int c = 0; c <= USBTC08_MAX_CHANNELS; c++) {
									sw.Write("{0:0.0000}", data[c, i]);
									if (c == USBTC08_MAX_CHANNELS) sw.WriteLine();
									else sw.Write(" ");
								}
							}
							sw.Close();
							Console.WriteLine("Write "+filename);
							blockCount++;
						}

						Thread.Sleep(intervalAcquireMs);
					} while (!Console.KeyAvailable);
				}
				Imports.TC08Stop(handle);

				// release arrays
				foreach (PinnedArray<float> p in pinned) {
					if (p != null) {
						p.Dispose();
					}
				}
			}
			Imports.TC08CloseUnit(handle);
		}

		static unsafe void Main() {
			(new TC08Logger()).Run();
		}
	}
}