using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService
{
	/// <summary>
	/// This will prevent exceptions from being allowed to bubble up on the server side so that the service host does not enter a faulted state - otherwise a single error
	/// (such as an invalid GetOrdinal call, for example) would prevent any further calls to the service being made, even if the exception was caught by the client
	/// </summary>
	internal sealed class ErrorWrappingBehavior : Attribute, IServiceBehavior // Courtesy of http://stackoverflow.com/a/14910873
	{
		private readonly Action<Exception> _optionalErrorLogger;
		public ErrorWrappingBehavior(Action<Exception> optionalErrorLogger = null)
		{
			_optionalErrorLogger = optionalErrorLogger;
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

		public void AddBindingParameters(
			ServiceDescription serviceDescription,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection bindingParameters) { }

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (var chanDispBase in serviceHostBase.ChannelDispatchers)
			{
				var channelDispatcher = chanDispBase as ChannelDispatcher;
				if (channelDispatcher == null)
					continue;
				channelDispatcher.ErrorHandlers.Add(new SvcErrorHandler(_optionalErrorLogger));
			}
		}

		private sealed class SvcErrorHandler : IErrorHandler
		{
			private readonly Action<Exception> _optionalErrorLogger;
			public SvcErrorHandler(Action<Exception> optionalErrorLogger)
			{
				_optionalErrorLogger = optionalErrorLogger;
			}

			public bool HandleError(Exception error)
			{
				return true;
			}

			public void ProvideFault(Exception error, MessageVersion version, ref Message msg)
			{
				if (_optionalErrorLogger != null)
				{
					try
					{
						_optionalErrorLogger(error);
					}
					catch { } // We're already in an error handler, there's no value to allowing an error that occurred while trying to log the error to be thrown
				}

				if (error is FaultException)
					return;

				var faultException = new FaultException(error.Message);
				msg = Message.CreateMessage(
					version,
					faultException.CreateMessageFault(),
					faultException.Action
				);
			}
		}
	}
}