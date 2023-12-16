using LOTRO_Companion_Combiner_Lib;

Console.WriteLine("LOTRO Companion Combiner:");

if (args.Length == 0)
{
    Console.WriteLine("Pass the lotro-data directory as the first argument.");
    return 1;
}

var path = args[0];
var lib = new LotroCompanionCombiner(path);
return 0;
