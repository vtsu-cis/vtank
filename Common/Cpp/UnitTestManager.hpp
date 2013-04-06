/*! \file    UnitTestManager.hpp
    \brief   Interface to the unit test manager abstract object.
    \author  Peter C. Chapin <pcc482719@gmail.com>
*/

#include <iostream>
#include <stdexcept>

namespace UnitTestManager {
    typedef bool ( *unittest_t )( );
    
    void register_test( unittest_t test_function, const char *test_title );
    void execute_tests( std::ostream &test_output, const char *title );
    void report_failure( const char *file_name, int line_number, const char *description );
    int  test_status( );
    
    class UnitException : public std::logic_error {
    public:
        UnitException( const std::string &message) : std::logic_error( message ) { }
    };
}

#define UNIT_FAIL( description ) \
    UnitTestManager::report_failure( __FILE__, __LINE__, description )
    
#define UNIT_RAISE( description ) \
    throw UnitTestManager::UnitException( description )

#define UNIT_CHECK( expression ) \
    if( !( expression ) ) UNIT_FAIL( #expression )
