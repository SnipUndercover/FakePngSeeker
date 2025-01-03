# FakePngSeeker

Looks for invalid `PNG` files, which commonly cause crashes and warnings in Celestecord's Banana Watch thread.

## Usage

Just run the executable. Running it from a CLI or double-clicking on it should work.

## Building

Simply clone the repository and `dotnet build`.

To distribute the executable you need to `dotnet publish` with a platform and configuration of your choosing.  
This will publish the project as a self-contained executable, which avoids the need to install the .NET 9 runtime.
