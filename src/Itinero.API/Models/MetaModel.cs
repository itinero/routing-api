using System.Collections.Generic;
using Itinero.Attributes;

namespace Itinero.API.Models
{
    public class MetaModel
    {
        public MetaModel(
            string id, 
            IAttributeCollection meta, 
            IEnumerable<string> profiles, 
            IEnumerable<string> contracted)
        {
            Id = id;
            Meta = meta;
            Profiles = profiles;
            Contracted = contracted;
        }

        public string Id { get; }

        public IAttributeCollection Meta { get; }

        public IEnumerable<string> Profiles { get; }

        public IEnumerable<string> Contracted { get; }

    }
}
