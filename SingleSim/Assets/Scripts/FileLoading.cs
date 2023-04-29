using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
public static class FileLoading
{
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
                        newItem.selfUnemployedReplacement = replacementNode.InnerText;
                        break;
                    default:
                        Debug.Log("invalid attribute on node " + replacementNode.Attributes["type"].Value.ToString());
                        break;
                }
            }

            messages.Add(newItem);
        }

        Debug.Log(messages[0].selfUnemployedReplacement);
        return messages;
    }
}
