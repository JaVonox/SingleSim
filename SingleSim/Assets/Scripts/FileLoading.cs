using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Linq;
public class FileLoading
{
    private static string savesPath = System.IO.Directory.GetCurrentDirectory().ToString() + "/Saves/";
    public static XmlWriterSettings settings = new XmlWriterSettings();

    public static List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> GetMessages()
    {
        List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> messages = new List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)>();
        
        XmlDocument xmlReader = new XmlDocument(); //Open xmlfile

        {
            TextAsset asset = (TextAsset)Resources.Load("Messages");
            xmlReader.LoadXml(asset.text);
        }

        XmlNode messageNodes = xmlReader.SelectSingleNode("messagesList");

        Dictionary<string, BodyType> refTypes = new Dictionary<string, BodyType>(); //Store the appropriate string to body type
        refTypes.Add("humanoid", BodyType.humanoid);
        refTypes.Add("automaton", BodyType.automaton);
        refTypes.Add("cephalopod", BodyType.cephalopod);
        refTypes.Add("insectoid", BodyType.insectoid);

        foreach(XmlNode node in messageNodes.ChildNodes) //Load each message from the xml file
        {
            (BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement) newItem = (BodyType.NoPref, "" , new Dictionary<System.Type, string>(), "", ""); //item to be added to list
            newItem.type = refTypes[node.Attributes["type"].Value];
            newItem.unprocessedContents = node["body"].InnerText;

            foreach (XmlNode replacementNode in node["replacements"].ChildNodes) //Load each message from the xml file
            {
                switch(replacementNode.Attributes["type"].Value.ToString())
                {
                    case "pref_body":
                        newItem.noPrefReplacements.Add(typeof(BodyType),replacementNode.InnerText);
                        break;
                    case "pref_age":
                        newItem.noPrefReplacements.Add(typeof(AgeType), replacementNode.InnerText);
                        break;
                    case "pref_job":
                        newItem.noPrefReplacements.Add(typeof(OccupationType), replacementNode.InnerText);
                        break;
                    case "pref_goal":
                        newItem.noPrefReplacements.Add(typeof(GoalsType), replacementNode.InnerText);
                        break;
                    case "self_unemployed":
                        newItem.selfUnemployedReplacement = replacementNode.InnerText;
                        break;
                    case "pref_unemployed":
                        newItem.prefUnemployedReplacement = replacementNode.InnerText;
                        break;
                    default:
                        Debug.Log("invalid attribute on node " + replacementNode.Attributes["type"].Value.ToString());
                        break;
                }
            }

            messages.Add(newItem);
        }

        return messages;
    }

    //TOOD ADD SETTINGS IN PLAYERPREFS

    public static bool DoesDirExist(string? addPath)
    {
        if (addPath == null) //Checks if the save file directory exists
        {
            return Directory.Exists(savesPath);
        }
        else //Checks if a save with the name specified exists
        {
            return File.Exists(savesPath + "/" + addPath + ".sav");
        }
    }
    public static bool IsValidFilename(string filename)
    {
        if (filename.Contains(".")) { return false; }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char x in invalidChars)
        {
            if (filename.Contains(x.ToString())) {
                return false; 
            }
        }

        return true;
    }
    public static void CreateSave(string saveName, bool overwriting)
    {
        settings.Indent = true;

        if (!Directory.Exists(savesPath)) //Create a save folder if one does not exist
        {
            Directory.CreateDirectory(savesPath);
        }

        if (!overwriting) //When creating a new file
        {
            XmlWriter xmlWriter = XmlWriter.Create(savesPath + "/" + saveName + ".sav", settings);
            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("Data");
            xmlWriter.WriteAttributeString("version", Gameplay.gameVersion);
            xmlWriter.WriteAttributeString("saveName", saveName);
            xmlWriter.WriteAttributeString("time", System.DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

            WriteGameplayVars(ref xmlWriter);
            WriteShopLevels(ref xmlWriter);
            WriteAliens(ref xmlWriter);

            xmlWriter.WriteStartElement("Emails");
            if (LaptopHandler.emails.Count > 0)
            {
                foreach (Email x in LaptopHandler.emails)
                {
                    xmlWriter.WriteStartElement("Email");
                    xmlWriter.WriteAttributeString("sender", x.sender);
                    xmlWriter.WriteAttributeString("subject", x.subject);
                    xmlWriter.WriteAttributeString("time", x.recievedTime.ToString());
                    xmlWriter.WriteString(x.text);
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("EmailsQueue");
            if (LaptopHandler.emailQueue.Count > 0)
            {
                List<Email> queuedEmails = new List<Email>();
                queuedEmails = LaptopHandler.emailQueue.ToList(); //TODO - if an email gets queued while saving occurs will this break stuff??

                foreach (Email x in queuedEmails)
                {
                    xmlWriter.WriteStartElement("Email");
                    xmlWriter.WriteAttributeString("sender", x.sender);
                    xmlWriter.WriteAttributeString("subject", x.subject);
                    xmlWriter.WriteAttributeString("time", x.recievedTime.ToString());
                    xmlWriter.WriteString(x.text);
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
    }

    public static void LoadSave(string path)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(path)); //Open the xml file at the path

            XmlNode mainDataNode = doc.SelectSingleNode("Data");
            Gameplay.prevSaveName = mainDataNode.Attributes["saveName"].Value;

            Gameplay.scanProg = float.Parse(mainDataNode["Gameplay"]["scanProg"].InnerText);

            if (mainDataNode["Gameplay"]["scanSpots"].HasChildNodes)
            {
                foreach (XmlNode i in mainDataNode["Gameplay"]["scanSpots"].ChildNodes)
                {
                    Scanspot nScan = new Scanspot(float.Parse(i.Attributes["x"].Value), float.Parse(i.Attributes["y"].Value), int.Parse(i.Attributes["freq"].Value));
                    Gameplay.scanCoords.Add(nScan);
                }
            }

            Gameplay.scanSpotsAreAvailable = bool.Parse(mainDataNode["Gameplay"]["scanSpotsAreAvailable"].InnerText);
            Gameplay.UIcoordinates = (double.Parse(mainDataNode["Gameplay"]["UIcoordinates"].Attributes["x"].Value), double.Parse(mainDataNode["Gameplay"]["UIcoordinates"].Attributes["x"].Value));
            Gameplay.scanUIText = mainDataNode["Gameplay"]["scanUIText"].InnerText;
            Gameplay.currentScanTextPos = int.Parse(mainDataNode["Gameplay"]["currentScanTextPos"].InnerText);
            Gameplay.credits = int.Parse(mainDataNode["Gameplay"]["credits"].InnerText);
            Gameplay.lifetimeCredits = int.Parse(mainDataNode["Gameplay"]["lifetimeCredits"].InnerText);
            Gameplay.tutorialState = byte.Parse(mainDataNode["Gameplay"]["tutorialState"].InnerText);
            Gameplay.tutorialStateUpdateNeeded = bool.Parse(mainDataNode["Gameplay"]["tutorialStateUpdateNeeded"].InnerText);
            Gameplay.lastLoadedHz = int.Parse(mainDataNode["Gameplay"]["lastLoadedHz"].InnerText);
            Gameplay.lastSentHz = int.Parse(mainDataNode["Gameplay"]["lastSentHz"].InnerText);
            ScannerControls.currentState = (ScanState)(ScanState.Parse(typeof(ScanState), mainDataNode["Gameplay"]["scannerEnumState"].InnerText));
            Gameplay.scannerState = mainDataNode["Gameplay"]["gameplayScannerState"].InnerText;
            Gameplay.decoderState = mainDataNode["Gameplay"]["gameplayDecoderState"].InnerText;

            for(int i = 0;i < Gameplay.shopItems.Count;i++)
            {
                (string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost) item = Gameplay.shopItems[i];
                int level = int.Parse(mainDataNode["ShopLevels"].ChildNodes.Cast<XmlNode>().First(x => x.Attributes["name"].Value == item.name).InnerText);

                for (int j = 0; j < level; j++)
                {
                    Gameplay.UpgradeVariable(item.name,false);
                }
            }


            if (mainDataNode["Emails"].HasChildNodes)
            {
                foreach (XmlNode child in mainDataNode["Emails"].ChildNodes)
                {
                    LaptopHandler.AddEmail(child.Attributes["sender"].Value, child.Attributes["subject"].Value, child.InnerText, System.DateTime.Parse(child.Attributes["time"].Value),true);
                }
            }

            if (mainDataNode["EmailsQueue"].HasChildNodes)
            {
                foreach (XmlNode child in mainDataNode["Emails"].ChildNodes)
                {
                    LaptopHandler.emailQueue.Enqueue(new Email(child.Attributes["sender"].Value, child.Attributes["subject"].Value, child.InnerText, System.DateTime.Parse(child.Attributes["time"].Value)));
                }
            }

            if (mainDataNode["Aliens"]["CurrentSignal"].HasChildNodes)
            {
                Gameplay.activeAlien = LoadAlien(mainDataNode["Aliens"]["CurrentSignal"]["Alien"]);
            }


            if (mainDataNode["Aliens"]["StoredSignals"].HasChildNodes)
            {
                foreach(XmlNode child in mainDataNode["Aliens"]["StoredSignals"].ChildNodes)
                {
                    Gameplay.storedAliens.Add(LoadAlien(child));
                }
            }

        }
        catch(System.Exception ex)
        {
            Debug.LogError("File loading error " + ex);
        }

    }
    private static void WriteGameplayVars(ref XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("Gameplay");

        xmlWriter.WriteStartElement("scanProg");
        xmlWriter.WriteString(Gameplay.scanProg.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("scanSpots");

        if (Gameplay.scanCoords.Count > 0)
        {
            foreach (Scanspot i in Gameplay.scanCoords)
            {
                xmlWriter.WriteStartElement("scanSpot");
                xmlWriter.WriteAttributeString("x", i.x.ToString());
                xmlWriter.WriteAttributeString("y", i.y.ToString());
                xmlWriter.WriteAttributeString("freq", i.freq.ToString());
                xmlWriter.WriteEndElement();
            }
        }
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("scanSpotsAreAvailable");
        xmlWriter.WriteString(Gameplay.scanSpotsAreAvailable.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("UIcoordinates");
        xmlWriter.WriteAttributeString("x", Gameplay.UIcoordinates.x.ToString());
        xmlWriter.WriteAttributeString("y", Gameplay.UIcoordinates.y.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("scanUIText");
        xmlWriter.WriteString(Gameplay.scanUIText.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("currentScanTextPos");
        xmlWriter.WriteString(Gameplay.currentScanTextPos.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("credits");
        xmlWriter.WriteString(Gameplay.credits.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("lifetimeCredits");
        xmlWriter.WriteString(Gameplay.lifetimeCredits.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("tutorialState");
        xmlWriter.WriteString(Gameplay.tutorialState.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("tutorialStateUpdateNeeded");
        xmlWriter.WriteString(Gameplay.tutorialStateUpdateNeeded.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("lastLoadedHz");
        xmlWriter.WriteString(Gameplay.lastLoadedHz.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("lastSentHz");
        xmlWriter.WriteString(Gameplay.lastSentHz.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("scannerEnumState");
        xmlWriter.WriteString(ScannerControls.currentState.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("gameplayScannerState");
        xmlWriter.WriteString(Gameplay.scannerState.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("gameplayDecoderState");
        xmlWriter.WriteString(Gameplay.decoderState.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    private static void WriteShopLevels(ref XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("ShopLevels");

        foreach ((string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost) item in Gameplay.shopItems)
        {
            xmlWriter.WriteStartElement("ShopItem");
            xmlWriter.WriteAttributeString("name", item.name);
            xmlWriter.WriteString(item.upgradeLevel.ToString());
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
    }

    private static void WriteAliens(ref XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("Aliens");

        xmlWriter.WriteStartElement("CurrentSignal");
        if (Gameplay.activeAlien != null)
        {
            WriteSingleAlien(ref xmlWriter, Gameplay.activeAlien);
        }
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StoredSignals");

        if (Gameplay.storedAliens.Count > 0)
        {
            foreach (Alien x in Gameplay.storedAliens)
            {
                WriteSingleAlien(ref xmlWriter, x);
            }
        }

        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    private static void WriteSingleAlien(ref XmlWriter xmlWriter, Alien alienToStore)
    {
        xmlWriter.WriteStartElement("Alien");
        xmlWriter.WriteAttributeString("signalName", alienToStore.signalName);

        xmlWriter.WriteStartElement("imageID");
        xmlWriter.WriteString(alienToStore.imageID.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("decoderProg");
        xmlWriter.WriteString(alienToStore.decoderProgress.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("baseDecodeSpeed");
        xmlWriter.WriteString(alienToStore.baseDecodeSpeed.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("decodeTextProg");
        xmlWriter.WriteString(alienToStore.decodeTextProg.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("decodeTextMessage");

        string processedMessage = alienToStore.decodeTextMessage;
        processedMessage = processedMessage.Replace("", "<color=#e7d112>");
        processedMessage = processedMessage.Replace("`", "<color=#A60EB8>");
        processedMessage = processedMessage.Replace("", "</color>");

        xmlWriter.WriteString(processedMessage.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("SelfParams");

        xmlWriter.WriteStartElement("body");
        xmlWriter.WriteString(alienToStore.selfParams.body.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("age");
        xmlWriter.WriteString(alienToStore.selfParams.age.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("job");
        xmlWriter.WriteString(alienToStore.selfParams.job.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("goal");
        xmlWriter.WriteString(alienToStore.selfParams.relationshipGoal.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PrefParams");

        xmlWriter.WriteStartElement("body");
        xmlWriter.WriteString(alienToStore.preferenceParams.body.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("age");
        xmlWriter.WriteString(alienToStore.preferenceParams.age.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("job");
        xmlWriter.WriteString(alienToStore.preferenceParams.job.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("goal");
        xmlWriter.WriteString(alienToStore.preferenceParams.relationshipGoal.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    private static Alien LoadAlien(XmlNode alienNode)
    {
        AlienStats selfParams = new AlienStats(
            (BodyType)(BodyType.Parse(typeof(BodyType), alienNode["SelfParams"]["body"].InnerText)),
            (AgeType)(AgeType.Parse(typeof(AgeType), alienNode["SelfParams"]["age"].InnerText)),
            (OccupationType)(OccupationType.Parse(typeof(OccupationType), alienNode["SelfParams"]["job"].InnerText)),
            (GoalsType)(GoalsType.Parse(typeof(GoalsType), alienNode["SelfParams"]["goal"].InnerText))
            );
        AlienStats prefParams = new AlienStats(
            (BodyType)(BodyType.Parse(typeof(BodyType), alienNode["PrefParams"]["body"].InnerText)),
            (AgeType)(AgeType.Parse(typeof(AgeType), alienNode["PrefParams"]["age"].InnerText)),
            (OccupationType)(OccupationType.Parse(typeof(OccupationType), alienNode["PrefParams"]["job"].InnerText)),
            (GoalsType)(GoalsType.Parse(typeof(GoalsType), alienNode["PrefParams"]["goal"].InnerText))
            );


        string messageParse = alienNode["decodeTextMessage"].InnerText;
        messageParse = messageParse.Replace("<color=#e7d112>", "");
        messageParse = messageParse.Replace("<color=#A60EB8>", "`");
        messageParse = messageParse.Replace("</color>","");

        Alien newAlien = new Alien(
            int.Parse(alienNode["imageID"].InnerText),
            double.Parse(alienNode["decoderProg"].InnerText),
            double.Parse(alienNode["baseDecodeSpeed"].InnerText),
            int.Parse(alienNode["decodeTextProg"].InnerText),
            messageParse,
            selfParams,
            prefParams,
            alienNode.Attributes["signalName"].Value
            );

        return newAlien;
    }
    public static List<SaveItem>? LoadSaves()
    {
        if (!DoesDirExist(null)) { return null; }

        List<SaveItem> saveObjs = new List<SaveItem>();
        try
        {
            string[] files = Directory.GetFiles(savesPath);
            string[] saves = files.Where(x => x.Contains(".sav")).ToArray();

            if (saves.Length > 0)
            {
                foreach (string path in saves)
                {
                    try
                    {
                        SaveItem newSave;

                        string xmlFile = File.ReadAllText(path);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xmlFile);

                        XmlNode mainDataNode = doc.SelectSingleNode("Data");
                        newSave.filepath = path;
                        newSave.version = mainDataNode.Attributes["version"].Value;
                        newSave.name = mainDataNode.Attributes["saveName"].Value;
                        newSave.lastTime = mainDataNode.Attributes["time"].Value;
                        saveObjs.Add(newSave);

                    }
                    catch (System.Exception ex) //Exception for when file is invalid or corrupt
                    {
                        Debug.LogError(ex);
                    }
                }
            }
            else
            {
                return null;
            }
               
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.Message);
            return null;
        }

        return saveObjs;
    }
}

public struct SaveItem
{
    public string name;
    public string version;
    public string lastTime;
    public string filepath;
}
