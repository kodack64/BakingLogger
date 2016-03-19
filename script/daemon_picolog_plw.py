
from struct import *
import matplotlib.pyplot as plt
import glob
import os.path
import numpy as np
import time
import shutil


def readData(fin,count,format):
	return unpack(format,fin.read(count))[0]

def convert():
	flist = glob.glob("*.PLW");
	for file in flist:
		# read data
		fin = open(file,"rb")
		fin.seek(46)
		channelCount = readData(fin,4,"L")
		fin.seek(550)
		sampleCount = readData(fin,4,"L")
		dat = [];
		for ind in range(channelCount):
			dat.append([]);
		fin.seek(1684)

#		print("name :{0}".format(file))
#		print("#channel:{0}".format(channelCount))
#		print("#sample :{0}".format(sampleCount))

		for ind in range(sampleCount):
			readData(fin,4,"L");
			for ch in range(channelCount):
				val = readData(fin,4,"f")
				dat[ch].append(val)
		fin.close()

		# read channel name
		name = [];
		fin = open(file,"r",errors="ignore");
		for line in fin:
			elem = line.split("=");
			if(elem[0]=="Name"):
				name.append(elem[1]);
		name.sort();
		fin.close();

		# output csv
		fout = open(file.replace(".PLW",".csv"),"w")
		for ch in range(channelCount):
			fout.write(name[ch].strip())
			if(ch+1<channelCount):
				fout.write(" ")
			else:
				fout.write("\n");
		for ind in range(sampleCount):
			for ch in range(channelCount):
				fout.write(str(dat[ch][ind]))
				if(ch+1<channelCount):
					fout.write(" ")
				else:
					fout.write("\n");
		fout.close();
	print("analyze {0} files".format(len(flist)))

def plot():
	ch = 8
	ind = 1
	dat = [];
	for i in range(ch):
		dat.append([]);
	name = []
	fst = 0
	while os.path.exists("data{0}.csv".format(ind)):
# 400 is dummy
#		print("data{0}.csv".format(ind))
		fin = open("data{0}.csv".format(ind),"r");
		line = fin.readline()
		if(fst==0):
			fst=1;
			name = line.split(" ")
			name = [n.strip() for n in name]
			
		for line in fin:
			elem = [float(ele) for ele in line.split(" ")]
			if(len(elem)==ch):
				for c in range(ch):
					dat[c].append(elem[c])
		fin.close();
		ind += 1
		
	print("gather {0} files".format(ind))

	# output graph
	maxx = 0
	for c in range(ch):
		xdat = np.array(range(len(dat[c])));
		xdat= xdat/60.0/60.0;
		plt.plot(xdat,dat[c],label=name[c])
		maxx = max(maxx,(max(xdat)))
	plt.title("temperature log")
	plt.xlabel("time(hour)");
	plt.ylabel("temperature(celcius)")
	plt.legend(loc=0,fontsize="small")
	plt.xlim(0,maxx)
	plt.savefig("_plot_all.png")
	plt.xlim(maxx-10,maxx)
	plt.savefig("_plot_recent.png")
	plt.xlim(maxx-3,maxx)
	plt.savefig("_plot_very_recent.png")
	plt.clf()
	print("plot and save")

def getfile():
	flist = glob.glob("C:\\Users\\Yasunari\\Google ドライブ\\実験\\bake_monitor\\*.PLW")
	for file in flist:
		shutil.copy(file,"./")
	print("get {0} files".format(len(flist)))

def exportfile():
	flist = glob.glob("*.png")
	for file in flist:
		shutil.copy(file,"C:\\Users\\Yasunari\\Google ドライブ\\実験\\bake_monitor")
	print("send {0} images".format(len(flist)))


tri = 0
while True:
	print("trial:{0}".format(tri))
	getfile()
	convert()
	plot()
	exportfile()
	wt = 60*15
	print("wait {0} sec".format(wt))
	time.sleep(wt);
	tri+=1
	
	