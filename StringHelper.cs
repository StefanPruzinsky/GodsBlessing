using System;

namespace GodsBlessing
{
    class StringHelper
    {
        public string FirstLetterToUpper(string text)
        {
            return String.Format("{0}{1}", text[0].ToString().ToUpper(), text.Substring(1));
        }

        public string FirstLetterToLower(string text)
        {
            return String.Format("{0}{1}", text[0].ToString().ToLower(), text.Substring(1));
        }

        public string ConvertFromCamelCaseToUnderscoreCase(string text)
        {
            string result = "";

            for (int i = 0; i < text.Length; i++)
                if (text[i].ToString().ToUpper() == text[i].ToString())
                    result += String.Format("_{0}", text[i].ToString().ToLower());
                else
                    result += text[i];

            return result;
        }
    }
}
