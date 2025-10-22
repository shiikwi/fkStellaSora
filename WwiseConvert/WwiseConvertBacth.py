import os
import subprocess
import argparse
import sys

BNK_EXE = "bnkextr.exe"
VGMSTREAM_CLI_EXE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "vgmstream", "vgmstream-cli.exe")

def checktool(toolname):
    if not os.path.exists(toolname):
        print(f"Error: {toolname} not found.")
        return False;
    return True;

def extract_bnk(rootpath):
    bnkfiles = []
    for root, _, files in os.walk(rootpath):
        for file in files:
            if file.lower().endswith('.bnk'):
                bnkfiles.append(os.path.join(root, file))
    print(f"Found {len(bnkfiles)} .bnk files.")

    for i, bnkpath in enumerate(bnkfiles):
        print(f"Extracting {i+1}/{len(bnkfiles)}: {bnkpath}")
        try:
            bnk_dir = os.path.dirname(bnkpath)
            bnk_name = os.path.basename(bnkpath)

            result = subprocess.run([BNK_EXE, bnk_name], cwd = bnk_dir, check = True ,capture_output=True, text=True)
        except Exception as e:
            print(f"Error extracting {bnkpath}: {e}")

def convert_wem(rootpath):
    wem_files = []
    for root, _, files in os.walk(rootpath):
        for file in files:
            if file.lower().endswith('.wem'):
                wem_files.append(os.path.join(root, file))
    print(f"Found {len(wem_files)} .wem files.")

    for i, wempath in enumerate(wem_files):
        print(f"Converting {i+1}/{len(wem_files)}: {wempath}")
        try:
            result = subprocess.run([VGMSTREAM_CLI_EXE, wempath], check= True, capture_output=True, text=True)
        except Exception as e:
            print(f"Error converting {wempath}: {e}")

def main():
    parser = argparse.ArgumentParser(
        description="Batch extract .bnk files and convert .wem files",
        formatter_class=argparse.RawTextHelpFormatter)
    
    parser.add_argument(
        "mode",
        choices=["bnk", "wem"],
        help = "file type"
    )

    parser.add_argument(
        "path",
        help="Root folder of bnk or wem files",
    )

    args = parser.parse_args()

    if args.mode == "bnk":
        if not checktool(BNK_EXE):
            sys.exit(1)
        extract_bnk(args.path)
    
    if args.mode == "wem":
        if not checktool(VGMSTREAM_CLI_EXE):
            sys.exit(1)
        convert_wem(args.path)

if __name__ == "__main__":
    main()