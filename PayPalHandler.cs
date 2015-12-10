using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Sapphire {
    public abstract class PayPalHandler {

        #region Parameters

        private const string ppsandbox = "https://www.sandbox.paypal.com/cgi-bin/webscr";
        private const string pplive = "https://www.paypal.com/cgi-bin/webscr";

        /// <summary>
        ///     Unique Guid generated on each construction of IPN request.
        /// </summary>
        public Guid RequestGuid { get; private set; }

        /// <summary>
        ///     Is live used or debug.
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        ///     Custom attribute that is passed taken from custom attribute
        /// </summary>
        public string Custom { get; protected set; }

        /// <summary>
        ///     Invoice id
        /// </summary>
        public string Invoice { get; protected set; }

        /// <summary>
        ///     The amount payed by the customer
        /// </summary>
        public double Amount { get; protected set; }
        /// <summary>
        ///     The email of the person who payed.
        /// </summary>
        public string PayerEmail { get; protected set; }

        /// <summary>
        ///     The status of the payment. Completed means its done.
        /// </summary>
        public string PaymentStatus { get; protected set; }

        /// <summary>
        ///     The email that the ipn describes the payment was sent. (usually the owner who receives the payments)
        /// </summary>
        public string PayedTo { get; set; }

        /// <summary>
        ///     The entire parameters that were sent with the IPN request in a http get stringquery format. ("ab=123&what=nothing&potato=twopotato"), see https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNandPDTVariables/
        /// </summary>
        public string RequestString { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs a handler for live version. (Not debug)
        /// </summary>
        public PayPalHandler() : this(false) {}

        /// <summary>     
        /// </summary>
        /// <param name="IsDebug">If passed as true, the paypal sandbox will be used to debug. leave false for live version</param>
        public PayPalHandler(bool IsDebug) {
            IsLive = !IsDebug;
            RequestGuid = Guid.NewGuid();
        }

        #endregion

        #region Abstract
        /// <summary>
        ///     The email that is suppose to receive the payment - used to verify if indeed he received the payment. Leave null to ignore.
        /// </summary>
        public abstract string ToBePayedEmail { get; }

        /// <summary>
        ///     Occurs when all tests are verified. respond to the payment.
        /// </summary>
        /// <param name="h"></param>
        public abstract void Verified();

        /// <summary>
        ///     Unhandled exception caught. IPN processing was interrupted. immediate attention required.
        /// </summary>
        public abstract void Unhandled(Exception ex);

        /// <summary>
        /// Something expected failed during the process.
        /// </summary>
        /// <param name="desc">The description for the reason that ipn has failed.</param>
        public abstract void Failed(string desc);
        #endregion

        #region Handling

        public void Handle() {
            try {
                var cntx = HttpContext.Current;

                if (cntx.Request.RequestType != "POST") {
                    Failed( "Not a POST request");
                    return;
                }

                loadVariables();
                if (VerifyIPN()) {
                    Verified();
                }
            } catch (Exception e) {
                Unhandled(e);
            }
        }

        private void loadVariables() {
            var req = HttpContext.Current.Request;
            PaymentStatus = req.Params["payment_status"];
            Custom = req.Params["custom"];
            Amount = Convert.ToDouble(req.Params["mc_gross"]);
            PayerEmail = req.Params["payer_email"];
            PayedTo = req.Params["receiver_email"];
            RequestString = req.Form.ToString();
        }

        private bool VerifyIPN() {
            var req = (HttpWebRequest)WebRequest.Create(IsLive ? pplive : ppsandbox);

            //'Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            var Param = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
            var strRequest = Encoding.ASCII.GetString(Param);
            strRequest = strRequest + "&cmd=_notify-validate";
            req.ContentLength = strRequest.Length;

            //'for proxy
            //'Dim proxy As New WebProxy(New System.Uri("http://url:port#"))
            //'req.Proxy = proxy

            //'Send the request to PayPal and get the response
            var streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII);
            streamOut.Write(strRequest);
            streamOut.Close();
            var streamIn = new StreamReader(req.GetResponse().GetResponseStream());
            var strResponse = streamIn.ReadToEnd();
            streamIn.Close();

            if (strResponse == "VERIFIED") {
                //'check the payment_status is Completed
                //'check that txn_id has not been previously processed
                //'check that receiver_email is your Primary PayPal email
                //'check that payment_amount/payment_currency are correct
                //'process payment

                if (!ToBePayedEmail.Equals(PayedTo)) {
                    Failed("IPN Verified but payed target does not equal to ipn's given target. (ToBePayed: "+ToBePayedEmail+", vs , PayedTo: "+PayedTo+")");
                }

                switch (PaymentStatus) {
                    case "Completed":
                        break;
                    default:
                        Failed("Unexpected payment status: "+PaymentStatus);
                        return false;
                }

                return true;
            } else if (strResponse == "INVALID") {
                //'log for manual investigation
                Failed("Paypal send back that the payment is INVALID (faked)");
                return false;
            } else {
                Unhandled(new Exception("Unexpected/unhandled returned status from paypal ipn verification: "+strResponse));
                return false;
            }
        }
        #endregion

    }
}