using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FirewallLibrary
{
    // Serves as a controller and container data being processed
    public class DataManager
    {
        // Holds the rules in their original state
        private List<Rule> unalteredRules { get; set; }
        // Holds the rules in their current state
        private List<Rule> processedRules { get; set; }

        private string lessUniqueField { get; set; }

        // Public getter for the original rules
        public List<Rule> OriginalRules
        {
            get { return unalteredRules; }
        }
        // Public getter for processed rule list
        public List<Rule> ProcessedRules
        {
            get { return processedRules; }
        }

        //Constructor
        public DataManager()
        {

        }

        // Constructor
        public DataManager(List<string> inputData)
        {
            unalteredRules = LoadDataSet(inputData);
            lessUniqueField = GetFieldWithLowerUniqueOccurrences();
        }
        // Will help with algorithm later
        private string GetFieldWithLowerUniqueOccurrences()
        {
            int uniquePorts = unalteredRules.GroupBy(rule => rule.server).Select(group => group.Count()).Max();

            int uniqueServers = unalteredRules.GroupBy(rule => rule.server).Select(group => group.Count()).Max();
            // Default to server if it's even
            if(uniquePorts < uniqueServers)
            {
                return "port";
            }
            else
            {
                return "server";
            }
        }

        // A dumb dedupe of data
        private static void Dedupe(ref List<string> dedupedData)
        {
            dedupedData = dedupedData.Distinct().ToList();
        }

        // Gets rid of rules that are functionally redundant (same server and port as another rule)
        public void FilterRedundantRules()
        {
            Console.WriteLine("Total rules: " + unalteredRules.Count); // O(1)
            processedRules = unalteredRules.Distinct(new Rule.RuleEqualityComparer()).ToList(); // O(2n)
            Console.WriteLine("After washing redundant rules: " + processedRules.Count); //O(1) or O(2)?
        }

        //Transforms records into more representative objects
        static List<Rule> LoadDataSet(List<string> dataRows)
        {
            Dedupe(ref dataRows);
            List<Rule> dataSet = new List<Rule>();

            for (int i = 0; i < dataRows.Count; i++)
            {
                dataSet.Add(new Rule(dataRows[i]));
            }

            return dataSet;
        }
        // If an exclusion field option is selected
        static List<Rule> LoadDataSet(List<string> dataRows,string[] exclusionFieldValues, string exclusionField)
        {
            Dedupe(ref dataRows);
            List<Rule> dataSet = new List<Rule>();

            for (int i = 0; i < dataRows.Count; i++)
            {
                dataSet.Add(new Rule(dataRows[i]));
            }
            foreach(string excludedData in exclusionFieldValues)
            {
                if(exclusionField == "rule_name")
                {
                    dataSet = dataSet.Where(rule => rule.rule_name != excludedData).ToList();
                }
                else if(exclusionField == "server")
                {
                    dataSet = dataSet.Where(rule => rule.server != excludedData).ToList();
                }
                else if(exclusionField == "port")
                {
                    dataSet = dataSet.Where(rule => rule.port != excludedData).ToList();
                }
                else
                {
                    Console.WriteLine("Invalid exclusion field, skipping step...");
                }
            }

            return dataSet;
        }

        // For option to have rules that reference only one server or port
        public void MergeRulesOnOnlyPortOrServer(string fieldOption)
        {
            int newRuleNumber = 1;

            if (fieldOption == "-ONEPORT")
            {
                var servers = processedRules.Select(e => e.server).Distinct();

                foreach (string server in servers)
                {
                    processedRules.FindAll(r => r.server == server).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                    newRuleNumber++;
                }
            }
            else if (fieldOption == "-ONESERVER")
            {
                var ports = processedRules.Select(e => e.port).Distinct();

                foreach (string port in ports)
                {
                    processedRules.FindAll(r => r.port == port).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                    newRuleNumber++;
                }
            }
        }

        // Goal here is reduce the number of rules
        public void ConsolidateServersAndPorts()
        {
            HashSet<Rule> oldRuleList = processedRules.ToHashSet();
            HashSet<Rule> newRuleList = new HashSet<Rule>();

            int newRuleNumber = 0;
            string currentKey;
            string type;

            while (oldRuleList.Any())
            {
                currentKey = getMostOccurringField(oldRuleList, out type);
                // We get which field we're working with so that we don't need to run collection.Any() in the loop, lower cost
                if (type == "server")
                {
                    newRuleNumber++;

                    newRuleList.UnionWith(oldRuleList.Where(rule => rule.server == currentKey)
                        .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port }));

                    oldRuleList.RemoveWhere(rule => rule.server == currentKey);
                }
                else if(type == "port")
                {
                    newRuleNumber++;

                    newRuleList.UnionWith(oldRuleList.Where(rule => rule.port == currentKey)
                        .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port }));

                    oldRuleList.RemoveWhere(rule => rule.port == currentKey);
                }
            }
            processedRules = newRuleList.ToList();
        } 

        //Below is high cost but using hashsets should alleviate it to a degree
        //Need to find highest grouped count each iteration as taking out one field takes from the other, the numbers shift
        private string getMostOccurringField(HashSet<Rule> ruleSet, out string type)
        {
            // Group by and get highest count for each field
            KeyValuePair<string, int> server = ruleSet.GroupBy(rule => rule.server)
                .OrderByDescending(group => group.Count())
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .First();

            KeyValuePair<string, int> port = ruleSet.GroupBy(rule => rule.port)
                .OrderByDescending(group => group.Count())
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .First();

            // TieBreaker is whichever field has less unique occurrences, results in less rules total
            if(server.Value > port.Value)
            {
                type = "server";
                return server.Key;
            }
            else if(port.Value > server.Value)
            {
                type = "port";
                return port.Key;
            }
            else
            {
                type = lessUniqueField;
                return (lessUniqueField == "server" ? server.Key : port.Key);
            }
        }

        // Allows options for different result sets by resetting and rerunning data operations, for possible GUI
        private void ResetData()
        {
            processedRules = unalteredRules;
        }
        // Called by outer process
        public void ProcessData(string loneFieldOption = "", string exclusionField = "", string exclusionValueList = "")
        {

            loneFieldOption = loneFieldOption.ToUpper();

            FilterRedundantRules();

            if (loneFieldOption != "" && (loneFieldOption == "-ONEPORT" || loneFieldOption == "-ONESERVER"))
            {
                MergeRulesOnOnlyPortOrServer(loneFieldOption);
            }
            else if(loneFieldOption != "")
            {
                Console.WriteLine("invalid -MergeField option, skipping.");
                ConsolidateServersAndPorts();
            }
            else
            {
                ConsolidateServersAndPorts();
            }
        }
    }
}
