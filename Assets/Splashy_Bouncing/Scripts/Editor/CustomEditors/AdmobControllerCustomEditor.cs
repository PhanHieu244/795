using UnityEditor;
using System.Collections.Generic;

namespace OnefallGames
{
    [CustomEditor(typeof(AdmobController))]
    public class AdmobControllerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!ScriptingSymbolsHandler.NamespaceExists(NamespaceData.GoogleMobileAdsNameSpace))
            {
                EditorGUILayout.HelpBox("To use Admob Ads, please import Google Mobile Ads plugin !!!", MessageType.Warning);
            }
            else
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                List<string> currentSymbols = new List<string>(symbolStr.Split(';'));
                if (!currentSymbols.Contains(ScriptingSymbolsData.ADMOB))
                {
                    List<string> sbs = new List<string>();
                    sbs.Add(ScriptingSymbolsData.ADMOB);
                    ScriptingSymbolsHandler.AddDefined_ScriptingSymbol(sbs.ToArray(), EditorUserBuildSettings.selectedBuildTargetGroup);
                }
            }
            base.OnInspectorGUI();
        }
    }
}

