using CliWrap;
using CliWrap.Buffered;
using SixLabors.Fonts;
using System.CommandLine;
using System.CommandLine.Parsing;
using static System.Net.Mime.MediaTypeNames;

// setup command line options
Option<FileInfo> assFileOption = new Option<FileInfo>("--ass-file", description: "The ASS scritp file to analyze.").ExistingOnly();
assFileOption.IsRequired = true;
assFileOption.AddValidator(result => {
  if (AssParser.IsValidAssFile(result.GetValueForOption(assFileOption)) == false) {
    result.ErrorMessage = $"{result.GetValueForOption(assFileOption)} is not a valid ASS file.";
  }
});
Option<FileInfo> fontFileOption = new Option<FileInfo>("--font-file", "The font used in the ASS script we want to strip.").ExistingOnly();
fontFileOption.IsRequired = true;
fontFileOption.AddValidator(result => { 
  if (PyFtSubset.IsValidFontFile(result.GetValueForOption(fontFileOption)) == false) {
    result.ErrorMessage = $"{result.GetValueForOption<FileInfo>(fontFileOption)} is not a valid font file.";
  }
});

// setup command line root command
RootCommand cmd = new(description: "Strip unwanted glyphs from a font file based on the usage of the font in a ASS subtitle file.") {
  assFileOption,
  fontFileOption
};
cmd.SetHandler(async (assFile, fontFile) => { await CheckAssFileAsync(assFile, fontFile); }, assFileOption, fontFileOption);
return await cmd.InvokeAsync(args);

// main method
static async Task<int> CheckAssFileAsync(FileInfo assFile, FileInfo fontFile) {
  try {
    // get the informations we need from the ASS file regarding the font
    var pyftSubset = new PyFtSubset(assFile, fontFile);

    //
    if (!pyftSubset.IsFontInUse) {
      // that's not the font you're looking for. Move along!
      Console.WriteLine($"Font '{fontFile.GetDisplayName()}' is not used in the ASS file. No subset will be generated.");
      return 0;
    }

    // let's go
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