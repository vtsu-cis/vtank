# Sendmail.py
# handles feedback
# Stackless Python Server

from os import popen;

class MailMessage:
    """
    The MailMessage class will handle mailing (via sendmail).
    """
    
    sender = "";
    to = "";
    subject = "";
    message = "";
    smtp = "";
    port = 0;
    
    def __init__(self, sender="", to="vtcit2@vtc.vsc.edu", subject="WOW Feedback", message="", from_whom=""):
        """
        Initialize the MailMessage class.  Sends a mail message to the target host.
        All parameters are optional (since they can be set later).
        @param sender Name of the person who sends the message.
        @param to Recepient of the message.
        @param subject Subject of the message.
        @param message Content of the message.
        @param host Target SMTP server.
        """
        self.sender = sender;
        self.to = to;
        self.subject = subject;
        self.message = message.replace("\\n", "\n");
        self.from_whom = from_whom;
    
    def setSender(self, sender):
        """Set the name/email of the person who sent this message."""
        self.sender = sender;
        
    def setMessage(self, newMessage):
        """Set the content."""
        self.message = newMessage;
    
    def getMessage(self):
        """Obtain the message that will be sent."""
        return self.message;
    
    def mail(self):
        try:
            # Define sendmail's location.
            SENDMAIL = "/usr/sbin/sendmail";
            
            # Open SENDMAIL for writing.
            p = popen("%s -t -oi" % (SENDMAIL), "w");
            p.write("From: %s\n" % (self.from_whom));
            p.write("To: %s\n" % (self.to));
            p.write("Subject: %s\n" % (self.subject));
            p.write("\n");
            p.write("Feedback sent by %s\n--- Feedback start\n\n%s\n\n--- Feedback end\n" % (self.sender, self.message));
            
            # Close SENDMAIL and obtain the result.
            result = p.close();
            
            if result:
                # Returned non-zero.
                print "ERROR: Sendmail returned",result;
                return False;
        except Exception, e:
            print "ERROR using sendmail:",e;
            return False;
        
        return True;


if __name__ == "__main__":
    answer = raw_input("Send a test e-mail (y/n)? ").strip();
    if answer.lower() == "y":
        to = raw_input("To: ").strip();
        sender = raw_input("From: ").strip();
        subject = raw_input("Subject: ").strip();
        message = raw_input("Message ('\\n' == line break): ").strip().replace("\\n", "\n");
        
        mailer = MailMessage(sender, to, subject, message);
        
        result = mailer.mail();
        
        print "Sendmail finished.  Success:",result;
    else:
        sys.exit(0);
