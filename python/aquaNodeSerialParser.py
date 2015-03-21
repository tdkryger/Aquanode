#!/usr/bin/python
# -*- coding: utf-8 -*-
# (c) 2015 Thomas D. Kryger
# Serial data to and from Arduino Master

import serial
import time
import MySQLdb as mdb

locations=[
    'COM3',
    'COM4',
    '/dev/ttyACM0',
    '/dev/ttyAMA0',
    '/dev/ttyUSB0',
    '/dev/ttyUSB1',
    '/dev/ttyUSB2',
    '/dev/ttyUSB3',
    '/dev/ttyS0',
    '/dev/ttyS1',
    '/dev/ttyS2',
    '/dev/ttyS3']

MYSQL_SERVER                            = "localhost"
MYSQL_USER                              = "anuser"
MYSQL_PASSWD                            = "mWZwbVCruMsh"

def local_time_offset(t=None):
    # Return offset of local zone from GMT, either at present or at time t.
    # python2.3 localtime() can't take None
    if t is None:
        t = time.time()

    if time.localtime(t).tm_isdst and time.daylight:
        return -time.altzone
    else:
        return -time.timezone


def getArduinoSerial():
    for device in locations:
        try:
            print "Trying...",device
            return serial.Serial(device, 115200, timeout=1)
        except:
            print "Failed to connect on",device
            return null

def checkForNodeConfig(nodeid):
    con = mdb.connect(MYSQL_SERVER, MYSQL_USER, MYSQL_PASSWD, 'aquanode');
    cur = con.cursor()
    cur.execute("SELECT count(*) FROM nodes WHERE nodeid=" + nodeid + ";")
    with con:
        count_row = cur.fetchone()
        count = count_row[0]
        if count == 0:
            # Node not registered, so we do it now
            sqlInsert = ''.join(["INSERT INTO `nodes`(`nodeid`,`dimmer1OnMinute`,`dimmer1OnHour`,`dimmer1OffMinute`,`dimmer1OffHour`,`dimmer1Delay`,`dimmer1MaxPWM`,`dimmer2OnMinute`,`dimmer2OnHour`,`dimmer2OffMinute`,`dimmer2OffHour`,`dimmer2Delay`,`dimmer2MaxPWM`,`dimmer3OnMinute`,`dimmer3OnHour`,`dimmer3OffMinute`,`dimmer3OffHour`,`dimmer3Delay`,`dimmer3MaxPWM`,`configVersion`,`lcdDefaultColor`,`ECProbeType`) VALUES (",
                                 nodeid, ", 15,11,45,20,15,100,30,11,30,20,15,100,0,11,0,21,15,100, 1.0, 0, 0)"])
            cur.execute(sqlInsert)

def is_number(s):
    try:
        float(s)
        return True
    except ValueError:
        pass
 
    try:
        import unicodedata
        unicodedata.numeric(s)
        return True
    except (TypeError, ValueError):
        pass
 
    return False        

def checkSensorData(data):
    if is_number(data):
        return data
    else:
        return 0
    
def parseSerialData(data):
    parts = data.split(';')
    if len(parts) > 12:
        nodeId = parts[1]
        disolvedOxygen = checkSensorData(parts[2])
        temperature = checkSensorData(parts[3])
        currentPWM1 = checkSensorData(parts[4])
        currentPWM2 = checkSensorData(parts[5])
        currentPWM3 = checkSensorData(parts[6])
        currentPWM4 = checkSensorData(parts[7])
        ph = checkSensorData(parts[8])
        electricalConductivity = checkSensorData(parts[9])
        salinity = checkSensorData(parts[10])
        gravityOfSeawater = checkSensorData(parts[11])
        totalDissolvedSolids = checkSensorData(parts[12])
        # Save data to MySQL
        checkForNodeConfig(nodeId)
        con = mdb.connect(MYSQL_SERVER, MYSQL_USER, MYSQL_PASSWD, 'aquanode');
        with con:
            cur = con.cursor()
            sqlInsert = ''.join(["INSERT INTO `nodeData`(`nodeid`,`pwm1`,`pwm2`,`pwm3`,`pwm4`,`temperature`,`disolvedOxygen`,`pH`,`electricalConductivity`,`salinity`,`gravityOfSeawater`,`totalDissolvedSolids`) VALUES (",
                nodeId, ",", currentPWM1, ",", currentPWM2, ",", currentPWM3, ",", currentPWM4, ",", temperature, ",", disolvedOxygen, ",", ph, ",", electricalConductivity, ",", salinity, ",", gravityOfSeawater, ",", totalDissolvedSolids, ");"])
            cur.execute(sqlInsert)



while True:
    arduino = getArduinoSerial()
    while arduino == null:
        arduino = getArduinoSerial()
        time.sleep(5)
        
    serialString = arduino.readline()
    if serialString:
        print("Datastring: " + serialString)
        if serialString.startswith('t'):
            # Time request
            ticks = time.time() + local_time_offset()
            returnString = 'T' + str(int(ticks)) + "TTTTT"
            print("\t\tReturnstring: " + returnString)
            arduino.write(returnString)
            #arduino.flush()
        if serialString.startswith('d'):
            # Data from a node
            
            
        
