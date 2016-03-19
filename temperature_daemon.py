
import glob;
import time;
import matplotlib.pyplot as plt;
import numpy as np;

updated = -1;
maxChannel = 8;
data = [];
interval = 1;
sleep = 60*15;
path = "C:\\Users\\koasilab\\Google ドライブ\\実験\\bake_monitor\\"
name = ["ch1_workdesk_bottom","ch2_workdesk_top","ch3_whiteboard_middle",
        "ch4_large_window_door","ch5_workdesk_middle","ch6_top_window",
        "ch7_whiteboard_bottom","ch8_whiteboard_top"]

for ind in range(maxChannel):
    data.append([]);

while True:
    flist = glob.glob("tc08_*.txt");
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
                vals = [float(val) for val in line.split(" ")[1:]]
                for ind in range(maxChannel):
                    data[ind].append(vals[ind]);
        updated = flist[-1][0];

        xd = np.array(range(len(data[0])))/60.0/60.0;
        for ind in range(maxChannel):
            plt.plot(xd,data[ind],label=name[ind]);
        plt.xlabel("time (h)");
        plt.ylabel("temperature (C)")
        plt.legend(loc = "upper left")
        plt.savefig(path+"_all.png")
        plt.xlim(max(max(xd)-10,0),max(xd))
        plt.savefig(path+"_recent.png")
        plt.xlim(max(max(xd)-3,0),max(xd))
        plt.savefig(path+"_very_recent.png")
        plt.clf()
    else:
        print("nothing to update")
    time.sleep(sleep)
    
