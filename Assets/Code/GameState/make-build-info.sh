#!/bin/sh

REV=$(curl -s --connect-time 3 staff/tools/sodadev.php)
NAME=$(scutil --get ComputerName)

printf "// note: this file is auto generated\n\n" > DebugBuildInfo.cs
printf "public static class DebugBuildInfo\n" >> DebugBuildInfo.cs
printf "{\n" >> DebugBuildInfo.cs
printf "\tpublic static string rev = \"%s%s\";\n" "$REV" "$NAME" >> DebugBuildInfo.cs
printf "}\n" >> DebugBuildInfo.cs
