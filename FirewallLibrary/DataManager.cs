using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        // Gets rid of rules that are functionally redundant (same server and port)
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

        // For option to have rules that reference only one server or port
        public void MergeRulesOnOnlyPortOrServer(string field = "")
        {
            int newRuleNumber = 1;

            if (field == "server")
            {
                var servers = processedRules.Select(e => e.server).Distinct();

                foreach (string server in servers)
                {
                    processedRules.FindAll(r => r.server == server).ForEach(r => r.rule_name = "R-" + newRuleNumber);
                    newRuleNumber++;
                }
            }
            else if(field == "port")
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

            // Group on server and sort by number of times it appears
            var servers = oldRuleList.GroupBy(rule => rule.server)
                .Select(group => new { server = group.Key, count = group.Count() })
                .OrderByDescending(group => group.count);

            // Group on port and sort by number of times it appears
            var ports = oldRuleList.GroupBy(rule => rule.port)
                .Select(group => new { port = group.Key, count = group.Count() })
                .OrderByDescending(group => group.count);

#if DEBUG
            Console.WriteLine(ports.Count());
            Console.WriteLine(servers.Count());
#endif      

            //Set up variables for algorithm
            int portsCountMax = ports.Select(p => p.count).Max();
            int serversCountMax = servers.Select(s => s.count).Max();
            // Get outer loop starting point
            int outerStartPoint = (serversCountMax > portsCountMax ? serversCountMax : portsCountMax);

            List<string> currentPorts = new List<string>();
            List<string> currentServers = new List<string>();
            
            int newRuleNumber = 0;

            // Starts with the elements that occur the most and works downwards
            for (int i = outerStartPoint; i >= 0; i--) //O(n)
            {
                currentPorts = ports.Where(p => p.count == i).Select(e => e.port).ToList();//O(n3)
                currentServers = servers.Where(s => s.count == i).Select(s => s.server).ToList();//O(n3)

                foreach (string server in currentServers)
                {
                    if (oldRuleList.Any(rule => rule.server == server)) //O(n)
                    {
                        newRuleNumber++; //O(1)

                        newRuleList.UnionWith(oldRuleList.Where(rule => rule.server == server)
                            .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port })); //O(n3)

                        oldRuleList.RemoveWhere(rule => rule.server == server);//O(n2)
                    }
                }
                foreach (string port in currentPorts)
                {
                    if (oldRuleList.Any(rule => rule.port == port))
                    {
                        newRuleNumber++;

                        newRuleList.UnionWith(oldRuleList.Where(rule => rule.port == port)
                        .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port }));

                        oldRuleList.RemoveWhere(rule => rule.port == port);
                    }
                }
            }
            while (oldRuleList.Any())
            {
                foreach (string server in currentServers)
                {
                    if (oldRuleList.Any(rule => rule.server == server)) //O(n)
                    {
                        newRuleNumber++; //O(1)

                        newRuleList.UnionWith(oldRuleList.Where(rule => rule.server == server)
                            .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port })); //O(n3)

                        oldRuleList.RemoveWhere(rule => rule.server == server);//O(n2)
                    }
                }
                foreach (string port in currentPorts)
                {
                    if (oldRuleList.Any(rule => rule.port == port))
                    {
                        newRuleNumber++;

                        newRuleList.UnionWith(oldRuleList.Where(rule => rule.port == port)
                        .Select(rule => new Rule { rule_name = "R-" + newRuleNumber, server = rule.server, port = rule.port }));

                        oldRuleList.RemoveWhere(rule => rule.port == port);
                    }
                }
            }

            if (!oldRuleList.Any())
            {
                processedRules = newRuleList.ToList();
            }
            else
            {
                throw new Exception("Rules were missed, aborting.");
            }
        }

        /*
        public int CalculateMaxPorts(HashSet<Rule> oldRuleList, HashSet<Rule> newRuleList, ref int portsCount, ref int serversCount)
        {
            var servers = oldRuleList.GroupBy(rule => rule.server)
                .Select(group => new { server = group.Key, count = group.Count() })
                .OrderByDescending(group => group.count).ToList();

            var ports = oldRuleList.GroupBy(rule => rule.port)
                .Select(group => new { port = group.Key, count = group.Count() })
                .OrderByDescending(group => group.count).ToList();
        }
        
        public void AnyPortsReferencedByAllRules()
        {
            var removerules = unalteredRules.Distinct().ToList();

            int reqd = unalteredRules.Select(r => r.rule_name).Distinct().ToList().Count();

            List<string> ports = new List<string>();

            ports = removerules.GroupBy(p => p.port).Where(o => o.Count() == reqd).Select(e => e.Key).ToList();

            foreach(string port in ports)
            {
                Console.WriteLine(port);
                Console.ReadLine();
            }
        }
        */

        // Allows options for different result sets by resetting data, for possible GUI
        private void ResetData()
        {
            processedRules = unalteredRules;
        }

        public void ProcessData(string option = "")
        {
            FilterRedundantRules();
            // AnyPortsReferencedByAllRules();
            if (option.ToUpper() == "-ONEPORT")
            {
                MergeRulesOnOnlyPortOrServer("port");
            }
            else if(option.ToUpper() == "-ONESERVER")
            {
                MergeRulesOnOnlyPortOrServer("server");
            }
            else
            {
                ConsolidateServersAndPorts();
            }
        }
    }
}
