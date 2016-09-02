using System.Linq;
using Itinero.Profiles;

namespace Itinero.API.Helpers
{
    public static class ProfileHelper
    {
        public static bool TryGetProfile(string profileName, out Profile profile)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                profile = Profile.GetAllRegistered().OrderBy(p => p.Name).First();
            }
            else if (!Profile.TryGet(profileName, out profile))
            {
                return false;
            }
            return true;
        }

    }
}
