// See https://aka.ms/new-console-template for more information
Console.WriteLine("LOTRO Companion to Lua:");

if (args.Length == 0)
{
    Console.WriteLine("Pass the lotro-data directory as the first argument.");
    return 1;
}

var path = args[0];
var langVariable = "LANGUAGE";
if (args.Length > 1)
{
    langVariable = args[1];
}

var lib = new LOTRO_Companion_to_Lua_Lib.ExtractTravelSkills(path, langVariable);
return 0;
