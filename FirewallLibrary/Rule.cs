using System;
using System.Collections.Generic;

namespace FirewallLibrary
{
    // This class corresponds to a single row of input data
    public class Rule
    {
        public string rule_name { get; set; }
        public string server { get; set; }
        public string port { get; set; }

        public Rule()
        {
            
        }

        public Rule(string singleRow)
        {
            string[] splitFields = singleRow.Split(',');
            rule_name = splitFields[0];
            server = splitFields[1];
            port = splitFields[2];
        }

        // Equality comparer for washing out redundant rules
        public class RuleEqualityComparer : IEqualityComparer<Rule>
        {
            public bool Equals(Rule x, Rule y)
            {
                return x.server == y.server && x.port == y.port;
            }

            public int GetHashCode(Rule rule)
            {
                return rule.server.GetHashCode() ^
                    rule.port.GetHashCode();
            }
        }
    }
}
