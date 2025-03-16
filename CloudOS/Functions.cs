using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudOS
{
    class Functions
    {
        public Functions() { } 

        public bool StringValidator(string value)
        {
            foreach (char c in value)
                if (!Char.IsAsciiLetter(c) && !Char.IsWhiteSpace(c))
                    return false;

            return true;
        }

        public bool NumberValidator(string value, bool isID = false)
        {
            //The number of digits in SA ID
            if (isID && value.Length != 13)
                return false;

            foreach (char c in value)
                if (!Char.IsDigit(c))
                    return false;

            return true;
        }

        public bool CellValidator(string value)
        {
            string pattern = @"(\+27|0)\s?\d{2}\s?\d{3}\s?\d{4}\b";
            Match match = Regex.Match(value, pattern);
            if (!match.Success)
                return false;
            
            return true;
        }

        public bool EmailValidator(string value)
        {
            //If contains space, return
            if (value.Contains(" "))
                return false;

            string pattern = @"[0-9a-zA-Z]+\.?@[a-zA-Z]{2,}\.([a-zA-Z]{2,}\.?)+\b";
            Match match = Regex.Match(value, pattern);

            if (match.Success)
                return true;

            return false;
        }

        public bool SpecialCharChecker(string value)
        {
            foreach(char c in value)
                if (!Char.IsAsciiLetterOrDigit(c) && !Char.IsWhiteSpace(c))
                    return false;

            return true;
        }

        public bool CheckInput(string t1, string t2, string t3, string t4, string t5, string t6, string t7 = "")
        {
            if (string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2) && string.IsNullOrEmpty(t3) && string.IsNullOrEmpty(t4) && string.IsNullOrEmpty(t5) && string.IsNullOrEmpty(t6))
                return true;
            else
                return false;
        }

        public string PasswordChecker(string value)
        {
            string pattern;
            
            //Number check
            pattern = @"\d";
            if (!Regex.Match(value, pattern).Success)
                return "Must have at least one number.";

            //Lowercase check
            pattern = @"[a-z]";
            if (!Regex.Match(value, pattern).Success)
                return "Must have at least one lowercase.";

            //Uppercase check
            pattern = @"[A-Z]";
            if (!Regex.Match(value, pattern).Success)
                return "Must have at least one uppercase.";

            //Special characters
            pattern = @"[^0-9a-zA-Z]";
            if (!Regex.Match(value, pattern).Success)
                return "Must have at least one special char like @#$$%.";

            //Length check
            if (value.Length < 9)
                return "Length of at least 8 characters.";


            return string.Empty;
        }
    }
}
