using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class MetadataEntity
    {
        public MetadataEntity()
        {
            Metadata = new List<Metadata>();
        }

        public virtual ICollection<Metadata> Metadata { get; set; }

        public void SetMetadata(string key, string value)
        {
            var data = Metadata.SingleOrDefault(x => x.Key == key);
            if (data != null)
            {
                data.Value = value;
                return;
            }

            var metadata = new Metadata
            {
                Key = key,
                Value = value
            };

            Metadata.Add(metadata);
        }

        public string GetMetadata(string key)
        {
            var metadata = Metadata.SingleOrDefault(x => x.Key == key);
            return metadata?.Value;
        }
    }
}