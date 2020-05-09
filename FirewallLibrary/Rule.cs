using System;
using System.Collections.Generic;
using System.Text;

namespace FirewallLibrary
{
    public class Rule
    {
        // rule can relate to multiple servers and ports
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

        // comparer for deduping redundant rules, rule_name is irrelevant
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
