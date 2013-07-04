using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_LOGGER
using UnityLogging;
#endif
//using LitJson;

#region Unity => JavaScript Support
static class Unity2JS
{
    /* Unity => JavaScript Support Methods
     * Hook for calling JavaScript functions embedded in HTML pages
     *   from Unity. The first argument must be the name of the JavaScript
     *   function to call (in the surrounding HTML page). After that, there
     *   can be an arbitrary number of arguments provided for that function
     *   (however many it requires). Parameter types don't matter.
     * Examples:
     *   Unity2JS.Call("JSfunction1");                  // JS: function JSfunction1() { }
     *   Unity2JS.Call("JSfunction2", "Hi!");           // JS: function JSfunction2(arg) { }
     *   Unity2JS.Call("JSfunction3", "Hi", "There!");  // JS: function JSfunction3(arg, arg) { }
     */
    static public void Call(string method, params object[] args)
    {
        if (Rlplog.TrcFlag)
        {
            string str = "(";
            foreach (object o in args)
                str += o.ToString() + ", ";
            str += ")";
            str = str.Replace(", )", ")");

            Rlplog.Trace("Unity2JS.Call",
                String.Format("Method = {0}, args = {1}", method, str));
        }
		
       	Application.ExternalCall(method, args);
    }
}
#endregion

#region DEBUGGING
/* Wrapper for logging functions.
 *   debug(): for temporary debugging statements; toggled by "dbg" boolean (default=false).
 *   trace(): for permanent tracing  statements; toggled by "trc" boolean (default=false).
 *   error(): for error statements; always on.
 */
/* USAGE POLICY
 * 
 *   Debug: used for transitory debugging messages. These should be removed before deploying to live site.
 *   Trace: used for tracing the flow of control. These should remain, but TrcFlag should be set to false before deploying.
 *   Error: used to report errors. These should always be left in.
 */
static public class Rlplog
{           
    static private bool displayMethod = true;   // If true, include the method name.
    static private string logMsg(String prefix, String method, String s)
    {
        // Optionally include the method name.
        String middle = displayMethod ? String.Format(" [{0,-32}]", method) : "";
        return (prefix + middle + ": " + s);
    }

    static public bool DbgFlag { get; set; }    // If true, print debug messages and the "debug" banner.
    static public bool TrcFlag { get; set; }    // If true, print trace messages.
	static public bool ErrFlag { get; set; }	//	If true, print error messages through Unity's error log which will stop Xcode. If false, don't do that shit.
	static public bool ShowDebugFlag {get; set; }	//	if true, show debug messges in the game
	
	static private GameObject		textMeshForDebugGO = null;
	const int msgQueueSize=6;
	static private string[]			msgQueue = new string[msgQueueSize];
	
	static private string				m_methodFilter = null;
	
    #if UNITY_LOGGER
    static public void Debug(String method, String message)
    {
        if (DbgFlag)
        {
            string s = logMsg("DBG", method, message);
            Ulog.Instance.WriteLine(s);
        }
    }
    static public void Trace(String method, String message)
    {
        if (TrcFlag)
        {
            string s = logMsg("TRC", method, message);
            Ulog.Instance.WriteLine(s);
        }
    }
    static public void Error(String method, String message)
    {
        string s = logMsg("ERR", method, message);
        Ulog.Instance.WriteLine(s);
        UnityEngine.Debug.LogError(s); // include this so that the error message is clickable in log file.
    }
    #else // UNITY_LOGGER
	static public void SetFilter(string f)
	{
		m_methodFilter = f;
	}
	
    static public string Debug(String method, String message)
    {
		string outString = "";
		
        if (DbgFlag)
        {
			if ((m_methodFilter == null) || (m_methodFilter == "") || (method.Contains(m_methodFilter))) {
            	string s = logMsg("DBG", method, message);
	            UnityEngine.Debug.LogWarning(s);
				outString = s;
			}
        }
		return outString;
    }

    static public string Trace(String method, String message)
    {
		string outString = "";
        if (TrcFlag)
        {
			if ((m_methodFilter == null) || (m_methodFilter == "") || (method.Contains(m_methodFilter))) {
	            string s = logMsg("TRC", method, message);
    	        UnityEngine.Debug.Log(s);
			
				if (textMeshForDebugGO == null) {
					textMeshForDebugGO = GameObject.Find("Debug Message");
				}
				else {
					TextMesh tm = textMeshForDebugGO.GetComponent<TextMesh>();
					if (tm != null) {
						for(int ii=0; ii<msgQueueSize-1;ii++) {
							msgQueue[ii] = msgQueue[ii+1];
						}
						msgQueue[msgQueueSize-1] = message;
						string finalMsg = msgQueue[0];
						for(int ii=1; ii<msgQueueSize; ii++) {
							finalMsg += "\n" + msgQueue[ii];
						}
						tm.text = finalMsg;
					}
				}
				outString = s;
			}
        }
		return outString;
    }
	
	static public void ShowDebug(bool bDebugOn)
	{
		ShowDebugFlag = bDebugOn;
		textMeshForDebugGO.active = bDebugOn;
	}
    static public string Error(String method, String message)
    {
        string s = logMsg("ERR", method, message);
		if (ErrFlag) {
        	UnityEngine.Debug.LogError(s);
		}
		else {
			UnityEngine.Debug.Log(s);	
		}
		return s;
	}
    #endif // UNITY_LOGGER
}
#endregion
