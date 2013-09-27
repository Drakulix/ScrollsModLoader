##Summoner : A ModLoader for Scrolls

####What is it?
Summoner is a project to provide a modding API for the game "Scrolls" by Mojang.

####Who is working on it?
Drakulix - Main Developer

Kbasten - Developer



####Table of Contents

1. Motivation
2. Concept
3. Release
4. Developer API




####1. Motivation

Why doing a ModLoader for Scrolls?

Scrolls is a game based on the Unity-Engine, which uses Mono in background to provide it's functionality easily over a wider range of platforms.
Mono/.net Assemblies are compiled into a VM-ASM similar to Java, that is interpreted by the Runtime Environment.
Making a ModLoader for Scrolls provides an example to do similar stuff with every other Unity-powered games (even for iPhone or Android implementations) and Mono/.net Assemblies in general.
Which is interesting to understand for myself and many more people, I assume.

Not at least Scrolls is a fun-making game, that deserves to have a nice community and modding always helps to turn the game into what the users expect.
Mojang should know about that from making Minecraft quite well.




####2. Concept

The Scrolls ModLoader loads itself into the assembly through a little patcher utilizing Mono.Cecil, ILRepack and the execellent LinFu Framework (both also based on Mono.Cecil).
Mono.Cecil provides a way to manipulated compiled assemblies very easily, LinFu takes that to the next level and ILRepack makes injecting code even easier.

The ModLoader assembly will be used as Patcher and injected Assembly.
When run on its own it patches some basic calls into the Scrolls Assembly and merges itself into it to be called by Scrolls at Runtime.
This gives us the possibility to do more patches at runtime without need to call the patcher directly.
Instead in case any changes to the Assembly are required, it will modify itself through the injected Patcher code on its own.
This includes game patches or blocking hooks of older incompatible mods.

Mods themselves will have multiple ways to modify the Scrolls gaming experience.
The Scrolls ModLoader provides two APIs:
The Low-Level-API and the High-Level-API.

The High-Level-API hides all hooks and gives some basic functionality through a standard API, that is guaranteed to be available on all game versions, but may change with new versions of the API.
The Low-Level-API provides a way to directly patch Scrolls like the ModLoader does, through a simplified API (specialized on Scrolls) for Mono.Cecil.

The Low-Level-API calls are likely to break on future game versions, as we are not making the game itself. However the ModLoader API is doing checks in background to prevent crashes. It acts as additional security layer to detect outdated or simply broken mods to ensure the game is never broken to an unplayable state through bad code.
However Mods are able to do anything through the Low-Level API, which means they can access the user account directly and modify the GUI in any way they want.

Although this is very insecure, it is clear that no High-Level-API could ever provide the possibilities, that the Low-Level-API gives to modders.
Even if we would not provide this API, developers would be able to use Mono.Cecil themselves to get access to certain functions.
So it is better to control those mods through this little layer for compatibility reasons, instead of giving all responsibility to the mod developers.
To ensure safety for the user we try to provide a trusted-platform for the Scrolls ModLoader provided by...




####3. Release

At ScrollsGuide.com/Summoner !

Any releases will be provided through ScrollsGuide.com.
It also acts as trusted Plugin-Library, all Plugins submitted will be tested and need to be open-source, so everybody can check the functionality before running any mods themselves (theoretically).

##What was changed from the Concept:

- The High-Level-API was not build into something fully-functional. You can use it, but it just contains some helper functions, nothing to build an entire mod out of it.
- ILRepack is not needed anymore for injecting code. Having the Mod-Assembly separately has proven to be as reliable and easier.



####4. Developer API

First tutorial over here: http://www.scrollsguide.com/forum/viewtopic.php?f=61&t=1873

####5. Build instructions for Windows

You will need:

- something that can open the project files (Xamarin Studio is free and recommended)
- the latest version of Mono (2.10.9)
- xbuild and mono need to be in your `PATH`-environment variable (`C:\Program Files (x86)\Mono-2.10.9\bin` on my computer).

How to setup the build-environment:

1. Copy `Assembly-CSharp.dll` (from the ModLoader-Folder or an unmodified Scrolls), `UnityEngine.dll` and `JsonFx.dll` to the main project folder
2. Run `go.bat compile-release` on the `cmd` in the `LinFu-master`-Folder.
3. Start Xamarin. The references to LinFu should now resolve correctly
4. You are now ready to build/execute the project in Xamarin.
