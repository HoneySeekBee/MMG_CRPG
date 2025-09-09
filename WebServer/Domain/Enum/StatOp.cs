using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum StatOp : short
    {
        // 고정값 더하기 (예: ATK +50)
        AddFlat = 0,

        // 퍼센트 더하기 (예: CRI_RATE +10%)  ← 입력은 10(%)로
        AddPct = 1,

        // 곱하기 계수 (예: 최종값 ×1.2)
        Mul = 2,

        // 값 고정 (예: DEF = 500)
        Set = 3,
    }
    public static class StatOpCodes
    {
        public static string ToCode(StatOp op) => op switch
        {
            StatOp.AddFlat => "add_flat",
            StatOp.AddPct => "add_pct",
            StatOp.Mul => "mul",
            StatOp.Set => "set",
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

        public static StatOp FromCode(string code) => code switch
        {
            "add_flat" => StatOp.AddFlat,
            "add_pct" => StatOp.AddPct,
            "mul" => StatOp.Mul,
            "set" => StatOp.Set,
            _ => throw new ArgumentOutOfRangeException(nameof(code))
        };
    }
}
