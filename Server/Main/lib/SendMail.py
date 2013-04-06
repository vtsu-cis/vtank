###########################################################################
# \file SendMail.py
# \brief Module to send emails to VTank developers.
# \author Copyright 2010 by Vermont Technical College
###########################################################################

from os import popen;
SENDMAIL = "/usr/sbin/sendmail -t -oi";

class MailMessage:
    """
    The MailMessage class will handle mailing (via sendmail).
    """
    
    def __init__(self, sender, subject, message, to):
        self.sender = sender;
        self.to = to;
        self.subject = subject;
        self.message = message.replace("\\n", "\n");
    
    def mail(self):
        # Define sendmail's location.
        SENDMAIL = "/usr/sbin/sendmail -t -oi";
        try:
            # Open SENDMAIL for writing.
            p = popen("%s" % (SENDMAIL), "w");
            p.write("From: %s\n" % self.sender);
            p.write("To: %s\n" % (self.to));
            p.write("Subject: %s\n" % (self.subject));
            p.write("\n");
            p.write("%s" % (self.message));
            
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

        
