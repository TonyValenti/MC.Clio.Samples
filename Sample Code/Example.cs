using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MC.Clio;
using MC.Clio.Wpf;

namespace MC.Clio.Wpf.Sample {
    public static class Application {

        public static async Task<int> Main(string[] args) {

            var App = new MC.Clio.ApiClientApplicationInfo() {
                FriendlyName = "My Clio App",
                Key = "xxxxxxxxxxxxxx", Secret = "xxxxxxxxxxxxxx"
            };

            var ClioClient = new CachedApiClient(App, new WpfAuthenticationProvider());
            var Authenticated = ClioClient.Authenticate();

            await ClioClient.Matters.Refresh();

            var OpenMatters =
                //Hi-Performance, Cached WHERE:
                from Matter in ClioClient.Matters.Where(x => x.Status == MatterStatus.Open)
                //Standard-Performance WHERE:
                where Matter.Billing_Method == BillingMethodType.Hourly                     
                select new {
                    Description = Matter.Description,
                    MatterNumber = Matter.Display_Number,
                    ClientID = Matter.Client?.ID,
                    PracticeAreaId = Matter.Practice_Area.ID
                };


            //This 

            //Find all duplicate contacts
            var DuplicateContacts =
                from Contact in ClioClient.Contacts.List()
                orderby Contact.ID ascending // The one with the lowest ID was created first.
                group Contact by Contact.Name.ToLower() into Group //Group by the name.
                let GroupItems = Group.ToList()
                where GroupItems.Count >= 2 // Duplicates only exist if >= 2 items in the group.
                select GroupItems;

            foreach (var DuplicateContact in DuplicateContacts) {
                var NewClient = DuplicateContact.First(); 
                var OtherContactIds = (from M in DuplicateContact.Skip(1) select M?.ID).ToArray();
                //Get All Matters that don't have the contact ID of our first contact.
                var MattersToUpdate =
                    from Matter in ClioClient.Matters.Where(x => OtherContactIds.Contains(x.Client))
                    select Matter;

                //Update the matter to have the new client.
                foreach (var Matter in MattersToUpdate) {
                    await ClioClient.Matters.Update(Matter.ID, new MatterCreateCommand() {
                        ClientId = NewClient.ID,
                    });
                }

                //Delete the duplicates
                foreach (var ContactId in OtherContactIds) {
                    await ClioClient.Contacts.Delete(ContactId.Value);
                }
            }



          


            

            return 0;
        }



        


    }
}
