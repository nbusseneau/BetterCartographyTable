# Better™️ Cartography Table

[Valheim](https://store.steampowered.com/app/892970/Valheim/) mod that allows precise control over sharing pins via **cartography tables**.
Supports **private pins**, **public pins**, and **guild pins**.
Supports `NoMap` worlds.

## Video showcase

**(Click on items to show details)**

<details>
<summary><b>Private</b> and <b>public pins</b></summary>

Astrid and Brynhild are playing together, however Astrid has a tad too many mushroom pins for Brynhild's liking.
Thanks to **Better™️ Cartography Table**, their friendship is safe, as Astrid can selectively decide which pins to share on the public cartography table.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/e13e6267-88ad-4aee-bdfe-f78e807bc2f8

</details>

<details>
<summary>Real-time collaboration</summary>

Astrid and Brynhild are planning their next expedition.
Thanks to **Better™️ Cartography Table**, they can collaborate in real time on the cartography table.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/e535a301-994f-4129-b9ec-3e51685bab2c

</details>

<details>
<summary><b>Guild pins</b></summary>

Thanks to **Better™️ Cartography Table**, guilds can privately share pins amongst members.
Brynhild is a member of The Ground Shakers guild, and Astrid does not have access to their cartography table.
Astrid creates The Mushroom Enjoyers guild so that she can share a Super Secret Hideout with other mushroom enjoyers, which Brynhild will never know about.

https://github.com/nbusseneau/BetterCartographyTable/assets/4659919/421e90b4-f00f-4047-b9ce-3839ac499035

</details>

## Features

- Pins are **private** by default, and can be individually shared on a **public cartography table**.
  - You can now safely go ham and pin all those nice berries / copper veins / whatever floats your boat, without worrying about cluttering another player's map.
- When multiple players are interacting with the same **cartography table**, changes are reflected in real time for all of them.
  - This allows collaborating over the map in real time, especially useful when planning the next expedition.
- **Cartography tables** can be **public** (default) or **restricted to a guild** (if [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) is installed).
  - For when you want to share super secret guild hideouts with your mates. Not that it ever happens. Definitely don't look for super secret guild hideouts on your servers. Nope...
- If using the `NoMap` world modifier, map will be accessible only through a cartography table.

## But why?

The vanilla **cartography table** is quirky and often leads to frustration on multiplayer servers: it only stores the pins of the last player that wrote to the table, completely replacing what was previously recorded.

This often results in situations where shared pins seem to disappear / reappear constantly when multiple players interact with the table on a regular basis (and it also never updates cross off status...).

On top of that, since the **cartography table** is "all or nothing" both ways, some players might refrain from interacting with the table (e.g. when someone is meticulously pinning all berries / copper veins / etc., or when they do not want to share some super secret pins they would prefer to keep private).

And of course, the vanilla cartography table is completely useless in `NoMap` runs.

## Vanilla cartography tables, but Better™️

The goal is to stick close to the vanilla experience and keep **cartography tables** relevant (or in the case of `NoMap` runs, give them a purpose). We are not bypassing **cartography tables**: sharing map data (both pins and exploration) still requires players to interact with the same **cartography table** on a regular basis to synchronize progress.

How it works:

- When interacting with a **cartography table**:
  - Synchronize map exploration (same as vanilla).
  - Retrieve **table pins** currently stored on the table.
  - Open the map:
    - **Private pins** can be interacted with, and can additionally be stored on the table, becoming **table pins** (either **public** or **guild pins**, depending on the table mode).
    - **Table pins** can be interacted with, and can additionally be unstored from the table, becoming **private** pins. If multiple players are currently interacting with the same **cartography table**, changes to **table pins** are reflected in real time for all of them.
- When opening the map without interacting with a **cartography table**:
  - **Private pins** can be interacted with.
  - **Public** or **guild pins** previously retrieved from a **cartography table** can be displayed / hidden, but cannot be interacted with.

### Compatibility with non-modded vanilla clients

**Table pins** are stored in a custom ZDO key under the **cartography table**'s ZNetView's ZDO, thus non-modded vanilla clients and modded clients can both interact with the same **cartography tables** without conflict.
Limited one-way sharing is provided, as modded clients also write public pins to the vanilla shared data, allowing non-modded vanilla clients to receive public pins seamlessly.
However, non-modded vanilla clients are not able to contribute back: modded clients can only receive pins from other modded clients.

## Install

This is technically a client-side mod, and is not strictly required server-side.
Yet, **it is strongly recommended to install the mod server-side** so as to enforce that all clients have the mod.

Exception: do not install the mod on servers intended for Xbox crossplay, otherwise Xbox players will not be able to join anymore.
Note that non-modded vanilla clients will not be able to share any of their pins with modded clients ([see above for details](#compatibility-with-non-modded-vanilla-clients)).

### Thunderstore (recommended)

- **[Prerequisite]** Install [**r2modman**](https://thunderstore.io/c/valheim/p/ebkr/r2modman/).
- Click **Install with Mod Manager** from the [mod page](https://thunderstore.io/c/valheim/p/nbusseneau/Better_Cartography_Table/).
- **[Optional]** Install [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) for guild support.

### Manually (not recommended)

- **[Prerequisites]**
  - Install [**BepInExPack Valheim**](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
  - Install [**Jötunn, the Valheim Library**](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
- Create a new directory `nbusseneau-Better_Cartography_Table` in your `BepInEx/plugins/` directory.
- Download [nbusseneau-Better_Cartography_Table-0.4.0.zip](https://github.com/nbusseneau/BetterCartographyTable/releases/latest/download/nbusseneau-Better_Cartography_Table-0.4.0.zip).
- Extract the archive.
- Move all files to your `BepInEx/plugins/nbusseneau-Better_Cartography_Table` directory. It should look like this:
  ```
  BepInEx/
  └── plugins/
      └── nbusseneau-Better_Cartography_Table
          ├── BetterCartographyTable.dll
          ├── CHANGELOG.md
          ├── icon.png
          ├── manifest.json
          └── README.md
  ```
- **[Optional]** Install [**Guilds**](https://thunderstore.io/c/valheim/p/Smoothbrain/Guilds/) for guild support.
