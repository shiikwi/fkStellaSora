using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveParser;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: ArchiveParser.exe <path_to_arcx_file>");
            return;
        }

        string inputFile = args[0];
        string dir = Path.GetDirectoryName(inputFile) ?? ".";
        string outfile = Path.Combine(dir, Path.GetFileNameWithoutExtension(inputFile) + "Unpack");

        try
        {
            var parser = new ArchiveRead();
            parser.Unpack(inputFile, outfile);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}