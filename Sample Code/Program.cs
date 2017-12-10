using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MC.Clio;
using MC.Clio.Wpf;

namespace MC.Clio.Samples {
    public static class Program {
        /*
        NOTE:  Because we're using the CEF auth provider, we need to add the following info to our CSProj.
        To do that, right-click on our project file, unload it, then right-click and edit it.  Add the following 
        above all other PropertyGroups:

        <PropertyGroup>
            <CefSharpAnyCpuSupport>true</CefSharpAnyCpuSupport>
        </PropertyGroup>
             
        */

        /*
        In order to get Async main, right-click on the project, select Properties > Build > Advanced and choose
        "C# Lastest Minor Version (Latest)" as your C# version.
        */
        public static async Task Main(string[] args) {

            //Make sure you update your app info!
            var App = new ClioAppInfo();

            var ClioClient = new CachedApiClient(App);

            Console.WriteLine("About to authenticate...");
            var Authenticated = await ClioClient.Authenticate<WpfCefAuthenticationProvider>()
                .DefaultAwait()
                ;

            if (!Authenticated.IsSuccess) {
                return;
            }

            Console.WriteLine("Downloading a list of all Matters...");
            await ClioClient.Matters.Refresh()
                .DefaultAwait()
                ;


            Console.WriteLine("Downloading a list of all Contacts...");
            await ClioClient.Contacts.Refresh()
                .DefaultAwait()
                ;

            var OpenMatters =
                (
                //Hi-Performance, Cached WHERE:
                from Matter in ClioClient.Matters.Where(x => x.Status == MatterStatus.Open)
                //Standard-Performance WHERE:
                where Matter.Billing_Method == BillingMethodType.Hourly                     
                select new {
                    Description = Matter.Description,
                    MatterNumber = Matter.Display_Number,
                    ClientID = Matter.Client?.ID,
                }).ToList();


            //Find all duplicate contacts
            var DuplicateContacts =
                (
                from Contact in ClioClient.Contacts.List()
                orderby Contact.ID ascending // The one with the lowest ID was created first.
                group Contact by Contact.Name.ToLower() into Group //Group by the name.
                let GroupItems = Group.ToList()
                where GroupItems.Count >= 2 // Duplicates only exist if >= 2 items in the group.
                select GroupItems
                ).ToList();

            foreach (var DuplicateContact in DuplicateContacts) {
                
                var NewClient = DuplicateContact.First(); 
                var OtherContactIds = (from M in DuplicateContact.Skip(1) select M?.ID).ToArray();

                Console.WriteLine($"{NewClient.Name} exists multiple times.");
                Console.WriteLine($"\t Contact #{NewClient.ID} will be preserved and the others will be deleted..");

                //Get All Matters that don't have the contact ID of our first contact.
                var MattersToUpdate =(
                    from Matter in ClioClient.Matters.Where(x => OtherContactIds.Contains(x.Client))
                    select Matter
                    ).ToList();

                //Update the matter to have the new client.
                foreach (var Matter in MattersToUpdate) {
                    Console.WriteLine($"\t Matter #{Matter.ID} is being updated to use Client# {NewClient.ID}");
                    await ClioClient.Matters.Update(Matter.ID, new UpdateMatterClientIdCommand() {
                        ClientId = NewClient.ID,
                    }).DefaultAwait();
                }

                //Delete the duplicates
                foreach (var ContactId in OtherContactIds) {
                    Console.WriteLine($"\t Contact #{ContactId} is being deleted since it was a duplicate.");
                    await ClioClient.Contacts.Delete(ContactId.Value)
                        .DefaultAwait()
                        ;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Application Complete");


            //Work around an issue with Async processes.
            System.Environment.Exit(0);
        }

    }
}
