using System;
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

        public InformationInstance CreateInformationInstance(string id, List<Character> characters)
        {
            return new InformationInstance(informations[id], characters);
        }

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

            //Actions without any action logic can't be triggered in FindInformationForAction
            //so we can just use FALSE so they'll always be unavailable.
            ILogic actionLogic = Logic.FALSE;

            //Actions without a top level exec shouldn't do anything in their exec.
            IExecute dirExec = Execute.NOOP;
            List<InformationParameter> parameters = new List<InformationParameter>();
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
                    parameters = ReadParameters(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "information")
                {
                    break;
                }
            }
            return new Information(identifier, description, parameters.ToArray());
        }

        private List<InformationParameter> ReadParameters(XmlReader reader)
        {
            List<InformationParameter> parameters = new List<InformationParameter>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "parameter")
                {
                    string type = reader.GetAttribute("type");
                    string name = reader.ReadElementContentAsString();
                    parameters.Add(new InformationParameter(XmlHelper.StringToType(type), name));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "parameters")
                {
                    break;
                }
            }
            return parameters;
        }
    }
}
