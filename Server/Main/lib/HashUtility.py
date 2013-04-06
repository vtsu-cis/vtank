###########################################################################
# \file HashUtility.py
# \brief Various utilities for VTank password hashing.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import random
import hashlib;

def smart_str(s, encoding='utf-8', strings_only=False, errors='strict'):
    """
    Function provided by Django located at:
    Located at /usr/local/lib/python2.5/site-packages/django/utils
    
    @param s String to convert to the provided encoding.
    @param encoding Encoding to convert the string to.
    @param strings_only If true, don't convert (some non-string-like objects.
    @param errors Tolerance level for errors.
    @return Bytestring version of 's'.
    """
    return s;

    #if strings_only and isinstance(s, (types.NoneType, int)):
    #    return s
    #elif not isinstance(s, basestring):
    #    try:
    #        return str(s)
    #    except UnicodeEncodeError:
    #        if isinstance(s, Exception):
                # An Exception subclass containing non-ASCII data that doesn't
                # know how to print itself properly. We shouldn't raise a
                # further exception.
    #            return ' '.join([smart_str(arg, encoding, strings_only,
    #                    errors) for arg in s])
    #        return unicode(s).encode(encoding, errors)
    #elif isinstance(s, unicode):
    #    return s.encode(encoding, errors)
    #elif s and encoding != 'utf-8':
    #    return s.decode('utf-8', errors).encode(encoding, errors)
    #else:
    #    return s

def get_hexdigest(salt, raw_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Get the hexdigest version of the hash produced from sha1 when mixing in a raw password
    with a given salt.
    @param salt Salt to use with the password for extra security.
    @param raw_password Plain password to mix with the salt to produce a hash.
    @return String of the hex digest from the sha1 algorithm.
    """
    raw_password, salt = smart_str(raw_password), smart_str(salt)
    return hashlib.sha1(salt + raw_password).hexdigest();

def check_password(raw_password, enc_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Test whether the raw_password was correct. Handles encryption formats behind the scenes.
    @param raw_password Password that has not been hashed.
    @param enc_password Password that has been hashed to test against.
    @return True if the raw_password matched enc_password; false otherwise.
    """
    algo, salt, hsh = enc_password.split('$')
    return hsh == get_hexdigest(salt, raw_password);

def hash_password(raw_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Hash the password using our custom hash algorithm.
    @param raw_password Password to hash.
    @return Hashed password.
    """
    algo = 'sha1'
    salt = get_hexdigest(str(random.random()), str(random.random()))[:5]
    hsh  = get_hexdigest(salt, raw_password);
    return '%s$%s$%s' % (algo, salt, hsh)

