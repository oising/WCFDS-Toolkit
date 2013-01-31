namespace Microsoft.Data.Services.Toolkit.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Specifies whether a service will apply a provided message inspector. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DispatchInspectorAttribute : Attribute, IServiceBehavior
    {
        private readonly Type dispatcherType;

        /// <summary>
        /// Initializes a new instance of the DispatchInspectorAttribute class.
        /// </summary>
        /// <param name="dispatcherType">The dispatcher implementation <see cref="Type"/>.</param>
        public DispatchInspectorAttribute(Type dispatcherType)
        {
            this.dispatcherType = dispatcherType;
        }

        /// <summary>
        /// Applies the dispatcher behavior.
        /// </summary>
        /// <param name="serviceDescription">A <see cref="ServiceDescription"/> object containing the service description.</param>
        /// <param name="serviceHostBase">A <see cref="ServiceHostBase"/> object containing the service host base.</param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add((IDispatchMessageInspector)Activator.CreateInstance(this.dispatcherType));
            }
        }

        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="serviceDescription">A <see cref="ServiceDescription"/> object containing the service description.</param>
        /// <param name="serviceHostBase">A <see cref="ServiceHostBase"/> object containing the service host base.</param>
        /// <param name="endpoints">A collection of <see cref="ServiceEndpoint"/> type.</param>
        /// <param name="bindingParameters">A collection of binding parameters.</param>
        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Validates the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">A <see cref="ServiceDescription"/> object containing the service description.</param>
        /// <param name="serviceHostBase">A <see cref="ServiceHostBase"/> object containing the service host base.</param>
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
