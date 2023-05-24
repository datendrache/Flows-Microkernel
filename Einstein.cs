//   Flows Microkernel -- Complex Event Processing Kernel
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Proliferation.Flows;
using System.Text.RegularExpressions;
using System.Runtime.ExceptionServices;
using Proliferation.Fatum;
using Proliferation.LanguageAdapters;

namespace Proliferation.Flows.Microkernel
{
    public class Einstein
    {
        public MasterStreamProcessor MSP = null;
        public CollectionState State = null;

        public Einstein(CollectionState state, MasterStreamProcessor msp)
        {
            State = state;
            MSP = msp;
        }

        [HandleProcessCorruptedStateExceptions]
        public void processDocument(BaseFlow FLOW, BaseDocument document, CollectionState State)
        {
            Match found = null;
            string rulematched = "";
            Regex regexmatched = null;

            try
            {
                // Step 1:  Locate which rules we need to process

                FlowReference currentReference = (FlowReference)FLOW.flowReference;
                
                if (currentReference != null)
                {
                    // Step 2:  Loop through rules until we find a match

                    Match match = null;

                    if (currentReference.Rules.Count == 0)
                    {
                        System.Console.Out.WriteLine("Warning: Document detected without a source rule <" + document.FlowID + ">: " + document.Document);
                    }
                    else
                    {
                        for (int i = 0; i < currentReference.Rules.Count; i++)
                        {
                            try
                            {
                                BaseRule currentRule = (BaseRule)currentReference.Rules[i];
                                match = currentRule.RuleRegex.Match(document.Document);
                                if (match.Success)
                                {
                                    // We need to pass this to each of the processors
                                    rulematched = currentRule.RuleName;
                                    regexmatched = currentRule.RuleRegex;
                                    document.Label = currentRule.DefaultLabel;
                                    document.Category = currentRule.DefaultCategory;
                                    document.triggeredRule = currentRule;
                                    if (currentRule.Parameter!=null)
                                    {
                                        Tree.MergeNode(currentRule.Parameter.ExtractedMetadata, document.Metadata);
                                    }
                                    found = match;
                                    break;
                                }
                            }
                            catch (Exception xyz)
                            {
                                TextLog.Log(State.fatumConfig.DBDirectory + "\\system.log", DateTime.Now.ToString() + ":" + xyz.Message + "\r\nStack Trace:\r\n" + xyz.StackTrace);
                            }
                        }

                        // Step 3:  Loop through processesors, sending each one the document

                        if (currentReference.Processors.Count > 0)
                        {
                            for (int i = 0; i < currentReference.Processors.Count; i++)
                            {
                                IntLanguage application = (IntLanguage)currentReference.Workspaces[i];

                                // Populate variables with match tag items

                                if (match != null)
                                {
                                    GroupCollection groups = match.Groups;
                                    foreach (string groupName in regexmatched.GetGroupNames())
                                    {
                                        if (groupName != "0")
                                        {
                                            if (groups[groupName].Value != "")
                                            {
                                                if (match.Captures != null)
                                                {
                                                    application.addVariable(groupName.ToLower(), groups[groupName].Value);
                                                }
                                            }
                                        }
                                    }
                                }
                                string executionOutput = "";

                                application.addVariable("documenttext", document.Document);
                                application.addVariable("flowid", document.assignedFlow.UniqueID);
                                application.addVariable("flowname", document.assignedFlow.FlowName);
                                application.addVariable("flowsource", document.assignedFlow.ParentService.ParentSource.SourceName);
                                application.addVariable("flowservice", document.assignedFlow.ParentService.ServiceType);
                                application.addVariable("documentcategory", document.Category);
                                application.addVariable("documentlabel", document.Label);
                                application.addVariable("documentmetadata", document.Metadata);
                                application.document(out executionOutput);
                                
                                regexmatched = null;
                                match = null;
                            }
                        }
                    }
                }
                else
                {
                    // If a message arrives that has processing enabled, but no processors, its disabled to prevent future heavy looping.
                    FLOW.ProcessingEnabled = false;
                    MSP.errorMessage("Einstein", "Error", "Document arrived without defined FlowReference " + document.FlowID + ", Text: " + document.Document);
                }
            }
            catch (Exception xyz)
            {
                MSP.errorMessage("Einstein", "Error", FLOW.ParentService.ServiceType + "." + FLOW.ParentService.ServiceSubtype + "." + FLOW.ParentService.ParentSource.SourceName + "." + FLOW.FlowName + "\r\nMessage: " +
                                   xyz.Message + "\r\n" + "Stack trace:\r\n" + xyz.StackTrace + "\r\nDocument: \r\n" + document.Document);
            }
        }
    }
}
