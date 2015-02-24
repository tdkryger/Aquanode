import serial
import time

#serName = "/dev/ttyACM0"
serName = "COM4"

ser =  serial.Serial(serName, 115200, timeout=1)
while True:
    serialString = ser.readline()
    if serialString:
        print("\tDatastring: " + serialString)
        if serialString.startswith('t'):
            # It returns UTC. Need localtime
            ticks = time.time()
            returnString = 'T' + str(int(ticks)) + "    "
            print("Returnstring: " + returnString)
            print("Returnstring: " + unicode(returnString))
            ser.write(returnString)
            #ser.flush()
            
        
