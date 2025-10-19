using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaDataParser;
public class Program
{
    public static void Main(string[] args)
    {
        string encFilepath = "global-metadata.dat";
        string decFilepath = "global-metadata.dec.dat";

        if (!File.Exists(encFilepath))
        {
            Console.WriteLine($"Read File Failed: '{encFilepath}'");
            return;
        }

        try
        {
            var decryptor = new Vm_Parser();
            byte[] decryptedData = decryptor.DecryptFile(encFilepath);

            File.WriteAllBytes(decFilepath, decryptedData);

            Console.WriteLine($"Decrypt success");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
