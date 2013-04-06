###########################################################################
# FILE     : Color.py
# SUBJECT  : Hold a single color.
# AUTHOR   : (C) Copyright 2008 by Vermont Technical College
#
# LICENSE
#
# This program is free software; you can redistribute it and/or modify it
# under the terms of the GNU General Public License as published by the
# Free Software Foundation; either version 2 of the License, or (at your
# option) any later version.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANT-
# ABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public
# License for more details.
#
# You should have received a copy of the GNU General Public License along
# with this program; if not, write to the Free Software Foundation, Inc.,
# 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
#
# TODO
#
#   + ...
#
# Please send comments and bug reports to
#
#      Summer of Software Engineering
#      Vermont Technical College
#      201 Lawrence Place
#      Williston, VT 05495
#      sosebugs@summerofsoftware.org (http://www.summerofsoftware.org)
###########################################################################

class Color:
    """
    Representation of a color (RGBA).
    """
    def __init__(self, red = 255, green = 255, blue = 255, alpha = 255):
        """
        Construct a new color.  By default, all values are set to white (255, 255, 255) and
        visible (255).
        """
        self.red    = red;
        self.green  = green;
        self.blue   = blue;
        self.alpha  = alpha;
        
        if red > 255 or green > 255 or blue > 255 or alpha > 255 or \
                    red < 0 or green < 0 or blue < 0 or alpha < 0:
            raise ValueError, "Colors must consist only of numbers between 0 and 255.";
    
    ###### Convenience methods below. #######
    
    def r(self):
        return self.red;
    
    def g(self):
        return self.green;
    
    def b(self):
        return self.blue;
    
    def a(self):
        return self.alpha;
    
    

def to_long(color):
    """
    Converts the object to a long integer that can be converted back into an object.
    @return Long int.
    """
    return ((color.red << 24) | (color.green << 16) | (color.blue << 8) | (255));

def to_color(long_int):
    """
    Converts a long integer to a RGBA color object.
    @param long_int Long int converted from color object.
    @return Color object.
    """
    r = long_int >> 24;
    g = long_int >> 16 & 0xFF;
    b = long_int >> 8 & 0xFF;
    a = long_int & 0xFF;
    
    return Color(r, g, b, a);
