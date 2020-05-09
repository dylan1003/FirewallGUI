using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FirewallLibrary
{
    // Serves as a controller and container data being processed
    public class DataManager
    {
        // Holds the rules in their original state
        private List<Rule> unalteredRules { get; set; }
        // Holds the rules in their current state
        private List<Rule> processedRules { get; set; }

        // Getter for the original rules
        public List<Rule> OriginalRules
        {
            get { return unalteredRules; }
        }
        // Getter for processed rule list
        public List<Rule> ProcessedRules
        {
            get { return processedRules; }
        }

        public int GetOriginalRulesCount()
        {
            return unalteredRules.Count;
        }

        public int GetProcessedRulesCount()
        {
            return processedRules.Count;
        }

        // Constructor for 
        public DataManager(List<string> inputData)
        {
            unalteredRules = LoadDataSet(inputData);
        }

        // Flat dedupe of data
        private static void Dedupe(ref List<string> dedupedData)
        {
            dedupedData = dedupedData.Distinct().ToList();
        }

        // Groups rules that are functionally redundant (same server and port)
        public void FilterRedundantRules()
        {
            Console.WriteLine("Total rules: " + unalteredRules.Count);
            processedRules = unalteredRules.Distinct(new Rule.RuleEqualityComparer()).ToList();
            //Benchmark difference between having sorts and no sorts
            Console.WriteLine("New total rules: " + processedRules.Count);
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

        public void MergeRulesOnServer()
        {
            var servers = processedRules.Select(e => e.server).Distinct();

            int newRuleNumber = 1;

            foreach (string server in servers)
            {
                processedRules.FindAll(r => r.server == server).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                newRuleNumber++;
            }
        }


        public void ConsolidateServersAndPorts()
        {
            var servers = processedRules.Select(e => e.server).Distinct();
            var ports = processedRules.Select(e => e.port).Distinct();
            int newRuleNumber = 1;

            if (ports.Count() > servers.Count())
            {
                Console.WriteLine("Went with ports");
                foreach (string port in ports)
                {
                    processedRules.FindAll(r => r.port == port).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                    newRuleNumber++;
                }
            }
            else
            {
                Console.WriteLine("Went with servers");
                foreach (string server in servers)
                {
                    processedRules.FindAll(r => r.server == server).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                    newRuleNumber++;
                }
            }
        }

        public void MergeRulesOnPort()
        {
            var ports = processedRules.Select(e => e.port).Distinct();

            int newRuleNumber = 1;

            foreach (string port in ports)
            {
                processedRules.FindAll(r => r.port == port).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                newRuleNumber++;
            }
        }

        // Allows options for different result sets by resetting data
        private void ResetData()
        {
            processedRules = unalteredRules;
        }

        public void MainDataManipulation()
        {
            FilterRedundantRules();
            ConsolidateServersAndPorts();
            //MergeRulesOnPort();
            //MergeRulesOnServer();
            processedRules.OrderBy(row => row.port);
        }
    }
}
