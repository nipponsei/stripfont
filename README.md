# StripFont

## What is it?
`stripfont` is a dotnet cross-platform command line utility to strip unwanted characters or glyphs from a font file.  
The characters we want to keep are identified by the usage of the font in an [ASS](http://www.tcax.org/docs/ass-specs.htm) script file, based on style definitions and tags (\fn).

Technically, it's just a wrapper around [fontTools](https://fonttools.readthedocs.io/en/latest/). The most annoying part being parsing the ASS file.

## But why?
While western font files are usually light in terms of size on the disk, that's not the case for CJK (Chinese/Japanese/Korean) fonts.  
Embedding such fonts in an MKV file just bloats its size and takes more time to load in memory when playing. 

## Install and prerequisites
**Prerequisites:**  
You're gonna need the following:
- dotnet 6. Can be downloaded from [here](https://dotnet.microsoft.com/en-us/download)
- Python3. Download link [here](https://www.python.org/downloads/)
- fontTools : `python3 -m pip install fonttools`
- brotli : `python3 -m pip install brotli`


**Install stripfont:**  
You can install it by cloning this repo and running `dotnet build` at the root of the **stripfont** folder.  
The binary file (*./stripfont* on Linux or *stripfont.exe* on Windows) will be available in the **bin/debug/net6.0** folder.


## How to run
```cmd
YUKI.N>stripfont -h
Description:
  Strip unwanted glyphs from a font file based on the usage of the font in a ASS subtitle file.

Usage:
  stripfont [options]

Options:
  --ass-file <ass-file> (REQUIRED)    The ASS scritp file to analyze.
  --font-file <font-file> (REQUIRED)  The font used in the ASS script we want to strip.
  --version                           Show version information
  -?, -h, --help                      Show help and usage information
```

It takes two parameters:
- `--font-file`: full path of the font file we want to strip 
- `--ass-file`: full path of an ASS file as a source to detect the characters we want to keep

If everything goes right, the newly created font will be available in the directory of the source font file, with a randomly generated file name like `XB0CGDCB.ttf`.

**Example:**
```bash
yuki.n@chiba$ ./stripfont --ass-file ~/_github/_wip/niehime/subs/NCOP1/NCOP1.ass --font-file ~/_github/_wip/niehime/fonts/DFHanziPenStdN-W5.otf 
OpenType font OSIOXC0X.ttf successfully created.
yuki.n@chiba$
```

## Some comparisons
Here are a few examples using the following fonts:
- DynaComware [DF ハンジペン Std W5](https://www.dynacw.co.jp/product/product_download_detail.aspx?fid=45)
- Fontworks [FOT-マティスはつひやまとPro B](https://fontworks.co.jp/fontsearch/matissehatsuhipro-b/)
- Morisawa [A-OTF すずむし Std M](https://www.morisawa.co.jp/fonts/specimen/1212)

> **Note: The resulting file size could be smaller, but in my case I decided to keep all OpenType layout features.**

|Font file|Originial file sizesize|Total # of glyphs|# of glyphs used|Stripped file size|
|:---|---:|---:|---:|---:|
|DFHanziPenStdN-W5.otf|6.64 MB|8310|117|102 KB|
|FOT-MatisseHatsuhiPro-B.otf|9.4 MB|9804|98|310 KB|
|A-OTF-SuzumushiStd-Medium.otf|3.77M|8207|17|14.1 KB|

For a more visual comparison, you can check [this page](sample/previews.md).

## What's next?
- Because I needed this tool like yesterday, I didn't bother to package it ~~properly~~ at all. That's on my TODO list.
- If you properly examine the original and generated fonts, you'll notice there might be some differences in the metadatas (especially Family and Style). I probably missed a parameter when using fonTools. For now, it works, but I'll have a look at that.
- I didn't check (yet) if there's a good ASS parser in dotnet out there. Maybe I'll give it a go.