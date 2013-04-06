/*!
    \file   Support.hpp
    \brief  Declaration (and documentation) of the support name space.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef UTILITY_HPP
#define UTILITY_HPP

#if TARGET == WINTARGET
const char path_delimiter = '\\';
#elif TARGET == LINTARGET
const char path_delimiter = '/';
#endif

//! Contains various supporting and utility functions, classes, etc.
/*! This name space contains a collection of utility and other supporting facilities that don't
    naturally fit into any other file.
 */
namespace Support {
    std::string normalize_path(const char *path);
    std::string normalize_path(const std::string &path);
    wxString normalize_path_wx(const wxChar *path);
    wxString normalize_path_wx(const wxString &path);
}

#define CATCH_LOGIC_ERRORS \
    catch (const std::logic_error &e) { \
        (void)wxMessageBox( \
            wxString(e.what(), wxConvUTF8), wxT("Map Editor Logic Error")); \
    }


#endif
