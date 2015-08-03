﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    class InformationManager
    {
        private Dictionary<string, Information> informations = new Dictionary<string, Information>();

        public void LoadInformationsFromFile(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "informations")
                    {
                        ReadInformations(reader);
                    }
                }
            }
        }

        private void ReadInformations(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "information")
                {
                    Information info = ReadInformation(reader);
                    informations.Add(info.Identifier, info);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "informations")
                {
                    break;
                }
            }
        }

        private Information ReadInformation(XmlReader reader)
        {
            string identifier = null;
            string description = null;
            IExecute onObserve = Execute.NOOP;
            IExecute onTold = Execute.NOOP;
            List<Parameter> parameters = new List<Parameter>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString().Trim();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameters")
                {
                    parameters = XmlHelper.ReadParameters(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "on_observe")
                {
                    onObserve = XmlHelper.ReadExecute(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "on_told")
                {
                    onTold = XmlHelper.ReadExecute(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "information")
                {
                    break;
                }
            }
            return new Information(identifier, description, parameters.ToArray(), onObserve, onTold);
        }


        public Information GetInformationById(string id)
        {
            return informations[id];
        }
    }
}
