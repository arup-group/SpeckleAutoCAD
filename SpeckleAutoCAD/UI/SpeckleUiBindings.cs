﻿extern alias SpeckleNewtonsoft;
using SNJ = SpeckleNewtonsoft.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using SpeckleUiBase;

namespace SpeckleAutoCAD.UI
{
  public partial class SpeckleUIBindingsAutoCAD : SpeckleUIBindings
    {

    public SpeckleUIBindingsAutoCAD(CivilDocument _doc) : base()
    {
      SpeckleCore.SpeckleInitializer.Initialize();
      SpeckleCore.LocalContext.Init();
    }

    /// <summary>
    /// Sends an event to the UI, bound to the global EventBus.
    /// </summary>
    /// <param name="eventName">The event's name.</param>
    /// <param name="eventInfo">The event args, which will be serialised to a string.</param>
    public void NotifyUi( string eventName, dynamic eventInfo )
    {
      var script = string.Format( "window.EventBus.$emit('{0}', {1})", eventName, SNJ.JsonConvert.SerializeObject( eventInfo ) );

    }

    /// <summary>
    /// Dispatches a store action directly. Please note, this will not work for any action.
    /// </summary>
    /// <param name="storeActionName"></param>
    /// <param name="args"></param>
    public void DispatchStoreActionUi( string storeActionName, string args = null )
    {
      var script = string.Format( "window.Store.dispatch('{0}', '{1}')", storeActionName, args );

    }

    /// <summary>
    /// Pops open the dev tools.
    /// </summary>
    public void ShowDev()
    {
 //     Browser.ShowDevTools();
    }

    /// <summary>
    /// Gets the current accounts.
    /// </summary>
    /// <returns></returns>
    public string GetAccounts()
    {
      return SNJ.JsonConvert.SerializeObject( SpeckleCore.LocalContext.GetAllAccounts() );
    }

    public abstract string GetApplicationHostName();
    public abstract string GetFileName();
    public abstract string GetDocumentId();
    public abstract string GetDocumentLocation();

    /// <summary>
    /// Returns the serialised clients present in the current open host file.
    /// </summary>
    /// <returns></returns>
    public abstract string GetFileClients();

    /// <summary>
    /// Adds a sender and persits the info to the host file
    /// </summary>
    public abstract void AddSender( string args );

    /// <summary>
    /// Adds the current selection to the provided client.
    /// </summary>
    public abstract void AddSelectionToSender( string args );

    /// <summary>
    /// Removes the current selection from the provided client.
    /// </summary>
    /// <param name="args"></param>
    public abstract void RemoveSelectionFromSender( string args );

    /// <summary>
    /// Adds a receiver and persits the info to the host file
    /// </summary>
    public abstract void AddReceiver( string args );

    /// <summary>
    /// Removes a client from the file and persists the info to the host file.
    /// </summary>
    /// <param name="args"></param>
    public abstract void RemoveClient( string args );

    /// <summary>
    /// Bakes the specified client in the host file.
    /// </summary>
    /// <param name="args"></param>
    public abstract void BakeReceiver( string args );

    public abstract void UpdateSender( string args );

    // TODO: See how we go about this
    public abstract void AddObjectsToSender( string args );
    public abstract void RemoveObjectsFromSender( string args );
    
    /// <summary>
    /// clients should be able to select/preview/hover one way or another their associated objects
    /// </summary>
    /// <param name="args"></param>
    public abstract void SelectClientObjects( string args );
  }
}
