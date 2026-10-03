#!/bin/bash
U=/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app/Contents/Resources/Scripting
P="/Users/siddharthgupta/Kadak Chai"
O="$1"
REFS=("-r:$U/NetStandard/ref/2.1.0/netstandard.dll")
for f in "$U"/Managed/UnityEngine/UnityEngine*.dll; do REFS+=("-r:$f"); done; REFS+=("-r:/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app/Contents/Resources/Scripting/Managed/UnityEngine/Unity.Scripting.dll")
REFS+=("-r:$P/Library/ScriptAssemblies/UnityEngine.UI.dll" "-r:$P/Library/ScriptAssemblies/Unity.InputSystem.dll")
CSC=("$U/DotNetSdk/dotnet" "$U/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll" -nologo -noconfig -nostdlib -langversion:9 -target:library -nowarn:1701,1702)
"${CSC[@]}" -out:"$O/Game.dll" "${REFS[@]}" -define:UNITY_5_3_OR_NEWER "$P"/Assets/_Game/Scripts/Core/*.cs "$P"/Assets/_Game/Scripts/Battle/*.cs "$P"/Assets/_Game/Scripts/View/*.cs
echo "--- runtime exit $?"
EREFS=("${REFS[@]}"); for f in "$U"/Managed/UnityEngine/UnityEditor*.dll; do EREFS+=("-r:$f"); done
"${CSC[@]}" -out:"$O/GameEditor.dll" "${EREFS[@]}" "-r:$O/Game.dll" "$P"/Assets/_Game/Scripts/Editor/*.cs
echo "--- editor exit $?"
