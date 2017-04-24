using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Texxtoor.BaseLibrary.Mashup.Adresses {

    public class Contacts {

        public string Name { get; set; }

        public string Email { get; set; }
    }

    public class OpenInviterBase {
        public string Service { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public bool Init() {
            return false;
        }

        protected string formAction;

        public HttpResponse Get(string uri, bool bla) {
            return null;
        }

        public HttpResponse Get(string uri) {
            return null;
        }

        public HttpResponse Post(string uri, object data, bool bla) {
            return null;
        }

        public void DebugRequest() { }

        public bool CheckResponse(string uri, HttpResponse res) { return true; }

        public bool CheckSession() { return true; }

        public void ResetDebugger() { }

        public void UpdateDebugger(string txt, string uri, string scheme) { }
        public void UpdateDebugger(string txt, string uri, string scheme, bool result) { }
        public void UpdateDebugger(string txt, string uri, string scheme, bool result, object data) { }
        public void StopPlugin() { }

    }

    //_pluginInfo=array(
    //"name"=>"Xing",
    //"version"=>"1.0.7",
    //"description"=>"Get the contacts from a Xing account",
    //"base_version"=>"1.8.0",
    //"type"=>"email",
    //"check_url"=>"https://mobile.xing.com/",
    //"requirement"=>"email",
    //"allowed_domains"=>false,
    //"imported_details"=>array("first_name","email_1"),
    //);
    /**
     * Xing Plugin
     * 
     * Import user"s contacts from Xing and send 
     * messages using the internal messaging system
     * 
     * @author OpenInviter
     * @version 1.0.0
     */
    public class Xing : OpenInviterBase {
        private string login_ok = null;
        public bool showContacts = true;
        public bool internalError = false;
        protected int timeout = 30;
        HttpResponse res;

        public bool Login(string user, string pass) {
            this.Service = "xing";
            this.User = user;
            this.Password = pass;

            if (!this.Init()) return false;

            res = this.Get("https://www.xing.com/");
            if (this.CheckResponse("initial_get", res))
                this.UpdateDebugger("initial_get", "https://mobile.xing.com/", "GET");
            else {
                this.UpdateDebugger("initial_get", "https://mobile.xing.com/", "GET", false);
                this.DebugRequest();
                this.StopPlugin();
                return false;
            }
            formAction = "https://www.xing.com/app/user";
            var postElements = new {
                Op = "login",
                Dest = "https://www.xing.com/",
                Login_user_name = user,
                Login_password = pass,
                Sv_remove_name = "Log in"
            };
            res = this.Post(formAction, postElements, true);

            if (this.CheckResponse("post_login", res))
                this.UpdateDebugger("post_login", formAction, "POST", true, postElements);
            else {
                this.UpdateDebugger("post_login", formAction, "POST", false, postElements);
                this.DebugRequest();
                this.StopPlugin();
                return false;
            }

            string url_adressbook = "https://www.xing.com/app/contact";
            this.login_ok = url_adressbook;
            return true;
        }

        /**
         * Get the current user"s contacts
         * 
         * Makes all the necesarry requests to import
         * the current user"s contacts
         * 
         * @return mixed The array if contacts if importing was successful, FALSE otherwise.
         */
        public IEnumerable<Contacts> getMyContacts() {
            string url;
            if (!String.IsNullOrEmpty(login_ok)) {
                this.DebugRequest();
                this.StopPlugin();
                return null;
            } else {
                url = this.login_ok;
            }
            var res = this.Get(url, true);
            var contacts = new List<Contacts>();
            if (this.CheckResponse("get_friends", res))
                this.UpdateDebugger("get_friends", url, "GET");
            else {
                this.UpdateDebugger("get_friends", url, "GET", false);
                this.DebugRequest();
                this.StopPlugin();
                return null;
            }
            //doc=new DOMDocument();
            //libxml_use_internal_errors(true);
            //if (!empty(res)) 
            //    doc.loadHTML(res);
            //libxml_use_internal_errors(false);
            //xpath=new DOMXPath(doc);
            //query="//a[@class="user-name"]";data=xpath.query(query);
            //foreach (data as node) users[node.getAttribute("href")]=node.nodeValue;
            //if (!empty(users))
            //    foreach(users as profileLink=>name)
            //        {
            //        res=this.get("https://www.xing.com".profileLink,true);				
            //        mails=this.getElementDOM(res,"//a[@class="url"]");
            //        if (!empty(mails[0])) contacts[mails[0]]=array("email_1"=>mails[0],"first_name"=>name);
            //        }
            //foreach (contacts as email=>name) if (!this.isEmail(email)) unset(contacts[email]);
            return contacts;
        }

        public bool Logout() {
            if (!this.CheckSession()) return false;
            res = this.Get("https://www.xing.com/app/user?op=logout", true);
            this.DebugRequest();
            this.ResetDebugger();
            this.StopPlugin();
            return true;
        }
    }
}