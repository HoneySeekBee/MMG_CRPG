using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Validation
{
    public static class Guard
    {
        static readonly Regex Hex6 = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public static void NotEmpty(string? v, string name)
        {
            if (string.IsNullOrWhiteSpace(v))
                throw new InvalidOperationException($"{name}는(은) 비워둘 수 없습니다.");
        }

        public static void Color(string? hex, string name)
        {
            if (hex is null) return;
            if (!Hex6.IsMatch(hex))
                throw new InvalidOperationException($"{name} 형식이 잘못되었습니다. 예) #RRGGBB");
        }

        public static void Range(short value, short min, short max, string name)
        {
            if (value < min || value > max)
                throw new InvalidOperationException($"{name} 범위는 {min}~{max} 입니다.");
        }
    }
}
