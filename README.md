# UnrealDemoScanner
Unreal Demo Scanner for CS 1.6

###### How to use:
1. Run UnrealDemoScanner.exe (LATEST VERSION!)
2. Drag&Drop .dem file in Console window or enter path manually 
3. Wait for analyze
###### Results:
**If demo has multiple [DETECTION] (MORE THAN 5-15 PER DEMO) player with probability of 100% uses illegal programs!**

**If demo has multiple [WARN] (MORE THAN 15-25 PER DEMO)  player with probability of 90% uses illegal programs!**




**ClearDemos/ClearDemos2** - for test false detections before adding it to code. This directory contain only clear demos ( without using hacks )
**CheatDemos/CheatDemos2** - Demos with AIM/BHOP/etc

Main code:
https://github.com/2020karaulov2020/UnrealDemoScanner/blob/DemoScanner/SourceCode/UnrealDemoScanner.cs 


Command line:

'-debug' - enable debug messages

'-alive' - force user alive at demo start

'-noteleport' - ignore user teleports

'-dump' - dump all demo to readable text format


**Detections description:**


AIM TYPE 1 - Attack delay

AIM TYPE 2 - Autoattack

AIM TYPE 3 - (Disabled for rewrite)

AIM TYPE 4 - Fake lag

AIM TYPE 5 - Smooth angles/etc

AIM TYPE 6 - HPP Autoattack

AIM TYPE 7 - HPP Trigger bot

AIM TYPE 8 - Universal AIMBOT detection

TRIGGER BOT - OLD AIM / TRIGGER HACK


DEMO PACK YOU CAN DOWNLOAD HERE :
 https://github.com/UnrealKaraulov/UnrealDemoScanner/tree/1935a1c9ba62c998bdf74125866728b0ddf4b58b
 https://drive.google.com/drive/folders/1VodMw7uSYcFXg2Ch4-XOYUMJMPQSroLE?usp=sharing
 https://disk.yandex.by/d/LYgrwo2Ysxj97g
