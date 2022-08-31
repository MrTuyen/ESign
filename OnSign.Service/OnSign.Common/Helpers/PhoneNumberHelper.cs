using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.Common.Helpers
{
    public static class PhoneNumberHelper
    {
        private static string[] m_Patterns = new string[] {
           @"^[0-9]{10}$",
           @"^\+[0-9]{2}\s+[0-9]{2}[0-9]{8}$",
           @"^[0-9]{3}-[0-9]{4}-[0-9]{4}$",
 };

        private static string MakeCombinedPattern()
        {
            return string.Join("|", m_Patterns
              .Select(item => "(" + item + ")"));
        }

    }
}
