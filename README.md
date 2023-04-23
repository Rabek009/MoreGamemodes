# More Gamemodes

## About the mod

More Gamemodes is the Among Us mod that addes new gamemodes. Only host need to have mod to work.<br>
Among Us version: 2023.3.28<br>

## Hotkeys

### Host only
| HotKey              | Function                       | Usable Scene |
| ------------------- | ------------------------------ | ------------ |
| `Shift`+`L`+`Enter` | End the game immediatly        | In Game      |
| `Shift`+`M`+`Enter` | Skip meeting to end            | In Game      |
| `Shift`+`Z`+`Enter` | Make yourself dead             | In Game      |
| `C`                 | Cancel game start              | In Countdown |
| `Shift`             | Start the game immediately     | In Countdown |

### Mod client only
| HotKey | Function              | Usable Scene |
| ------ | --------------------- | ------------ |
| `Ctrl` | Noclip throught walls | In Lobby     |

## Chat commands

### Host only
| Command                                         | Function                         |
| ----------------------------------------------- | -------------------------------- |
| /changesetting OPTION VALUE<br>/cs OPTION VALUE | Change setting value             |
| /gamemode GAMEMODE<br>/gm GAMEMODE              | Change current gamemode          |
| /kick PLAYER_ID                                 | Kick player from lobby           |
| /ban PLAYER_ID                                  | Ban player from lobby            |
| /announce MESSAGE<br>/ac MESSAGE                | Send message even if you're dead |

### All clients
| Command                                         | Function                            |
| ----------------------------------------------- | ----------------------------------- |
| /color COLOR_ID<br>/colour COLOR_ID             | Changes your color                  |
| /name NAME                                      | Changes your name                   |
| /h gm, /h gamemode<br>/help gm, /help gamemode  | Show gamemode description           |
| /h i, /h item<br>/help i, /help item            | Show item description(Random Items) |
| /stop                                           | Use stop item(Random Items)         |
| /now<br>/n                                      | Show current options                |
| /id colors<br>/id colours                       | Show color ids                      |
| /id players                                     | Show player ids                     |
| /commands<br>/cm                                | Show commands list                  |

## Game options
| Name                   |
| ---------------------- |
| Gamemode               |
| Can use /color command |
| can use /name command  |

## Gamemodes

### Hide and seek
Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences and sabotages. Impostors are red,<br> crewmates are blue. Depending on options impostors can vent and closing doors. Hide and seek is working with special roles.<br>

#### Game options
| Name                            |
| ------------------------------- |
| Teleport on start               |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |

### Shift and seek
Everyone is engineer or shapeshifter. Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences<br>
and sabotages. Depending on options impostors can vent and closing doors. Impostors are visible to others or not. Impostors must shapeshift<br>
into person they want to kill.<br>

#### Game options
| Name                            |
| ------------------------------- |
| Teleport on start               |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |
| Impostors are visible           |

### Bomb tag
Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports,<br>
meetings, sabotages or venting. Click kill button to give bomb away. Last standing alive wins.<br>

#### Game options
| Name                     |
| ------------------------ |
| Teleport on start        |
| Teleport after explosion |
| Explosion delay          |
| Players with bomb        |
| Max players with bomb    |

### Random items
Crewmates get items by doing tasks, impostors by killing. Items are given randomly. Pet your pet to use item. Some items(knowledge,<br>
gun, illusion, finder and rope) works on nearest player. Other rules are just like in classic game. Random items works with special roles.<br>

#### Items

##### Time slower
Increases discussion and voting time. Only crewmates can get this item.<br>

###### Game options
| Name                     |
| ------------------------ |
| Enable time slower       |
| Discussion time increase |
| Voting time increase     |


##### Knowledge
You can investigate nearest player. If this player is good, he has green name. If this player is bad, he has red name. Depending on options<br> target can see your name in black after this. Only crewmates can get this item.<br>

###### Game options
| Name                 |
| -------------------- |
| Enable knowledge     |
| Crewmates see reveal |
| Impostors see reveal |

##### Shield
Grant yourself a shield for some time. You cannot be killed during this time. Depending on options you can see who tried to kill you.<br>
Only crewmates can get this item.

###### Game options
| Name               |
| ------------------ |
| Enable shield      |
| Shield duration    |
| See who tried kill |

##### Gun
You can kill impostors. Depending on options you can kill crewmates. Only crewmates can get this item.<br>

###### Game options
| Name                   |
| ---------------------- |
| Enable gun             |
| Can kill crewmates     |
| Misfire kills crewmate |

##### Illusion
If nearest player is impostor, he kills you. Otherwise you see that player's name in green. Only crewmates can get this item.<br>

###### Game options
| Name            |
| --------------- |
| Enable illusion |

##### Radar
Do reactor flash if impostor is nearby. Only crewmates can get this item.<br>

###### Game Options
| Name         |
| ------------ |
| Enable radar |
| Radar range  |

##### Time speeder
Decreases discussion and voting time. Only impostors can get this item.<br>

###### Game options
| Name                     |
| ------------------------ |
| Enable time speeder      |
| Discussion time decrease |
| Voting time decrease     |

##### Flash
All crewmates go blind for few seconds, but impostor vision is decreased for that time. Only impostors can get this item.<br>

###### Game options
| Name                     |
| ------------------------ |
| Enable flash             |
| Flash duration           |
| Impostor vision in flash |

##### Hack
Prevent crewmates from doing anything(reporting, using items, doing some tasks, using role ability, venting, sabotaging,<br>
calling meetings). Depending on options it affects impostors too. Only impostors can get this item.<br>

###### Game options
| Name                   |
| ---------------------- |
| Enable hack            |
| Hack duration          |
| Hack affects impostors |

##### Camouflage
Make everyone look the same for some time. You can't use camouflage during camouflage. Only impostors can get this item.<br>

###### Game options
| Name                |
| ------------------- |
| Enable camouflage   |
| Camouflage duration |

##### Multi teleport
Teleports everyone to you. Only impostors can get this item.<br>

###### Game options
| Name                  |
| --------------------- |
| Enable multi teleport |

##### Bomb
You sacrifice yourself in order to kill everyone nearby. Only impostors can get this item.<br>

###### Game Options
| Name               |
| ------------------ |
| Enable bomb        |
| Bomb radius        |
| Can kill impostors |

##### Teleport
Teleports you to random vent. Everyone can get this item.<br>

###### Game options
| Name            |
| --------------- |
| Enable teleport |

##### Button
Call emergency from anywhere. Everyne can get this item.<br>

###### Game options
| Name                    |
| ----------------------- |
| Enable button           |
| Can use during sabotage |

##### Finder
Teleports you to nearest player. Everyone can get this item.<br>

###### Game options
| Name          |
| ------------- |
| Enable finder |

##### Rope
Teleports nearest player to you. Everyone can get this item.<br>

###### Game options
| Name        |
| ----------- |
| Enable rope |

##### Stop
Type /stop to end meeting immediately with skip result. Impostors can get this item. Depending on options crewmates can get it too.<br>

###### Game options
| Name                     |
| ------------------------ |
| Enable stop              |
| Can be given to crewmate |

##### Newsletter
Gives you info about how many people died and how they died, how many roles are alive. Everyone can get this item.<br>

### Battle royale
Everyone is impostors. Clik kill button to attack. If you get hit, you lose 1 live. When your lives drop down to 0, you die and lose.<br>
Last one alive wins! No reporting, emergiencies, venting and sabotaging. Depending on options you see arrow to nearst player.<br>

#### Game options
| Name                    |
| ----------------------- |
| Lives                   |
| Lives visible to others |
| Arrow to nearest player |

### Speedrun
Everyone is crewmate. Finish tasks first to win. You can play alone - finish tasks as fast as you can!<br>

#### Game options
| Name                    |
| ----------------------- |
| Body type               |
| Tasks visible to others |

### Additional gamemodes
Additional gamemodes works with every gamemode. You can turn on as many additional gamemodes as you want.<br>

#### Random spawn
At start teleports everyone to random vent. Depending on options it teleports after meeting too.<br>

##### Game options
| Name                   |
| ---------------------- |
| Teleport after meeting |

## Other functions

### Playing with fewer than 4 players
You can start game when you only want even if there's less than 4 players in lobby.<br>

### Unlimited impostors
There is no impostors limit. You can start 4 players game with 3 impostors. Or even do 1 crewmate vs 14 impostors.<br>

### You can play in public lobbies
When you join to non modded lobby mod is disabled and you can play normally. You can even play in lobbies with other host only mod.<br>
SWITCH GAMEMODE TO CLASSIC BEFORE JOINING PUBLIC LOBBY! OTHERWISE YOU CAN HAVE HUD ISSUES!<br>

### Walking throught walls in public lobby
`Ctrl` works in non modded lobbies, so you can just walk throught walls. This function works only in lobby.<br>

## Others

### Inverted controls
You can achieve this by setting player speed to negative amount.<br>

### Bugged options
Bugged options are: player speed 0 or lower, player speed higher than 3, custom kill distance, 0 or lower impostors, more than 3<br>
impostors, less than 0 players or more than 15 players. If you change then any setting by laptop, you get kicked from lobby. IF THIS<br>
HAPPEN CREATE LOBBY ON LOCAL AND REPAIR SETTING TO NON BUGGED!<br>

### Custom map and kill distance
Do this by typing /cs (map/killdistance) custom ID. You can have fun with this bugged thing.<br>

### Max 15 players lobby
When you set players to 16 or higher, still max 15 players can join.<br>

### Bugged cooldown
If you set cooldown to 0 or lower, you disable this function. 0 second cooldown don't working. Set this value to 0,001 seconds.<br>

## Credits
1. Using some code from Town of Host
2. Some code from Town Of Host: The Other Roles