using System;
using System.Globalization;
using System.Net.Http.Formatting;
using ThreewoodBatch.Constants;
using umbraco.BusinessLogic.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace ThreewoodBatch.Trees
{
    [Tree(TWBConstants.Application.Alias, TWBConstants.Tree.Alias, TWBConstants.Tree.Title)]
    [PluginController(TWBConstants.Controller.Alias)]
    public class TWBTreesController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            var textService = ApplicationContext.Services.TextService;


            nodes = new TreeNodeCollection
            {
                CreateTreeNode("phonebook-manager", id, queryStrings, textService.Localize("ThreewoodBatch/PhonebookManager.TreeSection", CultureInfo.CurrentCulture), "icon-old-phone", false),
                CreateTreeNode("news-manager", id, queryStrings, textService.Localize("ThreewoodBatch/NewsManager.TreeSection", CultureInfo.CurrentCulture), "icon-fa-newspaper-o", false)
            };

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;
            menu.Items.Add<ActionNew>("Create");
            return menu;            
        }
    }
}
