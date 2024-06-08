# Better™️ Cartography Table

[Valheim](https://store.steampowered.com/app/892970/Valheim/) mod that allows precise control over sharing **pins** and **map exploration** via **cartography tables**.
Supports **private pins**, **public pins**, and **guild pins**.
Supports `NoMap` runs, giving **cartography tables** a purpose in `NoMap`.
Translations available.

## Video showcase

**(Click on items to show details)**

<details>
<summary><b>Private</b> and <b>public pins</b></summary>

Astrid and Brynhild are playing together, however Astrid has a tad too many mushroom pins for Brynhild's liking.
Thanks to **Better™️ Cartography Table**, their friendship is safe, as Astrid can selectively decide which pins to share on the **public cartography table**.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/e13e6267-88ad-4aee-bdfe-f78e807bc2f8

</details>

<details>
<summary>Real-time collaboration</summary>

Astrid and Brynhild are planning their next expedition.
Thanks to **Better™️ Cartography Table**, it will be a great success, as they can collaborate and prepare it in real time on the **public cartography table**.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/e535a301-994f-4129-b9ec-3e51685bab2c

</details>

<details>
<summary><b>Guild pins</b></summary>

Astrid and Brynhild are members of rival guilds (The Mushroom Enjoyers and The Ground Shakers), and neither wants their archnemesis to know about their secret hideout.
Thanks to **Better™️ Cartography Table**, their rivalry is fueled, as guild members can share **guild-only pins** on their **guild-restricted cartography table**.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/421e90b4-f00f-4047-b9ce-3839ac499035

</details>

## Features

- Pins are **private** by default, and can be individually shared to **cartography tables**.
  - You can now safely go ham and pin all those nice berries / copper veins / whatever floats your boat, without worrying about cluttering another player's map.
- When multiple players interact with the same **cartography table** at the same time, all changes are reflected in real time.
  - This allows collaborating over the map in real time, especially useful when planning the next expedition.
- **Cartography tables** can be **public** (default) or **restricted to a guild** (if [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) is installed).
  - For when you want to share super secret guild hideouts with your mates. Not that it ever happens. Definitely don't look for super secret guild hideouts on your servers. Nope...
- **Map exploration** sharing can be **public** (default), **private**, or **restricted to a guild** (if [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) is installed).
  - In case you need to be extra careful that other players know not where you roamed.
- If using the `NoMap` world modifier, the map will be accessible through **cartography tables** (but only when directly interacting with them).
- Translations available: English, French. New languages can be added easily (see [below for details](#translations)).

## But why?

The vanilla **cartography table** is extremely quirky.
This often leads to frustration on multiplayer servers when multiple players interact with the table on a regular basis, as shared pins seem to disappear / reappear constantly, and crossing off shared pins does not update the table.

On top of that, since the vanilla **cartography table** is "all or nothing", some players might refrain from engaging altogether.
This is typically the case for those that like to meticulously pin all berries / copper veins / etc. but don't want others to yell at them for cluttering the table, or those that do not want to share some super secret locations they would prefer to keep private.

And of course, the vanilla **cartography table** has no purpose in `NoMap` runs.

## Vanilla cartography tables, but Better™️

The goal of this mod is to stick as close as possible to the vanilla experience.
We keep **cartography tables** relevant for sharing both pins and **map exploration**, same as in vanilla: players must still interact with the same **cartography table** on a regular basis to synchronize progress with other players.
For `NoMap` runs, the goal is to give **cartography tables** a purpose.

<details>
<summary>How it works <b>(click to show details)</b></summary>

- When opening the map without interacting with a **cartography table**:
  - Your **private pins** can be interacted with, same as in vanilla.
  - **Public** or **guild pins** previously retrieved from a **cartography table** can be shown or hidden (akin to vanilla shared pins), but cannot be interacted with (cannot cross off, cannot remove).
  - In `NoMap` runs, the map will still refuse to open: it can only open by interacting with a **cartography table**.
- When hovering a **cartography table**:
  - Text will appear (akin to vanilla) and list information about the table and how to interact with it.
  - If [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) is installed, the table can be switched between **public mode** (default) or **guild mode**. When a table is in **guild mode**, only its guild members can interact with it.
- When interacting with a **cartography table**:
  - Retrieve other players' **map exploration** currently shared to the table, same as in vanilla.
  - If **map exploration** sharing is in **public mode** (default) or **guild mode** (and we are interacting with a **guild table**), share your **map exploration** to the table.
  - Retrieve **public** or **guild pins** currently shared to the table.
  - Open the map:
    - Your **private pins** can be interacted with, same as in vanilla, but can additionally be shared to the table (becoming **public** or **guild pins**).
    - **Public** or **guild pins** can be crossed off / removed akin to vanilla, or unshared from the table (becoming **private pins** on your map).
    - When multiple players interact with the same **cartography table** at the same time, all changes to **public** or **guild pins** are reflected in real time.

</details>

### Compatibility with vanilla clients

**Public** and **guild pins** are stored separately from vanilla shared pins in the **cartography table**, thus both modded and vanilla clients can interact with the same **cartography tables** without conflict.
Additionally, modded clients will copy **public pins** to vanilla shared pins, allowing vanilla clients to receive them seamlessly.
However, vanilla clients are not able to contribute back: modded clients can only receive pins from other modded clients.

### Compatibility with other mods

**Better™️ Cartography Table** tries to play nice with other mods by isolating its behaviour as much as possible, and failing that by trying to ensure the other mods will contribute pins in a compatible way. Nevertheless, there might be instances where other mods interacting with pins will result in incompatiblities. Feel free to [report any issue you find](https://github.com/nbusseneau/BetterCartographyTable/issues/new).

### Translations

**Better™️ Cartography Table** comes with the following languages available out of the box:

- English
- French

To add a new language as a user:

- Navigate to your `BepInEx/plugins/nbusseneau-Better_Cartography_Table/plugins/Translations/` directory.
  - From `r2modman`, use `Settings > Browse profile folder` to find your `BepInEx` directory.
- Make a copy of the `English` directory, then rename it to the appropriate name for your language (see [valid folder names](https://valheim-modding.github.io/Jotunn/data/localization/language-list.html)).
- Edit `<your_language_name>/BetterCartographyTable.json` as appropriate using a text editor.

If you localize **Better™️ Cartography Table** for your own language, you are most welcome to [send your translation file my way](https://github.com/nbusseneau/BetterCartographyTable/issues/new), and I will integrate it as part of the languages available by default.

## Install

This is technically a client-side mod, but **it is strongly recommended to also install the mod server-side** for the best experience.
If installed on the server, it will enforce all clients to have the mod installed.

Exception: do not install the mod on servers intended for Xbox crossplay, otherwise Xbox players will not be able to join anymore.
If you do allow crossplay, note that vanilla clients will not be able to share any of their pins with modded clients ([see above for details](#compatibility-with-vanilla-clients)).

### Thunderstore (recommended)

- **[Prerequisite]** Install [**r2modman**](https://thunderstore.io/c/valheim/p/ebkr/r2modman/).
- Click **Install with Mod Manager** from the [mod page](https://thunderstore.io/c/valheim/p/nbusseneau/Better_Cartography_Table/).
- **[Optional]** Install [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) for guild support.

### Manually (not recommended)

- **[Prerequisites]**
  - Install [**BepInExPack Valheim**](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
  - Install [**Jötunn, the Valheim Library**](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
- Download [nbusseneau-Better_Cartography_Table-0.5.3.zip](https://github.com/nbusseneau/BetterCartographyTable/releases/latest/download/nbusseneau-Better_Cartography_Table-0.5.3.zip).
- Extract the archive and move everything to your `BepInEx/plugins/` directory. It should look like this:
  ```
  BepInEx/
  └── plugins/
      └── nbusseneau-Better_Cartography_Table
          ├── CHANGELOG.md
          ├── icon.png
          ├── manifest.json
          ├── plugins/
          └── README.md
  ```
- **[Optional]** Install [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) for guild support.
