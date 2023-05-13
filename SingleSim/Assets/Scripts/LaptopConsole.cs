using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class LaptopConsole
{
    private static Queue<string> consoleStorage = new Queue<string>();
    private static Dictionary<string, (bool visible, System.Action<string[]> command)> consoleCommands = new Dictionary<string, (bool visible, System.Action<string[]> command)>(); //Stores the default command, its action and the associated arguments
    public static void FirstConsoleLoad()
    {
        consoleStorage.Clear();
        consoleCommands.Clear();

        consoleStorage.Enqueue("<color=#FFFFFF>Enter Help for a list of commands</color>");

        //Add console commands
        consoleCommands.Add("Help",(true,(args) => DisplayCommands(args)));
        consoleCommands.Add("Clear", (true, (args) => ClearCommands(args)));

        //Debug commands
        consoleCommands.Add("Debug.AddCredits", (false, (args) => DebugAddCredits(args)));
        consoleCommands.Add("Debug.Peek", (false, (args) => DebugPeekAlien(args)));
        consoleCommands.Add("Debug.Decode", (false, (args) => DebugFinishDecode(args)));
        consoleCommands.Add("Debug.xray", (false, (args) => DebugSeeCommands(args)));
        consoleCommands.Add("Debug.swarm", (false, (args) => DebugSwarmSignals(args)));
        consoleCommands.Add("Debug.noise", (false, (args) => DebugPlaySound(args)));
        consoleCommands.Add("Debug.tutorial", (false, (args) => DebugGetTutorialState(args)));
        consoleCommands.Add("Debug.max", (false, (args) => DebugMaximumPower(args)));
        consoleCommands.Add("Debug.email", (false, (args) => DebugAddEmail(args)));
    }
    public static void ReloadConsole(ref TMPro.TextMeshProUGUI consoleObject)
    {
        consoleObject.text = "";
        int finalIndex = 0; //Check the size of the queue 

        foreach (string line in consoleStorage)
        {
            if (line == "") { break; } //Stop ammending to console if the array is empty
            else { consoleObject.text += "\n"; finalIndex += 1; }

            consoleObject.text += line;
        }

        for(int i=finalIndex;i>19;i--) //For each item over the console limit
        {
            consoleStorage.Dequeue(); //Remove all items over the limit
        }
    }
    public static void SubmitItem(ref TMPro.TextMeshProUGUI consoleObject, string input)
    {
        consoleStorage.Enqueue(">" + input);

        string[] delimInput = input.Split(' ');

        System.Action<string[]> associatedCommand = consoleCommands.FirstOrDefault(x => x.Key.ToUpper() == delimInput[0].ToUpper()).Value.command;
        if(associatedCommand == null) { consoleStorage.Enqueue("<color=#B80e20>Invalid command. Enter Help for a list of commands</color>"); } //If the command is invalid, display default text
        else 
        {
            associatedCommand.Invoke((delimInput.Length > 1 ? delimInput.Skip(1).ToArray() : new string[] { "null" }));
        } //If the command is valid, run the associated command with the passed arguments
        ReloadConsole(ref consoleObject);
    }

    public static void GlobalWrite(string writeObj)
    {
        consoleStorage.Enqueue("<color=#FFFFFF>" + writeObj + "</color>");
    }
    //Console commands
    private static void DisplayCommands(string[] args)
    {
        string commandList = "";
        foreach(string key in consoleCommands.Where(x=> x.Value.visible == true).Select(x=>x.Key)) //Iterate through all the keys with the visible attribute
        {
            commandList += key + ", ";
        }

        consoleStorage.Enqueue("<color=#FFFFFF>Currently accessible commands: " + commandList + "</color>");
    }
    private static void ClearCommands(string[] args)
    {
        consoleStorage.Clear();
        consoleStorage.Enqueue("<color=#FFFFFF>Enter Help for a list of commands</color>");
    }

    //Debug commands
    private static void DebugAddCredits(string[] args)
    {
        int creditsToAppend = 0;
        if (int.TryParse(args[0], out creditsToAppend)) { 
            consoleStorage.Enqueue("<color=#FFFFFF>Appended " + creditsToAppend + " credits</color>");
            Gameplay.credits += creditsToAppend;
        }
        else { consoleStorage.Enqueue("<color=#B80e20>Invalid credit amount: " + args[0] + "</color>"); }
    }
    private static void DebugPeekAlien(string[] args)
    {
        if(Gameplay.activeAlien != null)
        {
            string alienMessage = "<color=#FFFFFF>Message:\n" + Gameplay.activeAlien.decodeTextMessage +
                "\nSelfParams: " + Gameplay.activeAlien.selfParams.body + ", " + Gameplay.activeAlien.selfParams.age + ", " + Gameplay.activeAlien.selfParams.job + ", " + Gameplay.activeAlien.selfParams.relationshipGoal +
                "\nPrefParams: " + Gameplay.activeAlien.preferenceParams.body + ", " + Gameplay.activeAlien.preferenceParams.age + ", " + Gameplay.activeAlien.preferenceParams.job + ", " + Gameplay.activeAlien.preferenceParams.relationshipGoal + "</color>";
            consoleStorage.Enqueue(alienMessage);
        }
        else
        {
            consoleStorage.Enqueue("<color=#B80e20>No loaded signal</color>");
        }
    }

    private static void DebugFinishDecode(string[] args)
    {
        if (Gameplay.activeAlien != null && Gameplay.activeAlien.decoderProgress > 0)
        {
            Gameplay.activeAlien.decoderProgress = 1;
            consoleStorage.Enqueue("<color=#FFFFFF>Finished decoding</color>");
        }
        else
        {
            consoleStorage.Enqueue("<color=#B80e20>Signal either does not exist or is not in decoding phase</color>");
        }
    }
    private static void DebugSeeCommands(string[] args)
    {
        string[] commandsSet = consoleCommands.Keys.ToArray();
        foreach(string key in commandsSet)
        {
            consoleCommands[key] = (true, consoleCommands[key].command);
        }
        consoleStorage.Enqueue("<color=#FFFFFF>Made all commands visible in help menu</color>");
    }

    private static void DebugSwarmSignals(string[] args)
    {
        int signalsToAdd = 0;
        if (int.TryParse(args[0], out signalsToAdd))
        {
            for (int i = 0; i < signalsToAdd; i++)
            {
                Gameplay.storedAliens.Add(new Alien(Gameplay.ReturnImage));
            }
            consoleStorage.Enqueue("<color=#FFFFFF>Added " + args[0] + " new signals to database</color>");
        }
        else
        {
            consoleStorage.Enqueue("<color=#B80e20>Invalid signal count: " + args[0] + "</color>");
        }
    }

    private static void DebugPlaySound(string[] args)
    {
        float reqVolume;
        if (args.Length < 2) { consoleStorage.Enqueue("<color=#B80e20>Sound player requires two arguments: soundbyte name and float volume</color>"); }
        else
        {
            if (float.TryParse(args[1], out reqVolume))
            {
                if (AudioHandler.ConsolePlaySound(args[0], reqVolume))
                {
                    consoleStorage.Enqueue("<color=#FFFFFF>Playing soundbyte " + args[0] + "</color>");
                }
                else
                {
                    consoleStorage.Enqueue("<color=#B80e20>Invalid soundbyte " + args[0] + "</color>");
                }
            }
            else
            {
                consoleStorage.Enqueue("<color=#B80e20>Invalid volume " + args[1] + "</color>");
            }
        }
    }

    private static void DebugGetTutorialState(string[] args)
    {
        consoleStorage.Enqueue("<color=#FFFFFF>Current tutorial state: " + Gameplay.tutorialState + "</color>");
    }

    private static void DebugMaximumPower(string[] args)
    {
        string[] itemNames = Gameplay.shopItems.Select(x => x.name).ToArray();

        foreach(string itemName in itemNames)
        {
            for(int i = 0; i< 10; i++)
            {
                Gameplay.UpgradeVariable(itemName);
            }
        }

        consoleStorage.Enqueue("<color=#FFFFFF>Set all shop item values to maximum</color>");
    }

    private static void DebugAddEmail(string[] args)
    {
        if(args.Length < 4) { consoleStorage.Enqueue("<color=#B80e20>Insufficient Parameters</color>"); }
        else
        {
            LaptopHandler.AddEmail(args[1], args[2], args[3],false);
            consoleStorage.Enqueue("<color=#FFFFFF>Added email</color>");
        }
    }
}
