﻿using SaveFileManager;

internal class Program
{
    private static void Main(string[] args)
    {
        if (!args.Any() || string.IsNullOrWhiteSpace(args[0]))
        {
            Utils.PressKey("Open a \".savc\"(or similarly encoded) file with this exe.");
        }
        // manual mode
        else if (args.Length == 1)
        {
            var filePath = args[0];
            var fileExt = Path.GetExtension(filePath);
            string filePathStripped;
            if (fileExt != "")
            {
                fileExt = fileExt[1..];
                filePathStripped = filePath[..(filePath.Length - fileExt.Length - 1)];
            }
            else
            {
                filePathStripped = filePath;
            }
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
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (Utils.Input($"\n\nDo you want to save the results as \"{fileName}.decoded.json\"?(Y/N): ")?.ToUpper() == "Y")
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
        // auto mode
        else
        {
            if (!File.Exists(args[0]))
            {
                return;
            }

            var filePath = args[0];
            var fileExt = Path.GetExtension(filePath);
            string filePathStripped;
            if (fileExt != "")
            {
                fileExt = fileExt[1..];
                filePathStripped = filePath[..(filePath.Length - fileExt.Length - 1)];
            }
            else
            {
                filePathStripped = filePath;
            }

            if (!long.TryParse(args[1], out long fileSeed))
            {
                throw new ArgumentException("File seed argument is not a long value");
            }

            var fileData = FileConversion.DecodeFile(fileSeed, filePathStripped, fileExt);

            if (
                args.Length >= 3 &&
                bool.TryParse(args[2], out bool saveToFile) &&
                saveToFile
            )
            {
                using var f = new StreamWriter($"{filePathStripped}.decoded.json");
                foreach (var line in fileData)
                {
                    f.WriteLine(line);
                }
            }
            else
            {
                foreach (var line in fileData)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
}