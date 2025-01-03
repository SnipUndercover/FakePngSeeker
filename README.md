# [FakePngSeeker](https://github.com/SnipUndercover/FakePngSeeker)

Looks for invalid `PNG` files, which commonly cause crashes and warnings in Celestecord's Banana Watch thread.

## Usage

- Get the release from the repository page.
- Pick the folder which matches your operating system and run the executable.
- Enter the absolute path to your mods folder and press <kbd>Enter</kbd>.

The program will search all of your mod folders and .zips in search for invalid `PNG`s in the `Graphics/Atlases` folder.

> [!NOTE]
> If the mod folder does not have an `everest.yaml`, it will be skipped.

## Building

Simply clone the repository and **build** the solution.

To distribute the executable you need to **publish** the solution with a runtime identifier and configuration of your choosing.  
This will publish the project as a self-contained executable, which avoids the need to install the .NET 9 runtime.

> [!TIP]
> Rider users should have Windows (x64), Linux (x64) and OSX (ARM64) run configurations ready to use.
