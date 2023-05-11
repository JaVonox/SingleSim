using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
public class FileLoading
{
    private static string savesPath = System.IO.Directory.GetCurrentDirectory().ToString() + "/Saves/";
    public static XmlWriterSettings settings = new XmlWriterSettings();

    private const string gameVersion = "Prerelease";

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
            xmlWriter.WriteAttributeString("version", gameVersion);

            WriteGameplayVars(ref xmlWriter);
            WriteShopLevels(ref xmlWriter);
            WriteAliens(ref xmlWriter);

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
    }

    private static void WriteGameplayVars(ref XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("Gameplay");

        xmlWriter.WriteStartElement("scanProg");
        xmlWriter.WriteString(Gameplay.scanProg.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("scanSpotsAreAvailable");
        xmlWriter.WriteString(Gameplay.scanSpotsAreAvailable.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("bounds");
        xmlWriter.WriteAttributeString("x", Gameplay.bounds.xBound.ToString());
        xmlWriter.WriteAttributeString("y", Gameplay.bounds.yBound.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("isBoundsSet");
        xmlWriter.WriteString(Gameplay.isBoundsSet.ToString());
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
}
