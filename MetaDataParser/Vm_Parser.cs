using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDataParser
{
    public class Vm_Parser
    {
        private const uint Signature = 0x1357FEDA;
        private const int PAYLOAD_SIZE_OFFSET = 0x04;
        private const int OperationKey_OFFSET = 0x08;
        private const int OperationKey_SIZE = 256;
        private const int Opcode_OFFSET = 0x108;
        private const int Opcode_SIZE = 64;
        private const int PAYLOAD_OFFSET = 0x148;
        private const int CHUNK_SIZE = 64;
        private const string Verify = "CODEPHIL";

        public byte[] DecryptFile(string path)
        {
            byte[] filedata = File.ReadAllBytes(path);

            var sig = BitConverter.ToUInt32(filedata, 0);
            if (sig != Signature) throw new InvalidDataException($"Invalid file signature: {sig:X}");

            int payloadSize = BitConverter.ToInt32(filedata, PAYLOAD_SIZE_OFFSET);

            byte[] OperationKey = filedata.Skip(OperationKey_OFFSET).Take(OperationKey_SIZE).ToArray();
            byte[] Opcode = filedata.Skip(Opcode_OFFSET).Take(Opcode_SIZE).ToArray();
            byte[] Payload = filedata.Skip(PAYLOAD_OFFSET).Take(payloadSize).ToArray();

            for (int i = 0; i < payloadSize; i += CHUNK_SIZE)
            {
                int size = Math.Min(CHUNK_SIZE, payloadSize - i);
                var chunk = new Span<byte>(Payload, i, size);
                ExecuteVM(Opcode, OperationKey, chunk, size);
            }

            if (Verify != Encoding.ASCII.GetString(Payload, 0, Verify.Length))
            {
                throw new InvalidDataException("Decrypt failed");
            }

            return Payload.Skip(Verify.Length).ToArray();
        }

        private void ExecuteVM(byte[] opcode, byte[] keyData, Span<byte> dataChunk, int ChunkSize)
        {
            // LABEL_6
            void Op_Sub(Span<byte> chunk, uint const1, uint const2, int keyIndex)
            {
                uint addr1 = const1 % (uint)ChunkSize;
                uint addr2 = ((uint)keyData[keyIndex] + const2) % (uint)ChunkSize;

                byte temp = (byte)(chunk[(int)addr2] - 1);
                chunk[(int)addr1] -= temp;
                chunk[(int)addr2] = temp;
            }

            // LABEL_13, LABEL_69
            void Op_Swap(Span<byte> chunk, uint const1, uint const2, int keyIndex)
            {
                uint addr1 = const1 % (uint)ChunkSize;
                uint addr2 = ((uint)keyData[keyIndex] + const2) % (uint)ChunkSize;

                byte temp = chunk[(int)addr1];
                chunk[(int)addr1] = chunk[(int)addr2];
                chunk[(int)addr2] = temp;
            }

            byte Ror(byte value, int count)
            {
                count &= 7;
                return (byte)((value >> count) | (value << (8 - count)));
            }

            // LABEL_263, LABEL_264
            void Op_Ror(Span<byte> chunk, uint const1, int const2, int keyIndex)
            {
                uint addr = const1 % (uint)ChunkSize;
                int shift;
                unchecked
                {
                    shift = keyData[keyIndex] + const2;
                }
                chunk[(int)addr] = Ror(chunk[(int)addr], shift & 7);
            }

            for (int i = 0; i < opcode.Length; i++)
            {
                byte op = opcode[i];

                switch (op)
                {
                    // XOR
                    case 0x02: dataChunk[(int)(0x5ADBAA1 % ChunkSize)] ^= (byte)(keyData[224] ^ 0x23); break;
                    case 0x0A: dataChunk[(int)(0xACE7709 % ChunkSize)] ^= (byte)(keyData[168] ^ 0xCB); break;
                    case 0x22: dataChunk[(int)(0x5BECA241 % ChunkSize)] ^= (byte)(keyData[0] ^ 0xC3); break;
                    case 0x8A: dataChunk[(int)(0x56DF0589 % ChunkSize)] ^= (byte)(keyData[40] ^ 0x4B); break;
                    case 0x92: dataChunk[(int)(0xFC703AF1 % ChunkSize)] ^= (byte)(keyData[240] ^ 0xF3); break;
                    case 0xB2: dataChunk[(int)(0xDC2AAA91 % ChunkSize)] ^= (byte)(keyData[16] ^ 0x93); break;
                    case 0xD2: dataChunk[(int)(0x1F5AA31 % ChunkSize)] ^= (byte)(keyData[48] ^ 0x33); break;
                    case 0xFA: dataChunk[(int)(0xAF332439 % ChunkSize)] ^= (byte)(keyData[24] ^ 0x7B); break;

                    // ADD/SUB
                    case 0x2A: unchecked { dataChunk[(int)(0x87C0C2A9 % ChunkSize)] = (byte)(dataChunk[(int)(0x87C0C2A9 % ChunkSize)] - keyData[200] - 107); } break;
                    case 0x3A: unchecked { dataChunk[(int)(0xB169DE79 % ChunkSize)] = (byte)(dataChunk[(int)(0xB169DE79 % ChunkSize)] - keyData[88] + 69); } break;
                    case 0x4A: unchecked { dataChunk[(int)(0x1BF99E49 % ChunkSize)] = (byte)(dataChunk[(int)(0x1BF99E49 % ChunkSize)] - keyData[232] - 11); } break;
                    case 0x5A: Op_Ror(dataChunk, unchecked((uint)-506133991), 3, 120); break;
                    case 0x62: dataChunk[(int)(0x75A82181 % ChunkSize)] ^= (byte)(keyData[64] ^ 3); break;
                    case 0xA2: unchecked { dataChunk[(int)(0x5215E0C1 % ChunkSize)] = (byte)(dataChunk[(int)(0x5215E0C1 % ChunkSize)] - keyData[128] - 67); } break;
                    case 0xDA: Op_Ror(dataChunk, unchecked((uint)-1322569575), 3, 248); break;
                    case 0xE2: unchecked { dataChunk[(int)(0x4A75E001 % ChunkSize)] = (byte)(dataChunk[(int)(0x4A75E001 % ChunkSize)] - keyData[192] + 125); } break;
                    case 0xEA: unchecked { dataChunk[(int)(0x75B05869 % ChunkSize)] = (byte)(dataChunk[(int)(0x75B05869 % ChunkSize)] - keyData[136] - 43); } break;

                    // SWAP
                    case 0x12: Op_Swap(dataChunk, 0x70C1C71, unchecked((uint)-513902989), 112); break;
                    case 0x42: Op_Sub(dataChunk, 0xB8819E1, unchecked((uint)-756031645), 32); break;
                    case 0x6A: Op_Swap(dataChunk, 0xC9A109E9, 859017131, 8); break;
                    case 0x7A: Op_Sub(dataChunk, 0x1642B5B9, 57612795, 152); break;
                    case 0x82: Op_Sub(dataChunk, 0x3F74B921, unchecked((uint)-906790749), 96); break;
                    case 0x9A: Op_Sub(dataChunk, 0x36DAF959, 901937819, 184); break;
                    case 0xC2: Op_Swap(dataChunk, 0xDAB39861, unchecked((uint)-858041885), 160); break;
                    case 0xCA: Op_Sub(dataChunk, 0xFCBEACC9, 1038698891, 104); break;
                    case 0xF2: Op_Swap(dataChunk, 0xC0F939D1, 153321171, 80); break;

                    // ROR/ROL
                    case 0x1A: Op_Ror(dataChunk, 1624853209, 3, 56); break;
                    case 0x32: Op_Ror(dataChunk, unchecked((uint)-1105966063), 3, 144); break;
                    case 0x52: Op_Ror(dataChunk, 378342321, 3, 176); break;
                    case 0x72: Op_Ror(dataChunk, 1402886993, 3, 208); break;
                    case 0xAA: Op_Ror(dataChunk, 1306235177, 3, 72); break;
                    case 0xBA: Op_Ror(dataChunk, unchecked((uint)-272249607), 3, 216); break;

                    default:
                        throw new NotImplementedException($"Opcode 0x{opcode:X2} is not implemented yet.");
                }
            }
        }

    }
}
