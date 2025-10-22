using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataBinConvert;
using Google.Protobuf;
using Newtonsoft.Json;
using StellaSoraParser.Proto;

public class Program
{
    public static void Main(string[] args)
    {
        string inputFile = "355_5DEAD2A6189B75C6.bin";
        string ProtoTypeName = "StellaSoraParser.Proto.VampireTalent";

        string outputFile = ProtoTypeName + ".json";
        try
        {
            ConvertSingleBinToJson(inputFile, outputFile, ProtoTypeName);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }

    }

    public static void ConvertSingleBinToJson(string infile, string outfile, string typeName)
    {
        const int Magic = 0x00039354;
        if(!Path.Exists(infile))
        {
            throw new FileNotFoundException($"Cannot Find {infile}");
        }

        byte[] filebytes = File.ReadAllBytes(infile);

        if(filebytes.Length < 4 || BitConverter.ToInt32(filebytes, 0) != Magic)
        {
            Console.WriteLine("Plain Text, Skip Convert");

            try
            {
                object jsonobj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(filebytes));
                string formatJson = JsonConvert.SerializeObject(jsonobj, Formatting.Indented);
                File.WriteAllText(outfile, formatJson);
            }
            catch
            {
                File.WriteAllBytes(outfile, filebytes);
                return;
            }
        }

        var converter = new GameController();
        Dictionary<object, byte[]> rawRecords = converter.LoadCommonBinData(filebytes);

        Type ProtoType = Type.GetType(typeName, throwOnError: true);
        PropertyInfo parserProperty = ProtoType.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)!;
        object parserInstance = parserProperty.GetValue(null)!;
        MethodInfo parseMethod = parserInstance.GetType().GetMethod("ParseFrom", new[] { typeof(byte[]) })!;

        var JsonOutput = new Dictionary<object, object>();
        foreach(var record in rawRecords)
        {
            object msg = parseMethod.Invoke(parserInstance, new object[] { record.Value })!;
            JsonOutput.Add(record.Key, msg);
        }

        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<Newtonsoft.Json.JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
        string JsonOutputStr = JsonConvert.SerializeObject(JsonOutput, jsonSettings);

        File.WriteAllText(outfile, JsonOutputStr);
        Console.WriteLine($"Successfully Converted {infile} to {outfile}");
    }

}
