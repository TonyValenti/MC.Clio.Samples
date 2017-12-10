using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC.Clio.Samples {
    class ClioAppInfo : MC.Clio.ApiClientApplicationInfo {
        public ClioAppInfo() {
            this.FriendlyName = "My Clio App";
            this.Key = "xxxxxxxxx";
            this.Secret = "xxxxxxxxx";
            //Right now Clio uses different sets of API keys for North America and Europe.
            //You need to tell it what place you're connecting to.
            this.EndpointUrl = "https://app.Clio.com/";
        }
    }

}
