using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBinConvert
{
    public class GameController
    {
        public Dictionary<object, byte[]> LoadCommonBinData(byte[] data)
        {
            var records = new Dictionary<object, byte[]>();

            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms, Encoding.UTF8))
            {
                reader.ReadInt32();  //Magic
                short strLen = reader.ReadInt16();
                reader.ReadBytes(strLen);   //Sig String

                byte keyType = reader.ReadByte();
                byte valueType = reader.ReadByte();
                int recordCount = reader.ReadInt32();

                if (keyType == 1)
                {
                    if (valueType == 1)  //key: int, value: byte[]
                    {
                        for (int i = 0; i < recordCount; i++)
                        {
                            records.Add(reader.ReadInt32(), reader.ReadBytes(reader.ReadInt16()));
                        }
                    }
                    else if (valueType == 2)  //key: long, value: byte[]
                    {
                        for (int i = 0; i < recordCount; i++)
                        {
                            records.Add(reader.ReadInt64(), reader.ReadBytes(reader.ReadInt16()));
                        }
                    }
                    else  //Key: string, value: byte[]
                    {
                        for (int i = 0; i < recordCount; i++)
                        {
                            records.Add(Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt16())), reader.ReadBytes(reader.ReadInt16()));
                        }
                    }
                }
                else
                {
                    for(int i = 0; i< recordCount; i++)
                    {
                        records.Add(i + 1, reader.ReadBytes(reader.ReadInt16()));
                    }
                }

            }

            return records;
        }

    }
}
