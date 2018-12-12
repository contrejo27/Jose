using UnityEngine;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace BrandXR
{
    public class MailHelper: MonoBehaviour
    {

        public string sendFromAddress;
        public string sendFromPassword;
        public string displayName;
        public string subject;
        public string body;

        public delegate void CallOnComplete();
        public CallOnComplete OnSuccess;
        public CallOnComplete OnFailed;

        //-----------------------------------------------------//
        public void SendMail( List<string> sendToAddress, List<string> attachmentsPath, CallOnComplete OnSuccess, CallOnComplete OnFailed )
        //-----------------------------------------------------//
        {

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress( sendFromAddress, displayName );

            foreach( string address in sendToAddress )
            {
                mail.To.Add( address );
            }

            mail.Subject = subject;
            mail.Body = body + System.DateTime.Now.ToString( "MM/dd/yyyy" );

            //Add all of the attachments
            foreach( string path in attachmentsPath )
            {
                try
                {
                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment( path );
                    mail.Attachments.Add( attachment );
                }
                catch( Exception ex )
                {
                    Debug.Log( "Exception Error: " + ex );
                }
            }


            SmtpClient smtpServer = new SmtpClient( "smtp.gmail.com", 587 );

            smtpServer.Credentials = new System.Net.NetworkCredential( sendFromAddress, sendFromPassword ) as ICredentialsByHost;

            smtpServer.EnableSsl = true;
            smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;

            ServicePointManager.ServerCertificateValidationCallback =
                delegate ( object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors )
                { return true; };

            //Send the mail
            //https://forum.unity3d.com/threads/email-scrip-freezing-unity.427627/
            smtpServer.SendCompleted += ( s, e ) =>
            {
                SendCompleted( s, e, OnSuccess, OnFailed );
                mail.Dispose();
            };

            smtpServer.SendAsync( mail, null );

        } //END SendMail


        //---------------------------------//
        public void SendCompleted( object sender, System.ComponentModel.AsyncCompletedEventArgs e, CallOnComplete OnSuccess, CallOnComplete OnFailed )
        //---------------------------------//
        {

            if( e.Error == null )
            {
                if( OnSuccess != null ) { OnSuccess.Invoke(); }
            }
            else if( e.Cancelled )
            {
                if( OnFailed != null ) { OnFailed.Invoke(); }
            }
            else
            {
                //Debug.Log( "MailHelper.cs SendCompleted( FAILED ) Error = " + e.Error );
                if( OnFailed != null ) { OnFailed.Invoke(); }
            }

        } //END SendCompleted

    } //END Class

} //END Namespace