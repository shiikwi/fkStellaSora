## MetaDataParser
Decrypt `global-metadata.dat` so it can be dumped by tools like Il2CppDumper.  
Place `global-metadata.dat` in the same folder as the executable and run the program.
## ArchiveParser
Unpack `.arcx` files such as `lua.arcx` and `data.arcx`.  
Usage:
```bash
ArchiveParser.exe <path_to_arcx_file>
```
## DataBinConvert
Convert specific .bin files (unpacked from data.arcx) into JSON format.
Note: You need to manually edit the source code to specify the input file and its corresponding class type.
## WwiseConvert
Convert Wwise .wem and .bnk audio files to .wav format.
Usage:
```bash
WwiseConvertBacth.py <wem/bnk> <Path of Folder contains bnk or wem files>
```