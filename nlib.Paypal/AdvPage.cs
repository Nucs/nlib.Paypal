using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SapphireLib.Paypal {
    public abstract class AdvPage : Page {
        protected AdvPage() {
            this.Load += new EventHandler(Page_Load);
        }

        protected override void OnInit(EventArgs e) {
            MaintainScrollPositionOnPostBack = true;
            base.OnInit(e);
        }

        protected abstract void Page_Load(object sender, EventArgs eventArgs);
    }

}