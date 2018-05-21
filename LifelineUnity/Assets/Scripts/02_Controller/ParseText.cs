using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ParseText
{
    public static void AtScene(GameApp cm, string scene)
    {
        bool if_else = false;
        bool skip_line = false;
        // Debug.Log("AtScene: scenes[scene] = " + scenes[scene].ToString());
        cm.status["atScene"] = null;
        JSONArray sceneItem = cm.scenes[scene].AsArray;
        for (int i = 0; i < sceneItem.Count; i++)
        {
            // 转换成 string, 去掉首尾的 “
            string line = sceneItem[i].ToString().Substring(1, sceneItem[i].ToString().Length - 2);
            // Debug.Log("line = " + line);
            if (if_else)
            {
                if (line.StartsWith("<<else"))
                {
                    skip_line = !skip_line;
                    continue;
                }
                else if (line.Equals("<<endif>>"))
                {
                    if_else = false;
                    continue;
                }
                if (skip_line) continue;
            }
            if (line.StartsWith("<<if") || line.StartsWith("<<elseif"))
            {
                if_else = true;
                string LineWithNoTag = line.Substring(2, line.Length - 4);
                if (LineWithNoTag.Contains(" and "))
                {
                    skip_line = AndOrInLine.and_InLine(cm, LineWithNoTag);
                }
                else if (LineWithNoTag.Contains(" or "))
                {
                    skip_line = AndOrInLine.or_InLine(cm, LineWithNoTag);
                }
                else
                {
                    skip_line = AndOrInLine.and_or_NotInLine(cm, LineWithNoTag);
                }
            }
            else if (line.StartsWith("<<set")) ParseText.HandleSet(cm, line);
            else if (line.StartsWith("[[")) ParseText.ToNewScene(cm, line);
            else if (line.StartsWith("<<category")) ParseText.HandleChoice(cm, line, scene);
            else if (line.StartsWith("<<revert ")) { ParseText.HandleRevert(cm, line); break; }
            else ParseText.AddLeftChats(cm, line, scene);
        }
    }
    static void HandleSet(GameApp cm, string line)
    {
        string[] lines = line.Substring(7, line.Length - 9).Replace(" ", "").Split('=');
        if (lines[1].Contains("-1"))
        {
            int value = cm.status[lines[0]].AsInt - 1;
            cm.status[lines[0]] = value.ToString();
        }
        else cm.status[lines[0]] = lines[1];
    }
    static void ToNewScene(GameApp cm, string line)
    {
        string newLine = line.Substring(2, line.Length - 4);
        if (newLine.StartsWith("delay"))
        {
            string[] newLines = newLine.Split('|');
            cm.status["atScene"] = newLines[1];
        }
        else cm.status["atScene"] = newLine;
    }

    static void HandleChoice(GameApp cm, string line, string scene)
    {
        JSONNode choose = cm.choices[int.Parse(line.Substring(19, line.Length - 21))];
        //JSONArray choice = cm.choices[int.Parse(line.Substring(19, line.Length - 21))]["actions"].AsArray;

        cm.messageManager.AddOneChoice(choose);
    }


    static void AddLeftChats(GameApp cm, string line, string scene)
    {
        if (line.Contains("$pills") || line.Contains("$glowrods") || line.Contains("$power"))
        {
            // 替换$pills、$glowrods 和 $power
            string newLine = line.Replace("$pills", cm.status["pills"]).Replace("$glowrods", cm.status["glowrods"]).Replace("$power", cm.status["power"]);
            cm.messageManager.AddOneNormalMessage(newLine, scene);
        }
        else
            cm.messageManager.AddOneNormalMessage(line, scene);
    }

    public static void HandleRevert(GameApp cm, string line)
    {
        //Debug.Log("revert to:" + line);
        var scene = line.Substring(11, line.Length - 15);
        //Debug.Log("revert to:substring: " + content);

        cm.messageManager.RevertTo(scene);

        cm.SaveStatusData(scene);
    }
}

