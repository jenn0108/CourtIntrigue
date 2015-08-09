﻿using System;
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
        public bool Unique { get; private set; }
        public ILogic Requirements { get; private set; }
        public string OnHire { get; private set; }
        public string OnFire { get; private set; }

        public Job(string id, string label, string description, bool unique, ILogic requirements, string onHire, string onFire)
        {
            Identifier = id;
            Label = label;
            Description = description;
            Unique = unique;
            Requirements = requirements;
            OnHire = onHire;
            OnFire = onFire;
        }


        public bool CanPerformJob(Character c)
        {
            EventContext context = new EventContext(null, c, null);
            return Requirements.Evaluate(context);
        }
    }

    class JobManager
    {
        private Dictionary<string, Job> normalJobs = new Dictionary<string, Job>();
        private Dictionary<Job, Character> uniqueJobs = new Dictionary<Job, Character>();

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
                    if (job.Unique)
                        uniqueJobs.Add(job, null);
                    else
                        normalJobs.Add(job.Identifier, job);
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
            bool unique = false;
            ILogic requirements = Logic.TRUE;
            string onHire = null;
            string onFire = null;
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
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "unique")
                {
                    unique = true;
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "on_hire")
                {
                    onHire = reader.ReadElementContentAsString().Trim();
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "on_fire")
                {
                    onFire = reader.ReadElementContentAsString().Trim();
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "job")
                {
                    break;
                }
            }
            return new Job(identifier, label, description, unique, requirements, onHire, onFire);
        }

        public void InitializeJobs(List<Character> characters, Game game)
        {
            foreach(var c in characters)
            {
                foreach(var pair in uniqueJobs)
                {
                    if(pair.Value == null && pair.Key.Requirements.Evaluate(new EventContext(null, c, null)))
                    {
                        GiveJobTo(pair.Key, c, game);
                        break;
                    }
                }
            }
        }

        public Job GetJobById(string id)
        {
            if(normalJobs.ContainsKey(id))
                return normalJobs[id];

            foreach(var pair in uniqueJobs)
            {
                if (pair.Key.Identifier == id)
                    return pair.Key;
            }

            throw new KeyNotFoundException("Job id " + id + " not found.");
        }

        public void GiveJobTo(Job job, Character newHolder, Game game)
        {
            if(job.Unique)
            {
                Character oldHolder = uniqueJobs[job];

                if(oldHolder != null)
                {
                    oldHolder.FireFromJob(job);

                    Event fireEvent = game.GetEventById(job.OnFire);
                    if (fireEvent != null)
                    {
                        EventContext fireContext = new EventContext(null, oldHolder, null);
                        fireEvent.Execute(new EventResults(), game, fireContext);
                    }
                }

                if(newHolder != null)
                {
                    newHolder.GiveJob(job);

                    Event hireEvent = game.GetEventById(job.OnHire);
                    if (hireEvent != null)
                    {
                        EventContext hireContext = new EventContext(null, newHolder, null);
                        hireEvent.Execute(new EventResults(), game, hireContext);
                    }
                }

                uniqueJobs[job] = newHolder;
            }
            else
            {
                if(newHolder != null)
                {
                    newHolder.GiveJob(job);

                    Event hireEvent = game.GetEventById(job.OnHire);
                    if (hireEvent != null)
                    {
                        EventContext hireContext = new EventContext(null, newHolder, null);
                        hireEvent.Execute(new EventResults(), game, hireContext);
                    }
                }
            }
        }
    }
}
