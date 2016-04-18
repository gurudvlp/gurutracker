#!/bin/bash
########################################################################
#
#	This script builds archives of the raw resources.
#
########################################################################

mkdir -p gurumod/Resources/bin
cd Resources
tar -cvf ../gurumod/Resources/Samples.tar samples
tar -cvf ../gurumod/Resources/Interfaces.tar Interfaces
tar -cvf ../gurumod/Resources/Tracks.tar Tracks


cd ..
rm gurumod/Resources/bin/* -rf
cp Resources/Libraries/* bin/Debug
cp bin/Debug/* gurumod/Resources/bin

cd gurumod/Resources
tar -cvf Bin.tar bin
