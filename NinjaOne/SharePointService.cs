using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace NinjaOne.DataExtractUtility
{
    public class SharePointService
    {
        #region Properties
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        #endregion
        public void Execute(string listName, List<EntityField> entityFields, JToken myObject)
        {
            using (ClientContext ctx = new ClientContext(Url))
            {
                try
                {
                    SecureString securStr = new SecureString();
                    foreach (char ch in Password)
                    {
                        securStr.AppendChar(ch);
                    }
                    ctx.Credentials = new SharePointOnlineCredentials(UserName, securStr);
                    Web spWeb = ctx.Web;
                    ctx.Load(spWeb);
                    List spList = spWeb.Lists.GetByTitle(listName);
                    ListItemCollection listItems = spList.GetItems(CamlQuery.CreateAllItemsQuery());
                    ctx.Load(listItems,
                                        eachItem => eachItem.Include(
                                        item => item,
                                        item => item["ID"]));
                    ctx.ExecuteQuery();

                    var totalListItems = listItems.Count;
                    Console.WriteLine("Deletion in " + listName + " list:");
                    if (totalListItems > 0)
                    {
                        for (var counter = totalListItems - 1; counter > -1; counter--)
                        {
                            listItems[counter].DeleteObject();
                            ctx.ExecuteQuery();
                            Console.WriteLine("Row: " + counter + " Item Deleted");
                        }
                    }

                    foreach (var data in myObject)
                    {
                        try
                        {
                            ListItemCreationInformation spItemInfo = new ListItemCreationInformation();
                            ListItem spItem = spList.AddItem(spItemInfo);
                            foreach(EntityField field in entityFields)
                            {
                                spItem[field.Target] = ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JObject)data).SelectToken(field.Source)).Value;
                            }
                            spItem.Update();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error:- " + ex.ToString());
                        }
                    }
                    ctx.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:- " + ex.ToString());
                }
            }
        }
    }
}
