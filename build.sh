#!/bin/bash
########################################################################
#
#	This script is meant to build all of resources needed to run
#	gurutracker
#
########################################################################

./Resources/build-resources.sh

mdtool build gurutracker.sln > buildlog.txt
