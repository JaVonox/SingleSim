using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class LaptopConsole
{
    private static Queue<string> consoleStorage = new Queue<string>();
    private static Dictionary<string, System.Action<string[]>> consoleCommands = new Dictionary<string, System.Action<string[]>>(); //Stores the default command, its action and the associated arguments
    public static void FirstConsoleLoad()
    {
        consoleStorage.Enqueue("Enter Help for a list of commands");

        //Add console commands
        consoleCommands.Add("Help", (args) => DisplayCommands(args)); 
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

        System.Action<string[]> associatedCommand = consoleCommands.FirstOrDefault(x => x.Key.ToUpper() == delimInput[0].ToUpper()).Value;
        if(associatedCommand == null) { consoleStorage.Enqueue("Invalid command. Enter Help for a list of commands"); } //If the command is invalid, display default text
        else { associatedCommand.Invoke((delimInput.Length > 1 ? delimInput.Skip(1).ToArray() : new string[]{""})); } //If the command is valid, run the associated command with the passed arguments
        ReloadConsole(ref consoleObject);
    }

    //Console commands
    private static void DisplayCommands(string[] args)
    {
        string commandList = "";
        foreach(string key in consoleCommands.Keys)
        {
            commandList += key + ",";
        }

        consoleStorage.Enqueue("Currently accessible commands: " + commandList);
    }
}
