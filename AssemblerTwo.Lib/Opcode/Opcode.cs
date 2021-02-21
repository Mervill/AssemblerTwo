using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblerTwo.Lib
{
    public enum Opcode
    {
        // 0xA000
        ADD   = 0xA000,
        SUB   = 0xA100,
        MUL   = 0xA200,
        //DIV   = 0xA300,
        //DIVS  = 0xA400,
        MOD   = 0xA500,
        AND   = 0xA600,
        OR    = 0xA700,
        XOR   = 0xA800,
        SHL   = 0xA900,
        SHR   = 0xAA00,
        //POW   = 0xAB00,

        // 0xAF00
        ADDI  = 0xAF00,
        SUBI  = 0xAF10,
        MULI  = 0xAF20,
        //DIVI  = 0xAF30,
        //DIVSI = 0xAF40,
        MODI  = 0xAF50,
        ANDI  = 0xAF60,
        ORI   = 0xAF70,
        XORI  = 0xAF80,
        SHLI  = 0xAF90,
        SHRI  = 0xAFA0,
        //POWI  = 0xAFB0,
        INC   = 0xAFC0,
        DEC   = 0xAFD0,
        NOT   = 0xAFE0,
        NEG   = 0xAFF0,

        // 0xB000
        JLT   = 0xB000,
        JLTE  = 0xB100,
        JGT   = 0xB200,
        JGTE  = 0xB300,
        JB    = 0xB400,
        JBE   = 0xB500,
        JA    = 0xB600,
        JAE   = 0xB700,
        JEQ   = 0xB800,
        JNEQ  = 0xB900,
        
        // 0xC000
        RLT   = 0xC400,
        RLTE  = 0xC500,
        RGT   = 0xC600,
        RGTE  = 0xC700,
        RB    = 0xC800,
        RBE   = 0xC900,
        RA    = 0xCA00,
        RAE   = 0xCB00,
        REQ   = 0xCC00,
        RNEQ  = 0xCD00,

        // 0xD000
        COPY  = 0xD000,
        LOAD  = 0xD100,
        STOR  = 0xD200,
        //0xD300 PLOAD
        //0xD400 PSTOR
        //0xD500
        //0xD600
        //0xD700
        //0xD800
        RXR   = 0xD900,
        RXM   = 0xDA00,
        HI    = 0xDB00,
        LO    = 0xDC00,
        IN    = 0xDD00,

        // 0xDE00
        COPYI = 0xDE00,
        //0xDE10 PLOADI
        //0xDE20 PSTORI
        INI   = 0xDE30,
        OUTI  = 0xDE40,
        SHAL  = 0xDE50,
        SHAR  = 0xDE60,
        //0xDE70 ROTL
        //0xDE80 ROTR
        JUMPR = 0xDE90,
        
        // 0xDEA0
        SYSCALL = 0xDEA0,
        SYSRET = 0xDEA1,
        POPR  = 0xDEA2,
        PUSHR = 0xDEA3,
        //0xDEA4
        //0xDEA5
        //0xDEA6
        //0xDEA7
        //0xDEA8
        //0xDEA9
        JUMP  = 0xDEAA,
        CALL  = 0xDEAB,
        RET   = 0xDEAC,
        HALT  = 0xDEAD,
        EI    = 0xDEAE,
        DI    = 0xDEAF,

        // 0xDE00 (Contd)
        CALLR = 0xDEB0,
        PUSH  = 0xDEC0,
        POP   = 0xDED0,
        INTE  = 0xDEE0,
        //0xDEDF0

        // 0xD00 (Contd)
        OUT   = 0xDF00,

        // 0xE000
        // ...

        // 0xF000
        JEZ   = 0xFF00,
        JLZ   = 0xFF10,
        JLEZ  = 0xFF20,
        JGZ   = 0xFF30,
        JGEZ  = 0xFF40,
        // 0xFF50
        // 0xFF60
        // 0xFF70
        // 0xFF80
        // 0xFF90
        REZ   = 0xFFA0,
        RLZ   = 0xFFB0,
        RLEZ  = 0xFFC0,
        RGZ   = 0xFFD0,
        RGEZ  = 0xFFE0,

        NOP   = 0xFFF0,
    }
}
