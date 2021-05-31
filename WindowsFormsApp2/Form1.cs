using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using WindowsFormsApp2.webwsTotalExpress;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute a Soap WebService call
        /// </summary>
        public static void Execute()
        {
            HttpWebRequest request = CreateWebRequest();
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"
<soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
    xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
    xmlns:urn=""urn:calcularFrete"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:calcularFrete soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
         <calcularFreteRequest xsi:type=""web:calcularFreteRequest"" xmlns:web=""http://edi.totalexpress.com.br/soap/webservice_calculo_frete.total"">
            <TipoServico xsi:type=""xsd:string"">EXP</TipoServico>
            <CepDestino xsi:type=""xsd:nonNegativeInteger"">29165650</CepDestino>
            <Peso xsi:type=""xsd:string"">0.120</Peso>
            <ValorDeclarado xsi:type=""xsd:string"">120</ValorDeclarado>
            <TipoEntrega xsi:type=""xsd:nonNegativeInteger"">0</TipoEntrega>
            <ServicoCOD xsi:type=""xsd:boolean"">false</ServicoCOD>
            <Altura xsi:type=""xsd:nonNegativeInteger"">10</Altura>
            <Largura xsi:type=""xsd:nonNegativeInteger"">10</Largura>
            <Profundidade xsi:type=""xsd:nonNegativeInteger"">10</Profundidade>
         </calcularFreteRequest>
      </urn:calcularFrete>
   </soapenv:Body>
</soapenv:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    Debug.WriteLine(soapResult);
                }
            }
        }
        /// <summary>
        /// Create a soap webrequest to [Url]
        /// </summary>
        /// <returns></returns>
        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"https://edi.totalexpress.com.br/webservice_calculo_frete.php");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.Credentials = new NetworkCredential("USUARIO_ICS_INTEGRACAO", "SENHA_ICS_INTEGRACAO");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            return webRequest;
        }

        private wsTotalExpress.webservice_calculo_fretetotalPortTypeClient calculo_fretetotal(
                                                        string strWebService = @"https://edi.totalexpress.com.br/webservice_calculo_frete.php",
                                                        string strUser = "USUARIO_ICS_INTEGRACAO",
                                                        string strPassword = "SENHA_ICS_INTEGRACAO")
        {
            bool hasValidation = !(string.IsNullOrWhiteSpace(strUser)) && !(string.IsNullOrWhiteSpace(strPassword));

            BasicHttpBinding objBinding = new BasicHttpBinding();

            int nDefaultLength = 2000000;

            objBinding.ReaderQuotas.MaxDepth = nDefaultLength;
            objBinding.ReaderQuotas.MaxArrayLength = nDefaultLength;
            objBinding.ReaderQuotas.MaxBytesPerRead = nDefaultLength;
            objBinding.ReaderQuotas.MaxNameTableCharCount = nDefaultLength;
            objBinding.ReaderQuotas.MaxStringContentLength = nDefaultLength;

            objBinding.MaxReceivedMessageSize = nDefaultLength;
            objBinding.MaxBufferPoolSize = nDefaultLength;
            objBinding.MaxBufferSize = nDefaultLength;

            objBinding.CloseTimeout = new TimeSpan(0, 5, 0);
            objBinding.OpenTimeout = objBinding.CloseTimeout;
            objBinding.ReceiveTimeout = objBinding.CloseTimeout;
            objBinding.SendTimeout = objBinding.CloseTimeout;

            if (hasValidation)
            {
                objBinding.Security.Mode = BasicHttpSecurityMode.Transport;
                objBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            }


            CustomBinding cb = new CustomBinding(objBinding);
            BindingElementCollection bec = cb.Elements;

            bec.RemoveAt(0);
            bec.Insert(0, new CustomTextMessageBindingElement("ISO-8859-1", "text/xml", MessageVersion.Soap11));
            //bec.Insert(0, new CustomTextMessageBindingElement("UTF-8", "text/xml", MessageVersion.Soap11));
            cb = new CustomBinding(bec);


            wsTotalExpress.webservice_calculo_fretetotalPortTypeClient objServiceClient = new wsTotalExpress.webservice_calculo_fretetotalPortTypeClient();

            if (hasValidation)
            {
                objServiceClient.ClientCredentials.UserName.UserName = strUser;
                objServiceClient.ClientCredentials.UserName.Password = strPassword;
            }


            objServiceClient.Endpoint.Binding = cb;
            objServiceClient.Endpoint.Address = new EndpointAddress(strWebService);

            return objServiceClient;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            Execute();

            var cliente = calculo_fretetotal();
            var req = new wsTotalExpress.calcularFreteRequest();
            req.TipoServico = "EXP";
            req.CepDestino = "24020020";
            req.Peso = "0.120";
            req.ValorDeclarado = "120";
            req.TipoEntrega = "0";
            req.ServicoCOD = false;
            req.Altura = "10";
            req.Largura = "10";
            req.Profundidade = "10";



            var soapInspector = new SoapInspectorBehavior();
            cliente.Endpoint.EndpointBehaviors.Add(soapInspector);

            //https://github.com/dotnet/wcf/issues/3228
            var ret1 = cliente.calcularFrete(req);



            var webwsClient = new webwsTotalExpress.webservice_calculo_fretetotal();

            webwsClient.Credentials = new NetworkCredential("USUARIO_ICS_INTEGRACAO", "SENHA_ICS_INTEGRACAO");
            webwsClient.Url = @"https://edi.totalexpress.com.br/webservice_calculo_frete.php";

            webwsClient.calcularFreteCompleted += new calcularFreteCompletedEventHandler(evento);

            var webwsRequest = new webwsTotalExpress.calcularFreteRequest();

            webwsRequest.TipoServico = "EXP";
            webwsRequest.CepDestino = "29165650";
            webwsRequest.Peso = "0.120";
            webwsRequest.ValorDeclarado = "120";
            webwsRequest.TipoEntrega = "0";
            webwsRequest.ServicoCOD = false;
            webwsRequest.Altura = "10";
            webwsRequest.Largura = "10";
            webwsRequest.Profundidade = "10";

            var ret2 = (calcularFreteResponse)webwsClient.calcularFrete(webwsRequest);

        }

        public void evento(object sender, calcularFreteCompletedEventArgs e) {
            Debug.Print("asdf");

        }

        public class SoapMessageInspector : IClientMessageInspector
        {
            public string LastRequestXml { get; private set; }
            public string LastResponseXml { get; private set; }

            public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
            {
                LastRequestXml = request.ToString();
                return request;
            }

            public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
            {
                LastResponseXml = reply.ToString();
            }
        }

        public class SoapInspectorBehavior : IEndpointBehavior
        {
            private readonly SoapMessageInspector inspector_ = new SoapMessageInspector();

            public string LastRequestXml => inspector_.LastRequestXml;
            public string LastResponseXml => inspector_.LastResponseXml;

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
            }

            public void Validate(ServiceEndpoint endpoint)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.ClientMessageInspectors.Add(inspector_);
            }
        }
    }
}
