using CliWrap;
using CliWrap.Buffered;
using System.CommandLine;
using System.CommandLine.Parsing;

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
    // create the temporary subset woff2 font
    var pyftSubset = new PyFtSubset(assFile, fontFile);
    var pyftResult = await Cli.Wrap("pyftsubset")
      .WithWorkingDirectory(fontFile.Directory!.FullName)
      .WithArguments(pyftSubset.BuildArguments())
      .ExecuteBufferedAsync();
    if (pyftResult.ExitCode != 0) {
      Console.WriteLine(pyftResult.StandardError);
    }

    // decompress woff2 into a ttf font file
    var s = pyftSubset.BuildPythonArguments();
    var pythonResult = await Cli.Wrap("python")
      .WithWorkingDirectory(fontFile.Directory!.FullName)
      .WithArguments(pyftSubset.BuildPythonArguments(), false) // set to false to NOT escape the double quote!
      .ExecuteBufferedAsync();
    if (pythonResult.ExitCode == 0) {
      Console.WriteLine($"OpenType font {pyftSubset.RandomFileName}.ttf successfully created.");
    } else {
      Console.WriteLine(pythonResult.StandardError);
    }
    pyftSubset.DeleteTempFontFile();

    return 0;
	} catch (Exception ex) {
    Console.WriteLine($"stripfont failed with the following error: {ex.Message}");
    return 1;
	}
}