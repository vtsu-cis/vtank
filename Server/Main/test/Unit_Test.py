##########################################################################
# FILE     : Unit_Test.py
# SUBJECT  : Unit tests for the server.
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
import unittest;

# Unit tests
import Communications_Test;
import Player_Test;

def run_all_tests():
    """
    Run all unit tests.
    @return Number of unit tests that have failed (0 meaning that all unit tests were successful.)
    """
    
    return 1;

if __name__ == "__main__":
    # When this file is ran, run unit tests.
    import sys;
    sys.exit(run_all_tests());
