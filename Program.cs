using System.IO.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace FakePngSeeker;

internal static class Program
{
    private static readonly Lock ConsoleLock = new();

    public static void Main()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Enter the path to your Celeste \"Mods\" folder.");

        string modsPath;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("> ");
            Console.ResetColor();

            string? tmp = Console.ReadLine();

            if (tmp is null)
                return;

            if (string.IsNullOrWhiteSpace(tmp))
                continue;

            tmp = Path.TrimEndingDirectorySeparator(tmp);

            if (!Path.IsPathRooted(tmp))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("The path must be absolute.");
                continue;
            }

            if (!Directory.Exists(tmp))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"The folder \"{tmp}\" does not exist.");
                continue;
            }

            if (Path.GetFileName(tmp) != "Mods")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"The folder \"{tmp}\" does not point to the \"Mods\" folder.");
                continue;
            }

            modsPath = tmp;
            break;
        }

        List<string> directories = [];
        List<string> files = [];

        directories.AddRange(Directory.EnumerateDirectories(modsPath, "*", SearchOption.TopDirectoryOnly));
        directories.Remove(Path.Combine(modsPath, "Cache"));

        files.AddRange(Directory.EnumerateFiles(modsPath, "*.zip", SearchOption.TopDirectoryOnly));

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Searching for invalid PNGs...");
        Console.ResetColor();

        Console.WriteLine();

        Task.WaitAll(
            Parallel.ForEachAsync(directories, SearchDirectory),
            Parallel.ForEachAsync(files, SearchFile)
        );

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Search complete. See the above log for details.");
        Console.WriteLine("Press Enter to exit.");
        Console.ResetColor();

        Console.ReadLine();
    }

    private static async ValueTask SearchDirectory(string path, CancellationToken cancellationToken)
    {
        string directoryName = Path.GetFileName(path);

        if (!File.Exists(Path.Combine(path, "everest.yaml")))
        {
            ConsoleLock.Enter();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[{directoryName}] No everest.yaml, skipping.");
            Console.ResetColor();
            ConsoleLock.Exit();
            return;
        }

        string atlasesPath = Path.Combine(path, "Graphics", "Atlases");
        if (!Directory.Exists(atlasesPath))
            return;

        foreach (string imagePath in Directory.EnumerateFiles(atlasesPath, "*.png", SearchOption.AllDirectories))
        {
            string imageName = imagePath[(atlasesPath.Length + 1)..].Replace('\\', '/');
            try
            {
                using Image image = await Image.LoadAsync(imagePath, cancellationToken);
                IImageFormat imageFormat = image.Metadata.DecodedImageFormat!;
                if (imageFormat == PngFormat.Instance)
                    continue;

                ConsoleLock.Enter();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    $"[{directoryName}] Image \"{imageName}\" is not in the PNG format. (actual: {imageFormat.Name})");
                Console.ResetColor();
                ConsoleLock.Exit();
            }
            catch (UnknownImageFormatException)
            {
                ConsoleLock.Enter();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    $"[{directoryName}] Image \"{imageName}\" is invalid or corrupted.");
                Console.ResetColor();
                ConsoleLock.Exit();
            }
            catch (Exception e)
            {
                ConsoleLock.Enter();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    $"[{directoryName}] Image \"{imageName}\" could not be read. ({e.GetType().FullName}: {e.Message})");
                Console.ResetColor();
                ConsoleLock.Exit();
            }
        }
    }

    private static async ValueTask SearchFile(string path, CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(path);
        try
        {
            using ZipArchive zip = ZipFile.OpenRead(path);

            if (zip.GetEntry("everest.yaml") is null)
            {
                ConsoleLock.Enter();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{fileName}] No everest.yaml, skipping.");
                Console.ResetColor();
                ConsoleLock.Exit();
                return;
            }

            if (zip.GetEntry("Graphics/Atlases/") is null)
                return;

            foreach (ZipArchiveEntry zipEntry in zip.Entries)
            {
                string entryName = zipEntry.FullName;
                if (!(entryName.StartsWith("Graphics/Atlases/") && entryName.EndsWith(".png")))
                    continue;

                string imageName = entryName["Graphics/Atlases/".Length..];

                await using Stream entryStream = zipEntry.Open();
                try
                {
                    using Image image = await Image.LoadAsync(entryStream, cancellationToken);
                    IImageFormat imageFormat = image.Metadata.DecodedImageFormat!;
                    if (imageFormat == PngFormat.Instance)
                        continue;

                    ConsoleLock.Enter();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(
                        $"[{fileName}] Image \"{imageName}\" is not in the PNG format. (actual: {imageFormat.Name})");
                    Console.ResetColor();
                    ConsoleLock.Exit();
                }
                catch (UnknownImageFormatException)
                {
                    ConsoleLock.Enter();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(
                        $"[{fileName}] Image \"{imageName}\" is invalid or corrupted.");
                    Console.ResetColor();
                    ConsoleLock.Exit();
                }
                catch (Exception e)
                {
                    ConsoleLock.Enter();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(
                        $"[{fileName}] Image \"{imageName}\" could not be read. ({e.GetType().FullName}: {e.Message})");
                    Console.ResetColor();
                    ConsoleLock.Exit();
                }
            }
        }
        catch (Exception e)
        {
            ConsoleLock.Enter();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[{fileName}] The .zip file could not be read. ({e.GetType().FullName}: {e.Message})");
            Console.ResetColor();
            ConsoleLock.Exit();
        }
    }
}
