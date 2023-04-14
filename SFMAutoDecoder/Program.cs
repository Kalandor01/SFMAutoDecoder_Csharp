using SaveFileManager;

internal class Program
{
    private static void Main(string[] args)
    {
        if (!args.Any() || string.IsNullOrWhiteSpace(args[0]))
        {
            Utils.PressKey("Open a \".savc\"(or similarly encoded) file with this exe.");
        }
        else
        {
            var filePath = args[0];
            var fileExt = Path.GetExtension(filePath)[1..];
            var filePathStripped = filePath[..(filePath.Length - fileExt.Length - 1)];
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileSeed = Utils.ReadInt("What is the seed?: ");

            List<string> fileData;
            try
            {
                fileData = FileConversion.DecodeFile(fileSeed, filePathStripped, fileExt);
            }
            catch (FormatException)
            {
                Utils.PressKey("Couldn't decode file. Probably wrong seed.");
                return;
            }
            catch (FileNotFoundException)
            {
                Utils.PressKey("File not found: " + filePath);
                return;
            }

            foreach (var line in fileData)
            {
                Console.WriteLine(line);
            }
            if (Utils.Input($"\n\nDo you want to save the results as \"{fileName}.decoded.json\"?(Y/N): ").ToUpper() == "Y")
            {
                using (var f = new StreamWriter($"{filePathStripped}.decoded.json"))
                {
                    foreach (var line in fileData)
                    {
                        f.WriteLine(line);
                    }
                }
                Utils.PressKey("SAVED!");
            }
        }
    }
}