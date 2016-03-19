
import glob;
import time;
import matplotlib.pyplot as plt;
import numpy as np;
import sys;

sys.stdout.write("config file name >");
sys.stdout.flush();
dic = {};
cfgName = sys.stdin.readline();
fin = open(cfgName.strip(),"r");
for line in fin:
	elem = [word.strip() for word in line.split("=")]
	if(len(elem)==2):
		dic[elem[0]] = elem[1]
fin.close();

maxChannel = int(dic["channel"])
prefix = dic["prefix"]
sleep = int(dic["interval"])
path = dic["path"]
yname = dic["ylabel"]
logscale = ((int(dic["logscale"]))==1)
dataInterval = int(dic["dataInterval"])

name = [];
for ch in range(maxChannel):
	name.append(dic["ch"+str(ch+1)])

print("#channel = {0}".format(maxChannel));
for ch in range(maxChannel):
	print(" - ch{0} : {1}".format(ch,name[ch]))
print("prefix = {0}".format(prefix));
print("interval = {0} sec".format(sleep));
print("save image to = {0}".format(path));
print("yname = {0}".format(yname))
print("use logscale = {0}".format(logscale))
print("data acq interval = {0}".format(dataInterval))


updated = -1;
data = [];

for ind in range(maxChannel):
    data.append([]);

while True:
    flist = glob.glob(prefix+"_*.txt");
    flist = [[int(fn.split("_")[1].split(".")[0]),fn] for fn in flist];
    flist = [fl for fl in flist if (fl[0]>updated)]
    flist.sort()
    flist = flist[:-1]
    if(len(flist)>0):
        print(flist)
        for fl in flist:
            fn = fl[1];
            fin = open(fn,"r");
            for line in fin:
                vals = [float(val) for val in line.split(" ")]
                for ind in range(maxChannel):
                    data[ind].append(vals[ind]);
        updated = flist[-1][0];

        xd = np.array(range(len(data[0])))/60.0/60.0;
        for ind in range(maxChannel):
            plt.plot(xd,data[ind],label=name[ind]);
        plt.xlabel("time (h)");
        plt.ylabel(xname)
        if(logscale):
        	plt.yscale("log");        plt.legend(loc = "upper left")
        plt.savefig(path+"_"+prefix+"_all.png")
        plt.xlim(max(max(xd)-10,0),max(xd))
        plt.savefig(path+"_"+prefix+"_recent.png")
        plt.xlim(max(max(xd)-3,0),max(xd))
        plt.savefig(path+"_"+prefix+"_very_recent.png")
        plt.clf()
    else:
        print("nothing to update")
    time.sleep(sleep)
    
