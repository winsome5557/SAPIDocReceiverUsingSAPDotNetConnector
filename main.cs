//Paul Massen says:Ok here goes ...

//The sample code attached has been copied out of my prototype app,
  but you can paste into a Console App or architect however you wish. 

//First you need to create a class that implements IDestinationConfiguration:


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using SAP.Middleware;
using SAP.Middleware.Connector;


namespace YourCompany.SAP.Connector
{
    /// <summary>
    /// Encapsulates SAP Destination Configuration management. This
      is a singleton process.
    /// </summary>
    public class SAPIDocDestinationConfiguration :  IDestinationConfiguration
    {
        private string _destinationName = string.Empty;

        // holds all configured destinations
        static Dictionary<string, RfcConfigParameters> availableDestinations=null;

        // event fired when configuration is changed
        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="locationConfigDom">Configuration connection
          parameters</param>
        public SAPIDocDestinationConfiguration(String destinationName,XmlDocument locationConfigDom)
        {
            if (availableDestinations==null)
                availableDestinations = new Dictionary<string, RfcConfigParameters>();
            
            _destinationName = destinationName;
            UpdateParameters(locationConfigDom);
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="locationConfigDom">Configuration connection
          parameters</param>
        public SAPIDocDestinationConfiguration(String destinationName)
        {
            availableDestinations = new Dictionary<string, RfcConfigParameters>();
            AddParameters(destinationName);
        }

        /// <summary>
        /// Overloaded method called by constructor to add
        /// hard0-coded values to RfcConfigparams
        /// </summary>
        /// <param name="destinationName">The destination name used
          in repositiry</param>
        /// <returns>The collection of RFC parameters</returns>
        private void AddParameters(String destinationName)
        {
            if ("SAP_IDOC_DESTINATION_CONFIG".Equals(destinationName))
            {
                RfcConfigParameters parameters = new RfcConfigParameters();
                parameters.Add(RfcConfigParameters.AppServerHost, "********");
                parameters.Add(RfcConfigParameters.Client, "***");
                parameters.Add(RfcConfigParameters.Language, "EN");
                parameters.Add(RfcConfigParameters.MaxPoolSize, "10");
                parameters.Add(RfcConfigParameters.MaxPoolWaitTime, "300");
                parameters.Add(RfcConfigParameters.MessageServerHost, "*******");
                parameters.Add(RfcConfigParameters.SystemID, "DR3");
                parameters.Add(RfcConfigParameters.SystemNumber, "00");

                parameters.Add(RfcConfigParameters.User, "biztalk");
                parameters.Add(RfcConfigParameters.Password, "********");
                        
                RfcConfigParameters existingConfiguration;

                //if a destination of that name existed before, we need to fire
                  a change event
                if (availableDestinations.TryGetValue(destinationName, out existingConfiguration))
                {
                    availableDestinations[destinationName] = parameters;
                    RfcConfigurationEventArgs eventArgs = new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, parameters);
                    
                    System.Diagnostics.Trace.WriteLine("Fire change event " + eventArgs.ToString() + " for destination " + destinationName);

                    // fire changed event
                    if (ConfigurationChanged != null)
                        ConfigurationChanged(destinationName, eventArgs);
                }
                else
                {
                    availableDestinations[destinationName] = parameters;
                }
             
            }
           
        }


        /// <summary>
        /// Called by constructor, adds Rfc params contained in
        /// Xml document.
        /// </summary>
        /// <param name="destinationName">The destination name used
          in repositiry</param>
        /// <returns>The collection of RFC parameters</returns>
        public void UpdateParameters(XmlDocument configDOM)
        {
           
            RfcConfigParameters parameters = new RfcConfigParameters();

            parameters.Add(RfcConfigParameters.AppServerHost, SAPIDocUtils.Extract(configDOM, "/Config/appServerHost").ToString());
            parameters.Add(RfcConfigParameters.Client, SAPIDocUtils.Extract(configDOM, "/Config/client").ToString());
            parameters.Add(RfcConfigParameters.Language, SAPIDocUtils.Extract(configDOM, "/Config/language").ToString());
            parameters.Add(RfcConfigParameters.MaxPoolSize, SAPIDocUtils.Extract(configDOM, "/Config/maxPoolSize").ToString());
            parameters.Add(RfcConfigParameters.MaxPoolWaitTime, SAPIDocUtils.Extract(configDOM, "/Config/maxPoolSizeWaitTime").ToString());
            parameters.Add(RfcConfigParameters.MessageServerHost, SAPIDocUtils.Extract(configDOM, "/Config/messageServerHost").ToString());
            //parameters.Add(RfcConfigParameters.PoolSize, SAPIDocUtils.Extract(configDOM,
              "/Config/poolSize"));
            parameters.Add(RfcConfigParameters.SystemID, SAPIDocUtils.Extract(configDOM, "/Config/systemID").ToString());
            parameters.Add(RfcConfigParameters.SystemNumber, SAPIDocUtils.Extract(configDOM, "/Config/systemNumber").ToString());

            parameters.Add(RfcConfigParameters.User, SAPIDocUtils.Extract(configDOM, "/Config/username"));
            parameters.Add(RfcConfigParameters.Password, SAPIDocUtils.Extract(configDOM, "/Config/password"));
            
            parameters.Add(RfcConfigParameters.RepositoryDestination,_destinationName);

            RfcConfigParameters existingConfiguration;
      
            //if a destination of that name existed before, we need to fire
              a change event
            if (availableDestinations.TryGetValue(_destinationName, out existingConfiguration))
            {
                availableDestinations[_destinationName] = parameters;
                RfcConfigurationEventArgs eventArgs = new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, parameters);
                
                System.Diagnostics.Trace.WriteLine("Fire change event " + eventArgs.ToString() + " for destination " + _destinationName);
                
                // fire changed event
                if (ConfigurationChanged != null)
                    ConfigurationChanged(_destinationName, eventArgs);
            }
            else
            {
                availableDestinations[_destinationName] = parameters;
            }

        }
        /// <summary>
        /// Implemented from IDestinationConfiguration,returns populated
        
        /// Rfc parameters collection to RFC destination manager
        /// </summary>
        /// <param name="destinationName">The destination name used
          in repositiry</param>
        /// <returns>The collection of RFC parameters</returns>
        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters foundDestination;

            if (availableDestinations != null)
            {
                availableDestinations.TryGetValue(destinationName, out foundDestination);

                if (foundDestination == null)
                    throw new ApplicationException(string.Format("Destination:{0} do not exist
                        in Repository", destinationName));
            }
            else
            {
                throw new ApplicationException(string.Format("Destination:{0} do not exist
                    in Repository",destinationName));
            }
            
            return foundDestination;
        }


        /// <summary>
        /// Informs RFC Destination Manager that class supprts changes
        /// to Rfc configuration parameters 
        /// </summary>
        /// <returns></returns>
        public bool ChangeEventsSupported()
        {
            return true;
        }


    }
   
}


//I am using two overloaded constructors, one for using hard-coded
  values the second for an Xml file. 

//Next for receiving IDocs you need to create a class that implments
  IServerConfiguration.Again I am using to overloaded constructors for either hard-coded
  or Xml config.


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using SAP.Middleware;
using SAP.Middleware.Connector;


namespace YourCompany.SAP.Connector
{
    /// <summary>
    /// Encapsulates connection functionality using NCO 3.
    /// </summary>
    public  class SAPIDocReceiveConfiguration : IServerConfiguration
    {

        // holds all server configurations
        Dictionary<string, RfcConfigParameters> availableDestinations = null;
        RfcConfigParameters _parameters = new RfcConfigParameters();

        // event fired when configuration is changed
        public event RfcServerManager.ConfigurationChangeHandler ConfigurationChanged;


        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="locationConfigDom">Configuration connection
          parameters</param>
        public SAPIDocReceiveConfiguration(string serverName,string destinationName,XmlDocument locationConfigDom)
        {
            if (availableDestinations==null)
                availableDestinations = new Dictionary<string, RfcConfigParameters>();

            UpdateParameters(serverName, destinationName,locationConfigDom);
        }


        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="locationConfigDom">Configuration connection
          parameters</param>
        public SAPIDocReceiveConfiguration(string serverName,string destinationName,string progId)
        {
            if (availableDestinations == null)
                availableDestinations = new Dictionary<string, RfcConfigParameters>();

            AddParameters(serverName, progId);
        }


        /// <summary>
        /// Implemented from IDestinationConfiguration,returns populated
        
        /// Rfc parameters collection to RFC destination manager
        /// </summary>
        /// <param name="destinationName">The destination name used
          in repositiry</param>
        /// <returns>The collection of RFC parameters</returns>
        public RfcConfigParameters GetParameters(string serverName)
        {
            RfcConfigParameters foundDestination;
           
            if (availableDestinations != null)
            {
                availableDestinations.TryGetValue(serverName, out foundDestination);

                if (foundDestination==null)
                    throw new ApplicationException(string.Format("Server:{0} parameters do not
                        exist in repository", serverName));
            }
            else
            {
                throw new ApplicationException(string.Format("Server:{0} parameters do not
                    exist in repository",serverName));
            }

            //return foundDestination;
            RfcConfigParameters rfcConfigParameters = availableDestinations[serverName];

            return rfcConfigParameters;
        }

        /// <summary>
        /// Called from constructor
        /// </summary>
        /// <param name="locationConfigDom">Xml containing parameters,
          passed by BizTalk</param>
        public void UpdateParameters(string serverName, string destinationName, XmlDocument configDOM)
        {
            // temp hard-coded for prototyping
            //string destinationName = "SAP_IDOC_DESTINATION_CONFIG";


            RfcConfigParameters parameters = new RfcConfigParameters();

            parameters.Add(RfcConfigParameters.RepositoryDestination, destinationName);

            parameters.Add(RfcConfigParameters.Name, serverName);

            parameters.Add(RfcConfigParameters.GatewayHost, SAPIDocUtils.Extract(configDOM, "/Config/gatewayHost").ToString());
            parameters.Add(RfcConfigParameters.GatewayService, SAPIDocUtils.Extract(configDOM, "/Config/gatewayService").ToString());
            parameters.Add(RfcConfigParameters.ProgramID, SAPIDocUtils.Extract(configDOM, "/Config/programID").ToString());
            //parameters.Add(RfcConfigParameters.RegistrationCount, SAPIDocUtils.Extract(configDOM,
              "/Config/registrationCount").ToString());
            parameters.Add(RfcConfigParameters.RegistrationCount, "5");

            RfcConfigParameters existingConfiguration;

            //if a destination of that name existed before, we need to fire
              a change event
            if (availableDestinations.TryGetValue(serverName, out existingConfiguration))
            {
                availableDestinations[serverName] = parameters;

                RfcConfigurationEventArgs eventArgs = new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, parameters);

                System.Diagnostics.Trace.WriteLine("Fire change event " + eventArgs.ToString() + " for destination " + serverName);

                // fire changed event
                if (ConfigurationChanged != null)
                    ConfigurationChanged(serverName, eventArgs);
            }
            else
            {
                availableDestinations[serverName] = parameters;
            }
            
           
        }

        /// <summary>
        /// Called by constructor
        /// </summary>
        /// <param name="serverName"></param>
        public  void AddParameters(string serverName,string progId)//, string destinationName)
        {

            // temp hard-coded for prototyping
            string destinationName = "SAP_IDOC_DESTINATION_CONFIG";

  
            RfcConfigParameters parameters = new RfcConfigParameters();
            parameters.Add(RfcConfigParameters.Name, serverName);
            parameters.Add(RfcConfigParameters.RepositoryDestination, destinationName);
            parameters.Add(RfcConfigParameters.GatewayHost, "******");
            parameters.Add(RfcConfigParameters.GatewayService, "3300");
            parameters.Add(RfcConfigParameters.ProgramID, progId);
            parameters.Add(RfcConfigParameters.RegistrationCount, "1");

            RfcConfigParameters existingConfiguration;


            //if a destination of that name existed before, we need to fire
              a change event
            if (availableDestinations.TryGetValue(serverName, out existingConfiguration))
            {
                availableDestinations[serverName] = parameters;

                RfcConfigurationEventArgs eventArgs= new RfcConfigurationEventArgs(RfcConfigParameters.EventType.CHANGED, _parameters);

                System.Diagnostics.Trace.WriteLine("Fire change event " + eventArgs.ToString() + " for destination " + serverName);


                // fire changed event
                if (ConfigurationChanged != null)
                    ConfigurationChanged(serverName, eventArgs);
            }
            else
            {
                availableDestinations[serverName] = parameters;
            }

           
        }

        // The following code is not used in this example
        public bool ChangeEventsSupported()
        {
            return true;
        }

    }
}


//Next, to process IDocs sent from SAP you create a handler class,
  that is called by the NCO 3


using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using SAP.Middleware;
using SAP.Middleware.Connector;

namespace YourCompany.SAP.Connector
{
    /// <summary>
    /// Encapsulates NCO 3 IDoc receive functionality
    /// </summary>
    public  class SAPIDocReceiveHandler
    {
        
        public delegate void IDocReceiveEventHandler(SAPIDocReceiveEventArgs e);

        public static event IDocReceiveEventHandler IDocEndReceiveCompleteEvent;
        public static event IDocReceiveEventHandler IDocBeginReceiveCompleteEvent;

        /// <summary>
        /// Call back handler method,invoked by RFC Server.
        /// </summary>
        /// <param name="context">Session context</param>
        /// <param name="function">Rfc Function passed by Rfc Server
          manager</param>
        [RfcServerFunction(Name = "IDOC_INBOUND_ASYNCHRONOUS", Default = false)]
        public void IDOC_INBOUND_ASYNCHRONOUS(RfcServerContext context, IRfcFunction function)
        {

            List<string> IdocNumberList=new List<string>();
            string IDocNumber = string.Empty;
            IRfcTable IdocControlRec40;
            IRfcStructure structureIdocControlRec40;
            IRfcTable IdocDataRec40;
            IRfcStructure structureIdocDataRec40;

            string field = string.Empty;

            StringBuilder idocDataRec40lineBuilder = new StringBuilder();
            StringBuilder idocControlRec40lineBuilder = new StringBuilder();

            // notify any subscribed clients that we are about to receive an
              IDoc
            SAPIDocReceiveEventArgs e = new SAPIDocReceiveEventArgs();
            e.SessionId = context.SessionID;

            if (IDocBeginReceiveCompleteEvent != null)
                IDocBeginReceiveCompleteEvent(e);

            try
            {
   
                // Process the IDoc DC40 header segment
                IdocControlRec40 = function.GetTable("IDOC_CONTROL_REC_40");

                // Build DC40 Control record string
                foreach (IRfcStructure rfcControlRecord in IdocControlRec40)
                {
                    
                    structureIdocControlRec40 = IdocControlRec40.Metadata.LineType.CreateStructure();

                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("TABNAM").ToString().PadRight(structureIdocControlRec40["TABNAM"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("MANDT").ToString().PadRight(structureIdocControlRec40["MANDT"].Metadata.NucLength, ' '));
                    
                    IDocNumber=rfcControlRecord.GetValue("DOCNUM").ToString().PadRight(structureIdocControlRec40["DOCNUM"].Metadata.NucLength, ' ');
                    idocControlRec40lineBuilder.Append(IDocNumber);
                    IdocNumberList.Add(IDocNumber);

                    System.Diagnostics.Trace.WriteLine(String.Format("IDoc:{0} has been
                      received from SAP for Session:{1}", IDocNumber, context.SessionID));

                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("DOCREL").ToString().PadRight(structureIdocControlRec40["DOCREL"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("STATUS").ToString().PadRight(structureIdocControlRec40["STATUS"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("DIRECT").ToString().PadRight(structureIdocControlRec40["DIRECT"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("OUTMOD").ToString().PadRight(structureIdocControlRec40["OUTMOD"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("EXPRSS").ToString().PadRight(structureIdocControlRec40["EXPRSS"].Metadata.NucLength, ' '));
                
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("TEST").ToString().PadRight(structureIdocControlRec40["TEST"].Metadata.NucLength, ' '));
               
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("IDOCTYP").ToString().PadRight(structureIdocControlRec40["IDOCTYP"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("CIMTYP").ToString().PadRight(structureIdocControlRec40["CIMTYP"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("MESTYP").ToString().PadRight(structureIdocControlRec40["MESTYP"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("MESCOD").ToString().PadRight(structureIdocControlRec40["MESCOD"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("MESFCT").ToString().PadRight(structureIdocControlRec40["MESFCT"].Metadata.NucLength, ' '));
                
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("STD").ToString().PadRight(structureIdocControlRec40["STD"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("STDVRS").ToString().PadRight(structureIdocControlRec40["STDVRS"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("STDMES").ToString().PadRight(structureIdocControlRec40["STDMES"].Metadata.NucLength, ' '));
               
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDPOR").ToString().PadRight(structureIdocControlRec40["SNDPOR"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDPRT").ToString().PadRight(structureIdocControlRec40["SNDPRT"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDPFC").ToString().PadRight(structureIdocControlRec40["SNDPFC"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDPRN").ToString().PadRight(structureIdocControlRec40["SNDPRN"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDSAD").ToString().PadRight(structureIdocControlRec40["SNDSAD"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SNDLAD").ToString().PadRight(structureIdocControlRec40["SNDLAD"].Metadata.NucLength, ' '));
                
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVPOR").ToString().PadRight(structureIdocControlRec40["RCVPOR"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVPRT").ToString().PadRight(structureIdocControlRec40["RCVPRT"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVPFC").ToString().PadRight(structureIdocControlRec40["RCVPFC"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVPRN").ToString().PadRight(structureIdocControlRec40["RCVPRN"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVSAD").ToString().PadRight(structureIdocControlRec40["RCVSAD"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("RCVLAD").ToString().PadRight(structureIdocControlRec40["RCVLAD"].Metadata.NucLength, ' '));

                    string dateField = rfcControlRecord.GetValue("CREDAT").ToString().PadRight(structureIdocControlRec40["CREDAT"].Metadata.NucLength, ' ');
                    dateField = dateField.Replace("-", "");
                    idocControlRec40lineBuilder.Append(dateField);

                    string timeField = rfcControlRecord.GetValue("CRETIM").ToString().PadRight(structureIdocControlRec40["CRETIM"].Metadata.NucLength, ' ');
                    timeField = timeField.Replace(":", "");
                    idocControlRec40lineBuilder.Append(timeField);

                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("REFINT").ToString().PadRight(structureIdocControlRec40["REFINT"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("REFGRP").ToString().PadRight(structureIdocControlRec40["REFGRP"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("REFMES").ToString().PadRight(structureIdocControlRec40["REFMES"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("ARCKEY").ToString().PadRight(structureIdocControlRec40["ARCKEY"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append(rfcControlRecord.GetValue("SERIAL").ToString().PadRight(structureIdocControlRec40["SERIAL"].Metadata.NucLength, ' '));
                    idocControlRec40lineBuilder.Append("\r\n");


                }

                // Process the IDoc DDR40 data segment
                IdocDataRec40 = function.GetTable("IDOC_DATA_REC_40");

                //for (int i = 0; i < IdocDataRec40.RowCount; i++)
                foreach (IRfcStructure rfcDataRecord in IdocDataRec40)
                {
                    // Get the record
                    structureIdocDataRec40 = IdocDataRec40.Metadata.LineType.CreateStructure();

                    // Build the data segment
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("SEGNAM").ToString().PadRight(structureIdocDataRec40["SEGNAM"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("MANDT").ToString().PadRight(structureIdocDataRec40["MANDT"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("DOCNUM").ToString().PadRight(structureIdocDataRec40["DOCNUM"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("SEGNUM").ToString().PadRight(structureIdocDataRec40["SEGNUM"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("PSGNUM").ToString().PadRight(structureIdocDataRec40["PSGNUM"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("HLEVEL").ToString().PadRight(structureIdocDataRec40["HLEVEL"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append(rfcDataRecord.GetValue("SDATA").ToString().PadRight(structureIdocDataRec40["SDATA"].Metadata.NucLength, ' '));
                    idocDataRec40lineBuilder.Append("\r\n");
               
                }

                string Idoc = string.Concat(idocControlRec40lineBuilder.ToString(), idocDataRec40lineBuilder.ToString());

                byte[] buffer = Encoding.UTF8.GetBytes(Idoc);

                // Pass the IDoc back in the EventArgs parameter
                e.Data = Idoc;
                e.IDocBuffer = buffer;

                // Pass back current session Id
                e.SessionId = context.SessionID;
                
                // Pas back IDOcnumbers
                e.IDocNumberList = IdocNumberList;

                // Set IDocs are batched flag
                e.IsBatched=(IdocControlRec40.RowCount >1);

                if (IDocEndReceiveCompleteEvent != null)
                    IDocEndReceiveCompleteEvent(e);

            }
            catch (Exception ex)
            {
                // raise exception up
                throw ex;
            }
        }
    }

    /// <summary>
    /// Summary description for SAPAdapterErrorArgs.
    /// </summary>
    public class SAPIDocReceiveEventArgs : EventArgs
    {

        private List<string>    _iDocNumberList = null;
        private byte[]          _iDocBuffer = null;
        private string          _sessionId = string.Empty;
        private string          _data = string.Empty;
        private bool            _isBatched = false;


        /// <summary>
        /// IDoc returned Byte Array
        /// </summary>
        public byte[] IDocBuffer
        {
            get { return _iDocBuffer; }
            set { _iDocBuffer = value; }
        }

        public List<string> IDocNumberList
        {
            get { return _iDocNumberList; }
            set { _iDocNumberList = value; }
        }

        /// <summary>
        /// IDoc returned in string
        /// </summary>
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }


        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }

        public bool IsBatched
        {
            get { return _isBatched; }
            set { _isBatched = value; }
        }
    }
}



//The difficult bit is processing the function after it has been
  called and extracting the DC40 control table and DD40 data table. This I have done.
  You can dynamically loop round these 
//and process. I am also using a static callback event to pass the
  IDOc back to in string format to any subscribed clients and a few other useful bits,
  but you can change this to whatever you want.

//Next you need a client to implement all of this. I just use a
  helper class as shown below as I am implementing a BizTalk Custom Adapter. But this
  could be a Winforms App, ASP.Net etc ....


{code}
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using SAP.Middleware;
using SAP.Middleware.Connector;

namespace Your.SAP.Client
{
    /// <summary>
    /// Emulates the SAP Receive Endpoint in the SAP Adapter
    /// </summary>
    class SAPReceiverEndpoint
    {
        private RfcServer _server = null;
        private RfcDestination _rfcDestination = null;
        private static SAPIDocReceiveConfiguration _sapIDocReceiveConfiguration = null;

        private string _destinationName = "SAP_IDOC_DESTINATION_CONFIG";
        private string _IDocText = string.Empty;
        private string _sessionId = string.Empty;

        public delegate void IDocReadyHandler(EventArgs e);

        public event IDocReadyHandler IDocReady;

        public void Shutdown()
        {
            RfcSessionManager.EndContext(_rfcDestination);
            _server.Shutdown(true);
        }

        public void RegisterDestination(string destinationName)
        {
      

            
            // Only register if not already initialised
            try
            {
  
                    
                    RfcDestinationManager.RegisterDestinationConfiguration(new SAPIDocDestinationConfiguration(destinationName));//1

                    _rfcDestination = RfcDestinationManager.GetDestination(destinationName);
           
            }
            // ignore as destination already configured
            catch (RfcInvalidStateException rfcEx)
            {   
                // cascade up callstack
                throw rfcEx;   
            }
 

            //System.Diagnostics.Trace.WriteLine(
            // String.Format("Destination Confgured to:{0}", _rfcDestination.Monitor.OriginDestinationID));
        }

        /// <summary>
        /// Emulates SAP Adapter Receive Endpoint
        /// </summary>
        public void Initialise()
        {
            //hard coded for testing but will pick up from ProgId in config
            string serverName = "SQLARTMAS_SERVER";
            string progId = "SQLARTMAS";

            SAPTIDHandler rfcTIDHandler=new SAPTIDHandler();

            try
            {

                if (_sapIDocReceiveConfiguration == null)
                {
                    _sapIDocReceiveConfiguration =
                       new SAPIDocReceiveConfiguration(serverName, _rfcDestination.Name, progId);
                }
                else
                {
                    _sapIDocReceiveConfiguration.AddParameters(serverName, progId);
                }
                
                RfcServerManager.RegisterServerConfiguration(_sapIDocReceiveConfiguration);

                Type[] handlers = new Type[1] { typeof(SAPIDocReceiveHandler) };//3
                _server = RfcServerManager.GetServer(serverName, handlers);//3

                _server.RfcServerError += OnRfcServerError;
                _server.RfcServerApplicationError += OnRfcServerError;

                SAPIDocReceiveHandler.IDocEndReceiveCompleteEvent +=
                    new SAPIDocReceiveHandler.IDocReceiveEventHandler(OnIDocEndReceiveComplete);

                SAPIDocReceiveHandler.IDocBeginReceiveCompleteEvent +=
                    new SAPIDocReceiveHandler.IDocReceiveEventHandler(OnIDocBeginReceiveComplete);

                // register for session startevent in order to capture SessionId
                rfcTIDHandler.IDocSessionStart += new SAPTIDHandler.IDocsSessionEventHandler(OnRfcSessionStart);

                //register TID specific handler
                _server.TransactionIDHandler = rfcTIDHandler;

                // Create a new session for this particular destination
                RfcSessionManager.BeginContext(_rfcDestination);


                //server.TransactionIDHandler = new MyServerHandler();
                _server.Start();//4

            }

            catch (RfcInvalidStateException rfcEx)
            {

                // cascade up callstack
                throw rfcEx;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
 

        }


        public string IDocText
        {
            get { return _IDocText; }
            set { _IDocText = value; }
        }

        public void OnIDocBeginReceiveComplete(SAPIDocReceiveEventArgs e)
        {
        
            // static event so make sure callback is from the same session.
            if (string.Equals(e.SessionId, this._sessionId))
            {
                // start streaming in SAP Adapter
            }
        }

        public void OnIDocEndReceiveComplete(SAPIDocReceiveEventArgs e)
        {
            // static event so make sure callback is from the same session.
            if (string.Equals(e.SessionId, this._sessionId))
            {
              // StreamReader sr = new StreamReader(e.IDocStream, Encoding.Unicode);

                this._IDocText = e.Data;

                System.Diagnostics.Trace.WriteLine("Completed receiving an IDoc");
                System.Diagnostics.Trace.WriteLine(this._IDocText);

                // bubble upto form
                IDocReady(new EventArgs());
            }
        }

        public void OnRfcServerStateChanged(object server, RfcServerStateChangedEventArgs stateEventData)
        {
            RfcServer serverStatus=server as RfcServer;

            System.Diagnostics.Trace.WriteLine(serverStatus.Name);
        }

        public void OnRfcServerError(Object server, RfcServerErrorEventArgs errorEventData)
        {
            RfcServer rfcServer = server as RfcServer;
            RfcServerApplicationException appEx = errorEventData.Error as RfcServerApplicationException;

            if (appEx != null)
            {
                Console.WriteLine("RfcServerApplicationError occured in RFC server
                  {0} :", rfcServer.Name);
            }
            else
                Console.WriteLine("RfcServerError occured in RFC server {0} :", rfcServer.Name);


            if (errorEventData.ServerContextInfo != null)
            {
                Console.WriteLine("RFC Caller System ID: {0} ", errorEventData.ServerContextInfo.SystemAttributes.SystemID);
                Console.WriteLine("RFC function Name: {0} ", errorEventData.ServerContextInfo.FunctionName);
            }

            Console.WriteLine("Error type: {0}", errorEventData.Error.GetType().Name);
            Console.WriteLine("Error message: {0}", errorEventData.Error.Message);

            if (appEx != null)
            {
                Console.WriteLine("Inner exception type: {0}", appEx.InnerException.GetType().Name);
                Console.WriteLine("Inner exception message: {0}", appEx.InnerException.Message);
            }


        }

        // call back to capture the session Id
        public void OnRfcSessionStart(SAPSessionEventArgs sessionArgs)
        {
            this._sessionId = sessionArgs.SessionId;
        }
    }
}




//I'll post something about sending IDocs later.

//BTW my name is Paul. You can contact me on

//Paul.Massen@sign-online-solutions.net