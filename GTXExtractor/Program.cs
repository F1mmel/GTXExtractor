using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;

Console.WriteLine("Welcome to GTXExtractor for The Legend of Zelda: Twilight Princess Fan Edition");
Console.WriteLine();
Console.WriteLine(
    "This tool allows you to extract all textures from the HD version of the game, optimizing performance by preloading textures instead of loading them during gameplay. This step can significantly improve your gaming experience by reducing load times and enhancing overall performance.");
Console.WriteLine();
Console.WriteLine("Usage Instructions:");
Console.WriteLine("1. Ensure you have sufficient disk space for the extracted textures (> 5GB).");
Console.WriteLine(
    "2. Make sure to place the HD version files in the folder named 'HD_Files' located in the same directory as this tool.");
Console.WriteLine("3. Click 'Start' to begin the extraction process.");
Console.WriteLine();
Console.WriteLine(
    "Please note that this process may take some time depending on your system's speed and the size of the game files.");
Console.WriteLine();
Console.WriteLine("Press Enter to continue...");

// Wait for the Enter key to be pressed
/*while (Console.ReadKey(true).Key != ConsoleKey.Enter)
{
}*/

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

// Paths
string gameAssets = Path.Combine(Directory.GetCurrentDirectory(), "HD_Files");

if (!Directory.Exists(gameAssets))
{
    Console.WriteLine("Error: The folder 'HD_Files' was not found.");
    Console.WriteLine(
        "Please make sure to place the HD version files in the 'HD_Files' folder located in the following path:");
    Console.WriteLine(gameAssets);
    Console.WriteLine("The program will now exit.");

    Environment.Exit(1);
}

// Wenn Stage, dann muss auch StageName F103 dazu, ansonsten nicht identifizierbar

// Invoke extracting

//string tempFolder = GetTemporaryDirectory();
//Console.WriteLine(tempFolder);

// Extract all textures from HD version
/*ExtractGTX(gameAssets, tempFolder);

// Textures to bin file
CreateBinFile(tempFolder);

// Delete temp folder when creating bin is finished
Directory.Delete(tempFolder);*/

// Unterschied zwischen Layout und LayoutRevo???? Ist Revo nur WiiU?
List<string> neededDirectories = new List<string>()
{
    //"Stage", "Object", "Layout", "LayoutRevo", "Fonteu"
    "Stage"
};

foreach (string dir in neededDirectories)
{    
    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ZeldaFiles") + "/" + dir + ".bin"))
    {
        continue;
    }
    
    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "HD_Files/res/" + dir);
            
    if (dir == "Stage")
    {
        // Für Stage: Alle Unterordner separat verarbeiten
        ProcessStageDirectories(rootPath);
    }
    else
    {
        // Für andere Verzeichnisse: Einfache Verarbeitung
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ZeldaFiles") + "/" + dir + ".bin"))
        {
            List<TextureData> textures = new List<TextureData>();
            ProcessDirectory(rootPath, textures);
            CreateBinFile(dir, textures);
        }
    }
}

static void ProcessStageDirectories(string rootPath)
{
    // Dictionary für Texturen pro Unterordner
    Dictionary<string, List<TextureData>> texturesByFolder = new Dictionary<string, List<TextureData>>();

    // Verarbeite alle Unterverzeichnisse
    foreach (string subdirectory in Directory.GetDirectories(rootPath))
    {
        List<TextureData> textures = new List<TextureData>();
        ProcessDirectory(subdirectory, textures);
        //texturesByFolder[Path.GetFileName(subdirectory)] = textures;
        
        Console.WriteLine("ADDING: " + Path.GetFileName(subdirectory) + " :: " + textures.Count);

        // Erstelle für jeden Unterordner eine separate .bin-Datei
        CreateBinFile("Stages/" + Path.GetFileName(subdirectory), textures);
        foreach (var kvp in textures)
        {
            //Console.WriteLine("GATERH22222: " + kvp.);
        }
    }
}

/*foreach (string dir in neededDirectories)
{
    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ZeldaFiles") + "/" + dir + ".bin"))
    {
        continue;
    }

    List<TextureData> textures = new List<TextureData>();

    foreach (string file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "HD_Files/res/" + dir)))
    {
        if (file.EndsWith("pack.gz", StringComparison.OrdinalIgnoreCase))
        {
            // Den Pfad der Datei in der Konsole ausgeben
            Console.WriteLine(file);

            using (FileStream originalFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                decompressionStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                string fileName = Path.GetFileNameWithoutExtension(file);
                byte[] decompressedData = memoryStream.ToArray();

                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    var tmpk = new Tmpk(decompressedData);
                    var files = tmpk.GetFiles();

                    foreach (var kvp in files)
                    {
                        // Texture
                        if (kvp.Key.EndsWith("gtx"))
                        {
                            GTXFile f = new GTXFile();

                            using (MemoryStream gtxStream = new MemoryStream(kvp.Value.Data))
                            {
                                f.Load(gtxStream.ToArray());

                                string gtxName = Path.GetFileName(kvp.Key);
                                Console.WriteLine(">> Extracting " + gtxName + "...");

                                if(dir.Equals("Object") && gtxName.StartsWith("demo")) continue;

                                foreach (var t in f.textures)
                                {

                                    byte[] bitmapData = BitmapToByteArray(t.GetBitmapWithChannel());

                                    textures.Add(new TextureData
                                    {
                                        Name = $"{fileName}_{gtxName}_{f.textures.IndexOf(t)}.png",
                                        Data = bitmapData
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    CreateBinFile(dir, textures);
}*/

static void ProcessDirectory(string targetDirectory, List<TextureData> textures)
{
    // Process the list of files found in the directory.
    string[] fileEntries = Directory.GetFiles(targetDirectory);
    foreach (string file in fileEntries)
    {
        ProcessFile(Path.GetFileName(Path.GetDirectoryName(targetDirectory)), file, textures);
    }

    // Recurse into subdirectories of this directory.
    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
    foreach (string subdirectory in subdirectoryEntries)
    {
        ProcessDirectory(subdirectory, textures);
    }
}

static void ProcessFile(string root, string file, List<TextureData> textures)
{
    if (file.EndsWith("pack.gz", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine(file);

        using (FileStream originalFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
        using (MemoryStream memoryStream = new MemoryStream())
        {
            decompressionStream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            string fileName = Path.GetFileNameWithoutExtension(file);
            byte[] decompressedData = memoryStream.ToArray();

            var tmpk = new Tmpk(decompressedData);
            var files = tmpk.GetFiles();

            foreach (var kvp in files)
            {
                // Texture
                if (kvp.Key.EndsWith("gtx"))
                {
                    GTXFile f = new GTXFile();

                    using (MemoryStream gtxStream = new MemoryStream(kvp.Value.Data))
                    {
                        f.Load(gtxStream.ToArray());

                        string gtxName = Path.GetFileName(kvp.Key);
                       // Console.WriteLine(">> Extracting " + gtxName + "...");

                        if (root.Equals("Object") && gtxName.StartsWith("demo")) continue;

                        foreach (var t in f.textures)
                        {
                            byte[] bitmapData = BitmapToByteArray(t.GetBitmapWithChannel());

                            string texName = "";
                            if (root.Equals("Stage"))
                            {
                                string stageName = Path.GetFileName(Directory.GetParent(file).Name).Replace(".pack", "");
                                string currentStagePack = fileName.Replace(".pack", "");
                                texName = $"{stageName}_{currentStagePack}_{gtxName.Replace(".gtx", "")}_{f.textures.IndexOf(t)}";
                            }
                            else
                            {
                                texName = $"{fileName.Replace(".pack", "")}_{gtxName.Replace(".gtx", "")}_{f.textures.IndexOf(t)}";
                            }

                            textures.Add(new TextureData
                            {
                                Name = texName,
                                Data = bitmapData
                            });
                        }
                    }
                }
            }
        }
    }
}

//Directory.Delete(tempFolder);

static void ExtractGTX(string path, string output)
{
    // Alle Dateien im aktuellen Verzeichnis verarbeiten
    foreach (string file in Directory.GetFiles(path))
    {
        if (file.EndsWith("pack.gz", StringComparison.OrdinalIgnoreCase))
        {
            // Den Pfad der Datei in der Konsole ausgeben
            Console.WriteLine(file);

            using (FileStream originalFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                decompressionStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                string fileName = Path.GetFileNameWithoutExtension(file);
                byte[] decompressedData = memoryStream.ToArray();

                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    var tmpk = new Tmpk(decompressedData);
                    var files = tmpk.GetFiles();

                    foreach (var kvp in files)
                    {
                        // Texture
                        if (kvp.Key.EndsWith("gtx"))
                        {
                            GTXFile f = new GTXFile();

                            using (MemoryStream gtxStream = new MemoryStream(kvp.Value.Data))
                            {
                                f.Load(gtxStream.ToArray());

                                string gtxName = Path.GetFileName(kvp.Key);
                                Console.WriteLine(">> Extracting " + gtxName + "...");

                                foreach (var t in f.textures)
                                {
                                    string outputPath = Path.Combine(output,
                                        fileName + "_" + gtxName + "_" + f.textures.IndexOf(t) + ".png");
                                    t.SaveBitmap(outputPath);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Alle Unterverzeichnisse durchlaufen
    foreach (string dir in Directory.GetDirectories(path))
    {
        ExtractGTX(dir, output);
    }
}

static string GetTargetDirectoryName(string filePath)
{
    // Normalisiere den Pfad, um sowohl '/' als auch '\' zu unterstützen
    string normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

    // Hole das Verzeichnis des Pfads
    string directoryPath = Path.GetDirectoryName(normalizedPath);

    // Extrahiere das übergeordnete Verzeichnis
    string parentDirectory = Directory.GetParent(directoryPath).Name;

    return parentDirectory;
}

static byte[] BitmapToByteArray(Bitmap bitmap)
{
    using (MemoryStream stream = new MemoryStream())
    {
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}

static void CreateBinFile(string name, List<TextureData> textures)
{
    string dir = Path.Combine(Directory.GetCurrentDirectory(), "ZeldaFiles");
    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    string stages = Path.Combine(Directory.GetCurrentDirectory(), "ZeldaFiles/Stages");
    if (!Directory.Exists(stages)) Directory.CreateDirectory(stages);

    string binFilePath = Path.Combine(dir, name + ".bin");

    using (BinaryWriter writer = new BinaryWriter(File.Open(binFilePath, FileMode.Create)))
    {
        writer.Write(textures.Count);

        foreach (var texture in textures)
        {
            writer.Write(texture.Name);
            writer.Write(texture.Data.Length);
            writer.Write(texture.Data);
        }
    }
}

static string GetTemporaryDirectory()
{
    string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    if (Directory.Exists(tempDirectory))
    {
        return GetTemporaryDirectory();
    }
    else
    {
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
}


public class TextureData
{
    public string Name { get; set; }
    public byte[] Data { get; set; }
}