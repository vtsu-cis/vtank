/*!
    \file   target.hpp
    \brief  Defines a TARGET macro for distinguishing different OS builds
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef TARGET_HPP
#define TARGET_HPP

#define WINTARGET 1  // Values are arbitrary.
#define LINTARGET 2

#if !defined(TARGET)
#error The TARGET macro must be defined.
#endif

#if (TARGET != WINTARGET) && (TARGET != LINTARGET)
#error TARGET must be either WINTARGET or LINTARGET
#endif

#endif
