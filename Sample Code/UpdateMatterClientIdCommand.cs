using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC.Clio.Samples {
    class UpdateMatterClientIdCommand : IMatterCommand {
        public long ClientId { get; set; }

        public ApiCommand ToContent() {
            return ServiceCommand.CreateNew()
                .AddProperty("client.id", ClientId)
                ;
        }
    }
}
