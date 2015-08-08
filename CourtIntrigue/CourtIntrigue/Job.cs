using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CourtIntrigue
{
    class Job
    {
        public string Identifier { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public ILogic Requirements { get; private set; }

        public Job(string id, string label, string description, ILogic requirements)
        {
            Identifier = id;
            Label = label;
            Description = description;
            Requirements = requirements;
        }


        public bool CanPerformJob(Character c)
        {
            EventContext context = new EventContext(null, c, null);
            return Requirements.Evaluate(context);
        }
    }

    class JobManager
    {
        private Dictionary<string, Job> jobs = new Dictionary<string, Job>();

        public void LoadJobsFromFile(string filename, Dictionary<string, int> bagTags)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "jobs")
                    {
                        ReadJobs(reader, bagTags);
                    }
                }
            }
        }

        private void ReadJobs(XmlReader reader, Dictionary<string, int> badTags)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "job")
                {
                    Job job = ReadJob(reader, badTags);
                    jobs.Add(job.Identifier, job);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "jobs")
                {
                    break;
                }
            }
        }

        private Job ReadJob(XmlReader reader, Dictionary<string, int> badTags)
        {
            string identifier = null;
            string description = null;
            string label = null;
            ILogic requirements = Logic.TRUE;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "id")
                {
                    identifier = reader.ReadElementContentAsString();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "label")
                {
                    label = reader.ReadElementContentAsString().Trim();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "description")
                {
                    description = reader.ReadElementContentAsString().Trim();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "information")
                {
                    break;
                }
            }
            return new Job(identifier, label, description, requirements);
        }


        public Job GetJobById(string id)
        {
            return jobs[id];
        }
    }
}
