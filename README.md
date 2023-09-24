# BinkyBuilder

BinkyBuilder is a tool that let's you create **sound dialogue mods** for Elden Ring. 

It's made specifically for adding new NPCs to your mods, with the respective `TalkMsg.fmg`, `TalkParam`, wems, and audio bnk.

**NOTE:** this tool is a _hackish_ way of getting audio modding going for the community. The audio banks it builds aren't 100% as per how wwise builds them, but it's good enough for start prototyping your new NPCs and audio mods. The tool is also provided without any warranty. See LICENSE.md for details.

## Usage

Simply drag & drop `your_mod` folder onto the `BinkyBuilder.exe` to build the required sound files for your mod.

**KEEP IN MIND THIS FOLDER IS NOT YOUR ELDEN RING MOD FOLDER**, this folder is just for the purpose of generating the audio aspect of your mod.

See `BinkyBuilder/sample_mod/` to understand how to setup a sound mod.

```goat
BinkyBuilder/sample_mod/
├── TalkMsg.fmg.json
├── binkyconfig.json
├── dialogue.csv
└── wavs
    ├── 01_yes_indeed.wav
    ├── 02_was_welcoming_you.wav
    ├── 03_since_elden_ring.wav
    ├── 04_the_souls_community.wav
    ├── 05_trying_to_figure_out.wav
    ├── 06_the_games_new_audio.wav
    ├── 07_from_different_file_formats.wav
    ├── 08_to_alternate_ways.wav
    ├── 09_this_was_a_task.wav
    ├── 10_the_final_piece.wav
    ├── 11_this_means_modders.wav
    ├── 12_Including_custom_dialogue.wav
    ├── 13_let_this_humble.wav
    ├── Good Morning Guys.wav
    └── Welcome to audio modding in Elden Ring.wav
```

Let's understand each of those files.

`sample_mod/wavs/*.wav` these are the .wav audio files for your mod. You can name these however you want, but try to keep some structure so you know what goes where once you start building your `dialogue.csv` file.

The `dialogue.csv` is the main file for your mod. This file will be read by `BinkyBuilder` and from there your mod will be built. It's a .csv file using the pipe `|` character as separator. This is done because a dialogue in Elden Ring could have any of these characters `"`, `,`, and `;`. So for easer parsing the pipe was chosen.

```csv
400|Bom dia pessoal!|1|Good Morning Guys.wav
400|Bem vindos ao modding de áudio de Elden Ring!|1|Welcome to audio modding in Elden Ring.wav
401|Yes, indeed... That guy back there|1|01_yes_indeed.wav
401|Was welcoming you to audio modding in Brazilian Portuguese|1|02_was_welcoming_you.wav
401|Since Elden Ring came out in 2022|2|03_since_elden_ring.wav
401|The souls modding community has been hard at work|2|04_the_souls_community.wav
401|Trying to figure out Wwise,|2|05_trying_to_figure_out.wav
401|The game's new audio engine|2|06_the_games_new_audio.wav
401|From different file formats,|3|07_from_different_file_formats.wav
401|To alternate ways for the game to load sound files,|3|08_to_alternate_ways.wav
401|This was a task that required countless hours of reverse engineering.|3|09_this_was_a_task.wav
401|The final piece of the puzzle is here, custom sound banks!|4|10_the_final_piece.wav
401|This means modders will be able to work on adding new quest lines|4|11_this_means_modders.wav
401|Including custom dialogue with their own voice acting!|4|12_Including_custom_dialogue.wav
401|Let this humble video be the start of audio modding in Elden Ring.|5|13_let_this_humble.wav
```

The colums are: `NPC_ID|Dialogue|Dialogue Set|WAV`.

In Elden Ring each NPC is identified by 3 digits. See [NPC_LIST.csv](https://github.com/sekirodubi/BinkyBuilder/blob/main/NPC_LIST.csv) for a list of NPCs and their IDs.

For example, Tanith is `300`, Morgott is `204`, and so on. What matters here is that the audio banks used for NPC dialogue are nammed like this: `vc300.bnk`. So when you choose NPC ids for your mod, make sure to chose 3 digits that aren't used by Elden Ring, otherwise you will overwrite those sounds as well.

In the example above, there's 2 new NPCs, 400, and 401.

The `dialogue` column specifies the subtitles for that specific line.

The `Dialogue Set` number is used to group dialogue lines. Here's an example from Elden Ring Kalé's so you can see how this works:

```xml
<text id="800001000">You're a Tarnished, I can see it.</text>
<text id="800001010">And I can also see...</text>
<text id="800001020">That you're not after my throat.</text>
<text id="800001030">Then why not purchase a little something?</text>
<text id="800001040">I am Kalé. Purveyor of fine goods.</text>

<text id="800002000">What is it? Still going to purchase something?</text>

<text id="800003000">Wait, weren't you...?</text>
<text id="800003010">Well, you're back. Care to buy something?</text>
```

As you can see there, the first three digits of those ids are `800`, that's Kalé's NPC id. The last 4 digits, change per each dialogue line. There's the 1000 range, the 2000 range, 3000 range, and so on. Those ranges is what `BinkyBuilder` considers as `Dialogue Sets`. Each of those will become a `Talk` interaction between the player and the NPC.

The final column is the WAV file name. The mod will automatically convert those to wems.

Finally the file `TalkMsg.fmg.json` should be exported from DSMapStudio `Text Editor` tab. `BinkyBuilder` will take care of merging your mod dialogue with the file exported from DSMapStudio.

Let's see how to integrate your audio mod with your Elden Ring mod.

## Integrating your audio mod with your Elden Ring mod

Here's an example of the files that `BinkyBuilder` will generate for your mod:

```goat
sample_mod/output
├── TalkMsg.fmg.json
├── TalkParam.csv
└── mod
    └── sd
        └── enus
            ├── vc401.bnk
            └── wem
                ├── 15
                │   └── 159310185.wem
                ├── 20
                │   └── 202123389.wem
                ├── 23
                │   └── 237656542.wem
                ├── 33
                │   └── 334867124.wem
                ├── 35
                │   └── 357778652.wem
                ├── 37
                │   └── 375808961.wem
                ├── 39
                │   └── 39798180.wem
                ├── 40
                │   └── 405668519.wem
                ├── 43
                │   └── 433411425.wem
                ├── 44
                │   └── 448158228.wem
                ├── 63
                │   └── 637165793.wem
                ├── 66
                │   └── 668417097.wem
                ├── 67
                │   ├── 673035966.wem
                │   └── 674521293.wem
                └── 75
                    └── 759780827.wem
```

* `TalkMsg.fmg.json` import this file into DSMapStudio. Go to the `Text Editor` tab, then click on the `Import/Export > Import Files` menu. Select your `TalkMsg.fmg.json` and import it.

* For the game to play audio dialogue, it needs a way to find your new audio files. This mapping is specified in the `TalkParam` of the `regulation.bin` file. Merge `TalkParam.csv` using DSMapStudio. Go to the `Text Editor` tab, then click on the `Edit > Import CSV > All fields` menu, and select the `TalkParam.csv` to merge it.

Finally copy the `sd` folder into your Elden Ring mod `mod` folder.

The tool also outputs a file called `wavToWem.json`. This is required because in wwise wem IDs are randomly generated. This file is used to cache the generated IDs for successive runs of `BinkyBuilder` otherwise, every run could potentially generate new `.wem` files.

## Using the audio mod in game

These type of mods add new NPCs to the game. A process which is quite involved. To get your new NPC working with audio, you need the following:

### Create the NPC quest file

This would be an `.esd` file that goes into the respective map `talkesdbnd` in the `script\talk` folder of your mod. See [ESDLang](https://github.com/thefifthmatt/ESDLang) to learn how to create quest files, and where to actually place them. Look at the game own's quest files to see examples. Use the [NPC_LIST.csv](https://github.com/sekirodubi/BinkyBuilder/blob/main/NPC_LIST.csv) mentioned above to understand which quest file belongs to which NPC.

Name your NPC quest files like this: `t401006000.esd`, where the ID should be read as: `tXXX-YY-ZZZZ`, `XXX`: the NPC ID. `YY` the NPC variation. `ZZZZ` the map id, `6000` for limgrave, `1000` would be Stormveil, etc.

### Call TalkParams from the quest file

From the `esd` you need to call your `TalParams` like this `assert t401006000_x30(text4=40100300, z5=1042369308, mode6=1)`, meaning play dialogue `40100300`, and set flag `1042369308` to `ON` which should be a flag ID that relates to whatever should be happening at that point in your NPCs questline.

### Add your new NPC to the map in DSMapStudio. 

What's **really important here** is that you assign the `TalkID` property you just created to you NPC on the map. This will not only make your NPC interactable, but also will tell the game to load the audio bank. In this example, the game will load the `vc401.bnk`. You should repeat this process for every NPC that you add to the game.

And that should be it! Launch Elden Ring, and start testing your mod.

## TODO

Rewrite the `bnk` output, so it produces one bnk per NPC, instead of a single BNK per mod.

## Credits

The tools `bnk2json.exe` and `converter_test.exe` were kindly provided by ChainFailure.

Made with the help of ChainFailure and MagicalShion.

GitHub workflows adapted from [WitchyBND](https://github.com/ividyon/WitchyBND)