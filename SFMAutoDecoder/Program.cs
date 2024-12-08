using FileManager;
using NPrng;
using NPrng.Generators;
using System.Numerics;
using System.Reflection;
using System.Text;

internal class Program
{
    private const string DECODED_EXT = ".savc.decoded";

    private static long ReadLong(string text, string errorText = "Not a number!")
    {
        long result;
        while (true)
        {
            Console.Write(text);
            if (long.TryParse(Console.ReadLine(), out result))
            {
                break;
            }

            Console.WriteLine(errorText);
        }

        return result;
    }

    private static string Input(string message)
    {
        Console.Write(message);
        var response = Console.ReadLine();
        Console.WriteLine();
        return response;
    }

    private static (int version, bool isZipped, BigInteger internalSeed) GetFileMetadata(long seed, string filePathNoExtension, string fileExt)
    {
        var makeSeed = typeof(FileConversion).GetMethod("MakeSeed", BindingFlags.Static | BindingFlags.NonPublic);
        var makeRandom = typeof(FileConversion).GetMethod("MakeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        var decodeLine = typeof(FileConversion).GetMethod("DecodeLine", BindingFlags.Static | BindingFlags.NonPublic);

        BigInteger MakeSeed(long seed)
        {
            return (BigInteger)makeSeed.Invoke(null, [seed, Type.Missing, Type.Missing]);
        }

        SplittableRandom MakeRandom(BigInteger bigIntSeed)
        {
            return (SplittableRandom)makeRandom.Invoke(null, [bigIntSeed]);
        }

        string DecodeLine(
            IEnumerable<byte> bytes,
            AbstractPseudoRandomGenerator rand,
            Encoding encoding,
            bool unzip = false
        )
        {
            return (string)decodeLine.Invoke(null, [bytes, rand, encoding, unzip]);
        }


        var encoding = Encoding.UTF8;
        var fileBytes = File.ReadAllBytes($"{filePathNoExtension.Replace("*", seed.ToString())}.{fileExt}");
        var byteLines = new List<List<byte>>();
        var newL = new List<byte>();
        foreach (var by in fileBytes)
        {
            newL.Add(by);
            if (by == 10)
            {
                byteLines.Add(newL);
                newL = [];
            }
        }
        
        var r = MakeRandom(MakeSeed(seed));
        var version = int.Parse(DecodeLine(byteLines.ElementAt(0), r, encoding));
        var seedNum = BigInteger.Parse(DecodeLine(byteLines.ElementAt(1), r, encoding));
        var isZipped = false;
        if (byteLines.Count > 2)
        {
            try
            {
                isZipped = int.Parse(DecodeLine(byteLines.ElementAt(2), r, encoding)) == 1;
            }
            catch { }
        }
        return (version, isZipped, seedNum);
    }

    private static void Main(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            Utils.PressKey("Open a \".savc\"(or similarly encoded) file with this exe.");
            return;
        }

        var filePath = args[0];
        if (!File.Exists(args[0]))
        {
            throw new ArgumentException($"The file: \"{filePath}\" doesn't exist!");
        }

        var fileExt = Path.GetExtension(filePath);
        fileExt = fileExt.Length != 0 ? fileExt[1..] : "";
        var justFileName = Path.GetFileNameWithoutExtension(filePath);
        var filePathNoExtension = Path.Join(Path.GetDirectoryName(filePath), justFileName);

        // manual mode
        if (args.Length == 1)
        {
            var fileSeed = ReadLong("What is the seed?: ");

            List<string> fileData;
            try
            {
                fileData = FileConversion.DecodeFile(fileSeed, filePathNoExtension, fileExt);
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

            Console.WriteLine($"\nMetadata:");
            try
            {
                var (version, isZipped, internalSeed) = GetFileMetadata(fileSeed, filePathNoExtension, fileExt);
                Console.WriteLine($"\tVersion: {version}\n\tIs zipped: {isZipped}\n\tInternal version: {internalSeed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[COULD NOT READ METADATA]");
            }
            Console.WriteLine("---------------------------\n");

            foreach (var line in fileData)
            {
                Console.WriteLine(line);
            }
            if (Input($"\n\nDo you want to save the results as \"{justFileName}{DECODED_EXT}\"?(Y/N): ")?.ToUpper() == "Y")
            {
                using (var fileWriter = new StreamWriter($"{filePathNoExtension}{DECODED_EXT}"))
                {
                    foreach (var line in fileData)
                    {
                        fileWriter.WriteLine(line);
                    }
                }
                Utils.PressKey("SAVED!");
            }
            return;
        }
        // auto mode
        else
        {
            if (!long.TryParse(args[1], out var fileSeed))
            {
                throw new ArgumentException("File seed argument is not a long value");
            }

            var fileData = FileConversion.DecodeFile(fileSeed, filePathNoExtension, fileExt);

            if (
                args.Length >= 3 &&
                bool.TryParse(args[2], out var saveToFile) &&
                saveToFile
            )
            {
                using var fileWriter = new StreamWriter($"{filePathNoExtension}{DECODED_EXT}");
                foreach (var line in fileData)
                {
                    fileWriter.WriteLine(line);
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