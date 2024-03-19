# More Gamemodes

## About the mod

More Gamemodes is the Among Us mod that addes new gamemodes. Only host need to have mod to work.<br>
Among Us version: 2024.3.6<br>
Join to discord server: https://discord.gg/jJe5kPpbFJ

## Hotkeys

### Host only
| HotKey              | Function                       | Usable Scene |
| ------------------- | ------------------------------ | ------------ |
| `Shift`+`L`+`Enter` | End the game immediatly        | In Game      |
| `Shift`+`M`+`Enter` | Skip meeting to end            | In Meeting   |
| `Shift`+`Z`+`Enter` | Make yourself dead             | In Game      |
| `C`                 | Cancel game start              | In Countdown |
| `Shift`             | Start the game immediately     | In Countdown |
| `Ctrl` + `Delete`   | Reset options to default       | In Lobby     |

### Mod client only
| HotKey | Function              | Usable Scene |
| ------ | --------------------- | ------------ |
| `Ctrl` | Noclip throught walls | In Lobby     |

## Chat commands

### Host only
| Command                                         | Function                         |
| ----------------------------------------------- | -------------------------------- |
| /changesetting OPTION VALUE/<br>cs OPTION VALUE | Change setting value             |
| /gamemode GAMEMODE<br>/gm GAMEMODE              | Change current gamemode          |
| /kick PLAYER_ID                                 | Kick player from lobby           |
| /ban PLAYER_ID                                  | Ban player from lobby            |
| /announce MESSAGE<br>/ac MESSAGE                | Send message even if you're dead |

### All clients
| Command                                         | Function                                                |
| ----------------------------------------------- | ------------------------------------------------------- |
| /color COLOR_ID<br>/colour COLOR_ID             | Changes your color                                      |
| /name NAME                                      | Changes your name                                       |
| /h gm, /h gamemode<br>/help gm, /help gamemode  | Show gamemode description                               |
| /h i, /h item<br>/help i, /help item            | Show item description(Random Items)                     |
| /stop                                           | Use stop item(Random Items)                             |
| /now<br>/n                                      | Show current options                                    |
| /id colors<br>/id colours                       | Show color ids                                          |
| /id players                                     | Show player ids                                         |
| /commands<br>/cm                                | Show commands list                                      |
| /radio<br>/rd                                   | Send private message to other impostors (Mid Game Chat) |
| /lastresult<br>/l                               | Show last game result                                   |
| /info                                           | Use newsletter item(Random Items)                       |
| /h j, /h jailbreak<br>/help j, /help jailbreak  | Show how to play jailbreak gamemode on map              |
| /tpout                                          | Teleports you outside lobby ship                        |
| /tpin                                           | Teleports you into lobby ship                           |

## Game options
| Name                   |
| ---------------------- |
| Gamemode               |
| No game end            |
| Can use /color command |
| Enable fortegreen      |
| can use /name command  |
| Enable name repeating  |
| Maximum name length    |

## Gamemodes

### Hide and seek
Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences and sabotages. Impostors are red, crewmates are blue. Depending on options impostors can vent and closing doors. Hide and seek is working with special roles.

#### Game options
| Name                            |
| ------------------------------- |
| Teleport on start               |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |

### Shift and seek
Everyone is engineer or shapeshifter. Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences and sabotages. Depending on options impostors can vent and closing doors. Impostors are visible to others or not. Impostors must shapeshift into person they want to kill.

#### Game options
| Name                            |
| ------------------------------- |
| Teleport on start               |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |
| Impostors are visible           |
| Instant shapeshift              |

### Bomb tag
Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports,
meetings, sabotages or venting. Click kill button to give bomb away. Last standing alive wins.

#### Game options
| Name                     |
| ------------------------ |
| Teleport on start        |
| Teleport after explosion |
| Explosion delay          |
| Players with bomb        |
| Max players with bomb    |

### Random items
Crewmates get items by doing tasks, impostors by killing. Items are given randomly. Pet your pet to use item. Some items (knowledge, gun,illusion, finder and rope) works on nearest player. Other rules are just like in classic game. Random items works with special roles.

#### Items

##### Time slower
Increases discussion and voting time. Only crewmates can get this item.

###### Game options
| Name                     |
| ------------------------ |
| Enable time slower       |
| Discussion time increase |
| Voting time increase     |


##### Knowledge
You can investigate nearby player. If this player is good, he has green name. If this player is bad, he has red name. Depending on options target can see your name in gray after this. Only crewmates can get this item.

###### Game options
| Name                 |
| -------------------- |
| Enable knowledge     |
| Crewmates see reveal |
| Impostors see reveal |

##### Shield
Grant yourself a shield for some time. You cannot be killed during this time. Depending on options you can see who tried to kill you. Only crewmates can get this item.

###### Game options
| Name               |
| ------------------ |
| Enable shield      |
| Shield duration    |
| See who tried kill |

##### Gun
You can kill impostors. Depending on options you can kill crewmates. Only crewmates can get this item.

###### Game options
| Name                   |
| ---------------------- |
| Enable gun             |
| Can kill crewmates     |
| Misfire kills crewmate |

##### Illusion
If nearby player is impostor, he kills you. Otherwise you see that player's name in green. Only crewmates can get this item.

###### Game options
| Name            |
| --------------- |
| Enable illusion |

##### Radar
Do reactor flash if impostor is nearby. Only crewmates can get this item.

###### Game Options
| Name         |
| ------------ |
| Enable radar |
| Radar range  |

##### Swap
Swap your tasks with nearby player tasks. Only crewmates can get this item.

###### Game options
| Name        |
| ----------- |
| Enable swap |

##### Time speeder
Decreases discussion and voting time. Only impostors can get this item.

###### Game options
| Name                     |
| ------------------------ |
| Enable time speeder      |
| Discussion time decrease |
| Voting time decrease     |

##### Flash
All crewmates go blind for few seconds, but impostor vision is decreased for that time. Only impostors can get this item.

###### Game options
| Name                     |
| ------------------------ |
| Enable flash             |
| Flash duration           |
| Impostor vision in flash |

##### Hack
Prevent crewmates from doing anything(reporting, using items, doing some tasks, using role ability, venting, sabotaging, calling meetings).  Depending on options it affects impostors too. Only impostors can get this item.

###### Game options
| Name                   |
| ---------------------- |
| Enable hack            |
| Hack duration          |
| Hack affects impostors |

##### Camouflage
Make everyone look the same for some time. You can't use camouflage during camouflage. Only impostors can get this item.

###### Game options
| Name                |
| ------------------- |
| Enable camouflage   |
| Camouflage duration |

##### Multi teleport
Teleports everyone to you. Only impostors can get this item.

###### Game options
| Name                  |
| --------------------- |
| Enable multi teleport |

##### Bomb
You sacrifice yourself in order to kill everyone nearby. Only impostors can get this item.

###### Game Options
| Name               |
| ------------------ |
| Enable bomb        |
| Bomb radius        |
| Can kill impostors |

##### Trap
Place trap that kills first player touches it. Trap is completely invisible and works after few seconds from placing. Only impostors can get this item.

###### Game options
| Name           |
| -------------- |
| Enable trap    |
| Trap wait time |
| Trap radius    |

##### Teleport
Teleports you to random vent. Everyone can get this item.

###### Game options
| Name            |
| --------------- |
| Enable teleport |

##### Button
Call emergency from anywhere. Everyne can get this item.

###### Game options
| Name                    |
| ----------------------- |
| Enable button           |
| Can use during sabotage |

##### Finder
Teleports you to nearest player. Everyone can get this item.

###### Game options
| Name          |
| ------------- |
| Enable finder |

##### Rope
Teleports nearest player to you. Everyone can get this item.

###### Game options
| Name        |
| ----------- |
| Enable rope |

##### Stop
Type /stop to end meeting immediately with skip result. Impostors can get this item. Depending on options crewmates can get it too.

###### Game options
| Name                     |
| ------------------------ |
| Enable stop              |
| Can be given to crewmate |

##### Newsletter
Gives you info about how many people died and how they died, how many roles are alive. Use this item by typing /info. Everyone can get this item.

###### Game options
| Name              |
| ----------------- |
| Enable newsletter |

##### Compass
Show arrow to all players for short period of time. Everyone can get this item.

###### Game options
| Name             |
| ---------------- |
| Enable compass   |
| Compass duration |

### Battle royale
Everyone is impostors. Clik kill button to attack. If you get hit, you lose 1 live. When your lives drop down to 0, you die and lose. Last one alive wins! No reporting, emergiencies, venting and sabotaging. Depending on options you see arrow to nearst player.

#### Game options
| Name                    |
| ----------------------- |
| Lives                   |
| Lives visible to others |
| Arrow to nearest player |

### Speedrun
Everyone is crewmate. Finish tasks first to win. You can play alone - finish tasks as fast as you can!

#### Game options
| Name                    |
| ----------------------- |
| Body type               |
| Tasks visible to others |

### Paint battle
Type (/color ID) command to change paint color. Pet your pet to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.

#### Game options
| Name          |
| ------------- |
| Painting time |
| Voting time   |

### Kill or die
Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Last standing alive wins!

#### Game options
| Name                 |
| -------------------- |
| Teleport after round |
| Killer blind time    |
| Time to kill         |

### Zombies
Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Zombies show up as dead during meetings. Special roles and sabotages are disabled. Impostors can't vent. Depending on options you see arrow pointing to zombie(s).

#### Game options
| Name                          |
| ----------------------------- |
| Zombie kills turn into zombie |
| Zombie speed                  |
| Zombie vision                 |
| Can kill zombies after tasks  |
| Number of kills               |
| Zombie blind time             |
| Tracking zombie mode          |
| Ejected players are zombies   |

### Jailbreak
Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them. If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.\nItems:\nResources - prisoners use it to craft items.\nScrewdriver (illegal) - gives prisoner ability to vent\nWeapon (illegal) - has 10 levels. Increase damage depending on level.\nSpaceship part (illegal) - used to craft spaceship.\nSpaceship (illegal) - used to escape.\nBreathing mask - used to escape.\nPickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.\nGuard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.\nMoney - used to buy items by guards.\nEnergy drink - increase your speed temporarily.\n\nIllegal actions:\nAttacking\nVenting\nBeing in forbidden room\nDisguising as guard\nHaving illegal item\n\nAll prisoners are orange and all guards are blue.\nDo /help jailbreak to see how to play in actual map.

#### Maps
The skeld, Dleks eht: There are 5 forbidden areas, where is illegal for prisoners to be in. These are:<br>Reactor<br>Security<br>Admin<br>Storage<br>Navigation<br><br>Your health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 1 resource per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:<br>1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!<br>2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!<br>3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!

#### Game options
| Name                        |
| --------------------------- |
| Prisoner health             |
| Prisoner regeneration       |
| Prisoner damage             |
| Guard health                |
| Guard regeneration          |
| Guard damage                |
| Weapon damage               |
| Screwdriver price           |
| Prisoner weapon price       |
| Guard weapon price          |
| Guard outfit price          | 
| Respawn cooldown            |
| Search cooldown             |
| Maximum prisoner resources  |
| Spaceship part price        |
| Required spaceship parts    |
| Pickaxe price               |
| Pickaxe speed               |
| Prison takeover duration    |
| Breathing mask price        |
| Energy drink price          |
| Energy drink duration       |
| Energy drink speed increase |
| Game time                   |
| Escapists can help others   |
| Help cooldown               |
| Given resources             |

### Additional gamemodes
Additional gamemodes works with every gamemode. You can turn on as many additional gamemodes as you want.

#### Random spawn
At start teleports everyone to random vent. Depending on options it teleports after meeting too.

##### Game options
| Name                   |
| ---------------------- |
| Teleport after meeting |

#### Random map
Map is randomly chosen before game starts.

##### Game options
| Name            |
| --------------- |
| Add the skeld   |
| Add mira hq     |
| Add polus       |
| Add dleks eht   |
| Add the airship |
| Add the fungle  |

#### Disable gap platform
Players can't use gap platform on airship.

#### Mid game chat
You can chat during rounds. If proximity chat is on only nearby players see you messages. Depending on options impostors can communicate via radio by typing /radio MESSAGE.

##### Game options
| Name                          |
| ----------------------------- |
| Proximity chat                |
| Messages radius               |
| Impostor radio                |
| Fake shapeshift appearance    |
| Disable during comms sabotage |

#### Disable zipline
Players can't use zipline on the fungle.

## Other functions

### Playing with fewer than 4 players
You can start game when you only want even if there's less than 4 players in lobby.

### Unlimited impostors
There is no impostors limit. You can start 4 players game with 3 impostors. Or even do 1 crewmate vs 14 impostors.

### You can play in public lobbies
When you join to non modded lobby mod is disabled and you can play normally. You can even play in lobbies with other host only mod.

### Walking throught walls in public lobby
`Ctrl` works in non modded lobbies, so you can just walk throught walls. This function works only in lobby.

### No chat character limit
You can use any character in chat. Also using text formatting is allowed. Only limits are that you can't use chat formatting in chat and can't change size of text as non host. Tutorial on how to use text formatting: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/RichTextSupportedTags.html

## Others

### Max 15 players lobby
When you set players to 16 or higher, still max 15 players can join.

### Bugged cooldown
If you set cooldown to 0 or lower, you disable this function. 0 second cooldown don't working. Set this value to 0,001 seconds, if you want no kill cooldown.

### People not joining lobby
Due to au sever changes people can't find your lobby with find game option. Players can only join to your lobby using code. (PLEASE INNERSLOTH FIX IT)

## Credits
1. Using some code from Town of Host
2. Some code from Town Of Host: The Other Roles
3. /tpout and /tpin from TOHE