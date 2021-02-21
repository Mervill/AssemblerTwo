# AssemblerTwo
Hobbyist assembly language, assembler, and virtual machine

## Machine Definition

The virtual machine is a 16-bit byte-addressed processor that has:

- 16-bit Program Counter (64k Address Space)
- Registers: `A`, `B`, `C`, `D`, `E`, `F`, `G`, `H`, `I`, `J`, `K`, `L`, `M`, `N`, `P`, `S`. 
  - `A ... N` - General purpose registers 
  - `P` - General purpose register with conventions
  - `S` - Stack Pointer register manipulated by `PUSH`, `POP`, `CALL`, `RET` & others
- No hard-wired accumulator; most instructions are target/operand type (ie `ADD A,B`)

See `/Documents/opcodes.txt` for the complete instruction listing. Please note that the list of opcodes in still in active design/development and can change without notice. Some opcodes in the list have yet to be implemented in code :shipit:

## Sample Code

```
; MSG routine to print a null-terminated string, the first address
; of the string is passed as an argument via the P register
MSG: LOAD    P,M   ; Load the word at `P` into `M`
     HI      M,N   ; Put HI byte of `M` into `N`
     REZ     N     ; If `N` equals 0, return (we're done)
     COPY    N,A   ; Copy `N` into `A`
     CALL    PCHAR ; Print the char
     LO      M,N   ; Put LO byte of `M` into `N`
     REZ     N     ; if `N` equals 0, return (we're done)
     COPY    N,A   ; Copy `N` into `A`
     CALL    PCHAR ; Print the char
     ADDI    P,2   ; Increment the pointer
     JUMP    MSG
```

# AssemblerTwo.Cmd
Contains the command line progam for working with the Assembler and the VM.

```
AssemblerTwo.Cmd.exe -f ./target-file.asm
```

Will attempt to build `target-file.asm` and if successful will promt you to either exit the progam or launch into the virtual machine.

- `/Sources/` has asm programs that can be assembled and executed.
- Try `AssemblerTwo.Cmd.exe -f ./Sources/pangram.asm2.txt`!

## Alternate modes

- `-m build -f ./target-file.asm -o ./out-file.a` Will build the `target-file.asm` assembly and write the assembled binary to `out-file.a`
- `-m run -f ./target-binary.a` Will attempt to load `target-binary.a` as binary and launch the simple virtual machine.
- `-m vterm -f ./target-binary.a` Will attempt to load `target-binary.a` as binary and launch the virtual terminal.

## vterm (virtual terminal)
It's neat!

F1 - Start/Stop VM execution.

F2 - Single Step & Stop

F3 - 100 Steps & Stop

# AssemblerTwo.Lib
Primarily contains the actual Assembler code, as well as misc support code.

This library can stand alone if you want to embed the assembler in another project.

# AssemblerTwo.Machine
Contains only the virtual machine and minimal support code.

This library can stand alone if you want to embed the virtual machine in another project.

# Misc

- `Documents/Misc/npp_asm2.xml` Notepad++ User Defined Language file for the asm language.