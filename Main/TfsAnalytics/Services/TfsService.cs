using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using Microsoft.TeamFoundation.Framework.Client;
//using Microsoft.TeamFoundation.Framework.Common;
//using Microsoft.TeamFoundation.VersionControl.Client;
//using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;

namespace TfsAnalytics.Services
{
    public class TfsService
    {
        public static TfsTeamProjectCollection Connect(string collectionUrl)
        {
            //ConnectByImplementingCredentialsProvider connect = new ConnectByImplementingCredentialsProvider();
            //ICredentials iCred = new NetworkCredential("mathiaso", "S3m3ster13", "transcendentgro");
            //connect.GetCredentials(new Uri(collectionUrl), iCred);

            //return TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(collectionUrl), connect);
            return TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(collectionUrl));
        }
    }
}