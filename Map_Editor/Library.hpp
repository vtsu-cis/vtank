/*!
    \file   Library.hpp
    \brief  Master header file for libraries used by the map editor.
    \author (C) Copyright 2009 by Vermont Technical College

    This header includes all the other library headers needed by the map editor project. It is
    included at the top of each implementation file and marks the end of header precompilation.

    Headers are listed in three sections: one for the C++ standard library, one for the Boost
    libraries, and one for the wxWidgets library. Unless there is some compelling reason to do
    otherwise, the headers are listed in alphabetical order in each section. Exceptions to this
    rule must be documented.
*/

#ifdef LIBRARY_HPP
#error The file Library.hpp should only be included once in a translation unit.
#else
#define LIBRARY_HPP

// Bring in values for TARGET (this is first because it might be used in following headers).
#include <target.hpp>

// C++ Standard Library
#include <cctype>
#include <cmath>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <exception>
#include <fstream>
#include <iostream>
#include <limits>
#include <list>
#include <map>
#include <stdexcept>
#include <sstream>
#include <string>
#include <vector>
#include <queue>
#include <utility>

// wxWidgets Library
#include <wx/artprov.h>
#include <wx/app.h>
#include <wx/dcbuffer.h>
#include <wx/hyperlink.h>
#include <wx/image.h>
#include <wx/msgdlg.h>
#include <wx/string.h>
#include <wx/textctrl.h>
#include <wx/wx.h>
#include <wx/listbox.h>
#include <wx/arrstr.h>
#include <wx/toolbar.h>
#include <wx/html/helpctrl.h>



// Boost Library
#include <boost/noncopyable.hpp>
#include <boost/thread/thread.hpp>

//Ice Library
#include <Ice/Ice.h>

//Glacier2 Library
#include <Glacier2/Router.h>

#endif
