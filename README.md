# Void Combat Overhaul

Overhauls space combat to make positioning, sustained fire, and resource management more meaningful.

## Features

- **Weapon range** — short range grants a hit bonus, long range applies a penalty (floor of 5%)
- **Improved shields** — shields absorb more damage while active
- **Crew morale** — sustained hits degrade crew morale over time
- **Rear arc bonus** — attacking from the stern (rear 90°) grants a hit bonus
- **Harder escape** — ships can only disengage below a configurable hull threshold
- **Boarding** — successful boarding actions reduce enemy military rating
- **Repair costs** — scrap cost per hull point scales with damage taken
- **Combat speed** — configurable speed multipliers for turns, projectiles, and ship movement

All settings are adjustable live in the UMM panel (Ctrl+F10).

## Installation

Copy the `VoidCombatOverhaul` dll into:
```
%AppData%\..\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\
```

No other mods required. Compatible with ToyBox, Project BugFix, MechaFix.

## Building

Requires Visual Studio 2022 and .NET Framework 4.8.1. Open `VoidCombatOverhaul.csproj` and build — the DLL auto-copies to your UMM folder on success. Game path is detected automatically from `Player.log` on first build.
