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
        public void MergeRedundantRules()
        {
            var groupedRecords = unalteredRules.GroupBy(row => row.serverList[0] + row.portList[0]).ToList();
            foreach(Rule rule in groupedRecords)
            {
                processedRules.Add(rule);
            }
            //Benchmark difference between having sorts and no sorts
            processedRules.Sort();
        }

        //Transforms records into useable objects
        static List<Rule> LoadDataSet(List<string> dataRows)
        {
            Dedupe(ref dataRows);

            List<Rule> dataSet = new List<Rule>();

            for (int i = 0; i < dataRows.Count; i++)
            {
                dataSet.Add(new Rule(dataRows[i]));
            }
            //Benchmark difference between having sorts and no sorts
            dataSet.Sort();

            return dataSet;
        }

        public void 

        // Allows options for different result sets by resetting data
        private void ResetData()
        {
            processedRules = unalteredRules;
        }


    }
}
