using System;
using System.Data.SqlClient;
using System.Web;
using Sapphire;
using SapphireLib.Paypal;

partial class IpnHandler : AdvPage {
    public string Export { get; private set; }

    protected override void Page_Load(object sender, EventArgs e) {
        var handler = new SapphireIpnHandler();
        //handle the request that is sent to this page as configurated in the ipn config in the paypal site
        handler.Handle(); 
    }
}


public class SapphireIpnHandler : PayPalHandler {

    //  The payer's account username is passed via the custom variable.
    public string Username {
        get { return Custom; }
    }

    //Here configurate to whom's mail the payment is supposed to be to.
    public override string ToBePayedEmail {
        get { return "owner_paypal_account@gmail.com"; }
    } 

    #region Handling Events

    //Handle an event when it fails.
    public override void Failed(string desc) {
        Logger.LogDonation(GetIPAddress(), Username, desc, Amount, 0, "{"+RequestGuid +"} "+ RequestString);
    }

    //Handle an event that is familiar to failed but is a C# Exception
    public override void Unhandled(Exception ex) {
        Logger.LogDonation(GetIPAddress(), Username, ex.ToString(), Amount, 0, "{"+RequestGuid +"} "+ RequestString);
    }

    //When the payment is verified, just access the variables in this object and 
    public override void Verified() {
        //pay to the customer.
        var n = AddGems(Username, Amount);
        if (n == int.MinValue) //error signaled - something went wrong during the AddGems call. logged internally.
            return;
        Logger.LogDonation(GetIPAddress(), Username, "VERIFIED", Amount, n, "{"+RequestGuid +"} "+ RequestString);
    }

    #endregion

    public SapphireIpnHandler() : base(!Constants.Paypal.Live) {}

    #region Custom Private Methods

    private int AddGems(string username, double amount) {
        if (amount == 0)
            return 0;
        if (amount < 0) {
            Failed("Payed Amount (" + amount + ") seems to be negative. Might be a refund.");
            return int.MinValue;
        }
        if (string.IsNullOrEmpty(username)) {
            Failed("Username (" + (username ?? "") + ") is either null or empty.");
            return int.MinValue;
        }

        try {
            //todo pay to the customer.
            return 100;
        } catch (Exception e) {
            Unhandled(e);
            return int.MinValue;
        }
    }

    private string GetIPAddress() {
        var context = HttpContext.Current;
        var ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (!string.IsNullOrEmpty(ipAddress)) {
            var addresses = ipAddress.Split(',');
            if (addresses.Length != 0)
                return addresses[0];
        }

        if (!string.IsNullOrEmpty(ipAddress)) {
            var addresses = ipAddress.Split(',');
            if (addresses.Length != 0)
                return addresses[0];
        }
        return context.Request.ServerVariables["REMOTE_ADDR"];
    }

    #endregion
}

static class Constants {
    public static class Paypal {
        public static bool Live = true;
    }
}
static class Logger {
    public static void LogDonation(string getIpAddress, string username, string toString, double amount, int i, string s) {
        //todo logging to database or file..
    }
}