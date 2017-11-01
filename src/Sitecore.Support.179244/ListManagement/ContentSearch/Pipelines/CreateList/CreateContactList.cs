using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.ListManagement;
using Sitecore.ListManagement.ContentSearch.Model;
using Sitecore.ListManagement.ContentSearch.Pipelines;

namespace Sitecore.Support.ListManagement.ContentSearch.Pipelines.CreateList
{
  public class CreateContactList : ListProcessor
  {
    public virtual void Process(ListArgs args)
    {
      ID newID;
      Assert.ArgumentNotNull(args, "args");
      ContactList contactList = args.ContactList;
      if (ID.IsID(contactList.Id))
      {
        newID = ID.Parse(contactList.Id);
      }
      else
      {
        newID = ID.NewID;
        contactList.Id = newID.ToString();
      }
      ID templateId = base.TemplateId;
      if ((contactList is ISegmentedList) || (contactList.Type == "SegmentedList"))
      {
        templateId = base.SegmentedListTemplateId;
      }
      Item destinationItem = this.GetDestinationItem(contactList);
      if (destinationItem == null)
      {
        Item item = base.Database.GetItem(base.RootId);
        if ((contactList.Destination != null) && (item != null))
        {
          string path = FileUtil.MakePath(item.Paths.FullPath, contactList.Destination);
          destinationItem = base.Database.GetItem(path);
        }
        if (destinationItem == null)
        {
          destinationItem = item;
        }
      }
      Assert.IsNotNull(destinationItem, "Root item is not found.");
      this.AssertDestination(destinationItem, args.ContactList.Destination);
      Item target = ItemManager.AddFromTemplate(newID.ToShortID().ToString(), templateId, destinationItem, newID);
      this.MapItemFromModel(contactList, target);

      // Sitecore Support Fix #179244
      try
      {
        var languages = LanguageManager.GetLanguages(base.Database);
        foreach (var lang in languages)
        {
          using (new LanguageSwitcher(lang))
          {
            var itemInCurrentLanguage = base.Database.GetItem(target.ID);
            if (itemInCurrentLanguage.Versions.Count == 0)
            {
              itemInCurrentLanguage.Versions.AddVersion();
              base.MapItemFromModel(contactList, itemInCurrentLanguage);
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("An error occurs during executing patch #179244", this);
      }
      // Sitecore Support Fix #179244
    }

  }
}