namespace LOTRO_Companion_Combiner_Lib
{
    public class LotroCompanionHelpers
    {
        public static List<string> GetLotroCompanionLanguages(string lotroDataPath)
        {
            var results = new List<string>();

            var labelsPath = Path.Combine(lotroDataPath, "labels");
            var labelsDi = new DirectoryInfo(labelsPath);


            foreach (var directory in labelsDi.EnumerateDirectories())
            {
                results.Add(directory.Name);
            }

            return results;
        }
    }
}
