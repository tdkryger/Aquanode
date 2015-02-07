#!/usr/bin/python
# -*- coding: utf-8 -*-
# (c) 2015 Thomas D. Kryger
# Serial data to and from Arduino Master

import serial
import const
#import argparse
import datetime
import MySQLdb as mdb
import sys


DODEBUG = 1

#parser = argparse.ArgumentParser()
#parser.add_argument("serialName", help="The name of the serial port")
#args = parser.parse_args()

# Serial commands
SERIAL_CMD_CONFIG_REQUEST               = 'C'
SERIAL_CMD_CONFIG			= 'c'
SERIAL_CMD_DIMMER_CONFIG_REQUEST	= 'D'
SERIAL_CMD_DIMMER_CONFIG		= 'd'
SERIAL_CMD_TIME_REQUEST			= 'T'
SERIAL_CMD_TIME_UPDATE			= 't'
SERIAL_DATA_STRING			= 'Q'
DEFAULT_CEST_STRING                     = "410302120"
DEFAULT_CET_STRING                      = "410310060"

MYSQL_SERVER                            = "localhost"
MYSQL_USER                              = "anuser"
MYSQL_PASSWD                            = "mWZwbVCruMsh"

serName = "/dev/ttyACM0"

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




ser = serial.Serial(serName, 115200, timeout=1) # Establish the connection on a specific port
print("Datapacket types:")
print("\t" + SERIAL_CMD_CONFIG_REQUEST + "\tConfig request")
print("\t" + SERIAL_CMD_DIMMER_CONFIG_REQUEST + "\tDimmer Config request")
print("\t" + SERIAL_CMD_TIME_REQUEST + "\tTime request")
print("\t" + SERIAL_DATA_STRING + "\tDatapacket")
print("")
while True:
    print("Start of loop. Lets wait for some data")
    serialString = ser.readline()
    if not serialString:
        print("\t\tNo data")
    else:
        print("\tDatastring: " + serialString)
        dataParts = serialString.split(';')
        print("\tPacket type: [" + dataParts[0].strip() + "]")
    
        # A kingdom for a switch structure
        if dataParts[0].strip() == SERIAL_CMD_CONFIG_REQUEST:
            print("\t\tTODO: Handle Config Request")
        if dataParts[0].strip() == SERIAL_CMD_DIMMER_CONFIG_REQUEST:
            print("\t\tTODO: Handle Dimmer Config")
        if dataParts[0].strip() == SERIAL_CMD_TIME_REQUEST:
            print("\t\tGot request for datetime. Sending to arduino")
            today = datetime.datetime.utcnow()
            dateTimePart = today.strftime("%H%M%s%Y%m%d")
            #TODO: Get the 2 sets of timezone data from mysql
            returnString = dateTimePart + DEFAULT_CEST_STRING + DEFAULT_CET_STRING +'\n'
            print("Sending string to Arduino: " + returnString)
            ser.write(unicode(returnString))
            ser.flush() # it is buffering. required to get the data out *now*
        if dataParts[0].strip() == SERIAL_DATA_STRING:
            print("\t\tData packet. Saving it to MySQL")
            parseSerialData(serialString)
        print("---")
        print("")
        
        
        
    
