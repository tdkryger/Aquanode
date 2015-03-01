import serial
import time

locations=[
    'COM3',
    'COM4',
    '/dev/ttyACM0'
    '/dev/ttyUSB0',
    '/dev/ttyUSB1',
    '/dev/ttyUSB2',
    '/dev/ttyUSB3',
    '/dev/ttyS0',
    '/dev/ttyS1',
    '/dev/ttyS2',
    '/dev/ttyS3']

def local_time_offset(t=None):
    """Return offset of local zone from GMT, either at present or at time t."""
    # python2.3 localtime() can't take None
    if t is None:
        t = time.time()

    if time.localtime(t).tm_isdst and time.daylight:
        return -time.altzone
    else:
        return -time.timezone


#serName = "COM4"
#serName = "/dev/ttyACM0"

for device in locations:
    try:
        print "Trying...",device
        arduino = serial.Serial(device, 115200, timeout=1)
        break
    except:
        print "Failed to connect on",device



#arduino =  serial.Serial(serName, 115200, timeout=1)
while True:
    serialString = arduino.readline()
    if serialString:
        print("Datastring: " + serialString)
        if serialString.startswith('t'):
            ticks = time.time() + local_time_offset()
            returnString = 'T' + str(int(ticks)) + "TTTTT"
            print("\t\tReturnstring: " + returnString)
            arduino.write(returnString)
            #arduino.flush()
            
        
