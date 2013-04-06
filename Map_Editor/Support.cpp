/*!
    \file   Support.cpp
    \brief  Definition of various supporting and utility functions.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "Support.hpp"

namespace Support {

    //! Convert path delimiters to local operating system standard.
    /*! This function scans the given string replacing '/' or '\' characters with the
        appropriate type of slash as required by the local operating system. This allows the
        program (or user) to use either Windows or Unix style paths despite what the underlying
        system actually requires.

        \param path The path to normalize.

        \return The normalized path. The return value is a (modified) copy of the parameter.
        The parameter is not actually changed.
     */
    std::string normalize_path(const char * const path)
    {
        std::string adjusted_path(path);
        for (std::string::size_type i = 0; i < adjusted_path.size(); ++i) {
            if (adjusted_path[i] == '/' || adjusted_path[i] == '\\')
                adjusted_path[i] = path_delimiter;
        }
        return adjusted_path;
    }
    

    //! Convert path delimiters to local operating system standard.
    /*! This function scans the given string replacing '/' or '\' characters with the
        appropriate type of slash as required by the local operating system. This allows the
        program (or user) to use either Windows or Unix style paths despite what the underlying
        system actually requires.

        \param path The path to normalize.

        \return The normalized path. The return value is a (modified) copy of the parameter.
        The parameter is not actually changed.
     */
     std::string normalize_path(const std::string &path)
    {
        std::string adjusted_path(path);
        for (std::string::size_type i = 0; i < adjusted_path.size(); ++i) {
            if (adjusted_path[i] == '/' || adjusted_path[i] == '\\')
                adjusted_path[i] = path_delimiter;
        }
        return adjusted_path;
    }


    //! Convert path delimiters to local operating system standard.
    /*! This function scans the given string replacing '/' or '\' characters with the
        appropriate type of slash as required by the local operating system. This allows the
        program (or user) to use either Windows or Unix style paths despite what the underlying
        system actually requires.

        \param path The path to normalize.

        \return The normalized path. The return value is a (modified) copy of the parameter.
        The parameter is not actually changed.
     */
    wxString normalize_path_wx(const char * const path)
    {
        wxString adjusted_path(path, wxConvUTF8);
        for (size_t i = 0; i < adjusted_path.Len(); ++i) {
            if (adjusted_path[i] == '/' || adjusted_path[i] == '\\')
                adjusted_path[i] = path_delimiter;
        }
        return adjusted_path;
    }


    //! Convert path delimiters to local operating system standard.
    /*! This function scans the given string replacing '/' or '\' characters with the
        appropriate type of slash as required by the local operating system. This allows the
        program (or user) to use either Windows or Unix style paths despite what the underlying
        system actually requires.

        \param path The path to normalize.

        \return The normalized path. The return value is a (modified) copy of the parameter.
        The parameter is not actually changed.
     */
    wxString normalize_path_wx(const wxString &path)
    {
        wxString adjusted_path(path);
        for (size_t i = 0; i < adjusted_path.Len(); ++i) {
            if (adjusted_path[i] == '/' || adjusted_path[i] == '\\')
                adjusted_path[i] = path_delimiter;
        }
        return adjusted_path;
    }

}
