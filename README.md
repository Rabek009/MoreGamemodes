# More Gamemodes

## About the mod

More Gamemodes is the Among Us mod that addes new gamemodes and roles. Only host need to have mod for it to work.<br>
Among Us version: 16.1.0<br>
Join to discord server: https://discord.gg/jJe5kPpbFJ

## Hotkeys

### Host only
| HotKey              | Function                            | Usable Scene |
| ------------------- | ----------------------------------- | ------------ |
| `Shift`+`L`+`Enter` | End the game immediatly             | In Game      |
| `Shift`+`M`+`Enter` | Skip meeting to end                 | In Meeting   |
| `Shift`+`Z`+`Enter` | Make yourself dead                  | In Game      |
| `C`                 | Cancel game start                   | In Countdown |
| `Shift`             | Start the game immediately          | In Countdown |
| `Ctrl` + `Delete`   | Reset options to default            | In Lobby     |
| `Shift`+`V`+`Enter` | End meeting (with counting votes)   | In Meeting   |
| `Shift`+`R`+`Enter` | Enable all custom roles and add ons | In Lobby     |

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
| /hostcolor COLOR_HEX                            | Changes color of host tag        |

### All clients
| Command                                         | Function                                                |
| ----------------------------------------------- | ------------------------------------------------------- |
| /color COLOR_ID<br>/colour COLOR_ID             | Changes your color                                      |
| /name NAME                                      | Changes your name                                       |
| /h gm, /h gamemode<br>/help gm, /help gamemode  | Show gamemode description                               |
| /h i, /h item<br>/help i, /help item            | Show item description(Random items)                     |
| /stop                                           | Use stop item(Random items)                             |
| /now<br>/n                                      | Show current options                                    |
| /id colors<br>/id colours                       | Show color ids                                          |
| /id players                                     | Show player ids                                         |
| /commands<br>/cm                                | Show commands list                                      |
| /radio<br>/rd                                   | Send private message to other impostors (Mid game chat) |
| /lastresult<br>/l                               | Show last game result                                   |
| /info                                           | Use newsletter item(Random items)                       |
| /h j, /h jailbreak<br>/help j, /help jailbreak  | Show how to play jailbreak gamemode on map              |
| /tpout                                          | Teleports you outside lobby ship                        |
| /tpin                                           | Teleports you into lobby ship                           |
| /tagcolor COLOR_HEX                             | Changes color of your tag (not host tag)                |
| /myrole<br>/m                                   | Show your role description (Classic)                    |
| /guess, /shoot, /bet<br>/bt, /gs PLAYER_ID ROLE | You guess player as evil/nice guesser (Classic)         |
| /roles<br>/role, /r                             | Show role options (Classic)                             |
| /roles, /role<br>/r ROLE                        | Show role description (Classic)                         |
| /kcount<br>/kc                                  | Show amount of killers alive (Classic)                  |

## Game options
| Name                    |
| ----------------------- |
| Gamemode                |
| No game end             |
| Anti-cheat              |
| Cheating penalty        |
| Amogus                  |
| Tasks needed to win     |
| Can use /color command  |
| Enable fortegreen       |
| Can use /name command   |
| Enable name repeating   |
| Maximum name length     |
| Can use /kcount command |

## Gamemodes

### Hide and seek
Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences and sabotages. Impostors are red, crewmates are blue. Depending on options impostors can vent and closing doors. Hide and seek is working with special roles.

#### Game options
| Name                            |
| ------------------------------- |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |
| Impostors are visible           |
| Show danger meter               |

### Shift and seek
Everyone is engineer or shapeshifter. Impostors need to kill every single crewmate, crewmate need to do every task. No reports, emergiences and sabotages. Depending on options impostors can vent and closing doors. Impostors are visible to others or not. Impostors must shapeshift into person they want to kill.

#### Game options
| Name                            |
| ------------------------------- |
| Impostors blind time            |
| Impostors can kill during blind |
| Impostors can vent              |
| Impostors can close doors       |
| Impostors are visible           |
| Instant shapeshift              |
| Show danger meter               |

### Bomb tag
Everyone is impostor. Rounds last for some seconds. After every round players with bomb die and new players get bomb. No reports, meetings, sabotages or venting. Click kill button to give bomb away. Depending on options players with bomb see arrow to nearest non bombed. Last standing alive wins!

#### Game options
| Name                        |
| --------------------------- |
| Teleport on start           |
| Teleport after explosion    |
| Explosion delay             |
| Players with bomb           |
| Max players with bomb       |
| Arrow to nearest non bombed |
| Explosion creates hole      |
| Hole speed decrease         |

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

##### Medicine
Use report button to bring back dead player. But depending on options you die after using it. Only crewmates can get this item.

###### Game options
| Name            |
| --------------- |
| Enable medicine |
| Die on revive   |

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
| Name                   |
| ---------------------- |
| Enable bomb            |
| Bomb radius            |
| Can kill impostors     |
| Explosion creates hole |
| Hole speed decrease    |

##### Trap
Place trap that kills first player touches it. Trap is completely invisible and works after few seconds from placing. Only impostors can get this item.

###### Game options
| Name               |
| ------------------ |
| Enable trap        |
| Trap wait time     |
| Trap radius        |
| Crewmates see trap |
| Impostors see trap |

##### Team changer
You can turn nearby crewmate into impostor, but you die after doing it. Only impostors can get this item.

###### Game options
| Name                  |
| --------------------- |
| Enable team changer   |
| Target gets your role |

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
Type /stop to end meeting immediately with skip result. You can only use this item during voting time. Impostors can get this item. Depending on options crewmates can get it too.

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

##### Booster
Increases your speed temporarily. Everyone can get this item.

###### Game options
| Name                   |
| ---------------------- |
| Enable booster         |
| Booster duration       |
| Booster speed increase |

### Battle royale
Everyone is impostor. Click kill button to attack. If you get hit, you lose 1 live. When your lives drop down to 0, you die and lose. Last one alive wins! No reporting, emergiencies, venting and sabotaging. Depending on options you see arrow to nearst player.

#### Game options
| Name                    |
| ----------------------- |
| Lives                   |
| Lives visible to others |
| Arrow to nearest player |
| Grace period            |

### Speedrun
Everyone is crewmate. Finish tasks first to win. You can play alone - finish tasks as fast as you can!

#### Game options
| Name                    |
| ----------------------- |
| Body type               |
| Tasks visible to others |

### Paint battle
Type /color COLOR command to change paint color. Click kill button to paint. Paint something in specified theme. After painting time you can rate others paint by typing number from 1 to 10.

#### Game options
| Name          |
| ------------- |
| Painting time |
| Voting time   |

### Kill or die
Game lasts for few round. Random player become killer every round. Killer need to kill someone before timer runs out. If killer doesn't kill, he dies. The round ends after killer kill someone or die. Red player is killer. Depending on options killer gets arrow to nearest survivor. Last standing alive wins!

#### Game options
| Name                      |
| ------------------------- |
| Teleport after round      |
| Killer blind time         |
| Time to kill              |
| Arrow to nearest survivor |

### Zombies
Players killed by impostor are turned into zombies and are on impostors side. Zombies can kill crewmates. Zombies have green name and can see impostors. Depending on options crewmate can kill zombies after completing all tasks. Depending on options you become zombie after being killed by zombie. When you get turned into zombie, you can move after next meeting. Depending on options you see arrow pointing to zombie(s). Impostors and zombies can vent if option is turned on.

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
| Impostors can vent            |
| Zombies can vent              |

### Jailbreak
Jailbreak: There are prisoners and guards. Prisoner win, if he escape. Guards win together, if less than half of prisoners escape. Prisoners can only vent, if they have screwdriver, but guards can vent anytime. As prisoner you can use pet button to switch current recipe. Use shift button to craft item in current recipe or destroy wall in reactor. You use resources to craft. Also use kill button to attack someone. If you beat up prisoner, you steal all his items. If you beat up guard, you get 100 resources and steal his weapon. When your health go down to 0, you get eliminated and respawn after some time. Guards can only attack wanted prisoners. Prisoner will become wanted, when he do something illegal near guard. Guards can use kill button on not wanted players to check them. If that player has illegal (red) item, he becomes wanted. When player is beaten up, he is no longer wanted. As guard you can buy things with money like prisoners craft. You can also repair wall in reactor by using shift button. If guard beat up prisoner, then prisoner lose all illegal items. Depending on options prisoners can help other after escaping.<br>Items:<br>Resources - prisoners use it to craft items.<br>Screwdriver (illegal) - gives prisoner ability to vent<br>Weapon (illegal) - has 10 levels. Increase damage depending on level.<br>Spaceship part (illegal) - used to craft spaceship.<br>Spaceship (illegal) - used to escape.<br>Breathing mask - used to escape.<br>Pickaxe (illegal) - has 10 levels and gives you ability to destroy wall in reactor. Destroying speed depends on level.<br>Guard outfit (illegal) - gives you ability to disguise into guard. While disguised as guard, guards can use kill button on you to check uf you're fake guard.<br>Money - used to buy items by guards.<br>Energy drink - increase your speed temporarily.<br><br>Illegal actions:<br>Attacking<br>Venting<br>Being in forbidden room<br>Disguising as guard<br>Having illegal item<br><br>All prisoners are orange and all guards are blue.<br>Do /help jailbreak to see how to play in actual map.

#### Maps
The skeld, Dleks eht: There are 5 forbidden areas, where is illegal for prisoners to be in. These are:<br>Reactor<br>Security<br>Admin<br>Storage<br>Navigation<br><br>Your health is regenerating faster in medbay, while not fighting. You can fill spaceship with fuel in lower or upper engine. You can fill your breathing mask with oxygen in o2. In electrical prisoners get 2 resources per second. In storage prisoners get 5 resources per secons. Guards always get 2 dollars per second. There are 3 ways to escape:<br>1. Craft spaceship parts with resources. Then craft spaceship with them. Then fill it with fuel, go to storage and you escaped!<br>2. Craft breathing mask and pickaxe. Fill your breathing mask with oxygen. Then go to reactor and destroy a wall with your pickaxe. If you do it, you're free!<br>3. Prison takeover - prisoners have to work together to beat up every guard fast. Then go to navigation to change ship direction. Changing direction only works when all guards are beaten up. If guard come to navigation, the entire progress of changing direction resets. If you success, every prisoner win!

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
| Prisoner armor price        |
| Guard armor price           |
| Armor protection            |

### Deathrun
Deathrun is normal among us, but there are no cooldowns. There is no kill cooldown and ability cooldown for roles. There is only cooldown at the start of every round. Crewmates have only 1 short tasks (it can't be download data). Depending on options impostors can vent. Meetings can be disabled by host in options.

#### Game options
| Name               |
| ------------------ |
| Round cooldown     |
| Disable meetings   |
| Impostors can vent |
| Amount of tasks    |

### Base wars
Players are divided into two teams, Red and Blue, with the objective of destroying the opposing team's base while defending their own. Each team has two turrets - Red's in Upper and Lower Engine, and Blue's in Shields and Weapons - that can be attacked by players using the shift button. Depending on the options set by the host, turrets can also slow down enemy players, adding an extra layer of defense. These turrets automatically defend the base and do not require a teammate to be present to activate. Players attack enemy players using the kill button and can earn experience points (EXP) by eliminating opponents and controlling key areas - Storage and Cafeteria. Gaining EXP allows players to level up, enhancing their abilities. Health can be regenerated quickly at the team's base, and depending on options players can also teleport back to their base when needed. The game is won when one team successfully destroys the opposing team's base, securing victory for their side.

#### Game options
| Name                    |
| ----------------------- |
| Starting health         |
| Starting damage         |
| Regeneration            |
| Turret health           |
| Turret damage           |
| Turret regeneration     |
| Turret slow enemies     |
| Speed decrease          |
| Base health             |
| Base damage             |
| Base regeneration       |
| Regeneration in base    |
| Can teleport to base    |
| Teleport cooldown       |
| Exp gain in middle      |
| Exp for kill            |
| Health increase         |
| Damage increase         |
| Smaller team gets level |

### Freeze tag
Crewmates are green, impostors are red and frozen crewmates are cyan. Impostors can use kill button to freeze crewmates. When all crewmates are frozen, impostors win. Crewmates can unfreeze others by standing near them. Crewmates win by completing all tasks. Reporting, sabotages and meetings are disabled. When crewmate is frozen his tasks will slowly complete automatically. Frozen crewmates can't move, but can see and do task, if there is nearby. Most roles work like in classic, but noisemaker sends alert when frozen.

#### Game options
| Name                                 |
| ------------------------------------ |
| Impostors blind time                 |
| Impostors can freeze during blind    |
| Impostors can vent                   |
| Impostors can close doors            |
| Unfreeze duration                    |
| Unfreeze radius                      |
| Task complete duration during freeze |

### Color wars
At start there are few leaders with their own color, everyone else is gray. Gray people are slow and have low vision. Leaders can use kill button on gray person to recruit this player to their team. Use kill button to enemy to kill this player. Leaders have multiple lives and other players have only 1. The goal is to protect leader and attack enemies. If leader dies, entire team die and lose. Player can respawn after few seconds, if his leader is alive. Depending on options players see arrow to their leader and nearest enemy leader. Last remaining team wins!

#### Game options
| Name                          |
| ----------------------------- |
| Leaders amount                |
| Leader lives                  |
| Lives visible to enemies      |
| Leader cooldown               |
| Grace period                  |
| Player kill cooldown          |
| Player can respawn            |
| Respawn cooldown              |
| Arrow to leader               |
| Arrow to nearest enemy leader |
| Non team speed                |
| Non team vision               |

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

## Roles

### Crewmate investigative

#### Investigator
You can use pet button to switch between task and investigate mode. In task mode you can do tasks. In investigate mode you have kill button. Use kill button to investigate player. If players is good, his name will become green. But if player is evil then his name will turn red. But some roles that are good can show as evil, also sometimes evil roles show as good. You have limited ability uses, but you can do tasks to increase it.<br><br>If you have mod installed, you don't have task and investigate mode. You can do tasks and investigate at the same time.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Investigate cooldown                      |
| Neutral killing show red                  |
| Neutral evil show red                     |
| Neutral benign show red                   |
| Crewmate killing show red                 |
| Incorrect result chance                   |
| Initial ability use limit                 |
| Ability use gain with each task completed |

#### Mortician
When you report dead body, you know target's role, killer's role and how old is body. During meeting you see death reasons. Depending on options you have arrow pointing to nearest dead body.

| Name                      |
| ------------------------- |
| See arrow to nearest body |
| Can be guessed            |

#### Oracle
You can use pet button to switch between task and confess mode. In task mode you can do tasks. In confess mode you have kill button. Use kill button to select x players (amount is defined in options). When you select these players, the most evil of them will confess (you will see that he confessed and his name will be red). Only you know who confessed. If 2 or more players are the most evil, then 1 random of them will confess. These are aligments from the most evil:<br>1. Impostor<br>2. Neutral killer<br>3. Neutral evil<br>4.Neutral benign<br>5. Crewmate killing<br>6. Crewmate non-killing<br><br>If you have mod installed, you don't have task and confess mode. You can do tasks and make people confess at the same time.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Confess cooldown                          |
| Number of selected players                |
| Unselect dead players                     |
| Initial ability use limit                 |
| Ability use gain with each task completed |

#### Sniffer
You can use pet button to switch between task and sniff mode. In task mode you can do tasks. In sniff mode you have kill button. Use kill button to sniff a player once per round. During meeting you can see who was near that player after you sniffed him. These players are given in random order. Invisible players don't count and shapeshifter can cause fake result.<br><br>If you have mod installed, you don't have task and sniff mode. You can do tasks and sniff at the same time.

##### Game options
| Name           |
| -------------- |
| Sniff cooldown |
| Sniff radius   |

#### Snitch
After completing all tasks you see who is impostor and depending on options neutral killer. Depending on options you can also see arrows to them. But if you're close to complete tasks, killers will know you and see arrow to you. Depending on options you might have more tasks than other crewmates. Finish tasks fast and don't get killed!

##### Game options
| Name                          |
| ----------------------------- |
| Can find neutral killers      |
| See arrow to impostors        |
| Tasks remaining when revealed |
| Additional short tasks        |
| Additional long tasks         |

### Crewmate killing

#### Nice guesser
You can guess evil roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 6 is trapster, you should type <i>/guess 6 trapster</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead.

##### Game options
| Name                      |
| ------------------------- |
| Can guess neutral killing |
| Can guess neutral evil    |
| Can guess neutral benign  |
| Can guess add ons         |

#### Shaman
During meeting you can curse someone. To do this open kick menu (open chat and click red button), select player who you want to curse and click "kick". If that player is killer he has to kill someone next round, or he will die and is informed about it. If that player can't kill, nothing will happen. You can curse max 1 person per meeting. You can't call meeting, but you can still report dead body. You have limited ability uses, but you can do tasks to increase it.<br><br>If you have mod installed you can use curse button to curse someone.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Initial ability use limit                 |
| Ability use gain with each task completed |

#### Sheriff
You can use pet button to switch between task and kill mode. In task mode you can do tasks. In kill mode you have kill button. You can kill impostors and depending on options neutrals. If you try to kill someone you can't, you die from misfire. Depending on options your target dies on misfire too.<br><br>If you have mod installed, you don't have task and kill mode. You can do tasks and kill at the same time.

##### Game options
| Name                     |
| ------------------------ |
| Kill cooldown            |
| Misfire kills target     |
| Can kill neutral killing |
| Can kill neutral evil    |
| Can kill neutral benign  |
| Can kill jester          |

### Crewmate protective

#### Altruist
You can use pet button to switch between report and revive mode. In report mode you can report dead bodies. In revive mode you can use report button to revive someone. After reviving someone you die. Revived player can still use their abilities, chat and do everything else. Depending on options you have arrow pointing to nearest dead body.

##### Game options
| Name                        |
| --------------------------- |
| See arrow to nearest body   |
| Killer see arrow to revived |

#### Immortal
After completing all tasks you can survive few kill attempts. In addition after you complete task, you get temporarily protection. If impostor tries to kill you, his cooldown will reset to 50%.

##### Game options
| Name                                       |
| ------------------------------------------ |
| Protection after completing task duration  |
| Times protected after completing all tasks |
| Can be guessed                             |

#### Medic
You can use pet button to switch between task and shield mode. In task mode you can do tasks. In shield mode you have kill button. Use kill button to give shield to crewmate. Player with shield can't be killed. If player with shield die, you can give shield to another player. You get alerted when someone try to kill protected player and depending on options killer's cooldown reset to 50%. When you die, shield disappear.<br><br>If you have mod installed, you don't have task and shield mode. You can do tasks and give shield at the same time.

##### Game options
| Name                    |
| ----------------------- |
| Shield cooldown         |
| Reset kill cooldown     |
| Shielded can see shield |

### Crewmate support

#### Bloody
If you get killed, dead bodies of you are going to spawn on killer for some time making a body trail. If you get killed indirectly or get eaten by pelican, your ability doesn't work. Bloody can't be bait.

##### Game options
| Name                       |
| -------------------------- |
| Dead bodies spawn duration |
| Dead bodies interval       |

#### Judge
During meeting you can eject anyone you want one time. To do this open kick menu (open chat and click red button), select player who you want to exile and click "kick". After that meeting will instantly end and that player will be ejected. Depending on options you might die after using ability.<br><br>If you have mod installed you can use judge button to exile player.

##### Game options
| Name                    |
| ----------------------- |
| Die after using ability |

#### Mayor
Your vote counts as multiple votes. Use it to eject impostors easier.

##### Game options
| Name                  |
| --------------------- |
| Additional vote count |
| Hide additional votes |
| Can be guessed        |

#### Mutant
You can use pet button during sabotage to instantly fix it from anywhere. You can't fix mushroom mixup sabotage.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Initial ability use limit                 |
| Ability use gain with each task completed |

#### Security guard
You can use pet button near vent to block it permanently. Blocked vent can't be used by anyone. When you're looking at cameras/doorlog/binoculars you get alerted when someone die. Depending on options cameras don't blink when you're using it.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Block vent cooldown                       |
| Initial ability use limit                 |
| Ability use gain with each task completed |
| Hide camera usage                         |
| Blocked vents update mode                 |
| Can be guessed                            |

### Impostor concealing

#### Camouflager
Click vanish button to camouflage everyone temporarily. During camouflage everyone looks exactly the same and all names are hidden.

##### Game options
| Name                           |
| ------------------------------ |
| Camouflage cooldown            |
| Camouflage duration            |
| Can camouflage during sabotage |

#### Droner
You can use vanish button to launch the drone from your position. You can control this drone and everyone can see it. Drone can stay on map for limited amount of time. When controlling drone you can kill people using it. Use pet button to stop using it or it will stop automatically when ability duration is over. After that you have cooldown before you can use your ability again. When you're using a drone, your real character is not moving (other people see you standing still). People can still interact with you when you're using a drone. When using drone you can't vent, report, call meetings and interact with objects. When ability expires a drone stays on map, but is 2 times smaller.

##### Game options
| Name                |
| ------------------- |
| Drone cooldown      |
| Drone duration      |
| Drone speed         |
| Drone vision        |
| Drone kill distance |

#### Escapist
You can mark position with your vanish button. You can teleport to that position by using vanish button again. After teleporting or after meeting your marked position reset and you have to mark again.

##### Game options
| Name              |
| ----------------- |
| Mark cooldown     |
| Teleport cooldown |
| Can use vents     |

#### Time freezer
Click vanish button to freeze time for short period of time. When time is frozen other players can't move and are blind. They can't use their abilities, kill, report bodies, call meeting, sabotage, vent. You can't freeze time during sabotage.

##### Game options
| Name                   |
| ---------------------- |
| Freeze cooldown        |
| Freeze duration        |
| Can use vents          |
| Can kill during freeze |

### Impostor killing

#### Archer
You can use pet button to kill nearest player without teleporting to them, but it uses arrow. Kill range is limited, but you can kill through walls. You can kill normally without using it. You have limited arrows, but you can get more by killing. You can only shoot, when your cooldown is 0.

##### Game options
| Name                            |
| ------------------------------- |
| Can kill impostors              |
| Arrow distance                  |
| Initial ability use limit       |
| Ability use gain with each kill |

#### Evil guesser
You can guess roles during meeting. To guess player type <b>/guess PLAYER_ID ROLE_NAME</b>. You see player id in his name. For example: if you want to guess that player with number 2 is sheriff, you should type <i>/guess 2 sheriff</i>. If you guess role correctly, that player dies instantly. But if you're wrong, you die instead. During rounds you can kill like regular impostor.

##### Game options
| Name                      |
| ------------------------- |
| Can guess neutral killing |
| Can guess neutral evil    |
| Can guess neutral benign  |
| Can guess "Crewmate" role |
| Can guess add ons         |

#### Hitman
Your target is random non impostor and you're only allowed to kill that target. After killing you have very short kill cooldown and your target changes. Target changes when your current target dies, after meeting or when your shift timer goes to 0. Depending on options you have arrow pointing to your target. You can see your target name at any time and you see target's name in black.

##### Game options
| Name                     |
| ------------------------ |
| Target change time       |
| Show arrow to target     |
| Kill cooldown after kill |

### Impostor support

#### Parasite
You can use pet button to turn nearby player into impostor, but you die after doing it. Depending on options that person becomes regular impostor, parasite or random impostor role. You can kill like regular impostor, but after kill your ability cooldown reset. You can use ability when you're suspicious to turn someone less suspicious into impostor.

##### Game options
| Name                 |
| -------------------- |
| Infect cooldown      |
| Turned impostor role |

#### Trapster
After killing someone, you place trap on dead body. Next player, who tries to report that body (or interact with it in any way) will be trapped on it unable to move and use abilities. Dead body can trap only 1 person at the time. If someone gets trapped on your body, your kill cooldown will decrease and you will get alerted.

##### Game options
| Name                           |
| ------------------------------ |
| Kill cooldown decrease on trap |

#### Undertaker
You can use shift button to select a target (it must be another impostor). That impostor is target until either he kills someone, dies or after some time passes. If selected impostor kills someone, dead body is teleported to you. When you're last impostor you can't use your ability, but depending on options you might get kill cooldown reduction.

##### Game options
| Name                                      |
| ----------------------------------------- |
| Select target cooldown                    |
| Target time to kill                       |
| Kill cooldown decrease when last impostor |

### Neutral benign

#### Amnesiac
You can use pet button to switch between report and remember mode. In report mode you can report dead bodies. In remember mode you can use report button on dead body to steal role of dead player. Depending on options you have arrow pointing to nearest dead body. You don't have win condition, you have to steal role from dead body and then win.

##### Game options
| Name                      |
| ------------------------- |
| See arrow to nearest body |

#### Opportunist
Survive to the end to win with winning team. If you die, you lose.

#### Romantic
Use kill button to choose your lover. Your goal is to protect your lover and help him win. If your lover wins, you win. If your lover loses, you lose. If one of you die, the other one dies too. Your role/team can't change and other romantics can't select you as lover. Depending on options after choosing target, you can use vanish button to protect both of you for some time. You might know lover's role and be able to chat with him privately during round by typing /lc MESSAGE

##### Game options
| Name                               |
| ---------------------------------- |
| Romance cooldown                   |
| Can protect                        |
| Protect cooldown                   |
| Protect duration                   |
| Ability use limit                  |
| See lover role                     |
| Can chat with lover                |
| Disable chat during comms sabotage |

### Neutral evil

#### Executioner
You have random target that is crewmate. If your target is ejected, you win alone. You see your target name in black. If your target dies, you either become amnesiac, opportunist, jester or crewmate depending on options.

##### Game options
| Name                    |
| ----------------------- |
| Role after target death |
| Can vote for target     |
| Can win after death     |

#### Jester
Get voted out to win alone. Act suspicious to make people think you're impostor.

##### Game options
| Name                 |
| -------------------- |
| Can use vents        |
| Has impostor vision  |
| Can vote for himself |

#### Soul collector
Use kill button to predict that selected player will die. When your target dies in any way, you collect their soul and you can choose new target. Depending on options you can change target anytime. When you collect enough souls, you instantly win alone.

##### Game options
| Name                  |
| --------------------- |
| Predict cooldown      |
| Can change target     |
| Souls required to win |

### Neutral killing

#### Arsonist
Use kill button on not doused player to douse him. If you click kill button on doused player, you ignite him. Ignited player will ignite every doused player that he touches and die after some time. Doused players have black name, ignited have orange name. Igniting bypasses all protections. Kill everyone to win alone.

##### Game options
| Name                  |
| --------------------- |
| Douse/Ignite cooldown |
| Ignite duration       |
| Ignite radius         |
| Can use vents         |

#### Ninja
You can use vanish button to become invisible - there is no animation. During invisibility your speed is greatly increased and you don't teleport when killing. Depending on options you can vent when you're not invisible. Kill everyone to win alone.

##### Game options
| Name                           |
| ------------------------------ |
| Kill cooldown                  |
| Vanish cooldown                |
| Vanish duration                |
| Speed increase while invisible |
| Can use vents                  |

#### Pelican
You can eat players by using your kill button. It means you can kill players without leaving bodies. Eat everyone to win alone.

##### Game options
| Name          |
| ------------- |
| Eat cooldown  |
| Can use vents |

#### Serial killer
Your goal is to kill everyone. You have lower kill cooldown, so you can kill faster than impostors.

##### Game options
| Name          |
| ------------- |
| Kill cooldown |

## Add ons

### Helpful

#### Bait
When you're killed, your killer instantly self report. Depending on options there might be report delay.

##### Game options
| Name                          |
| ----------------------------- |
| Report delay                  |
| Warn killer about self report |
| Neutrals can become bait      |
| Impostors can become bait     |
| Can be guessed                |

#### Radar
You see arrow to nearest player. That arrow is always updated.

#### Watcher
You see who votes for who in meeting, like with anonymous votes turned off.

### Harmful

#### Blind
Your vision is decreased.

##### Game options
| Name                     |
| ------------------------ |
| Crewmate vision decrease |
| Impostor vision decrease |

#### Oblivious
You can't report dead bodies. Depending on options you also avoid bait self report. Mortician can't be oblivious.

##### Game options
| Name                      |
| ------------------------- |
| Report after killing bait |

### Impostor

#### Lurker
Your kill cooldown continues to go down when you're in a vent. Only impostors can get this add on.

##### Game options
| Name                    |
| ----------------------- |
| Cooldown decrease speed |

## Other functions

### Anti-cheat
Stop hackers from ruining your experience! Anti-cheat can detect most hacks and inform you about it.
WARNING - some hackers can cause fake anti-cheat alerts. If it says that some player is hacking, it's not always true.

### Ban list
If you ban a player, he can't join any More Gamemodes lobby you host in the future! You can always add/remove players from ban list in MGM_DATA folder (you have to remove player from both ban lists to get him unbanned). You can always disable ban list in client options if you don't want it for some reason.

### Playing with fewer than 4 players
You can start game when you only want even if there's less than 4 players in lobby.

### Unlimited impostors
There is no impostors limit. You can start 4 players game with 3 impostors or even do 1 crewmate vs 14 impostors.

### Walking throught walls in public lobby
`Ctrl` works in non modded lobbies, so you can just walk throught walls. This function works only in lobby.

### No chat character limit
You can use any character in chat. Also using text formatting is allowed. Tutorial on how to use text formatting: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/RichTextSupportedTags.html

### Tasks needed to win
You can decide how many tasks have to be done for task win (it affects all gamemodes with tasks except speedrun). It's 100% by default, but if you decrease that value, not all tasks have to be completed. It can stop afk and lazy players from delaying task win.

### Dleks eht compatibility
You can play dleks eht (reversed skeld) map!

## Others

### Max 15 players lobby
When you set players to 16 or higher, still max 15 players can join. But on custom servers you can host lobbies with more players.

### Bugged cooldown
If you set kill cooldown to 0 or lower, you disable kills. 0 cooldown doesn't work. Set kill cooldown to 0,001 seconds, if you want no kill cooldown.

## Credits
1. Using much code from Town of Host
https://github.com/tukasa0001/TownOfHost

2. Pet action from Town Of Host: The Other Roles
https://github.com/discus-sions/TownOfHost-TheOtherRoles

3. Many features from TOHE
https://github.com/0xDrMoe/TownofHost-Enhanced

4. Game options menu from TOHY
https://github.com/Yumenopai/TownOfHost_Y

5. Many role ideas from Town Of Us Reactivated
https://github.com/eDonnes124/Town-Of-Us-R

6. Security guard role idea and inspiration for bloody role from The Other Roles
https://github.com/TheOtherRolesAU/TheOtherRoles

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.