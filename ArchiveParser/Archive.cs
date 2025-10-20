using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Xxtea;
using K4os.Compression.LZ4;

namespace ArchiveParser
{
    public readonly struct ArchiveHeader
    {
        public readonly uint Magic;
        public readonly uint Version;
        public readonly uint HeaderFlag;
        public readonly uint BlockFlag;
        public readonly uint OriginSize;
        public readonly uint Size;
        public readonly uint BlockEntries;
        public readonly uint Reserved;

        public bool IsHeaderEncrypted => (HeaderFlag & 0x10) != 0;
        public bool IsBlockCompressed => (BlockFlag & 0x10) != 0;
        public bool IsBlockEncrypted => (BlockFlag & 0x100) != 0;

        public ArchiveHeader(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            HeaderFlag = reader.ReadUInt32();
            BlockFlag = reader.ReadUInt32();
            OriginSize = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            BlockEntries = reader.ReadUInt32();
            Reserved = reader.ReadUInt32();
        }

        public void Validate()
        {
            const uint EXPECTED_MAGIC = 0x5241421A;
            if (Magic != EXPECTED_MAGIC)
            {
                throw new InvalidDataException($"Invalid magic number. Expected {EXPECTED_MAGIC:X}, but got {Magic:X}.");
            }
        }
    }
    public readonly struct ArchiveEntry
    {
        public readonly ulong Hash;
        public readonly uint BlockOffset;
        public readonly uint OriginalSize;
        public readonly uint Size;

        public ArchiveEntry(BinaryReader reader)
        {
            Hash = reader.ReadUInt64();
            BlockOffset = reader.ReadUInt32();
            OriginalSize = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }

    public class ArchiveRead
    {
        private const string KEY_STR = "&^^%#$#_$!@![]<_>?GHBFR_1153SDR_";
        private byte[] globalkey = Encoding.UTF8.GetBytes(KEY_STR);
        private const int kx = 0xFF;
        private const int ky = 0xFF;

        private const int BLOCK_SHIFT = 12;

        public void Unpack(string filePath, string outPath)
        {

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs);
            var header = new ArchiveHeader(reader);
            header.Validate();

            Console.WriteLine($"* Magic: {header.Magic:X}");
            Console.WriteLine($"* Version: {header.Version}");
            Console.WriteLine($"* Entry Count: {header.BlockEntries}");
            Console.WriteLine($"* Blocks Encrypted: {header.IsBlockEncrypted}");
            Console.WriteLine($"* Blocks Compressed: {header.IsBlockCompressed}");


            byte[] entryData = reader.ReadBytes((int)header.Size);
            if (header.IsHeaderEncrypted)
            {
                entryData = DecryptBlock(entryData);
            }

            var entries = new List<ArchiveEntry>();
            using (var entrystream = new MemoryStream(entryData))
            using (var entryreader = new BinaryReader(entrystream))
            {
                for (int i = 0; i < header.BlockEntries; i++)
                {
                    entries.Add(new ArchiveEntry(entryreader));
                }
            }

            byte[] xxteakey = GetDerivedKey(kx, ky);

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);
            int unpackcount = 0;
            foreach (var entry in entries)
            {
                fs.Seek(entry.BlockOffset << BLOCK_SHIFT, SeekOrigin.Begin);
                byte[] data = reader.ReadBytes((int)entry.Size);

                if (header.IsBlockEncrypted)
                {
                    data = DecryptBlock(data);
                }
                else if (header.IsBlockCompressed)
                {
                    data = DecompressBlock(data, entry.OriginalSize);
                }
                else if (header.IsBlockCompressed && header.IsBlockEncrypted)
                {
                    throw new NotImplementedException("Did not implement decompressing and decrypting both.");
                }

                data = XXTEA.Decrypt(data, xxteakey);

                string filename = $"{unpackcount}_{entry.Hash:X16}.bin";
                string outfilepath = Path.Combine(outPath, filename);
                File.WriteAllBytes(outfilepath, data);
                unpackcount++;
            }
            Console.WriteLine($"Unpacked File: {unpackcount}");
        }

        private byte[] DecryptBlock(byte[] encdata)
        {
            byte[] decdata = new byte[encdata.Length];
            for (int i = 0; i < encdata.Length; i++)
            {
                decdata[i] = (byte)(encdata[i] ^ globalkey[i % globalkey.Length]);
            }
            return decdata;
        }

        private byte[] DecompressBlock(byte[] data, uint orisize)
        {
            if (orisize == 0) return Array.Empty<byte>();
            byte[] decompdata = new byte[orisize];
            LZ4Codec.Decode(data, decompdata);
            return decompdata;
        }

        private byte[] GetDerivedKey(int kx, int ky)
        {
            long product = (long)kx * ky;
            byte[] probytes = BitConverter.GetBytes((uint)product);
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(probytes);
            }
        }

    }
}
