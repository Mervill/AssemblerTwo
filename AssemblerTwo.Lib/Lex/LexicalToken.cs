using System;

namespace AssemblerTwo.Lib
{
    public enum LexicalToken
    {
        Unknown,
        EndOfLine,
        String,

        // = Symbols =
        SymExclaim,
        //SymDblQuote,
        SymSharp,
        SymDollar,
        SymPercent,
        SymAmp,
        SymQuote,
        SymLParen,
        SymRParen,
        SymAsterisk,
        SymPlus,
        SymComma,
        SymMinus,
        SymColon,
        //SymSemicolon,
        SymLessThan, // SymLessThan or SymLABrack? :thonk:
        SymEquals,
        SymGrtrThan,
        SymQuestMark,
        SymAt,
        SymPeriod,
        SymFwdslash,
        SymLSBrack,
        SymBackslash,
        SymRSBrack,
        SymCaret,
        //SymUndersc,
        SymBacktick,
        SymLCBrack,
        SymVertBar,
        SymRCBrack,
        SymTilde,

        //HexNumber,
        Number,
        //UnsignedNumber,
        //SignedNumber,
        Register,
        Opcode,
        Directive,
        //Label,
        Identifier,
    }
}
