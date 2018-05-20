using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TestMatch : MonoBehaviour {
    public const string strPattern = @"^choice\((\d)\)\[\[(.*)\]\], \[\[(.*)\]\]$";
    public string strFormat = "choice({0})[[{1}]], [[{2}]]";
    public int choose;
    public string ss1;
    public string ss2;

    public string output;

    public string outputChoose;
    public string outputSS1;
    public string outputSS2;

    [ContextMenu("Test")]
    void Test()
    {
        output = string.Format(strFormat, choose.ToString(), ss1, ss2);

        Regex r = new Regex(strPattern);
        var m = r.Match(output);
        if(m.Success)
        {
            var g = m.Groups;
            foreach(var c in g)
            {
                Debug.Log(c);
            }
            Debug.Log("success");
        }else
        {
            Debug.Log("fail");
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
