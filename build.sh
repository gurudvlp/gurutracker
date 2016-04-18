#!/bin/bash
########################################################################
#
#	This script is meant to build all of resources needed to run
#	gurutracker
#
########################################################################

tar -cvf gurumod/Resources/Samples.tar samples
tar -cvf gurumod/Resources/Interfaces.tar Interfaces
tar -cvf gurumod/Resources/Tracks.tar Tracks

rm gurumod/Resources/bin/* -rf
cp bin/Debug gurumod/Resources/bin

tar -cvf gurumod/Resources/Bin.tar bin

mdtool build gurutracker.sln > buildlog.txt
