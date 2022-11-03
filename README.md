# UnrealDemoScanner
Unreal Demo Scanner for CS 1.6

###### How to use:
1. Run UnrealDemoScanner.exe (LATEST VERSION!)
2. Drag&Drop .dem file in Console window or enter path manually 
3. Wait for analyze

###### Как просканировать демку:
1. Запустить UnrealDemoScanner.exe (ПОСЛЕДНЕЙ ВЕРСИЕЙ!)
2. Перенести демо файл в консоль или ввести путь вручную
3. Нажать Enter
4. Ждать завершения анализа
5. По завершению анализа можно выбрать дополнительные команды для анализа демки

###### Results:
**If demo has multiple [DETECTION] (MORE THAN 5-10 PER DEMO) player with probability of 100% uses illegal programs!

**If demo has multiple [WARN] (MORE THAN 20-30 PER DEMO)  player with probability of 90% uses illegal programs!

###### Результаты сканирования:
**Если демо содержит множество [ОБНАРУЖЕНО] (Более чем 5-10 за демку) игрок с вероятностью 100% использует запрещенные программы.

**Если демо содержит множество [ПРЕДУПРЕЖДЕНИЕ] (Более чем 20-30 за демку) игрок почти со 100% вероятностью использует запрещенные программы!


Command line:

'-debug' - enable debug messages

'-alive' - force user alive at demo start

'-noteleport' - ignore user teleports

'-dump' - dump all demo to readable text format


**Detections description:**


AIM TYPE 1 - Attack delay - Задержка атаки

AIM TYPE 2 - Autoattack - Автоатака

AIM TYPE 3 - Резкие рывки прицела(не заверешно)

AIM TYPE 4 - Fake lag - Фейк лаг

AIM TYPE 5 - Smooth angles/etc - Сглаживание углов - Изменение сенса программное

AIM TYPE 6 - HPP Autoattack - Автоатака из HPP v5

AIM TYPE 7 - HPP Trigger bot - Триггер бот или жесткий аимбот из HPP

AIM TYPE 8 - Universal AIMBOT detection - Универсальный детект аимботов

TRIGGER BOT - OLD AIM / TRIGGER HACK - Старые аим хаки
