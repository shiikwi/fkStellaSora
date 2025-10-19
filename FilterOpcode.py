import os

def filter_vm_opcode():
    OPCODE_OFFSET = 0x108
    OPCODE_SIZE = 64

    try:
        with open("global-metadata.dat", 'rb') as f:
            f.seek(OPCODE_OFFSET)
            opcode_data = f.read(OPCODE_SIZE)

        unique_opcodes = set(opcode_data)
        sorted_opcodes = sorted(list(unique_opcodes))

        with open("opcodes.txt", 'w') as f:
            f.write(f"Total Unique Opcodes: {len(sorted_opcodes)}\n")
            f.write("---------------------------\n")

            for op in sorted_opcodes:
                f.write(f"0x{op:02X}\n")
    except Exception as e:
        print(f"An error occurred: {e}")


if __name__ == "__main__":
    filter_vm_opcode()
