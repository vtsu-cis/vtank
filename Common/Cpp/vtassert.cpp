/*!
    \file   vtassert.cpp
    \brief  Definition of VTank assertion handling support facilities.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include <sstream>
#include <stdexcept>
#include "vtassert.hpp"

namespace Utility {

    //! Report a failed assertion to the user.
    /*!
     * This function should only be called if an internal logic error (bug) is detected. It does
     * not return normally but instead throws a std::logic_error exception that will either be
     * caught at some appropriate place in the program or cause the program to terminate.
     *
     * \param file_name The name of the source file where the failed assertion happened.
     * \param line_number The line number in the source file where the failed assertion
     * happened.
     * \param failed_expression The precise condition that failed, written as a string.
     */
    void vtank_assertion_failure(
        const char * const file_name, int line_number, const char * const failed_expression)
    {
        std::ostringstream formatter;
        formatter << file_name << "(" << line_number << "): " << failed_expression;
        throw std::logic_error(formatter.str());
    }

}
