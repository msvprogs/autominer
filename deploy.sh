#!/bin/bash

readonly MINER_DIR=/home/yamamoto/Mine/automine-new

readonly TEAMCITY_URL=http://192.168.1.189:84
readonly TEAMCITY_LOGIN=serv
readonly TEAMCITY_PASSWORD=NzrPodGzoVbnl7P5ZArS
readonly TEAMCITY_PROJECT=AutoMiner

# Download artifacts list
artifacts=$(curl $TEAMCITY_URL/app/rest/builds/project:$TEAMCITY_PROJECT/artifacts/children -s --basic -u $TEAMCITY_LOGIN:$TEAMCITY_PASSWORD | xpath -q -e "//file[contains(@name,'.zip')]/content/@href" | awk -F\" '{WORD=split($2,a,"\"");print a[1]}') 

# Kill 'em all
echo Stopping services...
killall dotnet
killall mono

# Deploy new versions
cd $MINER_DIR
for artifact in $artifacts; do
	echo Deploying $artifact...
	wget $TEAMCITY_URL$artifact --user $TEAMCITY_LOGIN --password $TEAMCITY_PASSWORD -q
	if [[ $artifact == coin-info-* ]]; then
		targetDir=coin-info
	elif [[ $artifact == control-center-* ]]; then
		targetDir=control-center
	elif [[ $artifact == frontend-* ]]; then
		targetDir=frontend
	elif [[ $artifact == rig-* ]]; then
		targetDir=rig
	else
		continue
	fi
	7z x $artifact -o{./$targetDir} -y
	rm $artifact
	echo Deployed $artifact
done

# Restart all services
echo Restarting services...
cd coin-info
screen -S CoinInfo -d -m dotnet Msv.AutoMiner.CoinInfoService.dll
cd ../control-center
screen -S ControlCenter -d -m dotnet Msv.AutoMiner.ControlCenterService.dll
cd ../frontend
screen -S FrontEnd -d -m dotnet Msv.AutoMiner.FrontEnd.dll
cd ../rig
screen -S Rig -d -m mono Msv.AutoMiner.Rig.exe

echo Deploy completed!