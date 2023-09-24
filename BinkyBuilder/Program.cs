using BinkyBuilder;
using FileHelpers;
using Scriban;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

TextWriter errorWriter = Console.Error;

if (args.Length == 0)
{
    errorWriter.WriteLine("You must provide the path to the mod folder with the binkyconfig.json file.");
    Environment.Exit(-1);
}

var currentDirectory = Directory.GetCurrentDirectory();
var templateDirectory = Path.Combine(currentDirectory, "templates");

var inputPath = args[0];
var modPath = Path.GetFullPath(inputPath);
Console.WriteLine($"Using Mod Path: {modPath}");

var wavToWemJsonPath = Path.Combine(modPath, "wavToWem.json");
var outputPath = Path.Combine(modPath, "output");
var configFile = Path.Combine(modPath, "binkyconfig.json");

if (!File.Exists(configFile))
{
    errorWriter.WriteLine($"{configFile} does not exist");
    Environment.Exit(-1);
}

IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configFile, false, true);
IConfigurationRoot binkyConfig = builder.Build();

var dialogueFile = $"{binkyConfig["dialogue_file"]}";
var language = $"{binkyConfig["mod_language"]}";
var modBnk = $"{binkyConfig["mod_voice_bnk"]}";
var wavsFolder = Path.Combine(modPath, $"{binkyConfig["wav_folder"]}");

// path to dialogue.csv
var dialogueCsvFile = Path.Combine(modPath, dialogueFile);

var wemTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "wem.scriban")));
var eventTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "event.scriban")));
var stopTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "stop_action.scriban")));
var playTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "play_action.scriban")));
var bnkTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "bnk.scriban")));
var talkParamTemplate = Template.Parse(File.ReadAllText(Path.Combine(templateDirectory, "talk_param.scriban")));

var hircObjects = new List<string>();
var wemPaths = new List<string>();
var talkMsgs = new Dictionary<string, string>();

// load wem uints from cache to avoid generating new wems IDs every time the program is run.
// after the mod dialogue.csv is processed, this dictionary should be cached in the mod folder as json.
var wavToWemIdDict = new Dictionary<string, uint>();
if (File.Exists(wavToWemJsonPath))
{
    wavToWemIdDict = JsonConvert.DeserializeObject<Dictionary<string, uint>>(File.ReadAllText(wavToWemJsonPath));
}

int currentDialogueId = 0;
int currentDialogueSet = 0;
string currentNpcId = "";

// NPC_ID|Dialogue|Dialogue Set|WAV
var engine = new FileHelperEngine<ModSoundData>();
var records = engine.ReadFile(dialogueCsvFile);

foreach (var record in records)
{
    if (record.NpcId != currentNpcId)
    {
        currentDialogueId = 0;
        currentNpcId = record.NpcId;
    }

    if (record.DialogueSet != currentDialogueSet)
    {
        currentDialogueId = 0;
        currentDialogueSet = record.DialogueSet;
    }

    uint wemObjectGuid = WwiseHash.HashGUID(Guid.NewGuid().ToString());
    uint playActionObjectGuid = WwiseHash.HashGUID(Guid.NewGuid().ToString());
    uint stopActionObjectGuid = WwiseHash.HashGUID(Guid.NewGuid().ToString());

    // calc wem id
    var wavFullPath = Path.Combine(wavsFolder, record.Wav);
    uint wemSourceId;
    if (wavToWemIdDict.ContainsKey(wavFullPath))
    {
        wemSourceId = wavToWemIdDict[wavFullPath];
    }
    else
    {
        wemSourceId = WwiseHash.HashGUID(Guid.NewGuid().ToString());
        wavToWemIdDict[wavFullPath] = wemSourceId;
    }

    hircObjects.Add(wemTemplate.Render(new { 
        sound_object_id = wemObjectGuid,
        plugin_codec = "WwiseCodecPcm",
        wem_source_id = wemSourceId, 
        override_bus_id = 3170124113 }));
    hircObjects.Add(playTemplate.Render(new { sound_object_id = playActionObjectGuid, external_id = wemObjectGuid }));
    hircObjects.Add(stopTemplate.Render(new { sound_object_id = stopActionObjectGuid, external_id = wemObjectGuid }));

    string msgId = $"{record.NpcId}00{1000 * record.DialogueSet + currentDialogueId * 10}";
    talkMsgs[msgId] = record.Dialogue;

    string inGamePlayEvent = $"Play_v{msgId}";
    string inGameStopEvent = $"Stop_v{msgId}";

    uint playEventId = WwiseHash.HashString(inGamePlayEvent);
    uint stopEventId = WwiseHash.HashString(inGameStopEvent);

    currentDialogueId++;

    hircObjects.Add(eventTemplate.Render(new { sound_object_id = playEventId, action_id = playActionObjectGuid }));
    hircObjects.Add(eventTemplate.Render(new { sound_object_id = stopEventId, action_id = stopActionObjectGuid }));

    wemPaths.Add($"{wemSourceId}");
}

string json = JsonConvert.SerializeObject(wavToWemIdDict);
System.IO.File.WriteAllText(wavToWemJsonPath, json);

/////////////////////////////
// start generating output //
/////////////////////////////

var outputFolder = outputPath;
System.IO.Directory.CreateDirectory(outputFolder);

// Render the bnkJson
Console.WriteLine($"Rendering {modBnk}.bnkjson");
var bnkjson = bnkTemplate.Render(new
{
    bank_id = WwiseHash.HashString($"{modBnk}"),
    language_fnv_hash = WwiseHash.HashString(language),
    hirc_objects = String.Join(",\n", hircObjects)
});
var bnkJsonOutput = $"{modBnk}.bnkjson";
var bnkJsonOutputFullPath = Path.Combine(outputFolder, bnkJsonOutput);
File.WriteAllText(bnkJsonOutputFullPath, bnkjson);

// create the mod/sd/enus folder
var enusFolder = Path.Combine(outputFolder, "mod", "sd", language);
Console.WriteLine($"Creating the {enusFolder} folder");
System.IO.Directory.CreateDirectory(enusFolder);

// Convert the .bnkjson file to the .bnk binary
string bnk2jsonToolPath = Path.Combine(currentDirectory, "tools", "bnk2json.exe");
ProcessStartInfo p = new ProcessStartInfo(bnk2jsonToolPath, bnkJsonOutputFullPath);
var process = Process.Start(p);
process.WaitForExit();

// rename the bnk and move it to the mod/sd/{language} folder
Console.WriteLine($"Moving the {modBnk}.bnk to the folder: {enusFolder}");
var bnkPath = Path.Combine(enusFolder, $"{modBnk}.bnk");
File.Move(Path.Combine(outputFolder, $"{modBnk}.bnk.rebuilt"), bnkPath, true);
File.Delete(Path.Combine(outputFolder, $"{modBnk}.bnkjson"));

// Convert the WAVs to WEMs, save in temp folder.
Console.WriteLine("Converting the WAVs to WEMs");
string tempWemsFolder = Path.Combine(outputFolder, "wems");
System.IO.Directory.CreateDirectory(tempWemsFolder);
string wavToWemTool = Path.Combine(currentDirectory, "tools", "converter_test.exe");
foreach (var kv in wavToWemIdDict)
{
    ProcessStartInfo wavToWemProcess = new ProcessStartInfo(wavToWemTool, AddQuotesIfRequired(kv.Key));
    Process.Start(wavToWemProcess).WaitForExit();
    
    File.Move(Path.Combine(currentDirectory, "test.wem"), Path.Combine(tempWemsFolder, $"{kv.Value}.wem"), true);
}

Console.WriteLine($"Moving the WEMs to the folder: {enusFolder}");
foreach (var wem in wemPaths)
{
    string wemFolder = wem.Substring(0, 2);
    string wemFinalFolder = Path.Combine(enusFolder, "wem", wemFolder);
    System.IO.Directory.CreateDirectory(wemFinalFolder);
    File.Move(Path.Combine(tempWemsFolder, $"{wem}.wem"), Path.Combine(wemFinalFolder, $"{wem}.wem"), true);
}
Directory.Delete(tempWemsFolder, true);

var talkData = File.ReadAllText(Path.Combine(modPath, "TalkMsg.fmg.json"));
JObject talkDataParsed = JObject.Parse(talkData);

var paramRows = new List<string>();

foreach (var msg in talkMsgs)
{
    paramRows.Add(talkParamTemplate.Render(new { 
        param_id = msg.Key.Remove(msg.Key.Length - 1),
        msg_id = msg.Key
    }));

    JObject obj = new JObject();
    obj.Add("ID", Int32.Parse(msg.Key));
    obj.Add("Text", msg.Value);
    ((JArray)talkDataParsed["Fmg"]["Entries"]).Add(obj);
}

// outputs a new TalkParam.csv that can be imported by DSMapStudio
var talkParamDiff = Path.Join(outputFolder, "TalkParam.csv");
Console.WriteLine($"Writing param diff to {talkParamDiff}");
var paramHeader = "ID,Name,disableParam_NT,disableParamReserve1,disableParamReserve2,msgId,voiceId,spEffectId0,motionId0,spEffectId1,motionId1,returnPos,reactionId,eventId,msgId_female,voiceId_female,lipSyncStart,lipSyncTime,pad2,timeout,talkAnimationId,isForceDisp,pad3,pad1,\n";
var paramData = paramHeader + String.Join("\n", paramRows);
System.IO.File.WriteAllText(talkParamDiff, paramData);

// outputs a new TalkMsg.fmg.json that can be imported by DSMapStudio
var talkFmgDiff = Path.Join(outputFolder, "TalkMsg.fmg.json");
Console.WriteLine($"Writing FMG diff to {talkFmgDiff}");
System.IO.File.WriteAllText(talkFmgDiff, talkDataParsed.ToString());

// TODO separate BNKs.
// get a wem dump to prevent overwriting the game wems.

// https://stackoverflow.com/questions/6521546/how-to-handle-spaces-in-file-path-if-the-folder-contains-the-space
static string AddQuotesIfRequired(string path)
{
    return !string.IsNullOrWhiteSpace(path) ?
        path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
            "\"" + path + "\"" : path :
            string.Empty;
}
