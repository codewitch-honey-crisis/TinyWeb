using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TinyWeb
{
	public partial class WebServer : Component
	{
		static readonly object _processRequestEventKey = new object();
		static readonly object _endPointChangedEventKey = new object();
		static readonly object _isStartedChangedEventKey = new object();
		const int _backLog = 10;

		bool _isStarted = false;
		Socket _listener = null;
		IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 8080);
		public WebServer()
		{

			InitializeComponent();
		}
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				IsStarted = false;
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}
		public WebServer(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}
		public bool IsStarted {
			get {
				return _isStarted;
			}
			set {
				if (value != _isStarted)
				{
					if (value)
					{
						_Start();
					}
					else
						_Stop();
					OnIsStartedChanged(EventArgs.Empty);
				}
			}
		}
		protected virtual void OnIsStartedChanged(EventArgs args)
		{
			(Events[_isStartedChangedEventKey] as EventHandler)?.Invoke(this, args);
		}
		public event EventHandler IsStartedChanged {
			add {
				Events.AddHandler(_isStartedChangedEventKey, value);
			}
			remove {
				Events.RemoveHandler(_isStartedChangedEventKey, value);
			}
		}
		public event ProcessRequestEventHandler ProcessRequest {
			add {
				Events.AddHandler(_processRequestEventKey, value);
			}
			remove {
				Events.RemoveHandler(_processRequestEventKey, value);
			}
		}
		protected virtual void OnProcessRequest(ProcessRequestEventArgs args)
		{
			(Events[_processRequestEventKey] as ProcessRequestEventHandler)?.Invoke(this, args);
		}
		[TypeConverter(typeof(_EndPointConverter))]
		public IPEndPoint EndPoint {
			get {
				return _endPoint;
			}
			set {
				if (value != _endPoint)
				{
					_endPoint = value;
					_Restart();
					OnEndPointChanged(EventArgs.Empty);
				}
			}
		}
		void _Stop()
		{
			if (_isStarted)
			{
				if (null != _listener)
					_listener.Close();

				_listener = null;
				_isStarted = false;
			}
		}
		void _Start()
		{
			if (!_isStarted)
			{
				var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
				_listener = socket;
				_listener.Bind(_endPoint);
				_listener.Listen(_backLog);
				ThreadPool.QueueUserWorkItem((l) =>
				{
					(l as Socket).ServeHttp((req, res) =>
					{
						OnProcessRequest(new ProcessRequestEventArgs(req, res));
					});
				}, _listener);
				_isStarted = true;
			}
		}
		void _Restart()
		{
			_Stop();
			_Start();
		}

		public event EventHandler EndPointChanged {
			add {
				Events.AddHandler(_endPointChangedEventKey, value);
			}
			remove {
				Events.RemoveHandler(_endPointChangedEventKey, value);
			}
		}
		protected virtual void OnEndPointChanged(EventArgs args)
		{
			(Events[_endPointChangedEventKey] as EventHandler)?.Invoke(this, args);
		}
		bool _IsDesignMode {
			get {
				if (null == Site)
					return false;
				return Site.DesignMode;
			}
		}
		#region _EndPointConverter
		class _EndPointConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (typeof(string) == sourceType || typeof(IPEndPoint) == sourceType)
					return true;
				return base.CanConvertFrom(context, sourceType);
			}
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (typeof(string) == destinationType || typeof(IPEndPoint) == destinationType)
					return true;
				return base.CanConvertTo(context, destinationType);
			}
			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				var s = value as string;
				if (null != s)
				{
					if (0 == s.Length)
						return new IPEndPoint(IPAddress.Any, 80);

					var i = s.LastIndexOf(':');
					var port = 80;
					if (0 > i)
					{
						port = int.Parse(s.Substring(i + 1));
						s = s.Substring(0, i);
					}
					var addr = (0 == s.Length || "*" == s) ? IPAddress.Any : IPAddress.Parse(s);

					return new IPEndPoint(addr, port);
				}

				var ep = value as IPEndPoint;

				if (null != ep)
				{
					var a = ep.Address.ToString();
					if ("0.0.0.0" == a)
						a = "*";
					return string.Concat(a, ":", ep.Port.ToString());
				}



				return base.ConvertFrom(context, culture, value);
			}
		}

		#endregion
	}
	public delegate void ProcessRequestEventHandler(object sender, ProcessRequestEventArgs args);
	public class ProcessRequestEventArgs : EventArgs
	{
		internal ProcessRequestEventArgs(HttpRequest request, HttpResponse response)
		{
			Request = request;
			Response = response;
		}
		public HttpRequest Request { get; }
		public HttpResponse Response { get; }
	}
}
