using CliWrap;
using CliWrap.Buffered;
using System.CommandLine;

// Define root command
RootCommand command = new RootCommand(description: "Strip unwanted glyphs from a font file based on the usage of the font in a ASS subtitle file.");

#pragma warning disable CS8603
// Define --ass-file option with validators
var assFileOption = new Option<FileInfo>(
  name: "--ass-file",
  description: "The ASS script file to analyze.",
  parseArgument: result => {
    string? filePath = result.Tokens.Single().Value;
    if (!File.Exists(filePath)) {
      result.ErrorMessage = $"File '{filePath}' does not exist.";
      return null;
    }
    FileInfo assFile = new(filePath);
    if (!assFile.IsValidAssFile() ) {
      result.ErrorMessage = $"File '{assFile}' is not a valid ASS script file.";
      return null;
    }
    return new FileInfo(filePath);
  }) { IsRequired = true };

// Define --font-file option with validators
var fontFileOption = new Option<FileInfo>(
  name: "--font-file",
  description: "The font used in the ASS script we want to strip.",
  parseArgument: result => {
    string? filePath = result.Tokens.Single().Value;
    if (!File.Exists(filePath)) {
      result.ErrorMessage = $"File '{filePath}' does not exist.";
      return null;
    }
    FileInfo fontFile = new(filePath);
    if (!fontFile.IsValidFontFile() ) {
      result.ErrorMessage = $"File '{fontFile}' is not a valid font file.";
      return null;
    }
    return new FileInfo(filePath);
  }) { IsRequired = true };
#pragma warning restore CS8603

command.AddOption(assFileOption);
command.AddOption(fontFileOption);
command.SetHandler(CreateFontSubsetAsync, assFileOption, fontFileOption);

return await command.InvokeAsync(args);

// Main method
async Task<int> CreateFontSubsetAsync(FileInfo assFile, FileInfo fontFile) {
  try {
    // Get the informations we need from the ASS file regarding the font
    var pyftSubset = new PyFtSubset(assFile, fontFile);

    //
    if (!pyftSubset.IsFontInUse) {
      // That's not the font you're looking for. Move along!
      Console.WriteLine($"Font '{fontFile.GetDisplayName()}' is not used in the ASS file. No subset will be generated.");
      return 0;
    }

    // Let's a go!
    var pyftResult = await Cli.Wrap("pyftsubset")
      .WithWorkingDirectory(fontFile.Directory!.FullName)
      .WithArguments(pyftSubset.BuildArguments())
      .ExecuteBufferedAsync();
    if (pyftResult.ExitCode == 0) {
      Console.WriteLine($"Subset of font '{fontFile.GetDisplayName()}' successfully created.");
      return 0;
    }
    return 1;
  } catch (Exception ex) {
    Console.WriteLine($"stripfont failed with the following error: {ex.Message}");
    return 1;
  }
}