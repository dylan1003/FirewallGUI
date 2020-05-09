using System;
using System.Collections.Generic;
using System.Text;

namespace FirewallLibrary
{
    public class Rule
    {
        // rule can relate to multiple servers and ports
        public string rule_name { get; set; }
        public List<string> serverList { get; set; }
        public List<string> portList { get; set; }

        public Rule()
        {
            
        }

        public Rule(string singleRow)
        {
            string[] splitFields = singleRow.Split(',');
            rule_name = splitFields[0];
            serverList.Add(splitFields[1]);
            portList.Add(splitFields[2]);
        }
    }
}
