﻿using System;

// float
// -----
// there are two types of floating numbers: float & double
public enum FloatSuffix {
    NONE,
    F,
    LF
}

public class TokenFloat : Token {
    public TokenFloat(Double _val, FloatSuffix _suffix, String _raw)
        : base(TokenType.FLOAT) {
        val = _val;
        suffix = _suffix;
        raw = _raw;
    }
    public readonly Double val;
    public readonly String raw;
    public readonly FloatSuffix suffix;
    public override string ToString() {
        string str = type.ToString();
        switch (suffix) {
        case FloatSuffix.F:
            str += "(float)";
            break;
        case FloatSuffix.LF:
            str += "(long double)";
            break;
        default:
            break;
        }
        return str + ": " + val.ToString() + " \"" + raw + "\"";
    }
}

public class FSAFloat : FSA {
    string raw;
    Int64 int_part;
    Int64 frac_part;
    Int64 frac_count;
    Int64 exp_part;
    bool exp_pos;
    FloatSuffix float_type;

    public enum FloatState {
        START,
        END,
        ERROR,
        D,
        P,
        DP,
        PD,
        DE,
        DES,
        DED,
        PDF,
        DPL
    };

    public FloatState state;

    public FSAFloat() {
        state = FloatState.START;
        int_part = 0;
        frac_part = 0;
        frac_count = 0;
        exp_part = 0;
        float_type = FloatSuffix.NONE;
        exp_pos = true;
        raw = "";
    }
    public void Reset() {
        state = FloatState.START;
        int_part = 0;
        frac_part = 0;
        frac_count = 0;
        exp_part = 0;
        float_type = FloatSuffix.NONE;
        exp_pos = true;
        raw = "";
    }

    public FSAStatus GetStatus() {
        switch (state) {
        case FloatState.START:
            return FSAStatus.NONE;
        case FloatState.END:
            return FSAStatus.END;
        case FloatState.ERROR:
            return FSAStatus.ERROR;
        default:
            return FSAStatus.RUN;
        }
    }

    public Token RetrieveToken() {
        Double val;
        if (exp_pos) {
            val = (int_part + frac_part * Math.Pow(0.1, frac_count)) * Math.Pow(10, exp_part);
        } else {
            val = (int_part + frac_part * Math.Pow(0.1, frac_count)) * Math.Pow(10, -exp_part);
        }
        return new TokenFloat(val, float_type, raw.Substring(0, raw.Length - 1));
    }

    public void ReadChar(char ch) {
        raw += ch;
        switch (state) {
        case FloatState.ERROR:
        case FloatState.END:
            state = FloatState.ERROR;
            break;

        case FloatState.START:
            if (char.IsDigit(ch)) {
                int_part = ch - '0';
                state = FloatState.D;
            } else if (ch == '.') {
                state = FloatState.P;
            } else {
                state = FloatState.ERROR;
            }
            break;

        case FloatState.D:
            if (char.IsDigit(ch)) {
                int_part *= 10;
                int_part += ch - '0';
                state = FloatState.D;
            } else if (ch == 'e' || ch == 'E') {
                state = FloatState.DE;
            } else if (ch == '.') {
                state = FloatState.DP;
            } else {
                state = FloatState.ERROR;
            }
            break;

        case FloatState.P:
            if (char.IsDigit(ch)) {
                frac_part = ch - '0';
                frac_count = 1;
                state = FloatState.PD;
            } else {
                state = FloatState.ERROR;
            }
            break;

        case FloatState.DP:
            if (char.IsDigit(ch)) {
                frac_part = ch - '0';
                frac_count = 1;
                state = FloatState.PD;
            } else if (ch == 'e' || ch == 'E') {
                state = FloatState.DE;
            } else if (ch == 'f' || ch == 'F') {
                float_type = FloatSuffix.F;
                state = FloatState.PDF;
            } else if (ch == 'l' || ch == 'L') {
                float_type = FloatSuffix.LF;
                state = FloatState.DPL;
            } else {
                state = FloatState.END;
            }
            break;

        case FloatState.PD:
            if (char.IsDigit(ch)) {
                frac_part *= 10;
                frac_part += ch - '0';
                frac_count++;
                state = FloatState.PD;
            } else if (ch == 'e' || ch == 'E') {
                state = FloatState.DE;
            } else if (ch == 'f' || ch == 'F') {
                float_type = FloatSuffix.F;
                state = FloatState.PDF;
            } else if (ch == 'l' || ch == 'L') {
                float_type = FloatSuffix.LF;
                state = FloatState.DPL;
            } else {
                state = FloatState.END;
            }
            break;

        case FloatState.DE:
            if (char.IsDigit(ch)) {
                exp_part = ch - '0';
                state = FloatState.DED;
            } else if (ch == '+' || ch == '-') {
                if (ch == '-') {
                    exp_pos = false;
                }
                state = FloatState.DES;
            } else {
                state = FloatState.ERROR;
            }
            break;

        case FloatState.DES:
            if (char.IsDigit(ch)) {
                exp_part = ch - '0';
                state = FloatState.DED;
            } else {
                state = FloatState.ERROR;
            }
            break;

        case FloatState.DPL:
            //if (ch == 'f') {
            //    float_type = FloatType.LF;
            //    state = FloatState.PDF;
            //} else {
            //    state = FloatState.ERROR;
            //}
            //break;
            float_type = FloatSuffix.LF;
            state = FloatState.END;
            break;

        case FloatState.DED:
            if (char.IsDigit(ch)) {
                exp_part *= 10;
                exp_part += ch - '0';
                state = FloatState.DED;
            } else if (ch == 'f' || ch == 'F') {
                float_type = FloatSuffix.F;
                state = FloatState.PDF;
            } else if (ch == 'l' || ch == 'L') {
                float_type = FloatSuffix.LF;
                state = FloatState.DPL;
            } else {
                state = FloatState.END;
            }
            break;

        case FloatState.PDF:
            state = FloatState.END;
            break;

        default:
            state = FloatState.ERROR;
            break;
        }

    }

    public void ReadEOF() {
        switch (state) {
        case FloatState.DP:
        case FloatState.PD:
        case FloatState.DED:
        case FloatState.PDF:
        case FloatState.DPL:
            state = FloatState.END;
            break;
        default:
            state = FloatState.ERROR;
            break;
        }
    }

}

