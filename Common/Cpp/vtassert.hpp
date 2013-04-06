/*!
    \file   vtassert.hpp
    \brief  VTank specific assertion handling macros and supporting facilities.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef VTASSERT_HPP
#define VTASSERT_HPP

namespace Utility {
    void vtank_assertion_failure(
        const char *file_name, int line_number, const char *failed_expression);
}

#if defined(DEBUG)
#define VTANK_ASSERT(expression) if (!(expression)) \
    Utility::vtank_assertion_failure(__FILE__, __LINE__, #expression)
#else
#define VTANK_ASSERT(expression)
#endif

#endif
